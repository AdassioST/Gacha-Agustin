using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    /// <summary>Modificadores de un golpe que vienen de su contexto (Cadena, Aspecto, Spark, Éxtasis…).</summary>
    internal struct HitContext
    {
        public HitSource Source;
        public int Frame;
        public bool IsSpark;
        /// <summary>A: bono de Aspecto.</summary>
        public double Aspect;
        /// <summary>Multiplicador de la Firmeza quitada (Cuadratura).</summary>
        public double ToughnessMultiplier;
        /// <summary>Escala del M_hab (Elixir ×2, Éxtasis ×2, Vislumbre en Enlace y Cierre).</summary>
        public double SkillScale;
        public bool ByAscendant;
        public int ChainLink;
        /// <summary>Golpe de reacción (Tempestad): reparte el daño entre los dos planetas de la reacción.</summary>
        public bool IsReaction;
        public Planet ReactionA;
        public Planet ReactionB;

        public static HitContext For(HitSource source, int frame) => new HitContext
        {
            Source = source,
            Frame = frame,
            Aspect = 1.0,
            ToughnessMultiplier = 1.0,
            SkillScale = 1.0,
            ChainLink = -1,
        };
    }

    public sealed partial class Battle
    {
        /// <summary>Resultado de un golpe de bruja, para sumar el daño de la Cadena.</summary>
        internal struct HitOutcome
        {
            public int Damage;
            public bool Broke;
        }

        /// <summary>
        /// Golpe de bruja contra un enemigo. Reparte el daño entre su planeta dominante y el secundario según la
        /// pureza (GDD 7.2) y aplica la fórmula completa de la sección 9.8 a cada parte.
        /// </summary>
        internal HitOutcome ResolveWitchHit(WitchUnit attacker, EnemyUnit target, double mHab, double toughness, HitContext ctx)
        {
            var outcome = new HitOutcome();
            if (target == null || !target.IsAlive) return outcome;

            // Partes por planeta: (planeta, M_p para el daño, parte de la Firmeza)
            var parts = new List<(Planet planet, double mp, double toughShare)>(2);
            if (ctx.IsReaction)
            {
                parts.Add((ctx.ReactionA, 0.5, 0.5));
                parts.Add((ctx.ReactionB, 0.5, 0.5));
            }
            else
            {
                double p = Purity(attacker);
                parts.Add((attacker.Dominant, PurityMath.DominantShare(p, Rules.Purity.DominantSlope, Rules.Purity.Pivot), p));
                parts.Add((attacker.Recessive, PurityMath.SecondaryShare(p), 1.0 - p));
            }

            bool dealsDamage = !attacker.Def.DealsNoDamage && mHab > 0;
            bool crit = false;
            if (dealsDamage) crit = _rng.NextDouble() < CritRate(attacker);
            double critMult = crit ? 1.0 + attacker.Def.CritDamage : 1.0;
            double bonus = DamageBonus(attacker, ctx.ByAscendant) + target.Sum(StatusKind.DamageTaken);
            double vuln = target.Broken ? 1.0 + Rules.Break.Vulnerability : 1.0;

            double domDamage = 0, secDamage = 0;
            if (dealsDamage)
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    var input = new DamageInput
                    {
                        Atk = Atk(attacker),
                        SkillMultiplier = mHab * ctx.SkillScale,
                        BonusSum = bonus,
                        PurityMultiplier = parts[i].mp,
                        CritMultiplier = critMult,
                        AttackerLevel = attacker.Level,
                        TargetLevel = target.Level,
                        DefenseReduction = DefenseReduction(target),
                        TargetRes = Res(target, parts[i].planet),
                        Penetration = 0,
                        Vulnerability = vuln,
                        Spark = ctx.IsSpark ? 1.0 + Rules.Chain.SparkDamageBonus : 1.0,
                        Aspect = ctx.Aspect,
                    };
                    double d = DamageFormula.Compute(input);
                    if (i == 0) domDamage = d; else secDamage = d;
                }
            }
            int total = dealsDamage ? Math.Max(1, DamageFormula.Round(domDamage + secDamage)) : 0;

            // Firmeza: solo los planetas débiles la bajan, cada uno con su parte de la pureza (GDD 9.4).
            double toughDamage = 0;
            Planet breakPlanet = parts[0].planet;
            double bestShare = -1;
            if (!target.Broken && toughness > 0)
            {
                double extra = 1.0 + attacker.Sum(StatusKind.NextToughness);
                foreach (var part in parts)
                {
                    if (!target.IsWeakTo(part.planet) || part.toughShare <= 0) continue;
                    toughDamage += toughness * part.toughShare * ctx.ToughnessMultiplier * extra;
                    if (part.toughShare > bestShare) { bestShare = part.toughShare; breakPlanet = part.planet; }
                }
            }

            int absorbed = 0;
            int dealt = ApplyDamage(target, total, out absorbed);
            bool broke = false;
            if (toughDamage > 0 && target.IsAlive)
            {
                target.Toughness = Math.Max(0.0, target.Toughness - toughDamage);
                broke = target.Toughness <= 1e-9;
            }

            Emit(new HitLanded
            {
                AttackerId = attacker.Id,
                AttackerName = attacker.Name,
                TargetId = target.Id,
                TargetName = target.Name,
                Source = ctx.Source,
                Frame = ctx.Frame,
                Damage = total,
                IsSpark = ctx.IsSpark,
                IsCrit = crit,
                Planet = parts[0].planet,
                DominantDamage = DamageFormula.Round(domDamage),
                SecondaryDamage = DamageFormula.Round(secDamage),
                ToughnessDamage = Math.Round(toughDamage, 3),
                ShieldAbsorbed = absorbed,
                TargetHp = target.Hp,
                ChainLink = ctx.ChainLink,
            });

            if (broke) TriggerBreak(target, attacker, breakPlanet);
            AnnounceDeath(target);
            outcome.Damage = dealt + absorbed;
            outcome.Broke = broke;
            return outcome;
        }

        /// <summary>Golpe de enemigo contra una bruja (mismo GDD 9.8; los enemigos tienen un solo planeta, M_p = 1).</summary>
        internal void ResolveEnemyHit(EnemyUnit attacker, WitchUnit target, EnemySkillDef skill, double share, int frame)
        {
            if (target == null || !target.IsAlive) return;
            bool crit = Rules.Combat.EnemyCritRate > 0 && _rng.NextDouble() < Rules.Combat.EnemyCritRate;
            double bonus = DamageBonus(attacker, false) + target.Sum(StatusKind.DamageTaken);
            var charm = attacker.Find(StatusKind.Charm);
            if (charm != null) bonus -= charm.Value;

            var input = new DamageInput
            {
                Atk = Atk(attacker),
                SkillMultiplier = skill.Multiplier * share,
                BonusSum = bonus,
                PurityMultiplier = 1.0,
                CritMultiplier = crit ? 1.0 + Rules.Combat.BaseCritDamage : 1.0,
                AttackerLevel = attacker.Level,
                TargetLevel = target.Level,
                DefenseReduction = DefenseReduction(target),
                TargetRes = Res(target, attacker.Def.AttackPlanet),
                Penetration = 0,
                Vulnerability = 1.0,
                Spark = 1.0,
                Aspect = 1.0,
            };
            int total = skill.Multiplier > 0 ? Math.Max(1, DamageFormula.Round(DamageFormula.Compute(input))) : 0;
            int dealt = ApplyDamage(target, total, out int absorbed);

            Emit(new HitLanded
            {
                AttackerId = attacker.Id,
                AttackerName = attacker.Name,
                TargetId = target.Id,
                TargetName = target.Name,
                Source = HitSource.Enemy,
                Frame = frame,
                Damage = total,
                IsSpark = false,
                IsCrit = crit,
                Planet = attacker.Def.AttackPlanet,
                DominantDamage = total,
                SecondaryDamage = 0,
                ToughnessDamage = 0,
                ShieldAbsorbed = absorbed,
                TargetHp = target.Hp,
                ChainLink = -1,
            });

            if (total > 0) ChangeAnima(target, Rules.Anima.HitTaken);
            AnnounceDeath(target);
            _ = dealt;
        }

        /// <summary>Aplica daño: primero los escudos, luego la vida. Devuelve la vida que quitó.</summary>
        internal int ApplyDamage(Unit target, int amount, out int absorbed)
        {
            absorbed = 0;
            if (amount <= 0 || !target.IsAlive) return 0;
            int remaining = amount;
            var shields = new List<Status>();
            foreach (var s in target.StatusList) if (s.Kind == StatusKind.Shield) shields.Add(s);
            foreach (var s in shields)
            {
                if (remaining <= 0) break;
                int take = (int)Math.Min(remaining, Math.Floor(s.Value));
                s.Value -= take;
                remaining -= take;
                absorbed += take;
                if (s.Value < 1) RemoveStatus(target, s, "roto");
            }
            int dealt = Math.Min(remaining, target.Hp);
            target.Hp -= dealt;
            return dealt;
        }

        // ------------------------------------------------------------------ Quiebre

        public static BreakEffect BreakEffectFor(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return BreakEffect.Quemadura;
                case Planet.Luna: return BreakEffect.Encierro;
                case Planet.Marte: return BreakEffect.SangradoFuerte;
                case Planet.Mercurio: return BreakEffect.VenenoYRetraso;
                case Planet.Venus: return BreakEffect.Encanto;
                case Planet.Jupiter: return BreakEffect.Electrocucion;
                default: return BreakEffect.Gravedad;
            }
        }

        public static string BreakEffectName(BreakEffect e)
        {
            switch (e)
            {
                case BreakEffect.Quemadura: return "Quemadura";
                case BreakEffect.Encierro: return "Encierro";
                case BreakEffect.SangradoFuerte: return "Sangrado fuerte";
                case BreakEffect.VenenoYRetraso: return "Veneno y retraso";
                case BreakEffect.Encanto: return "Encanto";
                case BreakEffect.Electrocucion: return "Electrocución";
                default: return "Gravedad";
            }
        }

        /// <summary>Quiebre (GDD 9.4): retraso de 25 %, +10 % de daño recibido y el efecto del planeta que lo quebró.</summary>
        internal void TriggerBreak(EnemyUnit e, Unit attacker, Planet planet)
        {
            e.Toughness = 0;
            e.Broken = true;
            var effect = BreakEffectFor(planet);
            Emit(new BreakTriggered
            {
                AttackerId = attacker.Id,
                AttackerName = attacker.Name,
                TargetId = e.Id,
                TargetName = e.Name,
                Planet = planet,
                Effect = effect,
                EffectName = BreakEffectName(effect),
            });

            var b = Rules.Break;
            double delay = b.Delay;
            string name = BreakEffectName(effect);
            switch (effect)
            {
                case BreakEffect.Quemadura:
                    AddDot(e, DotKind.Burn, b.SolBurnCoef, b.DotTurns, attacker, name);
                    break;
                case BreakEffect.Encierro:
                    delay += b.LunaExtraDelay;
                    AddStatus(e, StatusKind.SpeedPct, -b.LunaSpeedDown, b.DotTurns, name, isDebuff: true, source: attacker);
                    break;
                case BreakEffect.SangradoFuerte:
                    AddDot(e, DotKind.Bleed, b.MarteBleedCoef, b.DotTurns, attacker, name);
                    break;
                case BreakEffect.VenenoYRetraso:
                    delay += b.MercurioExtraDelay;
                    AddDot(e, DotKind.Poison, b.MercurioPoisonCoef, b.DotTurns, attacker, "Veneno");
                    break;
                case BreakEffect.Encanto:
                    AddStatus(e, StatusKind.Charm, b.VenusCharmDamageDown, -1, name, isDebuff: true, source: attacker);
                    break;
                case BreakEffect.Electrocucion:
                    AddDot(e, DotKind.Shock, b.JupiterShockCoef, b.DotTurns, attacker, name, jumpShare: b.JupiterJumpShare);
                    break;
                case BreakEffect.Gravedad:
                    delay += b.SaturnoExtraDelay;
                    break;
            }
            ShiftAction(e, -delay, "Quiebre");
            ChangeResonance(Rules.Resonance.Break, "Quiebre");

            if (b.CancelsCharge && e.IsCharging)
            {
                Emit(new AttackCanceled { UnitId = e.Id, UnitName = e.Name, SkillName = e.ChargedSkill.Name, Reason = "Quiebre" });
                e.ChargedSkill = null;
            }
        }

        // ------------------------------------------------------------------ Movimientos de bruja

        private EnemyUnit Retarget(EnemyUnit current)
        {
            if (current != null && current.IsAlive) return current;
            foreach (var e in _enemies) if (e.IsAlive) return e;
            return null;
        }

        /// <summary>Ejecuta los golpes de un movimiento fuera de la Cadena. Devuelve el daño total.</summary>
        internal int PerformMoveHits(WitchUnit actor, MoveDef move, EnemyUnit target, HitContext baseCtx, Func<int, bool> sparkForHit = null)
        {
            int total = 0;
            for (int h = 0; h < move.Hits.Count; h++)
            {
                var hit = move.Hits[h];
                var ctx = baseCtx;
                ctx.Frame = baseCtx.Frame + hit.Frame + actor.FrameShift;
                ctx.ByAscendant = move.ByAscendant;
                ctx.IsSpark = sparkForHit != null && sparkForHit(h);
                if (move.Target == TargetMode.AllEnemies)
                {
                    foreach (var e in AliveEnemies())
                        total += ResolveWitchHit(actor, e, move.Multiplier * hit.Share, move.Toughness * hit.Share, ctx).Damage;
                }
                else
                {
                    target = Retarget(target);
                    if (target == null) break;
                    total += ResolveWitchHit(actor, target, move.Multiplier * hit.Share, move.Toughness * hit.Share, ctx).Damage;
                }
            }
            if (move.Hits.Count > 0) ConsumeNextToughness(actor);
            return total;
        }

        private void ConsumeNextToughness(Unit actor)
        {
            var s = actor.Find(StatusKind.NextToughness);
            if (s != null) RemoveStatus(actor, s, "consumido");
        }
    }
}
