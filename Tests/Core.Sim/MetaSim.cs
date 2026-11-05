using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Progress;

namespace SalYAlmas.Sim
{
    /// <summary>Comandos del simulador para el bucle de progreso de la Fase 2.</summary>
    public static class MetaSim
    {
        private static readonly string[] Team = { "marisal", "selenia", "solenne", "tormentina" };

        private static MetaDatabase Load(GameDatabase db) => MetaLoader.Load(MetaDataSources.FromDirectory(Program.FindDataDir()), db);

        /// <summary>Promedios del gacha contra el GDD 16.2 (cerca de 53 por 5★ y de 66 por la destacada, ±1).</summary>
        public static int Gacha(GameDatabase db, long pulls, ulong seed)
        {
            var sim = GachaSimulation.Run(Load(db).Gacha, pulls, seed);
            Console.WriteLine($"== Gacha · {pulls:N0} tiradas · semilla {seed}");
            Console.WriteLine($"   tiradas por 5★        {sim.AveragePerFive:0.00} (GDD: ~53)   máximo {sim.MaxPullsForFive}");
            Console.WriteLine($"   tiradas por destacada {sim.AveragePerFeatured:0.00} (GDD: ~66)   máximo {sim.MaxPullsForFeatured}");
            Console.WriteLine($"   tasa consolidada 5★   {sim.ConsolidatedRate:P2} · 4★ {sim.FourStarRate:P2} · hueco máximo de 4★ {sim.MaxGapFourStar}");
            Console.WriteLine($"   perdió el 75/25 {sim.LostFeatured:N0} veces de {sim.FiveStars:N0} 5★ ({(double)sim.LostFeatured / Math.Max(1, sim.FiveStars):P1})");
            bool ok = Math.Abs(sim.AveragePerFive - 53) <= 1 && Math.Abs(sim.AveragePerFeatured - 66) <= 1;
            Console.WriteLine(ok ? "   dentro de ±1 tirada" : "   FUERA de ±1 tirada");
            return ok ? 0 : 1;
        }

        private static MetaGame NewGame(MetaDatabase meta, ulong seed)
        {
            var p = MetaGame.TestProfile(meta, seed, 300);
            return new MetaGame(meta, p);
        }

        /// <summary>Tres runs seguidas por campaña (la segunda y la tercera con Eco y Mancha), con Raíces compradas entre runs.</summary>
        public static int Fresno(GameDatabase db, int campaigns)
        {
            var meta = Load(db);
            var reports = new List<ClimbReport>[3];
            for (int i = 0; i < 3; i++) reports[i] = new List<ClimbReport>();
            for (int c = 0; c < campaigns; c++)
            {
                var g = NewGame(meta, (ulong)(9000 + c));
                var climber = new AutoClimber(g);
                for (int r = 0; r < 3; r++)
                {
                    int eco = r > 0 && g.Profile.Fresno.Eco[0] != null ? 0 : -1;
                    reports[r].Add(climber.Climb(Team, eco, (ulong)(c * 100 + r + 1)));
                    BuyRoots(g);
                }
            }
            Console.WriteLine($"== Fresno · {campaigns} campañas de 3 runs · perfil de prueba · equipo {string.Join(", ", Team)}");
            for (int r = 0; r < 3; r++)
            {
                var l = reports[r];
                Console.WriteLine($"-- Run {r + 1}");
                Console.WriteLine($"   victorias {l.Count(x => x.Outcome == RunOutcome.Victory)}/{l.Count} · zona alcanzada media {l.Average(ZoneOf):0.0}/4 · nodos {l.Average(x => x.Nodes):0} · combates {l.Average(x => x.Combats):0} (ganados {l.Average(x => x.CombatsWon):0})");
                Console.WriteLine($"   almitas ganadas {l.Average(x => x.AlmitasEarned):N0} · guardadas {l.Average(x => x.AlmitasBanked):N0} · Mancha dejada {l.Average(x => x.ManchaLeft):N0} · recuperada {l.Average(x => x.ManchaRecovered):N0} · Savia {l.Average(x => x.SaviaEarned):N0}");
                Console.WriteLine($"   Injertos {l.Average(x => x.Grafts):0.0} · Frutos {l.Average(x => x.Fruits):0.0} · con Eco {l.Count(x => x.EcoUsed.Length > 0)}/{l.Count}");
                double seconds = l.Average(x => x.AllyTurns * 7.0 + x.Chains * 30.0 + x.Combats * 25.0 + (x.Nodes - x.Combats) * 20.0);
                Console.WriteLine($"   tiempo estimado {seconds / 60:0} min (7 s por turno, 30 s por Cadena, 25 s de reproducción por combate, 20 s por nodo sin combate)");
                var deaths = l.Where(x => x.Outcome == RunOutcome.Defeat).GroupBy(x => x.ReachedNode.Split('-')[0]).OrderBy(x => x.Key).Select(x => $"zona {x.Key}: {x.Count()}");
                Console.WriteLine("   caídas por zona: " + string.Join(" · ", deaths) + $" · ante un jefe: {l.Count(x => x.Outcome == RunOutcome.Defeat && x.ReachedNode.Split((char)45)[1] == "10")}");
            }
            return 0;
        }

