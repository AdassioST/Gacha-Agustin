using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;

namespace SalYAlmas.Core.Combat
{
    public sealed partial class Battle
    {
        /// <summary>
        /// IA por patrones: cada enemigo recorre su lista de habilidades en orden y la repite. Los jefes pueden
        /// tener varias acciones por turno. Empezar a cargar o soltar una carga ocupa el turno completo.
        /// </summary>
        private void RunEnemyTurn(EnemyUnit e)
        {
            CheckPhase(e);
            int actions = Math.Max(1, e.Def.ActionsPerTurn);
            for (int i = 0; i < actions; i++)
            {
                if (!e.IsAlive || AliveWitches().Count == 0) break;
                if (e.IsCharging)
                {
                    var charged = e.ChargedSkill;
                    e.ChargedSkill = null;
                    UseOrCancel(e, charged);
                    break;
                }

                var pattern = e.CurrentPattern;
                var skill = e.Def.Skill(pattern[e.PatternIndex % pattern.Count]);
                e.PatternIndex++;
                if (skill.Charge)
                {
                    e.ChargedSkill = skill;
                    Emit(new ActionUsed
                    {
                        ActorId = e.Id, ActorName = e.Name, Action = ActionKind.EnemyCharge, MoveId = skill.Id, MoveName = skill.Name,
                        TargetId = -1, TargetName = "", Detail = skill.ChargeText,
                    });
                    Emit(new EnemyCharging { UnitId = e.Id, UnitName = e.Name, SkillName = skill.Name, Text = skill.ChargeText });
                    break;
                }
                UseOrCancel(e, skill);
            }
            EnemyTraitsAfterTurn(e);
            FinishTurn();
        }

        /// <summary>Rasgos que actúan al final del turno del enemigo: el que crece y la que cura.</summary>
        private void EnemyTraitsAfterTurn(EnemyUnit e)
        {
            if (!e.IsAlive) return;
            var t = e.Def.Traits;
            if (t.RampAtk > 0) AddStatus(e, StatusKind.AtkPct, t.RampAtk, -1, "Crece y olvida", isBuff: true, source: e);
            if (t.HealAllies > 0)
            {
                EnemyUnit low = null;
                foreach (var o in AliveEnemies())
                    if (o.Hp < o.MaxHp && (low == null || o.Hp / (double)o.MaxHp < low.Hp / (double)low.MaxHp)) low = o;
                if (low != null) Heal(low, (int)Math.Round(low.MaxHp * t.HealAllies, MidpointRounding.AwayFromZero), e.Name);
            }
        }

        /// <summary>Rasgos que responden a un golpe de bruja: el reflejo y la furia contra Hechizos.</summary>
        internal void EnemyTraitsOnHit(WitchUnit attacker, EnemyUnit target, int dealt, HitContext ctx)
        {
            var t = target.Def.Traits;
            if (t.ReflectPct > 0 && dealt > 0 && attacker != null && attacker.IsAlive)
            {
                int back = (int)Math.Round(dealt * t.ReflectPct, MidpointRounding.AwayFromZero);
                if (back > 0)
                {
                    int got = ApplyDamage(attacker, back, out _);
                    Emit(new DotTicked { UnitId = attacker.Id, UnitName = attacker.Name, StatusName = "Reflejo de " + target.Name, Planet = target.Def.AttackPlanet, Damage = got, Hp = attacker.Hp });
                    AnnounceDeath(attacker);
                }
            }
            if (t.SpellEnrage > 0 && ctx.Kind == MoveKind.Spell && target.IsAlive)
            {
                var old = target.Find(StatusKind.DamagePct);
                if (old == null || old.Name != "Furia")
                    AddStatus(target, StatusKind.DamagePct, t.SpellEnrage, 2, "Furia", isBuff: true, source: attacker);
            }
        }

        private void UseOrCancel(EnemyUnit e, EnemySkillDef skill)
        {
            var feign = e.Find(StatusKind.FeignDeath);
            if (skill.IsUltimate && feign != null)
            {
                RemoveStatus(e, feign, "consumido");
                Emit(new AttackCanceled { UnitId = e.Id, UnitName = e.Name, SkillName = skill.Name, Reason = "Muerte Fingida" });
                return;
            }
            UseEnemySkill(e, skill);
        }

