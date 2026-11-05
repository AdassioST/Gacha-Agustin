using System;
using System.Collections.Generic;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    /// <summary>
    /// Combate automático (GDD 9.9). Decide una orden a la vez a partir del estado, sin memoria propia, así que
    /// es determinista. Para las Cadenas usa primero las Partituras guardadas y, si ninguna sirve, el orden con
    /// más Spark previstos.
    /// </summary>
    public sealed class AutoPilot
    {
        public List<SavedPartitura> Partituras = new List<SavedPartitura>();
        /// <summary>Si es false, el piloto solo usa ataques básicos y Hechizos (sirve de línea base para el balance).</summary>
        public bool UseChains = true;
        public bool UseAscensions = true;
        public bool UsePotions = true;
        public bool UseSelfCasts = true;
        /// <summary>Solo para balance: si es true, abre las Cadenas con el peor orden posible.</summary>
        public bool WorstChainOrder;

        public BattleCommand Next(Battle b)
        {
            switch (b.Pending)
            {
                case DecisionKind.BattleOver:
                case DecisionKind.None:
                    return null;
                case DecisionKind.EnemyTurn:
                case DecisionKind.AutoTurn:
                    return TryAscension(b) ?? BattleCommand.Advance();
            }

            var w = (WitchUnit)b.ActiveUnit;
            var target = PreferredTarget(b, w);
            int targetId = target?.Id ?? -1;

            var asc = TryAscension(b);
            if (asc != null) return asc;
            if (UsePotions)
            {
                var potion = TryPotion(b, w);
                if (potion != null) return potion;
            }
            if (UseSelfCasts)
            {
                var self = TrySelfCast(b, w);
                if (self != null) return self;
            }
            if (UseChains && b.ChainBlocker().Length == 0)
            {
                var chain = PickChain(b);
                if (chain != null) return BattleCommand.Chain(w.Id, chain.Value.order, chain.Value.guest, targetId);
            }
            return MainAction(b, w, targetId);
        }

        /// <summary>Juega el combate completo. Devuelve la cantidad de órdenes ejecutadas.</summary>
        public int RunToEnd(Battle b, int maxCommands = 5000)
        {
            int count = 0;
            while (!b.IsOver && count < maxCommands)
            {
                var cmd = Next(b);
                if (cmd == null) break;
                var r = b.Execute(cmd);
                if (!r.Ok)
                {
                    // Una orden opcional no sirvió: cae al ataque básico o a Avanzar.
                    var fallback = b.Pending == DecisionKind.AllyTurn
                        ? BattleCommand.Basic(b.ActiveUnit.Id, PreferredTarget(b, (WitchUnit)b.ActiveUnit)?.Id ?? -1)
                        : BattleCommand.Advance();
                    r = b.Execute(fallback);
                    if (!r.Ok) throw new InvalidOperationException("El piloto automático se atascó: " + r.Error);
                }
                count++;
            }
            return count;
        }

        public static EnemyUnit PreferredTarget(Battle b, WitchUnit w)
        {
            EnemyUnit best = null;
            double bestScore = double.MinValue;
            foreach (var e in b.AliveEnemies())
            {
                double score = 0;
                if (e.IsBoss) score += 1000;
                if (w != null && (e.IsWeakTo(w.Dominant) || e.IsWeakTo(w.Recessive))) score += 200;
                if (e.IsCharging) score += 300;
                score -= e.Hp / (double)Math.Max(1, e.MaxHp) * 100;
                if (score > bestScore) { bestScore = score; best = e; }
            }
            return best;
        }

        private BattleCommand TryAscension(Battle b)
        {
            if (!UseAscensions) return null;
            foreach (var w in b.Witches)
            {
                for (int level = 3; level >= 2; level--)
                {
                    if (b.AscensionBlocker(w, level).Length != 0) continue;
                    var move = w.Def.Ascensions[level - 1];
                    int target = move.Target == TargetMode.Ally ? LowestHpAlly(b)?.Id ?? -1 : PreferredTarget(b, w)?.Id ?? -1;
                    return BattleCommand.Ascension(w.Id, level, target);
                }
            }
            return null;
        }

        private static WitchUnit LowestHpAlly(Battle b)
        {
            WitchUnit best = null;
            foreach (var w in b.AliveWitches())
                if (best == null || w.Hp / (double)w.MaxHp < best.Hp / (double)best.MaxHp) best = w;
            return best;
        }

        private BattleCommand TryPotion(Battle b, WitchUnit w)
        {
            if (b.PotionUsedThisTurn) return null;
            var low = LowestHpAlly(b);
            for (int i = 0; i < b.Belt.Count; i++)
            {
                var slot = b.Belt[i];
                if (slot.Charges <= 0) continue;
                var def = b.Db.Potion(slot.PotionId);
                if (!def.UsableInCombat) continue;
                foreach (var fx in def.Effects)
                {
                    if (fx.Type == EffectType.Heal && low != null && low.Hp < low.MaxHp * 0.45)
                        return BattleCommand.Potion(w.Id, i, low.Id);
                    if (fx.Type == EffectType.Shield && fx.Target == EffectTarget.AllAllies && CountBelow(b, 0.6) >= 2)
                        return BattleCommand.Potion(w.Id, i, w.Id);
                    if (fx.Type == EffectType.Salinity && fx.Value < 0 && w.Salinity >= 95)
                        return BattleCommand.Potion(w.Id, i, w.Id);
                    if (fx.Type == EffectType.Grains && b.Grains == 0)
                        return BattleCommand.Potion(w.Id, i, w.Id);
                }
            }
            return null;
        }

        private static int CountBelow(Battle b, double ratio)
        {
            int n = 0;
            foreach (var w in b.AliveWitches()) if (w.Hp < w.MaxHp * ratio) n++;
            return n;
        }

        private static BattleCommand TrySelfCast(Battle b, WitchUnit w)
        {
            if (b.SelfCastUsedThisTurn || !w.CanCastMagic) return null;
            switch (w.Def.SelfCast)
            {
                case SelfCastForm.AgujaYSello:
                    if (b.Grains >= b.Rules.SelfCast.AgujaGrains + 1 && w.Salinity < 70 && w.Find(StatusKind.AtkPct) == null)
                        return BattleCommand.SelfCast(w.Id);
                    break;
                case SelfCastForm.FrenesiSalNegra:
                    if (b.BlackSalt >= 2 && b.Grains >= 1 && w.Salinity <= 90)
                        return BattleCommand.SelfCast(w.Id);
                    break;
                case SelfCastForm.Hilos:
                    if (!b.IsExtraTurn && w.Hp > w.MaxHp * 0.6 && w.Salinity < 60 && b.Grains >= 2)
                        return BattleCommand.SelfCast(w.Id);
                    break;
            }
            return null;
        }

        private static BattleCommand MainAction(Battle b, WitchUnit w, int targetId)
        {
            bool canSpell = w.CanCastMagic && b.Grains >= b.Rules.Grains.SpellCost && w.Salinity < 90;
            if (canSpell)
            {
                var spell = w.Def.Spell;
                bool support = spell.Hits.Count == 0;
                if (support && spell.Effects.Exists(fx => fx.Type == EffectType.GrainBoost) && b.GrainBoostActive) canSpell = false;
                if (!support && b.Grains < 2 && w.Def.SelfCast == SelfCastForm.None) canSpell = false;
            }
            if (canSpell)
            {
                int t = w.Def.Spell.Target == TargetMode.Ally ? LowestHpAlly(b)?.Id ?? w.Id : targetId;
                return BattleCommand.Spell(w.Id, t);
            }
            return BattleCommand.Basic(w.Id, targetId);
        }

        // ------------------------------------------------------------------ Cadena

        public (List<int> order, bool guest)? PickChain(Battle b)
        {
            foreach (var p in Partituras)
            {
                var order = new List<int>();
                bool ok = true;
                foreach (var id in p.WitchIds)
                {
                    WitchUnit unit = null;
                    foreach (var w in b.Witches) if (w.Key == id) unit = w;
                    if (unit == null) { ok = false; break; }
                    order.Add(unit.Id);
                }
                if (!ok) continue;
                bool guest = p.WithGuest && b.CanUseGuestInChain;
                if (b.PreviewChain(order, guest).IsValid) return (order, guest);
            }
            var best = BestChain(b, WorstChainOrder);
            return best == null ? ((List<int>, bool)?)null : (best.Value.order, best.Value.guest);
        }

        /// <summary>Busca entre todos los órdenes posibles el de mejor puntaje (Spark primero, luego Aspectos).</summary>
        public static (List<int> order, bool guest, ChainPreview preview)? BestChain(Battle b, bool worst = false)
        {
            var eligible = b.ChainEligibleWitches();
            var ids = new List<int>();
            foreach (var w in eligible) ids.Add(w.Id);
            int min = b.Rules.Chain.MinWitches, max = Math.Min(b.Rules.Chain.MaxWitches, ids.Count);
            if (ids.Count < min) return null;

            (List<int> order, bool guest, ChainPreview preview)? best = null;
            double bestScore = double.MinValue;
            var guestOptions = b.CanUseGuestInChain ? new[] { true, false } : new[] { false };
            for (int size = max; size >= min; size--)
            {
                foreach (var perm in Permutations(ids, size))
                {
                    foreach (bool g in guestOptions)
                    {
                        var preview = b.PreviewChain(perm, g);
                        if (!preview.IsValid) continue;
                        double score = Score(b, preview) * (worst ? -1 : 1);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = (new List<int>(perm), g, preview);
                        }
                    }
                }
            }
            return best;
        }

        public static double Score(Battle b, ChainPreview p)
        {
            double score = p.SparkCount * 10 + p.Links.Count * 6;
            var target = PreferredTarget(b, null);
            foreach (var a in p.Aspects)
            {
                switch (a.Aspect)
                {
                    case Aspect.Oposicion: score += 12; break;
                    case Aspect.Conjuncion: score += 6; break;
                    case Aspect.Trigono: score += 5; break;
                    case Aspect.Cuadratura: score += target != null && !target.Broken ? 8 : 2; break;
                    case Aspect.Sextil: score += 3; break;
                }
            }
            return score;
        }

        public static IEnumerable<List<int>> Permutations(List<int> items, int size)
        {
            var used = new bool[items.Count];
            var current = new List<int>();
            foreach (var p in Permute(items, size, used, current)) yield return p;
        }

        private static IEnumerable<List<int>> Permute(List<int> items, int size, bool[] used, List<int> current)
        {
            if (current.Count == size)
            {
                yield return new List<int>(current);
                yield break;
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (used[i]) continue;
                used[i] = true;
                current.Add(items[i]);
                foreach (var p in Permute(items, size, used, current)) yield return p;
                current.RemoveAt(current.Count - 1);
                used[i] = false;
            }
        }
    }
}