        private static double ZoneOf(ClimbReport r) => int.Parse(r.ReachedNode.Split('-')[0]);

        private static void BuyRoots(MetaGame g)
        {
            foreach (var root in g.Meta.Fresno.Roots)
                while (g.Fresno.BuyRoot(root.Id).Ok) { }
        }

        public static int FresnoLog(GameDatabase db, ulong seed)
        {
            var g = NewGame(Load(db), seed);
            var climber = new AutoClimber(g);
            for (int r = 0; r < 3; r++)
            {
                int eco = r > 0 && g.Profile.Fresno.Eco[0] != null ? 0 : -1;
                var rep = climber.Climb(Team, eco, seed * 10 + (ulong)r);
                Console.WriteLine($"===== Run {r + 1}: {rep.Outcome} en {rep.ReachedNode} · huella {rep.Fingerprint}");
                foreach (var line in rep.Log) Console.WriteLine(line);
            }
            return 0;
        }

        /// <summary>Juega cada Juicio con el piloto del combate e informa victorias y condiciones cumplidas.</summary>
        public static int Trials(GameDatabase db, string witch, int n)
        {
            var meta = Load(db);
            var witches = witch == "todas" ? db.WitchOrder.ToList() : new List<string> { witch };
            foreach (var id in witches)
            {
                var g = NewGame(meta, 3);
                var wp = g.Progress.Get(id);
                wp.Grade = Grade.D;
                wp.ViaChosen = false;
                var allies = db.WitchOrder.Where(a => a != id).Take(3).ToList();
                while (wp.Grade != Grade.SS)
                {
                    var req = g.Progress.Requirement(id);
                    if (wp.Level < req.Step.MinLevel) wp.Level = req.Step.MinLevel;
                    // Los Juicios exigen poder pagar; aquí solo interesa el combate.
                    foreach (var item in req.Cost.Items) g.Ledger.Apply(item.Key, item.Value, "prueba");
                    int wins = 0, pass = 0;
                    var turns = new List<int>();
                    string title = "", conditions = "";
                    for (int i = 0; i < n; i++)
                    {
                        var ok = g.Trials.Prepare(id, allies, (ulong)(100 + i), out var plan);
                        if (!ok.Ok) { Console.WriteLine($"   {id}: {ok.Message}"); return 1; }
                        title = plan.Title;
                        conditions = string.Join(" y ", plan.Conditions.Select(c => c.TextFor(plan.WitchName)));
                        var b = new Battle(db, plan.Setup);
                        b.Start();
                        new AutoPilot().RunToEnd(b);
                        turns.Add(b.TurnNumber);
                        bool won = b.Outcome == Core.Events.BattleOutcome.Victory;
                        if (won) wins++;
                        if (won && TrialService.Evaluate(plan.Conditions, b, id, plan.WitchName).TrueForAll(c => c.Passed)) pass++;
                    }
                    Console.WriteLine($"{id,-11} {title,-10} → {req.Step.Grade,-2} nivel {wp.Level}: gana {wins}/{n}, cumple {pass}/{n}, turnos {turns.Average():0}  [{conditions}]");
                    // Sube el grado para medir el siguiente Juicio sin depender de este.
                    wp.Grade = req.Step.Grade;
                    wp.ViaChosen = true;
                }
            }
            return 0;
        }
    }
}
