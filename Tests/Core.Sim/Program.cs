using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;

namespace SalYAlmas.Sim
{
    /// <summary>
    /// Uso:
    ///   dotnet run --project Tests/Core.Sim -- log [encuentro] [semilla]   registro completo de un combate automático
    ///   dotnet run --project Tests/Core.Sim -- balance [encuentro] [n]     estadísticas de n combates
    ///   dotnet run --project Tests/Core.Sim -- chains                      Spark previstos de cada orden de Cadena
    ///   dotnet run --project Tests/Core.Sim -- gacha [tiradas] [semilla]   promedios del gacha contra el GDD 16.2
    ///   dotnet run --project Tests/Core.Sim -- fresno [campañas]           balance del Fresno: tres runs seguidas por campaña
    ///   dotnet run --project Tests/Core.Sim -- fresno-log [semilla]        registro de tres runs seguidas
    ///   dotnet run --project Tests/Core.Sim -- juicio [bruja] [n]          victorias y condiciones de cada Juicio
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            var db = DataLoader.LoadFromDirectory(FindDataDir());
            string mode = args.Length > 0 ? args[0] : "log";
            string encounter = args.Length > 1 ? args[1] : "guardian";
            switch (mode)
            {
                case "log": return Log(db, encounter, args.Length > 2 ? ulong.Parse(args[2]) : 1UL);
                case "balance": return Balance(db, encounter, args.Length > 2 ? int.Parse(args[2]) : 200);
                case "chains": return Chains(db);
                case "tiempo": return Timing(db, encounter, args.Length > 2 ? int.Parse(args[2]) : 50);
                case "gacha": return MetaSim.Gacha(db, args.Length > 1 ? long.Parse(args[1]) : 1000000L, args.Length > 2 ? ulong.Parse(args[2]) : 2026UL);
                case "fresno": return MetaSim.Fresno(db, args.Length > 1 ? int.Parse(args[1]) : 100);
                case "fresno-log": return MetaSim.FresnoLog(db, args.Length > 1 ? ulong.Parse(args[1]) : 1UL);
                case "historia": return MetaSim.Story(db, args.Length > 1 ? int.Parse(args[1]) : 20);
                case "juicio": return MetaSim.Trials(db, args.Length > 1 ? args[1] : "todas", args.Length > 2 ? int.Parse(args[2]) : 50);
                default: Console.Error.WriteLine("Modos: log, balance, chains, tiempo, gacha, fresno, fresno-log, juicio, historia"); return 1;
            }
        }

        public static string FindDataDir()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Assets", "SalYAlmas", "Data");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("No encontré Assets/SalYAlmas/Data");
        }

        public static BattleSetup DefaultSetup(string encounter, ulong seed) => new BattleSetup
        {
            Team = new List<string> { "marisal", "selenia", "solenne", "tormentina" },
            GuestId = "zarigueya",
            EncounterId = encounter,
            Seed = seed,
            Belt = new List<BeltSlot> { new BeltSlot("balsamo_cuprico", 3), new BeltSlot("leche_luna", 2), new BeltSlot("destilado_rocio", 2) },
        };

        private static int Log(GameDatabase db, string encounter, ulong seed)
        {
            var b = new Battle(db, DefaultSetup(encounter, seed));
            b.Start();
            new AutoPilot().RunToEnd(b);
            var fmt = new CombatLogFormatter(db) { IncludeHits = false };
            foreach (var e in b.Events)
            {
                var line = fmt.Format(e);
                if (line != null) Console.WriteLine(line);
            }
            return 0;
        }

        private static int Balance(GameDatabase db, string encounter, int n)
        {
            foreach (var variant in new[] { "piloto completo", "peor orden de Cadena", "sin Cadenas", "sin Cadenas ni Ascensiones" })
            {
                int wins = 0; var turns = new List<int>(); var allyTurns = new List<int>(); var sparks = new List<int>(); var chains = new List<int>();
                var endHp = new List<double>(); var minHp = new List<double>(); var bossDmg = new List<int>(); var asc = new List<int>();
                var bossTurns = new List<int>(); var breaks = new List<int>(); var ecstasy = new List<int>(); var cancels = new List<int>(); var deaths = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    var b = new Battle(db, DefaultSetup(encounter, (ulong)(1000 + i)));
                    b.Start();
                    var pilot = new AutoPilot();
                    if (variant == "peor orden de Cadena") pilot.WorstChainOrder = true;
                    if (variant.StartsWith("sin Cadenas")) pilot.UseChains = false;
                    if (variant == "sin Cadenas ni Ascensiones") pilot.UseAscensions = false;
                    pilot.RunToEnd(b);
                    if (b.Outcome == BattleOutcome.Victory) wins++;
                    turns.Add(b.TurnNumber);
                    allyTurns.Add(b.Events.OfType<TurnStarted>().Count(t => t.Side == Side.Ally));
                    bossTurns.Add(b.Events.OfType<TurnStarted>().Count(t => t.Side == Side.Enemy));
                    sparks.Add(b.TotalSparks);
                    chains.Add(b.Events.OfType<ChainStarted>().Count());
                    breaks.Add(b.Events.OfType<BreakTriggered>().Count());
                    ecstasy.Add(b.Events.OfType<EcstasyStarted>().Count());
                    cancels.Add(b.Events.OfType<AttackCanceled>().Count());
                    deaths.Add(b.Events.OfType<UnitDefeated>().Count(d => d.Side == Side.Ally));
                    endHp.Add(b.Witches.Sum(w => (double)w.Hp) / b.Witches.Sum(w => (double)w.MaxHp));
                    var hpByWitch = b.Witches.ToDictionary(w => w.Id, w => w.MaxHp);
                    double low = 1.0;
                    foreach (var h in b.Events.OfType<HitLanded>().Where(h => hpByWitch.ContainsKey(h.TargetId)))
                        low = Math.Min(low, h.TargetHp / (double)hpByWitch[h.TargetId]);
                    minHp.Add(low);
                    bossDmg.Add(b.Events.OfType<HitLanded>().Where(h => hpByWitch.ContainsKey(h.TargetId)).Sum(h => h.Damage));
                    asc.Add(b.Events.OfType<AscensionCast>().Count());
                }
                Console.WriteLine($"== {encounter} · {variant} · {n} combates");
                Console.WriteLine($"   victorias {wins}/{n} ({100.0 * wins / n:0}%)");
                Console.WriteLine($"   turnos totales  media {turns.Average():0.0}  min {turns.Min()}  max {turns.Max()}");
                Console.WriteLine($"   turnos de bruja media {allyTurns.Average():0.0} · turnos enemigos media {bossTurns.Average():0.0}");
                Console.WriteLine($"   vida final del equipo {endHp.Average():P0} · vida más baja de una bruja {minHp.Average():P0} · daño recibido {bossDmg.Average():0} · ascensiones {asc.Average():0.0}");
                Console.WriteLine($"   cadenas {chains.Average():0.0} · spark {sparks.Average():0.0} · quiebres {breaks.Average():0.0} · éxtasis {ecstasy.Average():0.00} · cancelados {cancels.Average():0.00} · caídas {deaths.Average():0.00}");
            }
            return 0;
        }

        /// <summary>
        /// Estima el tiempo real de un combate en la escena: las pausas de reproducción a x1 (las mismas que usa
        /// BattleDirector) más un tiempo de decisión por turno de bruja y por pantalla de Cadena.
        /// </summary>
        private static int Timing(GameDatabase db, string encounter, int n)
        {
            const double thinkPerTurn = 7.0, thinkPerChain = 30.0, enemyWindow = 0.55;
            var totals = new List<double>();
            var playback = new List<double>();
            for (int i = 0; i < n; i++)
            {
                var b = new Battle(db, DefaultSetup(encounter, (ulong)(500 + i)));
                b.Start();
                new AutoPilot().RunToEnd(b);
                double t = 0; int lastFrame = 0;
                foreach (var e in b.Events)
                {
                    switch (e)
                    {
                        case HitLanded h when h.Source == HitSource.Chain || h.Source == HitSource.Reaction:
                            t += Math.Max(0, h.Frame - lastFrame) / 60.0; lastFrame = Math.Max(lastFrame, h.Frame); break;
                        case ChainLinkStarted l: t += Math.Max(0, l.Frame - lastFrame) / 60.0; lastFrame = Math.Max(lastFrame, l.Frame); break;
                        case ChainStarted _: lastFrame = 0; t += 0.5; break;
                        case BattleStarted _: t += 0.6; break;
                        case TurnStarted _: t += 0.3; break;
                        case ActionUsed a: t += a.Action == ActionKind.Potion || a.Action == ActionKind.SelfCast ? 0.35 : 0.45; break;
                        case HitLanded _: t += 0.12; break;
                        case BreakTriggered _: t += 0.7; break;
                        case AspectFormed _: t += 0.35; break;
                        case ReactionTriggered _: t += 0.45; break;
                        case ChainEnded _: t += 0.9; break;
                        case AscensionCast a: t += a.Level == 1 ? 1.2 : a.Level == 2 ? 2.0 : 3.0; break;
                        case EcstasyStarted _: t += 1.0; break;
                        case EnemyCharging _: t += 0.8; break;
                        case AttackCanceled _: t += 0.8; break;
                        case PhaseChanged _: t += 1.2; break;
                        case UnitDefeated _: t += 0.6; break;
                        case TurnSkipped _: t += 0.5; break;
                        default: t += 0.02; break;
                    }
                }
                int allyTurns = b.Events.OfType<TurnStarted>().Count(x => x.Side == Side.Ally);
                int enemyTurns = b.Events.OfType<TurnStarted>().Count(x => x.Side == Side.Enemy);
                int chains = b.Events.OfType<ChainStarted>().Count();
                playback.Add(t + enemyTurns * enemyWindow);
                totals.Add(t + enemyTurns * enemyWindow + allyTurns * thinkPerTurn + chains * thinkPerChain);
            }
            Console.WriteLine($"== {encounter} · {n} combates del piloto automático");
            Console.WriteLine($"   solo reproducción a x1: {playback.Average() / 60:0.0} min (x2: {playback.Average() / 120:0.0} min)");
            Console.WriteLine($"   con {thinkPerTurn} s de decisión por turno y {thinkPerChain} s por Cadena: {totals.Average() / 60:0.0} min (máx {totals.Max() / 60:0.0})");
            return 0;
        }

        private static int Chains(GameDatabase db)
        {
            var setup = DefaultSetup("guardian", 1);
            foreach (var team in new[] {
                new[] { "marisal", "selenia", "solenne", "tormentina" },
                new[] { "marisal", "solenne", "tormentina", "topo" },
                new[] { "marisal", "selenia", "tormentina", "topo" } })
            {
                setup.Team = team.ToList();
                setup.GuestId = db.Witches.Keys.First(k => !team.Contains(k) && k == "zarigueya") ?? null;
                var b = new Battle(db, setup);
                b.Start();
                var ids = b.Witches.Select(w => w.Id).ToList();
                var rows = new List<(string order, int sparks, string aspects)>();
                for (int size = 3; size <= 4; size++)
                    foreach (var perm in AutoPilot.Permutations(ids, size))
                        foreach (var g in new[] { false, true })
                        {
                            var p = b.PreviewChain(perm, g);
                            if (!p.IsValid) continue;
                            rows.Add((string.Join(" > ", p.Links.Select(l => l.Name)), p.SparkCount,
                                string.Join(", ", p.Aspects.Select(a => a.Aspect + (a.Reaction != SalYAlmas.Core.Magic.HybridReaction.None ? "/" + a.Reaction : "")))));
                        }
                Console.WriteLine($"== Equipo {string.Join(", ", team)} + Invitada {setup.GuestId}: {rows.Count} órdenes");
                var hist = rows.GroupBy(r => r.sparks).OrderBy(g => g.Key).Select(g => $"{g.Key} spark: {g.Count()}");
                Console.WriteLine("   " + string.Join(" · ", hist));
                foreach (var r in rows.OrderByDescending(r => r.sparks).Take(6)) Console.WriteLine($"   {r.sparks,2}  {r.order}   [{r.aspects}]");
                foreach (var r in rows.OrderBy(r => r.sparks).Take(3)) Console.WriteLine($"   {r.sparks,2}  {r.order}   [{r.aspects}]");
            }
            return 0;
        }
    }
}
