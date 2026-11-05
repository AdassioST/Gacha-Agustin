using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Events;
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

        /// <summary>
        /// Prueba estándar de daño (GDD 17.1). Cada bruja entra a un equipo con tres compañeras fijas contra un maniquí que no
        /// muere (el Guardián con 50 veces su vida y 2 % de su ataque, para que nadie caiga, débil a los dos planetas de la bruja). Se mide el daño
        /// total del equipo, con golpes y daño por estados, hasta el límite de turnos: así cuentan también las mejoras de las
        /// Tejedoras. Ninguna 5★ nueva debe pasar en más de 5 % a la mejor de su papel, y ninguna 4★ o 3★ a la mejor 5★.
        /// </summary>
        public static int Kits(GameDatabase db, int n)
        {
            var meta = Load(db);
            var combat = meta.Combat;
            var fillers = new[] { "topo", "zarigueya", "selenia", "tormentina" };
            var guardian = combat.Enemies["guardian"];
            var rows = new List<(WitchDef def, double team, double own, double heal, double deaths)>();
            foreach (var id in combat.WitchOrder)
            {
                var def = combat.Witch(id);
                var team = new List<string> { id };
                foreach (var f in fillers) if (f != id && team.Count < 4) team.Add(f);
                double total = 0, own = 0, heal = 0, deaths = 0;
                for (int i = 0; i < n; i++)
                {
                    var dummy = FresnoService.Scaled(guardian, "maniqui", 50, 0.02, -2, "");
                    dummy.Name = dummy.ShortName = "Maniquí de Sal";
                    dummy.Phase2 = null;
                    dummy.Weaknesses.Clear();
                    dummy.Weaknesses.Add(def.Primary);
                    dummy.Weaknesses.Add(def.Secondary);
                    dummy.Res.Clear();
                    var enc = new EncounterDef { Id = "maniqui", Name = "Maniquí" };
                    enc.Enemies.Add(dummy.Id);
                    var setup = new BattleSetup { Team = team, Seed = (ulong)(500 + i), CustomEncounter = enc };
                    setup.CustomEnemies.Add(dummy);
                    foreach (var t in team) setup.Loadouts[t] = new WitchLoadout { Level = 50 };
                    var b = new Battle(combat, setup);
                    b.Start();
                    new AutoPilot().RunToEnd(b);
                    var unit = b.Witches.First(w => w.Key == id);
                    var enemyIds = new HashSet<int>(b.Enemies.Select(e => e.Id));
                    total += b.Events.OfType<HitLanded>().Where(h => enemyIds.Contains(h.TargetId)).Sum(h => (double)h.Damage);
                    total += b.Events.OfType<DotTicked>().Where(d => enemyIds.Contains(d.UnitId)).Sum(d => (double)d.Damage);
                    own += b.Events.OfType<HitLanded>().Where(h => h.AttackerId == unit.Id).Sum(h => (double)h.Damage);
                    heal += b.Events.OfType<Healed>().Sum(h => (double)h.Amount);
                    deaths += b.Events.OfType<UnitDefeated>().Count(d => d.Side == Side.Ally);
                }
                rows.Add((def, total / n, own / n, heal / n, deaths / n));
            }
            Console.WriteLine($"== Kits · {n} combates por bruja contra un maniquí · nivel 50 · grado B · límite de turnos");
            Console.WriteLine("   bruja         ★  papel                   daño del equipo  golpes propios  curación  caídas");
            foreach (var r in rows.OrderByDescending(r => r.team))
                Console.WriteLine($"   {r.def.ShortName,-13} {r.def.Rarity}  {r.def.Role,-22} {r.team,15:N0}  {r.own,14:N0}  {r.heal,8:N0}  {r.deaths,6:0.0}");
            bool ok = true;
            var baseIds = new HashSet<string>(MetaGame.TestWitches);
            foreach (var r in rows.Where(r => r.def.Rarity == 5 && !baseIds.Contains(r.def.Id)))
            {
                var best = rows.Where(o => baseIds.Contains(o.def.Id) && o.def.Role == r.def.Role).Select(o => o.team).DefaultIfEmpty(0).Max();
                if (best > 0 && r.team > best * 1.05) { ok = false; Console.WriteLine($"   FALLA: {r.def.ShortName} pasa en más de 5 % a la mejor de su papel"); }
            }
            double topFive = rows.Where(r => r.def.Rarity == 5).Max(r => r.team);
            foreach (var r in rows.Where(r => r.def.Rarity < 5 && r.team > topFive))
            { ok = false; Console.WriteLine($"   FALLA: {r.def.ShortName} ({r.def.Rarity}★) pega más que la mejor 5★"); }
            Console.WriteLine(ok ? "   dentro de las reglas del GDD 17.1" : "   FUERA de las reglas");
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

        /// <summary>Juega la historia con una cuenta nueva, en los dos órdenes, y cuenta los intentos por combate.</summary>
        public static int Story(GameDatabase db, int n)
        {
            var meta = Load(db);
            // Una cuenta nueva por cada signo cardinal y sentido: juega el anillo desde su región, gasta las almitas en niveles
            // antes de cada misión (como una jugadora) y, si una sección no sale en 3 intentos, hace Expediciones para subir.
            foreach (var sign in new[] { "aries", "cancer", "libra", "capricornio" })
                foreach (bool direct in new[] { true, false })
                {
                    var attempts = new Dictionary<string, List<int>>();
                    var keys = new List<string>();
                    var regionInfo = new Dictionary<string, List<string>>();
                    int finished = 0, farmed = 0;
                    long earned = 0;
                    for (int c = 0; c < n; c++)
                    {
                        var g = new MetaGame(meta, MetaGame.NewProfile(meta, (ulong)(c + 1), 300));
                        g.Story.Create("Sim", "Elle", meta.World.Story.Species[0], sign);
                        g.Story.StartMission("vado", "prologo");
                        PlayMission(g, new[] { "marisal", "zarigueya", "topo" }, attempts, keys, c);
                        g.Termas.SetStarterChoice("solenne");
                        g.Termas.Pull("inicio", 20, out _);
                        var team = g.Profile.Witches.Keys.Where(meta.HasKit).OrderBy(x => x, StringComparer.Ordinal).Take(4).ToArray();
                        var ring = g.Story.BuiltRing;
                        int start = ring.FindIndex(r => r.Id == g.Story.StartRegion);
                        long before = g.Ledger.Balance(Res.Almitas);
                        for (int i = 0; i < ring.Count; i++)
                        {
                            var region = ring[((start + (direct ? i : -i)) % ring.Count + ring.Count) % ring.Count];
                            Train(g, team);
                            string tag = $"{i + 1}. {region.Id}";
                            if (!regionInfo.ContainsKey(tag)) regionInfo[tag] = new List<string>();
                            regionInfo[tag].Add($"nv {g.Story.RegionLevel(region.Id)}-{g.Story.RegionLevel(region.Id) + 6} · equipo {team.Average(id => g.Profile.Witches[id].Level):0}");
                            foreach (var m in region.Missions)
                            {
                                Train(g, team);
                                if (!g.Story.StartMission(region.Id, m.Id).Ok) break;
                                farmed += PlayMission(g, team, attempts, keys, c);
                            }
                        }
                        earned += g.Ledger.Balance(Res.Almitas) - before;
                        if (g.Story.GameFinished) finished++;
                    }
                    Console.WriteLine($"== Historia desde {sign} · {(direct ? "Directo" : "Retrógrado")} · {n} cuentas nuevas · terminadas {finished}/{n} · Expediciones extra {farmed / (double)n:0.0} por cuenta");
                    foreach (var kv in regionInfo) Console.WriteLine($"   {kv.Key,-15} {kv.Value[0]}");
                    foreach (var k in keys.Where(k => attempts[k].Average() > 1.2)) Console.WriteLine($"   {k,-18} intentos: media {attempts[k].Average():0.0}  máximo {attempts[k].Max()}");
                }
            return 0;
        }

        private static void Train(MetaGame g, IReadOnlyList<string> team)
        {
            for (bool any = true; any;)
            {
                any = false;
                foreach (var id in team.OrderBy(id => g.Profile.Witches[id].Level))
                    if (g.Progress.LevelUp(id).Ok) { any = true; break; }
            }
        }

        /// <summary>Una Expedición en la zona terminada más alta, para subir de nivel cuando la historia se atora.</summary>
        private static bool Farm(MetaGame g, IReadOnlyList<string> team, int c)
        {
            foreach (var regionId in g.Profile.Calculista.RegionsVisited.ToList())
            {
                var region = g.Meta.World.Story.Region(regionId);
                foreach (var z in region.Zones.AsEnumerable().Reverse())
                {
                    if (g.Story.CanExpedition(region.Id, z.Id, g.Story.MaxTideFor(region.Id)).Length > 0) continue;
                    g.Story.StartExpedition(region.Id, z.Id, g.Story.MaxTideFor(region.Id));
                    for (int guard = 0; guard < 40 && g.Story.InExpedition; guard++)
                    {
                        var b = new Battle(g.Combat, g.Story.PrepareExpeditionBattle(team, (ulong)(c * 977 + guard)));
                        b.Start();
                        new AutoPilot().RunToEnd(b);
                        if (!g.Story.ResolveExpeditionBattle(b).Ok && guard > 8) g.Story.LeaveExpedition();
                    }
                    Train(g, team);
                    return true;
                }
            }
            return false;
        }

        private static int PlayMission(MetaGame g, IReadOnlyList<string> team, Dictionary<string, List<int>> attempts, List<string> keys, int c)
        {
            int farmed = 0;
            for (int guard = 0; guard < 400 && g.Story.InMission; guard++)
            {
                var scene = g.Story.CurrentScene;
                if (scene.Battle != null)
                {
                    string key = g.Profile.Calculista.Mission + "/" + scene.Id;
                    int tries = 0;
                    Core.Meta.OpResult r;
                    do
                    {
                        var b = new Battle(g.Combat, g.Story.PrepareBattle(team, (ulong)(c * 100 + ++tries)));
                        b.Start();
                        new AutoPilot().RunToEnd(b);
                        r = g.Story.ResolveBattle(b);
                        if (!r.Ok && tries % 3 == 0 && team.Count > 0)
                        {
                            // Se atoró: con la misión en pausa, hace una Expedición para subir y vuelve a la misma sección.
                            if (Farm(g, team, c + tries)) farmed++;
                        }
                    } while (!r.Ok && tries < 60);
                    if (!attempts.ContainsKey(key)) { attempts[key] = new List<int>(); keys.Add(key); }
                    attempts[key].Add(tries);
                }
                else if (scene.Choices.Count > 0)
                {
                    var o = g.Story.Choices(team).First(x => x.Available);
                    g.Story.Choose(o.Index, team);
                }
                else g.Story.Continue();
            }
            return farmed;
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
