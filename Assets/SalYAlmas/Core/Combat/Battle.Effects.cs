using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    public sealed partial class Battle
    {
        // ------------------------------------------------------------------ Estados

        internal Status AddStatus(Unit u, StatusKind kind, double value, int turns, string name,
            bool isBuff = false, bool isDebuff = false, bool dispellable = true, Unit source = null, bool skipFirstTick = false)
        {
            if (u == null || !u.IsAlive) return null;
            if (string.IsNullOrEmpty(name)) name = Status.DefaultName(kind);

            // Mismo tipo y mismo nombre: se renueva en lugar de acumularse.
            Status existing = null;
            foreach (var s in u.StatusList)
                if (s.Kind == kind && s.Name == name) { existing = s; break; }

            Status st = existing ?? new Status();
            st.Kind = kind;
            st.Name = name;
            st.SourceId = source?.Id ?? -1;
            st.Value = existing != null && Math.Abs(existing.Value) > Math.Abs(value) ? existing.Value : value;
            st.TurnsLeft = existing != null && existing.TurnsLeft > turns && turns >= 0 ? existing.TurnsLeft : turns;
            st.IsBuff = isBuff;
            st.IsDebuff = isDebuff;
            st.Dispellable = dispellable;
            st.SkipFirstTick = skipFirstTick;
            if (existing == null) u.StatusList.Add(st);

            Emit(new StatusApplied
            {
                UnitId = u.Id,
                UnitName = u.Name,
                Status = kind,
                StatusName = name,
                Value = st.Value,
                Turns = st.TurnsLeft,
                IsBuff = isBuff,
            });
            return st;
        }

        internal void RemoveStatus(Unit u, Status s, string reason)
        {
            if (!u.StatusList.Remove(s)) return;
            Emit(new StatusRemoved { UnitId = u.Id, UnitName = u.Name, Status = s.Kind, StatusName = s.Name, Reason = reason });
        }

        /// <summary>Baja la duración de los estados al final del turno de quien los lleva.</summary>
        private void TickStatuses(Unit u)
        {
            var copy = new List<Status>(u.StatusList);
            foreach (var s in copy)
            {
                if (s.TurnsLeft < 0) continue;
                if (s.SkipFirstTick) { s.SkipFirstTick = false; continue; }
                s.TurnsLeft--;
                if (s.TurnsLeft > 0) continue;
                RemoveStatus(u, s, "terminó");
                if (s.Kind == StatusKind.Ecstasy && u is WitchUnit w) EndEcstasy(w);
            }
        }

        /// <summary>Los daños por turno se aplican al inicio del turno de quien los sufre.</summary>
        private void TickDots(Unit u)
        {
            var copy = new List<Status>(u.StatusList);
            foreach (var s in copy)
            {
                if (!s.IsDot || !u.IsAlive) continue;
                var input = DamageInput.Neutral(s.SnapshotAtk, s.Value, s.AttackerLevel, u.Level);
                input.DefenseReduction = DefenseReduction(u);
                input.TargetRes = Res(u, s.Planet);
                input.Vulnerability = u is EnemyUnit en && en.Broken ? 1.0 + Rules.Break.Vulnerability : 1.0;
                int dmg = DamageFormula.Round(DamageFormula.Compute(input));
                int dealt = ApplyDamage(u, dmg, out _);
                Emit(new DotTicked { UnitId = u.Id, UnitName = u.Name, StatusName = s.Name, Planet = s.Planet, Damage = dealt, Hp = u.Hp });

                if (s.HealShare > 0 && dmg > 0)
                {
                    int heal = (int)Math.Round(dmg * s.HealShare, MidpointRounding.AwayFromZero);
                    foreach (var w in _witches) if (w.IsAlive) Heal(w, heal, s.Name);
                }
                if (s.JumpShare > 0 && dmg > 0)
                {
                    int jump = (int)Math.Round(dmg * s.JumpShare, MidpointRounding.AwayFromZero);
                    foreach (var other in _enemies)
                    {
                        if (other == u || !other.IsAlive) continue;
                        int d2 = ApplyDamage(other, jump, out _);
                        Emit(new DotTicked { UnitId = other.Id, UnitName = other.Name, StatusName = s.Name + " (salto)", Planet = s.Planet, Damage = d2, Hp = other.Hp });
                        AnnounceDeath(other);
                    }
                }
                AnnounceDeath(u);
            }
        }

        internal void AddDot(Unit target, DotKind kind, double coef, int turns, Unit attacker, string name, double healShare = 0, double jumpShare = 0)
        {
            if (target == null || !target.IsAlive || attacker == null) return;
            if (attacker.Side == Side.Ally) coef *= 1.0 + Mod(attacker, ModKeys.DotPct);
            var st = AddStatus(target, Status.KindForDot(kind), coef, turns, name, isDebuff: true, source: attacker);
            if (st == null) return;
            st.Planet = Status.PlanetForDot(kind);
            st.SnapshotAtk = Atk(attacker);
            st.AttackerLevel = attacker.Level;
            st.HealShare = healShare;
            st.JumpShare = jumpShare;
        }

        // ------------------------------------------------------------------ Recursos

        internal void ChangeSalinity(WitchUnit w, int delta, string reason)
        {
            if (w == null || w.IsGuest || !w.IsAlive || delta == 0) return;
            if (w.InEcstasy) return; // fija en 150 mientras el Ascendente tiene el control
            if (delta > 0)
            {
                double gain = Mod(w, ModKeys.SalinityGainPct);
                if (gain != 0) delta = Math.Max(0, (int)Math.Round(delta * (1.0 + gain), MidpointRounding.AwayFromZero));
                if (delta == 0) return;
            }
            int nv = Math.Max(0, Math.Min(Rules.Salinity.Max, w.Salinity + delta));
            if (nv == w.Salinity) return;
            SetSalinity(w, nv, reason);
            if (nv >= Rules.Salinity.ExtasisAt) StartEcstasy(w);
        }

        private void SetSalinity(WitchUnit w, int value, string reason)
        {
            int old = w.Salinity;
            var oldBand = Band(w);
            w.Salinity = value;
            Emit(new SalinityChanged { WitchId = w.Id, WitchName = w.Name, Old = old, New = value, Reason = reason });
            var newBand = Band(w);
            if (newBand != oldBand)
                Emit(new SalinityBandChanged { WitchId = w.Id, WitchName = w.Name, OldBand = oldBand, NewBand = newBand, Value = value });
        }

        private void StartEcstasy(WitchUnit w)
        {
            if (w.InEcstasy) return;
            // Si llega a 150 durante su propio turno, ese turno no cuenta: el Ascendente toma los dos siguientes.
            AddStatus(w, StatusKind.Ecstasy, Rules.Salinity.ExtasisDamageMultiplier, Rules.Salinity.ExtasisTurns, "Éxtasis Abisal",
                dispellable: false, skipFirstTick: w == ActiveUnit);
            Emit(new EcstasyStarted { WitchId = w.Id, WitchName = w.Name });
        }

        private void EndEcstasy(WitchUnit w)
        {
            SetSalinity(w, Rules.Salinity.ExtasisResetTo, "fin del Éxtasis");
            // Se aplica al final de su último turno de Éxtasis: dura su siguiente turno.
            AddStatus(w, StatusKind.Desalada, 0, Rules.Salinity.DesaladaTurns, "Desalada", isDebuff: true, dispellable: false);
            Emit(new EcstasyEnded { WitchId = w.Id, WitchName = w.Name, SalinityAfter = w.Salinity });
        }

        internal void ChangeAnima(WitchUnit w, int delta)
        {
            if (w == null || w.IsGuest || !w.IsAlive || delta == 0) return;
            if (delta > 0)
            {
                double gain = Mod(w, ModKeys.AnimaGainPct);
                if (gain != 0) delta = Math.Max(0, (int)Math.Round(delta * (1.0 + gain), MidpointRounding.AwayFromZero));
            }
            int nv = Math.Max(0, Math.Min(Rules.Anima.Max, w.Anima + delta));
            if (nv == w.Anima) return;
            int old = w.Anima;
            w.Anima = nv;
            Emit(new AnimaChanged { WitchId = w.Id, WitchName = w.Name, Old = old, New = nv });
        }

        internal void ChangeResonance(int delta, string reason)
        {
            if (delta > 0)
            {
                double gain = _mods.Get(ModKeys.ResonanceGainPct);
                if (gain != 0) delta = Math.Max(0, (int)Math.Round(delta * (1.0 + gain), MidpointRounding.AwayFromZero));
            }
            int nv = Math.Max(0, Math.Min(Rules.Resonance.Max, Resonance + delta));
            if (nv == Resonance) return;
            int old = Resonance;
            Resonance = nv;
            Emit(new ResonanceChanged { Old = old, New = nv, Reason = reason });
        }

        internal void AddGrains(int amount, string reason, bool allowBoost = true)
        {
            if (amount > 0 && allowBoost && GrainBoostActive) amount *= 2;
            int nv = Math.Max(0, Math.Min(Rules.Grains.Max, Grains + amount));
            if (nv == Grains) return;
            int old = Grains;
            Grains = nv;
            Emit(new GrainsChanged { Old = old, New = nv, Reason = reason });
        }

        internal void Heal(Unit u, int amount, string reason)
        {
            if (u == null || !u.IsAlive || amount <= 0) return;
            if (u.Has(StatusKind.InvertHealing))
            {
                int dmg = Math.Min(amount, u.Hp - 1);
                if (dmg <= 0) return;
                u.Hp -= dmg;
                Emit(new Healed { UnitId = u.Id, UnitName = u.Name, Amount = -dmg, Hp = u.Hp, Inverted = true, Reason = reason });
                return;
            }
            double healing = Mod(u, ModKeys.HealingPct);
            if (healing != 0) amount = Math.Max(0, (int)Math.Round(amount * (1.0 + healing), MidpointRounding.AwayFromZero));
            int healed = Math.Min(amount, u.MaxHp - u.Hp);
            if (healed <= 0) return;
            u.Hp += healed;
            Emit(new Healed { UnitId = u.Id, UnitName = u.Name, Amount = healed, Hp = u.Hp, Reason = reason });
        }

        internal void AddShield(Unit u, int amount, int turns, string name, Unit source)
        {
            if (u == null || !u.IsAlive || amount <= 0) return;
            double bonus = Mod(u, ModKeys.ShieldPct);
            if (bonus != 0) amount = Math.Max(1, (int)Math.Round(amount * (1.0 + bonus), MidpointRounding.AwayFromZero));
            AddStatus(u, StatusKind.Shield, amount, turns, name, isBuff: true, source: source, skipFirstTick: false);
            Emit(new ShieldGained { UnitId = u.Id, UnitName = u.Name, Amount = amount });
        }

        /// <summary>Adelanta (pct &gt; 0) o retrasa (pct &lt; 0) la acción de una unidad: mueve pct · 10 000 (GDD 9.2).</summary>
        internal void ShiftAction(Unit u, double pct, string reason)
        {
            if (u == null || !u.IsAlive || pct == 0) return;
            double delta = pct * Rules.TurnOrder.AvBase;
            if (u == ActiveUnit) u.PendingDistanceDelta -= delta;
            else u.Distance = Math.Max(0.0, u.Distance - delta);
            Emit(new ActionShifted { UnitId = u.Id, UnitName = u.Name, Percent = pct, NewAv = ActionValue(u), Reason = reason });
        }

        // ------------------------------------------------------------------ Efectos de datos

        private List<Unit> Recipients(EffectDef fx, Unit actor, Unit target)
        {
            var list = new List<Unit>();
            switch (fx.Target)
            {
                case EffectTarget.Target:
                    if (target != null && target.IsAlive) list.Add(target);
                    break;
                case EffectTarget.Self:
                    if (actor != null) list.Add(actor);
                    break;
                case EffectTarget.AllAllies:
                case EffectTarget.OtherAllies:
                    if (actor == null) break;
                    if (actor.Side == Side.Ally)
                    {
                        foreach (var w in _witches)
                            if (w.IsAlive && !(fx.Target == EffectTarget.OtherAllies && w == actor)) list.Add(w);
                    }
                    else
                    {
                        foreach (var e in _enemies)
                            if (e.IsAlive && !(fx.Target == EffectTarget.OtherAllies && e == actor)) list.Add(e);
                    }
                    break;
                case EffectTarget.AllEnemies:
                    if (actor == null) break;
                    if (actor.Side == Side.Ally) { foreach (var e in _enemies) if (e.IsAlive) list.Add(e); }
                    else { foreach (var w in _witches) if (w.IsAlive) list.Add(w); }
                    break;
            }
            return list;
        }

        internal void ApplyEffects(IEnumerable<EffectDef> effects, EffectTiming timing, Unit actor, Unit target, string sourceName)
        {
            foreach (var fx in effects)
                if (fx.At == timing) ApplyEffect(fx, actor, target, sourceName);
        }

        internal void ApplyEffect(EffectDef fx, Unit actor, Unit target, string sourceName)
        {
            string name = string.IsNullOrEmpty(fx.Name) ? sourceName : fx.Name;
            switch (fx.Type)
            {
                case EffectType.Grains:
                    AddGrains((int)Math.Round(fx.Value), name);
                    return;
                case EffectType.Resonance:
                    ChangeResonance((int)Math.Round(fx.Value), name);
                    return;
                case EffectType.Paralaje:
                    return; // Solo fuera de combate (GDD 6.5).
            }

            foreach (var u in Recipients(fx, actor, target))
            {
                bool selfDuringOwnTurn = u == ActiveUnit;
                switch (fx.Type)
                {
                    case EffectType.Advance:
                        ShiftAction(u, fx.Value, name);
                        break;
                    case EffectType.Delay:
                    {
                        double delay = fx.Value * (actor != null && actor.Side != u.Side ? 1.0 + Mod(actor, ModKeys.DelayPct) : 1.0);
                        ShiftAction(u, -delay, name);
                        if (actor != null && actor.Side != u.Side) OnDelayCaused(actor, u);
                        break;
                    }
                    case EffectType.Buff:
                    case EffectType.Debuff:
                    {
                        bool debuff = fx.Type == EffectType.Debuff;
                        AddStatus(u, Status.KindForStat(fx.Stat), Status.SignedValue(fx.Stat, fx.Value, debuff), fx.Turns, name,
                            isBuff: !debuff, isDebuff: debuff, source: actor);
                        break;
                    }
                    case EffectType.Dot:
                        AddDot(u, fx.Dot, fx.Value, fx.Turns, actor, name);
                        break;
                    case EffectType.Shield:
                        AddShield(u, (int)Math.Round(u.MaxHp * fx.Value, MidpointRounding.AwayFromZero), fx.Turns > 0 ? fx.Turns : -1, name, actor);
                        break;
                    case EffectType.Heal:
                        Heal(u, (int)Math.Round(u.MaxHp * fx.Value, MidpointRounding.AwayFromZero), name);
                        break;
                    case EffectType.Salinity:
                        ChangeSalinity(u as WitchUnit, (int)Math.Round(fx.Value), name);
                        break;
                    case EffectType.Anima:
                        ChangeAnima(u as WitchUnit, (int)Math.Round(fx.Value));
                        break;
                    case EffectType.GrainBoost:
                        AddStatus(u, StatusKind.GrainBoost, 2, fx.Turns, name, isBuff: true, source: actor);
                        break;
                    case EffectType.FeignDeath:
                        if (!FeignDeathUsed && u is EnemyUnit)
                        {
                            FeignDeathUsed = true;
                            AddStatus(u, StatusKind.FeignDeath, 0, -1, "Muerte Fingida", isDebuff: true, dispellable: false, source: actor);
                        }
                        break;
                    case EffectType.Cleanse:
                        RemoveMatching(u, s => s.IsDebuff && s.Dispellable, fx.Count, "limpiado");
                        break;
                    case EffectType.Dispel:
                        RemoveMatching(u, s => s.IsBuff && s.Dispellable, fx.Count, "disipado");
                        break;
                    case EffectType.NextSpellDouble:
                        AddStatus(u, StatusKind.NextSpellDouble, 2, -1, name, isBuff: true, source: actor);
                        break;
                    case EffectType.InvertHealing:
                        AddStatus(u, StatusKind.InvertHealing, 0, Math.Max(1, fx.Turns), name, isDebuff: true, source: actor,
                            skipFirstTick: selfDuringOwnTurn);
                        break;
                    case EffectType.FreeVislumbre:
                        AddStatus(u, StatusKind.FreeVislumbre, 0, -1, name, isBuff: true, dispellable: false, source: actor);
                        break;
                    case EffectType.NextToughness:
                        AddStatus(u, StatusKind.NextToughness, fx.Value, -1, name, isBuff: true, source: actor);
                        break;
                }
            }
        }

        /// <summary>Quita hasta count estados que cumplan la condición, empezando por el más reciente.</summary>
        private int RemoveMatching(Unit u, Predicate<Status> match, int count, string reason)
        {
            int removed = 0;
            for (int i = u.StatusList.Count - 1; i >= 0 && removed < count; i--)
            {
                var s = u.StatusList[i];
                if (!match(s)) continue;
                RemoveStatus(u, s, reason);
                removed++;
            }
            return removed;
        }
    }
}