        private void CheckPhase(EnemyUnit e)
        {
            var ph = e.Def.Phase2;
            if (ph == null || e.InPhase2 || e.Hp > ph.HpBelow * e.MaxHp) return;
            e.InPhase2 = true;
            e.PatternIndex = 0;
            if (ph.SpeedBonus > 0)
                AddStatus(e, StatusKind.SpeedFlat, ph.SpeedBonus, -1, "Segunda fase", isBuff: true, dispellable: false, source: e);
            Emit(new PhaseChanged { UnitId = e.Id, UnitName = e.Name, Text = ph.Text });
        }

        private List<WitchUnit> PickEnemyTargets(EnemyUnit e, EnemySkillDef skill)
        {
            var alive = AliveWitches();
            var result = new List<WitchUnit>();
            if (alive.Count == 0) return result;
            if (skill.TargetRule != EnemyTargetRule.All && skill.TargetRule != EnemyTargetRule.Self)
            {
                // Provocación: quien la lleva atrae los golpes dirigidos.
                var taunter = alive.Find(w => w.Has(StatusKind.Taunt));
                if (taunter != null) { result.Add(taunter); return result; }
            }
            switch (skill.TargetRule)
            {
                case EnemyTargetRule.All:
                    result.AddRange(alive);
                    break;
                case EnemyTargetRule.Self:
                    break;
                case EnemyTargetRule.LowestHp:
                {
                    var best = alive[0];
                    foreach (var w in alive) if (w.Hp < best.Hp) best = w;
                    result.Add(best);
                    break;
                }
                case EnemyTargetRule.HighestSalinity:
                {
                    // El Fresno corrupto huele la sal: va por la bruja más salada. Empate: al azar.
                    int max = -1;
                    foreach (var w in alive) if (w.Salinity > max) max = w.Salinity;
                    var tied = alive.FindAll(w => w.Salinity == max);
                    result.Add(tied[_rng.NextInt(tied.Count)]);
                    break;
                }
                default:
                    result.Add(alive[_rng.NextInt(alive.Count)]);
                    break;
            }
            return result;
        }

        private void UseEnemySkill(EnemyUnit e, EnemySkillDef skill)
        {
            var targets = PickEnemyTargets(e, skill);
            Unit mainTarget = skill.TargetRule == EnemyTargetRule.Self ? e : (targets.Count == 1 ? targets[0] : null);
            Emit(new ActionUsed
            {
                ActorId = e.Id, ActorName = e.Name, Action = ActionKind.EnemySkill, MoveId = skill.Id, MoveName = skill.Name,
                TargetId = mainTarget?.Id ?? -1, TargetName = mainTarget?.Name ?? (targets.Count > 1 ? "todas" : ""), Detail = skill.Description,
            });

            ApplyEnemyEffects(skill, EffectTiming.Start, e, targets);
            bool dealtDamage = false;
            foreach (var hit in skill.Hits)
            {
                foreach (var t in targets)
                {
                    if (!t.IsAlive) continue;
                    ResolveEnemyHit(e, t, skill, hit.Share, hit.Frame);
                    dealtDamage |= skill.Multiplier > 0;
                }
            }
            ApplyEnemyEffects(skill, EffectTiming.End, e, targets);

            var charm = e.Find(StatusKind.Charm);
            if (dealtDamage && charm != null) RemoveStatus(e, charm, "consumido");
        }

        private void ApplyEnemyEffects(EnemySkillDef skill, EffectTiming timing, EnemyUnit e, List<WitchUnit> targets)
        {
            foreach (var fx in skill.Effects)
            {
                if (fx.At != timing) continue;
                if (fx.Target == EffectTarget.Target)
                {
                    foreach (var t in targets) ApplyEffect(fx, e, t, skill.Name);
                    if (skill.TargetRule == EnemyTargetRule.Self) ApplyEffect(fx, e, e, skill.Name);
                }
                else ApplyEffect(fx, e, null, skill.Name);
            }
        }
    }
}
