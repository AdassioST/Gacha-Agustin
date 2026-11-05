using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.World
{
    // ------------------------------------------------------------------ Datos (propuesta de meta-juego, secciones 4, 5 y 8)

    /// <summary>Un encargo o logro: cuenta un evento hasta llegar a Need.</summary>
    public sealed class QuestDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        /// <summary>Hueco del tablero diario: ancla, hazana, termas o taller. Los semanales usan "fijo" o "azar".</summary>
        public string Slot = "";
        public string Event;
        public long Need = 1;
        public readonly List<KeyValuePair<string, long>> Rewards = new List<KeyValuePair<string, long>>();
        public int Growth;
    }

    public sealed class QuestRules
    {
        public bool Enabled;
        public readonly List<QuestDef> Daily = new List<QuestDef>();
        public readonly List<QuestDef> Weekly = new List<QuestDef>();
        public readonly List<QuestDef> Achievements = new List<QuestDef>();
        public readonly string[] DailySlots = { "ancla", "hazana", "termas", "taller" };
        public readonly List<KeyValuePair<string, long>> DailyReward = new List<KeyValuePair<string, long>>();
        public readonly List<KeyValuePair<string, long>> DailyBonus = new List<KeyValuePair<string, long>>();
        public int DailyGrowth = 100;
        public int DailyBonusGrowth = 100;
        public int DailyPass = 100;
        public readonly List<KeyValuePair<string, long>> WeeklyReward = new List<KeyValuePair<string, long>>();
        public int WeeklyGrowth = 300;
        public int WeeklyPass = 1000;
        public int WeeklyRandom = 2;
        /// <summary>Crecimiento automático por evento (prólogo, misión, zona, Juicio…).</summary>
        public readonly Dictionary<string, int> GrowthPerEvent = new Dictionary<string, int>();
        /// <summary>Tramos de la Altura: (hasta el nivel, Crecimiento por nivel).</summary>
        public readonly List<KeyValuePair<int, int>> AlturaCurve = new List<KeyValuePair<int, int>>();
        /// <summary>Sales de Baño por cada nivel de Altura del 2 al 5 (parte del inicio).</summary>
        public int AlturaSales = 80;
        public int AlturaSalesUntil = 5;

        public QuestDef Find(string id) =>
            Daily.Find(m => m.Id == id) ?? Weekly.Find(m => m.Id == id) ?? Achievements.Find(m => m.Id == id);
    }

    public sealed class QuestView
    {
        public QuestDef Def;
        public long Progress;
        public bool Done => Progress >= Def.Need;
        public bool Claimed;
        public bool Claimable => Done && !Claimed;
    }

    /// <summary>
    /// Encargos diarios, semanales, logros y Altura. Los demás sistemas le reportan eventos (secciones, Spark, Salmueras,
    /// baños…). Nunca hay encargos ni logros de tirar, pagar o entrar varios días seguidos. Las recompensas pasan por el
    /// libro contable con las fuentes "misiones" y "logros".
    /// </summary>
    public sealed class QuestService
    {
        private readonly MetaGame _g;
        public QuestService(MetaGame game) { _g = game; }

        private QuestRules R => _g.Meta.World?.Quests;
        private QuestsState S => _g.Profile.Quests;
        public bool Enabled => R != null && R.Enabled;

        // ------------------------------------------------------------------ Eventos

        public void Report(string evt, long amount = 1)
        {
            if (!Enabled || amount <= 0 || string.IsNullOrEmpty(evt)) return;
            Refresh();
            S.Lifetime[evt] = (S.Lifetime.TryGetValue(evt, out var l) ? l : 0) + amount;
            foreach (var id in S.DailyIds)
            {
                var m = R.Find(id);
                if (m != null && m.Event == evt) S.DailyProgress[id] = Math.Min(m.Need, (S.DailyProgress.TryGetValue(id, out var p) ? p : 0) + amount);
            }
            foreach (var id in S.WeeklyIds)
            {
                var m = R.Find(id);
                if (m != null && m.Event == evt) S.WeeklyProgress[id] = Math.Min(m.Need, (S.WeeklyProgress.TryGetValue(id, out var p) ? p : 0) + amount);
            }
            if (R.GrowthPerEvent.TryGetValue(evt, out var growth)) AddGrowth(growth * amount);
        }

        /// <summary>Cuenta lo que pasó en un combate ganado: victoria, Cadenas, Spark, Ascensiones y Quiebres.</summary>
        public void ReportBattle(Battle b)
        {
            if (!Enabled || b == null || b.Outcome != BattleOutcome.Victory) return;
            Report("victoria");
            int chains = 0, asc = 0, breaks = 0;
            foreach (var e in b.Events)
            {
                if (e is ChainStarted) chains++;
                else if (e is AscensionCast) asc++;
                else if (e is BreakTriggered) breaks++;
            }
            Report("cadena", chains);
            Report("spark", b.TotalSparks);
            Report("ascension", asc);
            Report("quiebre", breaks);
        }

        // ------------------------------------------------------------------ Rotación (META 5.3)

        /// <summary>Cambia el tablero al empezar un día o una semana. Determinista con el día del reloj.</summary>
        public void Refresh()
        {
            if (!Enabled) return;
            int day = _g.Profile.Day;
            if (S.Day != day)
            {
                var yesterday = new List<string>(S.DailyIds);
                S.Day = day;
                S.DailyIds.Clear();
                S.DailyProgress.Clear();
                S.DailyClaimed.Clear();
                S.BonusClaimed = false;
                foreach (var slot in R.DailySlots)
                {
                    var pool = R.Daily.FindAll(m => m.Slot == slot);
                    if (pool.Count == 0) continue;
                    int i = (int)(Mix((ulong)day, (ulong)slot.GetHashCodeStable()) % (ulong)pool.Count);
                    // Ningún encargo que no sea ancla se repite dos días seguidos.
                    if (slot != "ancla" && pool.Count > 1 && yesterday.Contains(pool[i].Id)) i = (i + 1) % pool.Count;
                    S.DailyIds.Add(pool[i].Id);
                }
            }
            int week = day / 7;
            if (S.Week != week)
            {
                S.Week = week;
                S.WeeklyIds.Clear();
                S.WeeklyProgress.Clear();
                S.WeeklyClaimed.Clear();
                foreach (var m in R.Weekly) if (m.Slot == "fijo") S.WeeklyIds.Add(m.Id);
                var random = R.Weekly.FindAll(m => m.Slot != "fijo");
                for (int k = 0; k < R.WeeklyRandom && random.Count > 0; k++)
                {
                    int i = (int)(Mix((ulong)week, (ulong)(77 + k)) % (ulong)random.Count);
                    S.WeeklyIds.Add(random[i].Id);
                    random.RemoveAt(i);
                }
            }
        }

        public List<QuestView> DailyBoard()
        {
            Refresh();
            var list = new List<QuestView>();
            if (!Enabled) return list;
            foreach (var id in S.DailyIds)
                list.Add(new QuestView { Def = R.Find(id), Progress = S.DailyProgress.TryGetValue(id, out var p) ? p : 0, Claimed = S.DailyClaimed.Contains(id) });
            return list;
        }

        public List<QuestView> WeeklyBoard()
        {
            Refresh();
            var list = new List<QuestView>();
            if (!Enabled) return list;
            foreach (var id in S.WeeklyIds)
                list.Add(new QuestView { Def = R.Find(id), Progress = S.WeeklyProgress.TryGetValue(id, out var p) ? p : 0, Claimed = S.WeeklyClaimed.Contains(id) });
            return list;
        }

        public List<QuestView> AchievementBoard()
        {
            var list = new List<QuestView>();
            if (!Enabled) return list;
            foreach (var m in R.Achievements)
                list.Add(new QuestView { Def = m, Progress = Math.Min(m.Need, S.Lifetime.TryGetValue(m.Event, out var p) ? p : 0), Claimed = S.AchievementsClaimed.Contains(m.Id) });
            return list;
        }

        /// <summary>Cuántas recompensas esperan en el tablero (para el aviso de la barra inferior).</summary>
        public int Pending
        {
            get
            {
                if (!Enabled) return 0;
                int n = 0;
                foreach (var v in DailyBoard()) if (v.Claimable) n++;
                foreach (var v in WeeklyBoard()) if (v.Claimable) n++;
                foreach (var v in AchievementBoard()) if (v.Claimable) n++;
                if (BonusReady) n++;
                return n;
            }
        }

        public bool BonusReady => Enabled && !S.BonusClaimed && S.DailyIds.Count > 0 && S.DailyIds.TrueForAll(id => S.DailyClaimed.Contains(id));

        // ------------------------------------------------------------------ Cobrar

        public OpResult Claim(string id)
        {
            if (!Enabled) return OpResult.Fail("Los encargos no están activos.");
            Refresh();
            var m = R.Find(id);
            if (m == null) return OpResult.Fail("Encargo desconocido.");
            if (S.DailyIds.Contains(id))
            {
                if (S.DailyClaimed.Contains(id)) return OpResult.Fail("Ya lo cobraste.");
                if ((S.DailyProgress.TryGetValue(id, out var p) ? p : 0) < m.Need) return OpResult.Fail("Todavía no está hecho.");
                S.DailyClaimed.Add(id);
                Pay(R.DailyReward, "misiones", id);
                AddGrowth(R.DailyGrowth);
                _g.Store.AddGrimorioXp(R.DailyPass);
                return OpResult.Success(m.Name);
            }
            if (S.WeeklyIds.Contains(id))
            {
                if (S.WeeklyClaimed.Contains(id)) return OpResult.Fail("Ya lo cobraste.");
                if ((S.WeeklyProgress.TryGetValue(id, out var p) ? p : 0) < m.Need) return OpResult.Fail("Todavía no está hecho.");
                S.WeeklyClaimed.Add(id);
                Pay(R.WeeklyReward, "misiones", id);
                AddGrowth(R.WeeklyGrowth);
                _g.Store.AddGrimorioXp(R.WeeklyPass);
                return OpResult.Success(m.Name);
            }
            if (R.Achievements.Contains(m))
            {
                if (S.AchievementsClaimed.Contains(id)) return OpResult.Fail("Ya lo cobraste.");
                if ((S.Lifetime.TryGetValue(m.Event, out var p) ? p : 0) < m.Need) return OpResult.Fail("Todavía no está hecho.");
                S.AchievementsClaimed.Add(id);
                Pay(m.Rewards, "logros", id);
                AddGrowth(m.Growth);
                return OpResult.Success(m.Name);
            }
            return OpResult.Fail("Ese encargo no es de hoy.");
        }

        /// <summary>El bono por terminar los cuatro encargos del día. Cuenta para el semanal de "cinco días".</summary>
        public OpResult ClaimBonus()
        {
            if (!BonusReady) return OpResult.Fail("Termina y cobra los cuatro encargos del día.");
            S.BonusClaimed = true;
            Pay(R.DailyBonus, "misiones", "bono_diario");
            AddGrowth(R.DailyBonusGrowth);
            _g.Store.AddGrimorioXp(R.DailyPass);
            Report("diarios_completos");
            return OpResult.Success("Bono del día");
        }

        /// <summary>Cobra todo lo que esté listo. Devuelve cuántas recompensas cobró.</summary>
        public int ClaimAll()
        {
            int n = 0;
            foreach (var v in DailyBoard()) if (v.Claimable && Claim(v.Def.Id).Ok) n++;
            if (BonusReady && ClaimBonus().Ok) n++;
            foreach (var v in WeeklyBoard()) if (v.Claimable && Claim(v.Def.Id).Ok) n++;
            foreach (var v in AchievementBoard()) if (v.Claimable && Claim(v.Def.Id).Ok) n++;
            return n;
        }

        private void Pay(List<KeyValuePair<string, long>> rewards, string source, string note)
        {
            foreach (var kv in rewards) _g.Ledger.Apply(kv.Key, kv.Value, source, note);
        }

        // ------------------------------------------------------------------ Altura (META 4)

        public long Growth => S.Growth;

        private void AddGrowth(long amount)
        {
            if (amount <= 0) return;
            S.Growth += amount;
            int level = Altura;
            while (S.AlturaPaid < level)
            {
                S.AlturaPaid++;
                if (S.AlturaPaid <= R.AlturaSalesUntil && R.AlturaSales > 0) _g.Ledger.Apply(Res.SalesDeBano, R.AlturaSales, "misiones", "altura_" + S.AlturaPaid);
            }
        }

        /// <summary>Nivel de cuenta por Crecimiento acumulado.</summary>
        public int Altura => AlturaFor(S.Growth);

        public int AlturaFor(long growth)
        {
            if (R == null || R.AlturaCurve.Count == 0) return 1;
            int level = 1;
            long left = growth;
            foreach (var step in R.AlturaCurve)
            {
                while (level < step.Key && left >= step.Value) { left -= step.Value; level++; }
                if (level < step.Key) break;
            }
            return level;
        }

        /// <summary>Crecimiento dentro del nivel actual y lo que falta para el siguiente.</summary>
        public (long into, long need) AlturaProgress()
        {
            if (R == null || R.AlturaCurve.Count == 0) return (0, 1);
            int level = 1;
            long left = S.Growth;
            foreach (var step in R.AlturaCurve)
            {
                while (level < step.Key)
                {
                    if (left < step.Value) return (left, step.Value);
                    left -= step.Value;
                    level++;
                }
            }
            return (0, 1);
        }

        private static ulong Mix(ulong a, ulong b) => Fresno.MapGenerator.Mix(a * 0x9E3779B97F4A7C15UL + 1, b);
    }

    internal static class StableHash
    {
        /// <summary>Hash estable entre ejecuciones (string.GetHashCode cambia en .NET Core).</summary>
        public static int GetHashCodeStable(this string s)
        {
            unchecked
            {
                int h = 23;
                foreach (char c in s) h = h * 31 + c;
                return h & 0x7FFFFFFF;
            }
        }
    }
}
