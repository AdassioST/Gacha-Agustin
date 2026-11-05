using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    public sealed partial class Battle
    {
        /// <summary>Lo que un Aspecto cambia en el eslabón (o la Ascensión) que lo completa.</summary>
        internal sealed class AspectApplication
        {
            public double DamageMultiplier = 1.0;
            public double ToughnessMultiplier = 1.0;
            public WitchUnit SextileWitch;
            public bool Tempestad;
        }

        private EnemyUnit ResolveEnemyTarget(int id)
        {
            if (id < 0) return Retarget(null);
            var e = FindEnemy(id);
            return e != null && e.IsAlive ? e : null;
        }

        private WitchUnit ResolveAllyTarget(int id, WitchUnit actor)
        {
            if (id < 0) return actor;
            foreach (var w in _witches) if (w.Id == id && w.IsAlive) return w;
            return null;
        }

        private void SpendGrains(int amount, string reason)
        {
            if (amount <= 0) return;
            int old = Grains;
            Grains = Math.Max(0, Grains - amount);
            Emit(new GrainsChanged { Old = old, New = Grains, Reason = reason });
        }

        private void EmitAction(Unit actor, ActionKind kind, MoveDef move, Unit target, string detail = "", string moveName = null)
        {
            Emit(new ActionUsed
            {
                ActorId = actor.Id,
                ActorName = actor.Name,
                Action = kind,
                MoveId = move?.Id ?? "",
                MoveName = moveName ?? move?.Name ?? "",
                TargetId = target?.Id ?? -1,
                TargetName = target?.Name ?? "",
                Detail = detail,
            });
        }

        /// <summary>Objetivo de un movimiento de bruja según su modo. Devuelve false si el objetivo no sirve.</summary>
        private bool PickTarget(MoveDef move, int targetId, WitchUnit actor, out Unit target, out EnemyUnit enemy)
        {
            target = null;
            enemy = null;
            switch (move.Target)
            {
                case TargetMode.Enemy:
                case TargetMode.AllEnemies:
                    enemy = ResolveEnemyTarget(targetId);
                    target = enemy;
                    return enemy != null;
                case TargetMode.Ally:
                    target = ResolveAllyTarget(targetId, actor);
                    return target != null;
                default:
                    target = actor;
                    return true;
            }
        }

        // ------------------------------------------------------------------ Ataque básico y Hechizo

        private CommandResult DoBasic(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var w);
            if (!r.Ok) return r;
            var move = w.BasicMove;
            if (!PickTarget(move, cmd.TargetId, w, out var target, out var enemy)) return CommandResult.Fail("Objetivo inválido.");

            EmitAction(w, ActionKind.Basic, move, target);
            ApplyEffects(move.Effects, EffectTiming.Start, w, target, move.Name);
            PerformMoveHits(w, move, enemy, HitContext.For(HitSource.Action, 0));
            ApplyEffects(move.Effects, EffectTiming.End, w, target, move.Name);

            AddGrains(Rules.Grains.BasicGain, "Ataque básico");
            ChangeSalinity(w, Rules.Salinity.Basic, "ataque básico");
            ChangeAnima(w, Rules.Anima.Basic);
            ChangeResonance(Rules.Resonance.Basic, "Ataque básico");
            FinishTurn();
            return CommandResult.Success;
        }

        private CommandResult DoSpell(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var w);
            if (!r.Ok) return r;
            if (!w.CanCastMagic) return CommandResult.Fail(w.Name + " no puede lanzar magia ahora (" + (w.IsDesalada ? "Desalada" : "Éxtasis") + ").");
            if (Grains < Rules.Grains.SpellCost) return CommandResult.Fail("No hay Granos de Sal.");
            var move = w.Def.Spell;
            if (!PickTarget(move, cmd.TargetId, w, out var target, out var enemy)) return CommandResult.Fail("Objetivo inválido.");

            SpendGrains(Rules.Grains.SpellCost, "Hechizo");
            EmitAction(w, ActionKind.Spell, move, target, w.Def.Ritual);
            ApplyEffects(move.Effects, EffectTiming.Start, w, target, move.Name);

            var ctx = HitContext.For(HitSource.Action, 0);
            var elixir = w.Find(StatusKind.NextSpellDouble);
            if (elixir != null && move.Hits.Count > 0)
            {
                ctx.SkillScale = elixir.Value;
                RemoveStatus(w, elixir, "consumido");
            }
            PerformMoveHits(w, move, enemy, ctx);
            ApplyEffects(move.Effects, EffectTiming.End, w, target, move.Name);

            _magicThisTurn = true;
            ChangeSalinity(w, Rules.Salinity.Spell + (int)Math.Round(Mod(w, ModKeys.SalinityPerSpell)), "Hechizo");
            ChangeAnima(w, Rules.Anima.Spell);
            ChangeResonance(Rules.Resonance.Spell, "Hechizo");
            FinishTurn();
            return CommandResult.Success;
        }

        // ------------------------------------------------------------------ Autohechizo y Vislumbre

        private CommandResult DoSelfCast(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var w);
            if (!r.Ok) return r;
            if (!w.CanCastMagic) return CommandResult.Fail(w.Name + " no puede lanzar magia ahora.");
            if (_selfCastThisTurn) return CommandResult.Fail("El Autohechizo es una vez por turno.");
            var sc = Rules.SelfCast;
            string name = w.Def.SelfCastName;

            switch (w.Def.SelfCast)
            {
                case SelfCastForm.None:
                    return CommandResult.Fail(w.Name + " no tiene Autohechizo.");

                case SelfCastForm.AgujaYSello:
                    if (Grains < sc.AgujaGrains) return CommandResult.Fail($"Aguja y Sello cuesta {sc.AgujaGrains} Granos.");
                    SpendGrains(sc.AgujaGrains, name);
                    EmitAction(w, ActionKind.SelfCast, null, w, "Aguja y Sello", name);
                    AddStatus(w, StatusKind.AtkPct, sc.AgujaAtk, sc.AgujaTurns, name, isBuff: true, source: w);
                    ChangeSalinity(w, Rules.Salinity.SelfCast, "Autohechizo");
                    break;

                case SelfCastForm.Hilos:
                {
                    if (_isExtraTurn) return CommandResult.Fail("Los Hilos no se encadenan: ya es una acción extra.");
                    int cost = (int)Math.Round(w.MaxHp * sc.HilosHpCost * Math.Max(0.0, 1.0 + Mod(w, ModKeys.HilosCostPct)), MidpointRounding.AwayFromZero);
                    if (w.Hp <= cost) return CommandResult.Fail("No le queda vida para los Hilos.");
                    EmitAction(w, ActionKind.SelfCast, null, w, "Hilos", name);
                    w.Hp -= cost;
                    Emit(new DotTicked { UnitId = w.Id, UnitName = w.Name, StatusName = name, Planet = w.Dominant, Damage = cost, Hp = w.Hp });
                    _extraTurnRequested = true;
                    ChangeSalinity(w, Rules.Salinity.SelfCast, "Autohechizo");
                    break;
                }

                case SelfCastForm.FrenesiSalNegra:
                    if (BlackSalt < sc.FrenesiBlackSalt) return CommandResult.Fail("No queda Sal Negra.");
                    BlackSalt -= sc.FrenesiBlackSalt;
                    EmitAction(w, ActionKind.SelfCast, null, w, "Frenesí de Sal Negra", name);
                    AddStatus(w, StatusKind.DamagePct, sc.FrenesiDamage, sc.FrenesiTurns, name, isBuff: true, source: w);
                    // El precio (−50 % de defensa) dura hasta el final de su siguiente turno: si venciera con este,
                    // nunca coincidiría con un turno enemigo. // TUNING
                    AddStatus(w, StatusKind.DefenseDown, sc.FrenesiDefenseDown, sc.FrenesiTurns, name + " (precio)",
                        isDebuff: true, dispellable: false, source: w, skipFirstTick: true);
                    ChangeSalinity(w, Rules.Salinity.BlackSalt, "Sal Negra");
                    break;
            }

            _selfCastThisTurn = true;
            _magicThisTurn = true;
            AfterFreeAction();
            return CommandResult.Success;
        }

        private CommandResult DoVislumbre(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var w);
            if (!r.Ok) return r;
            if (!w.CanCastMagic) return CommandResult.Fail(w.Name + " no puede lanzar magia ahora.");
            if (w.VislumbreUsed) return CommandResult.Fail("El Vislumbre es una vez por combate.");
            var free = w.Find(StatusKind.FreeVislumbre);
            if (free == null && BlackSalt < Rules.Vislumbre.BlackSaltCost) return CommandResult.Fail("El Vislumbre cuesta una Sal Negra.");

            if (free != null) RemoveStatus(w, free, "consumido");
            else BlackSalt -= Rules.Vislumbre.BlackSaltCost;
            w.VislumbreUsed = true;

            var next = PurityMath.Next(w.Grade);
            string label = w.Grade == Grade.SS ? "Apoteosis" : "Vislumbre";
            EmitAction(w, ActionKind.Vislumbre, null, w, $"Grado {w.Grade} → {next}", label);
            AddStatus(w, StatusKind.Vislumbre, 1, Rules.Vislumbre.Turns, label, isBuff: true, dispellable: false, source: w);
            ChangeSalinity(w, Rules.Salinity.Vislumbre, "Vislumbre");
            _magicThisTurn = true;
            AfterFreeAction();
            return CommandResult.Success;
        }

        // ------------------------------------------------------------------ Pociones

        private CommandResult DoPotion(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var w);
            if (!r.Ok) return r;
            if (_potionsThisTurn >= Rules.Potions.PerTurn) return CommandResult.Fail("Una poción por turno.");
            if (cmd.Slot < 0 || cmd.Slot >= _belt.Count) return CommandResult.Fail("Ranura vacía.");
            var slot = _belt[cmd.Slot];
            if (slot.Charges <= 0) return CommandResult.Fail("Esa ranura no tiene cargas.");
            var def = PotionDef(slot.PotionId);
            if (!def.UsableInCombat) return CommandResult.Fail(def.Name + " solo se usa fuera de combate.");

            Unit target;
            switch (def.Target)
            {
                case PotionTarget.Ally: target = ResolveAllyTarget(cmd.TargetId, w); break;
                case PotionTarget.Enemy: target = ResolveEnemyTarget(cmd.TargetId); break;
                default: target = w; break;
            }
            if (target == null) return CommandResult.Fail("Objetivo inválido.");

            slot.Charges--;
            _potionsThisTurn++;
            EmitAction(w, ActionKind.Potion, null, target, def.Description, def.Name);
            double potency = Mod(w, ModKeys.PotionPotencyPct);
            foreach (var fx in def.Effects) ApplyEffect(potency != 0 ? ScaleEffect(fx, 1.0 + potency) : fx, w, target, def.Name);
            AfterFreeAction();
            return CommandResult.Success;
        }

        /// <summary>Copia de un efecto con su valor escalado (Caldero de mano). Los Granos y los efectos sin número no cambian.</summary>
        internal static EffectDef ScaleEffect(EffectDef fx, double scale)
        {
            switch (fx.Type)
            {
                case EffectType.Grains:
                case EffectType.NextSpellDouble:
                case EffectType.InvertHealing:
                case EffectType.FreeVislumbre:
                case EffectType.FeignDeath:
                case EffectType.Cleanse:
                case EffectType.Dispel:
                case EffectType.GrainBoost:
                case EffectType.Paralaje:
                    return fx;
            }
            return new EffectDef
            {
                Type = fx.Type, Target = fx.Target, At = fx.At, Stat = fx.Stat, Dot = fx.Dot,
                Value = fx.Value * scale, Turns = fx.Turns, Count = fx.Count, Name = fx.Name,
            };
        }

        // ------------------------------------------------------------------ Ascensión

        public int AscensionCost(int level)
        {
            switch (level)
            {
                case 1: return Rules.Anima.Level1Cost;
                case 2: return Rules.Anima.Level2Cost;
                case 3: return Rules.Anima.Level3Cost;
                default: return int.MaxValue;
            }
        }

        /// <summary>Por qué una bruja no puede lanzar una Ascensión ahora ("" si puede).</summary>
        public string AscensionBlocker(WitchUnit w, int level)
        {
            if (Pending != DecisionKind.AllyTurn && Pending != DecisionKind.EnemyTurn && Pending != DecisionKind.AutoTurn) return "Ahora no.";
            if (w == null || w.IsGuest || !_witches.Contains(w)) return "Solo las brujas en el campo.";
            if (!w.IsAlive) return w.Name + " cayó.";
            if (!w.CanCastMagic) return w.Name + " no puede lanzar magia ahora.";
            if (level < 1 || level > 3) return "Nivel inválido.";
            if (w.Anima < AscensionCost(level)) return $"Necesita {AscensionCost(level)} de Ánima.";
            if (level == 3 && w.EffectiveGrade < Grade.S) return "La Ascensión III requiere grado S.";
            if (w.Def.Ascensions[level - 1] == null) return "Sin datos.";
            return "";
        }

        private CommandResult DoAscension(BattleCommand cmd)
        {
            var w = FindWitch(cmd.ActorId);
            string blocker = AscensionBlocker(w, cmd.Level);
            if (blocker.Length > 0) return CommandResult.Fail(blocker);
            var move = w.Def.Ascensions[cmd.Level - 1];
            if (!PickTarget(move, cmd.TargetId, w, out var target, out var enemy)) return CommandResult.Fail("Objetivo inválido.");

            int cost = AscensionCost(cmd.Level);
            ChangeAnima(w, -cost);
            Emit(new AscensionCast { WitchId = w.Id, WitchName = w.Name, Level = cmd.Level, MoveName = move.Name, AnimaSpent = cost, TargetId = target?.Id ?? -1 });

            // Aspecto con la Ascensión anterior si fue de otra bruja en la misma ronda (GDD 7.3).
            var app = new AspectApplication();
            if (_lastAscensionCaster >= 0 && _lastAscensionCaster != w.Id && _lastAscensionCycle == Cycle)
            {
                var prev = FindWitch(_lastAscensionCaster);
                if (prev != null)
                {
                    var aspect = AspectRules.Resolve(prev.Dominant, w.Dominant);
                    var reaction = aspect == Aspect.Oposicion ? AspectRules.ReactionFor(prev.Dominant, w.Dominant) : HybridReaction.None;
                    Emit(new AspectFormed
                    {
                        FirstId = prev.Id, FirstName = prev.Name, SecondId = w.Id, SecondName = w.Name,
                        FirstPlanet = prev.Dominant, SecondPlanet = w.Dominant, Aspect = aspect, Reaction = reaction,
                        Context = "Ascensión", Frame = 0,
                    });
                    app = ApplyAspect(aspect, reaction, prev, w, enemy ?? Retarget(null));
                }
            }
            _lastAscensionCaster = w.Id;
            _lastAscensionCycle = Cycle;

            ApplyEffects(move.Effects, EffectTiming.Start, w, target, move.Name);

            // Spark entre Ascensiones encimadas: las lanzadas en la misma ventana, sin turnos de por medio (GDD 9.6).
            int offset = Rules.Chain.AscensionOverlayOffsetFrames;
            var sparkFlags = new bool[move.Hits.Count];
            var partners = new List<int>();
            for (int h = 0; h < move.Hits.Count; h++)
            {
                int frame = offset + move.Hits[h].Frame + w.FrameShift;
                foreach (var kv in _ascensionWindowHits)
                {
                    if (kv.Key == w.Id || Math.Abs(kv.Value - frame) > Rules.Chain.SparkWindowFrames) continue;
                    sparkFlags[h] = true;
                    if (!partners.Contains(kv.Key)) partners.Add(kv.Key);
                }
            }

            var ctx = HitContext.For(HitSource.Ascension, offset);
            ctx.Aspect = app.DamageMultiplier;
            ctx.ToughnessMultiplier = app.ToughnessMultiplier;
            if (move.Hits.Count > 0) PerformMoveHits(w, move, enemy, ctx, h => sparkFlags[h]);

            for (int h = 0; h < sparkFlags.Length; h++)
            {
                if (!sparkFlags[h]) continue;
                TotalSparks++;
                Almitas += Rules.Chain.SparkAlmitas;
                ChangeAnima(w, Rules.Anima.Spark);
                int res = Math.Min(Rules.Resonance.Spark, Rules.Resonance.SparkCapPerChain - _ascensionWindowSparkResonance);
                if (res > 0) { _ascensionWindowSparkResonance += res; ChangeResonance(res, "Spark"); }
            }
            foreach (var p in partners) ChangeAnima(FindWitch(p), Rules.Anima.Spark * CountTrue(sparkFlags));
            for (int h = 0; h < move.Hits.Count; h++)
                _ascensionWindowHits.Add(new KeyValuePair<int, int>(w.Id, offset + move.Hits[h].Frame + w.FrameShift));

            if (app.Tempestad) RunTempestadHits(w, 0);
            ApplyEffects(move.Effects, EffectTiming.End, w, target, move.Name);
            if (app.SextileWitch != null) ShiftAction(app.SextileWitch, Rules.Aspects.SextileAdvance, "Sextil");

            ChangeSalinity(w, Rules.Salinity.Ascension, "Ascensión");
            if (w == ActiveUnit) _magicThisTurn = true;
            AfterFreeAction();
            return CommandResult.Success;
        }

        private static int CountTrue(bool[] flags)
        {
            int n = 0;
            foreach (var f in flags) if (f) n++;
            return n;
        }

        // ------------------------------------------------------------------ Aspectos y reacciones

        internal AspectApplication ApplyAspect(Aspect aspect, HybridReaction reaction, WitchUnit first, WitchUnit second, EnemyUnit target)
        {
            var app = new AspectApplication();
            var a = Rules.Aspects;
            if (target != null && target.Def.Traits.AspectImmune)
            {
                // Los Sin Carta (GDD 6.5): sin carta natal, los Aspectos no tienen dónde agarrarse.
                Emit(new ReactionTriggered { Reaction = HybridReaction.None, TargetId = target.Id, TargetName = target.Name, Detail = "no tiene carta natal: el Aspecto se pierde" });
                return app;
            }
            switch (aspect)
            {
                case Aspect.Conjuncion:
                    app.DamageMultiplier = 1.0 + a.ConjunctionDamage;
                    break;
                case Aspect.Oposicion:
                    TriggerReaction(reaction, second, target);
                    app.Tempestad = reaction == HybridReaction.Tempestad;
                    ChangeSalinity(first, a.OppositionSalinity, "Oposición");
                    ChangeSalinity(second, a.OppositionSalinity, "Oposición");
                    break;
                case Aspect.Sextil:
                    app.SextileWitch = second.IsGuest ? null : second;
                    break;
                case Aspect.Trigono:
                    AddGrains(a.TrineGrains, "Trígono");
                    Heal(first, (int)Math.Round(first.MaxHp * a.TrineHeal, MidpointRounding.AwayFromZero), "Trígono");
                    Heal(second, (int)Math.Round(second.MaxHp * a.TrineHeal, MidpointRounding.AwayFromZero), "Trígono");
                    break;
                case Aspect.Cuadratura:
                    app.ToughnessMultiplier = 1.0 + a.SquareToughness;
                    ChangeSalinity(second, a.SquareSalinity, "Cuadratura");
                    break;
            }
            return app;
        }

        internal void TriggerReaction(HybridReaction reaction, WitchUnit provoker, EnemyUnit target)
        {
            var rr = Rules.Reactions;
            switch (reaction)
            {
                case HybridReaction.Eclipse:
                    if (target == null) return;
                    Emit(new ReactionTriggered { Reaction = reaction, TargetId = target.Id, TargetName = target.Name, Detail = $"quita {rr.EclipseDispel} mejoras y retrasa {rr.EclipseDelay:P0}" });
                    RemoveMatching(target, s => s.IsBuff && s.Dispellable, rr.EclipseDispel, "Eclipse");
                    ShiftAction(target, -rr.EclipseDelay, "Eclipse");
                    OnDelayCaused(provoker, target);
                    break;
                case HybridReaction.HeladaNegra:
                    if (target == null) return;
                    if (target.IsBoss)
                    {
                        Emit(new ReactionTriggered { Reaction = reaction, TargetId = target.Id, TargetName = target.Name, Detail = $"pierde {rr.HeladaBossSpeedDown:P0} de Velocidad por un turno (es jefe)" });
                        AddStatus(target, StatusKind.SpeedPct, -rr.HeladaBossSpeedDown, rr.HeladaBossTurns, "Helada Negra", isDebuff: true, source: provoker);
                    }
                    else
                    {
                        Emit(new ReactionTriggered { Reaction = reaction, TargetId = target.Id, TargetName = target.Name, Detail = "pierde su siguiente turno" });
                        AddStatus(target, StatusKind.Frozen, 0, -1, "Helada Negra", isDebuff: true, source: provoker);
                    }
                    break;
                case HybridReaction.EspinasRojas:
                    if (target == null) return;
                    Emit(new ReactionTriggered { Reaction = reaction, TargetId = target.Id, TargetName = target.Name, Detail = "sangrado que cura al equipo" });
                    AddDot(target, DotKind.Bleed, rr.EspinasBleedCoef, rr.EspinasTurns, provoker, "Espinas Rojas", healShare: rr.EspinasHealShare);
                    break;
                case HybridReaction.Tempestad:
                    Emit(new ReactionTriggered { Reaction = reaction, TargetId = -1, TargetName = "", Detail = "rayo en cadena a todos los enemigos" });
                    break;
            }
        }

        /// <summary>Golpes extra de la Tempestad fuera de la Cadena (en la Cadena los planifica ChainPlanner).</summary>
        private void RunTempestadHits(WitchUnit provoker, int baseFrame)
        {
            var rr = Rules.Reactions;
            int extra = Math.Min(rr.TempestadExtraHits, rr.TempestadHitFrames.Length);
            for (int k = 0; k < extra; k++)
            {
                var ctx = ReactionContext(baseFrame + rr.TempestadHitFrames[k], -1);
                foreach (var e in AliveEnemies())
                    ResolveWitchHit(provoker, e, rr.TempestadCoef, rr.TempestadToughness, ctx);
            }
        }

        private static HitContext ReactionContext(int frame, int link)
        {
            var ctx = HitContext.For(HitSource.Reaction, frame);
            ctx.IsReaction = true;
            ctx.ReactionA = Planet.Mercurio;
            ctx.ReactionB = Planet.Jupiter;
            ctx.ChainLink = link;
            return ctx;
        }

        // ------------------------------------------------------------------ Éxtasis Abisal

        /// <summary>El Ascendente toma el control: usa su Cierre descontrolado (daño ×2) contra un enemigo al azar.</summary>
        private void RunEcstasyTurn(WitchUnit w)
        {
            var enemies = AliveEnemies();
            var target = enemies[_rng.NextInt(enemies.Count)];
            var move = w.Def.Closing;
            EmitAction(w, ActionKind.Ecstasy, move, target, "El Ascendente toma el control");
            ApplyEffects(move.Effects, EffectTiming.Start, w, target, move.Name);
            var ctx = HitContext.For(HitSource.Ecstasy, 0);
            ctx.SkillScale = Rules.Salinity.ExtasisDamageMultiplier;
            PerformMoveHits(w, move, target, ctx);
            ApplyEffects(move.Effects, EffectTiming.End, w, target, move.Name);
            ChangeAnima(w, Rules.Anima.Ecstasy);
            ChangeResonance(Rules.Resonance.Ecstasy, "Éxtasis");
            _magicThisTurn = true;
            FinishTurn();
        }
    }
}
