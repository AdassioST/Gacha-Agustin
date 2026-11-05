using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Fresno (GDD 10): generador, Mancha de Sal, Eco, maldiciones, Paria, tope semanal, Raíces y Ofrenda.</summary>
    public class FresnoTests
    {
        private static readonly string[] Team = { "marisal", "selenia", "solenne", "tormentina" };

        // ------------------------------------------------------------------ Generador

        [Test]
        public void ElGeneradorConSemillaFijaDaSiempreElMismoMapa()
        {
            // Huella grabada: si cambia, cambiaron el generador o los pesos y las runs guardadas ya no se repiten.
            var map = MapGenerator.Generate(Meta().Fresno, 42);
            Assert.AreEqual(GoldenMapHash, Hash(map.Fingerprint()), map.Fingerprint());
        }

        private const string GoldenMapHash = "2142f7f5";

        private static string Hash(string text)
        {
            uint h = 2166136261;
            foreach (char c in text) { h ^= c; h *= 16777619; }
            return h.ToString("x8");
        }

        [TestCase(1UL)]
        [TestCase(42UL)]
        [TestCase(2026UL)]
        public void ElMapaRespetaLaEstructuraDelGdd(ulong seed)
        {
            var rules = Meta().Fresno;
            var map = MapGenerator.Generate(rules, seed);
            Assert.AreEqual(4, map.Zones.Count, "cuatro zonas");
            bool forks = false;
            foreach (var zone in map.Zones)
            {
                Assert.AreEqual(10, zone.Count, "diez pisos");
                Assert.AreEqual(NodeType.Combate, zone[0].Single().Type, "se entra por un combate");
                Assert.AreEqual(NodeType.Jefe, zone[9].Single().Type, "el Jefe cierra la zona");
                Assert.IsTrue(zone[8].All(n => n.Type == NodeType.Santuario), "el piso 9 es todo Santuario");
                Assert.IsTrue(zone[4].Any(n => n.Type == NodeType.Santuario), "el piso 5 tiene un Santuario");
                Assert.IsTrue(zone[0].Concat(zone[1]).All(n => n.Type != NodeType.Elite), "sin Élites antes del piso 3");

                var reached = new HashSet<int> { zone[0].Single().Lane };
                for (int f = 0; f < 9; f++)
                {
                    var next = new HashSet<int>();
                    foreach (var n in zone[f].Where(x => reached.Contains(x.Lane)))
                    {
                        Assert.IsNotEmpty(n.Next, "todo nodo lleva a algún lado");
                        if (n.Next.Count > 1) forks = true;
                        foreach (var lane in n.Next) next.Add(lane);
                    }
                    CollectionAssert.AreEquivalent(zone[f + 1].Select(n => n.Lane), next, "todo nodo es alcanzable");
                    reached = next;
                }
            }
            Assert.IsTrue(forks, "las rutas se bifurcan");
        }

        [Test]
        public void TodosLosTiposDeNodoDelGddAparecen()
        {
            var seen = new HashSet<NodeType>();
            for (ulong s = 1; s <= 10; s++)
                foreach (var n in MapGenerator.Generate(Meta().Fresno, s).AllNodes()) seen.Add(n.Type);
            seen.Add(NodeType.Mancha);
            foreach (NodeType t in Enum.GetValues(typeof(NodeType))) Assert.IsTrue(seen.Contains(t), t.ToString());
        }

        // ------------------------------------------------------------------ Mancha de Sal (GDD 10.4)

        /// <summary>Juega una run que cae al primer combate después de juntar almitas, en la zona indicada por el nodo donde muera.</summary>
        private static void DieAfterEarning(MetaGame g, long loose, ulong seed = 5)
        {
            Assert.IsTrue(g.Fresno.StartRun(Team, seed: seed).Ok);
            g.Fresno.ChooseCurses(new string[0]);
            g.Fresno.EnterNode(g.Fresno.AvailableNodes()[0].Lane);
            g.Fresno.Run.Loose = loose;
            LoseCurrentBattle(g);
            g.Fresno.SkipEco();
        }

        private static void LoseCurrentBattle(MetaGame g)
        {
            var setup = g.Fresno.PrepareBattle();
            setup.Modifiers.Add(ModKeys.EnemyAtkPct, 100);
            var battle = new Battle(g.Combat, setup);
            battle.Start();
            new AutoPilot().RunToEnd(battle);
            Assert.AreNotEqual(Core.Events.BattleOutcome.Victory, battle.Outcome, "el combate debía perderse");
            Assert.IsTrue(g.Fresno.ResolveBattle(battle).Ok);
        }

        [Test]
        public void AlCaerLasAlmitasSueltasQuedanComoManchaEnEseNodo()
        {
            var g = NewGame();
            long before = g.Ledger.Balance(Res.Almitas);
            DieAfterEarning(g, 5000);
            var m = g.Profile.Fresno.Mancha;
            Assert.IsNotNull(m);
            Assert.AreEqual(5000, m.Almitas);
            Assert.AreEqual(0, m.Zone);
            Assert.AreEqual(0, m.Floor);
            Assert.AreEqual(before, g.Ledger.Balance(Res.Almitas), "las almitas sueltas nunca entraron a la cartera");
        }

        [Test]
        public void ManchaCaso1LlegarAlNodoLaRecuperaCompleta()
        {
            var g = NewGame();
            DieAfterEarning(g, 5000);
            long savia = g.Ledger.Balance(Res.Savia);
            g.Fresno.StartRun(Team, seed: 6);
            Assert.IsNull(g.Profile.Fresno.Mancha, "pasa a la run");
            g.Fresno.ChooseCurses(new string[0]);
            g.Fresno.EnterNode(g.Fresno.AvailableNodes()[0].Lane);
            Assert.AreEqual(5000, g.Fresno.Run.Loose);
            Assert.AreEqual(5000, g.Fresno.Run.ManchaRecovered);
            Assert.IsTrue(g.Fresno.Run.ManchaResolved);
            g.Fresno.Abandon();
            Assert.AreEqual(savia, g.Ledger.Balance(Res.Savia), "recuperada no devuelve Savia");
        }

        [Test]
        public void ManchaCaso2MorirAntesDeLlegarLaAbsorbeConTreintaPorCiento()
        {
            var g = NewGame();
            // La Mancha está lejos, en la zona 2.
            g.Profile.Fresno.Mancha = new ManchaState { Zone = 1, Floor = 5, Lane = 1, Almitas = 10000, FromRun = 0 };
            long savia = g.Ledger.Balance(Res.Savia);
            DieAfterEarning(g, 700);
            Assert.AreEqual(savia + 3000, g.Ledger.Balance(Res.Savia), "GDD 10.4: 30 % como Savia");
            Assert.AreEqual(3000, g.Ledger.TotalIn(Res.Savia, "fresno.mancha"));
            Assert.AreEqual(700, g.Profile.Fresno.Mancha.Almitas, "la Mancha nueva sustituye a la vieja");
            Assert.AreEqual(3000, g.Profile.Fresno.History.Last().SaviaFromMancha);
        }

        [Test]
        public void ManchaCaso3TerminarSinPasarPorElNodoTambienLaAbsorbe()
        {
            var g = NewGame();
            g.Profile.Fresno.Mancha = new ManchaState { Zone = 1, Floor = 5, Lane = 1, Almitas = 10000, FromRun = 0 };
            long savia = g.Ledger.Balance(Res.Savia);
            g.Fresno.StartRun(Team, seed: 7);
            g.Fresno.Abandon();
            g.Fresno.SkipEco();
            Assert.AreEqual(savia + 3000, g.Ledger.Balance(Res.Savia));
            Assert.IsNull(g.Profile.Fresno.Mancha);
        }

        [Test]
        public void CaerConLaManchaRecuperadaLaDejaOtraVezEnElArbol()
        {
            var g = NewGame();
            DieAfterEarning(g, 5000);
            g.Fresno.StartRun(Team, seed: 6);
            g.Fresno.ChooseCurses(new string[0]);
            g.Fresno.EnterNode(g.Fresno.AvailableNodes()[0].Lane);
            LoseCurrentBattle(g);
            g.Fresno.SkipEco();
            Assert.AreEqual(5000, g.Profile.Fresno.Mancha.Almitas);
        }

        [Test]
        public void ElMapaMarcaLaManchaYConvierteElNodo()
        {
            var mancha = new ManchaState { Zone = 2, Floor = 3, Lane = 2, Almitas = 100 };
            var map = MapGenerator.Generate(Meta().Fresno, 42, mancha);
            var node = map.Node(2, 3, 2);
            Assert.IsTrue(node.HasMancha);
            Assert.AreEqual(NodeType.Mancha, node.Type);
            Assert.AreEqual(1, map.AllNodes().Count(n => n.HasMancha));
        }

        [Test]
        public void GuardarEnElSantuarioPasaLasAlmitasALaCartera()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            var run = g.Fresno.Run;
            g.Fresno.ChooseCurses(new string[0]);
            Assert.IsTrue(WalkTo(g, NodeType.Santuario));
            // WalkTo ya entró al Santuario: lo suelto se guardó al llegar.
            Assert.AreEqual(0, run.Loose);
            Assert.AreEqual(run.Banked, g.Ledger.TotalIn(Res.Almitas, "fresno.santuario"));
            Assert.Greater(run.Banked, 0);
        }

        /// <summary>Avanza con combates fáciles hasta entrar a un nodo del tipo pedido.</summary>
        private static bool WalkTo(MetaGame g, NodeType type)
        {
            var climber = new AutoClimber(g);
            for (int guard = 0; guard < 60; guard++)
            {
                var run = g.Fresno.Run;
                if (run.Phase == RunPhase.ChooseNode)
                {
                    var options = g.Fresno.AvailableNodes();
                    var pick = options.Find(n => n.Type == type) ?? options[0];
                    g.Fresno.EnterNode(pick.Lane);
                    if (pick.Type == type) return true;
                }
                else if (run.Phase == RunPhase.Battle)
                {
                    var setup = g.Fresno.PrepareBattle();
                    setup.Modifiers.Add(ModKeys.EnemyHpPct, -0.9);
                    var b = new Battle(g.Combat, setup);
                    b.Start();
                    new AutoPilot().RunToEnd(b);
                    g.Fresno.ResolveBattle(b);
                }
                else if (run.Phase == RunPhase.Choice)
                {
                    switch (run.Pending.Kind)
                    {
                        case ChoiceKind.Event:
                            var ev = g.Meta.Fresno.Event(run.Pending.EventId);
                            int i = ev.Options.FindIndex(o => g.Fresno.OptionBlocker(o).Length == 0);
                            g.Fresno.ChooseEventOption(i);
                            break;
                        case ChoiceKind.Santuario:
                            g.Fresno.SantuarioRest();
                            break;
                        default:
                            g.Fresno.LeaveChoice();
                            break;
                    }
                }
                else return false;
            }
            return false;
        }

        // ------------------------------------------------------------------ Eco (GDD 10.5)

        private static List<string> ActiveGrafts(MetaGame g, Planet rama) =>
            g.Meta.Fresno.Rama(rama).Grafts.Where(x => !x.Pending).Select(x => x.Id).ToList();

        [Test]
        public void ElEcoGuardaTresInjertosYUnMovimientoYLaSiguienteRunEmpiezaConEllos()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            var sat = ActiveGrafts(g, Planet.Saturno).Take(3).ToList();
            g.Fresno.Run.Grafts.AddRange(sat);
            g.Fresno.Run.Trained.Add(new TrainedMove("marisal", ChainRole.Cierre));
            g.Fresno.Abandon();
            var r = g.Fresno.SaveEco(0, sat, new TrainedMove("marisal", ChainRole.Cierre), "Build de Saturno");
            Assert.IsTrue(r.Ok, r.Message);

            Assert.IsTrue(g.Fresno.StartRun(Team, ecoSlot: 0, seed: 4).Ok);
            CollectionAssert.AreEquivalent(sat, g.Fresno.Run.Grafts);
            Assert.AreEqual(ChainRole.Cierre, g.Fresno.Run.Trained.Single().Role);
            Assert.IsTrue(g.Fresno.FruitActive(Planet.Saturno), "tres de la misma Rama activan el Fruto desde el inicio");
            Assert.AreEqual("Build de Saturno", g.Fresno.Run.EcoName);
            g.Fresno.Abandon();
            g.Fresno.SkipEco();
            Assert.IsNotNull(g.Profile.Fresno.Eco[0], "el Eco no se gasta");
        }

        [Test]
        public void ElEcoRechazaLoQueNoEsValido()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            var sat = ActiveGrafts(g, Planet.Saturno);
            g.Fresno.Run.Grafts.AddRange(sat.Take(4));
            g.Fresno.Abandon();
            Assert.IsFalse(g.Fresno.SaveEco(0, sat.Take(4).ToList(), null).Ok, "más de tres Injertos");
            Assert.IsFalse(g.Fresno.SaveEco(0, new[] { sat[4] }, null).Ok, "Injerto de otra run");
            Assert.IsFalse(g.Fresno.SaveEco(0, sat.Take(3).ToList(), new TrainedMove("marisal", ChainRole.Cierre)).Ok, "movimiento no entrenado");
            Assert.IsFalse(g.Fresno.SaveEco(1, sat.Take(3).ToList(), null).Ok, "la ranura 2 se gana venciendo al Tronco");
            Assert.IsFalse(g.Fresno.SaveEco(3, sat.Take(3).ToList(), null).Ok, "solo hay tres ranuras");
            g.Profile.Fresno.BossesDefeated.Add("tronco");
            Assert.IsTrue(g.Fresno.SaveEco(1, sat.Take(3).ToList(), null).Ok);
        }

        [Test]
        public void EmpezarConUnaRanuraVaciaFalla()
        {
            var g = NewGame();
            Assert.IsFalse(g.Fresno.StartRun(Team, ecoSlot: 0).Ok);
            Assert.IsFalse(g.Fresno.InRun);
        }

        // ------------------------------------------------------------------ Injertos, maldiciones y Paria

        [Test]
        public void HaySieteRamasConCincoInjertosActivosYDiezPendientes()
        {
            var rules = Meta().Fresno;
            Assert.AreEqual(7, rules.Ramas.Count);
            foreach (var rama in rules.Ramas)
            {
                Assert.AreEqual(15, rama.Grafts.Count, rama.Name);
                Assert.AreEqual(5, rama.Grafts.Count(x => !x.Pending), rama.Name);
                Assert.IsNotNull(rama.Fruit, rama.Name);
            }
        }

        [Test]
        public void LosInjertosPendientesNuncaSeOfrecen()
        {
            for (ulong seed = 1; seed <= 15; seed++)
            {
                var g = NewGame(seed);
                new AutoClimber(g).Climb(Team, seed: seed);
                foreach (var id in g.Profile.Fresno.History.Last().Grafts) Assert.IsFalse(g.Meta.Fresno.Graft(id).Pending, id);
            }
        }

        [Test]
        public void CadaMaldicionMultiplicaLasAlmitasPorUnoVeinticinco()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            var ids = g.Meta.Fresno.Curses.Select(c => c.Id).ToList();
            Assert.IsFalse(g.Fresno.ChooseCurses(ids.Take(6).ToList()).Ok, "hasta cinco");
            Assert.IsFalse(g.Fresno.ChooseCurses(new[] { ids[0], ids[0] }).Ok, "sin repetir");
            Assert.IsTrue(g.Fresno.ChooseCurses(ids.Take(5).ToList()).Ok);
            Assert.AreEqual(Math.Pow(1.25, 5), g.Fresno.CurseMultiplier(0), 1e-9);
            Assert.AreEqual(Math.Pow(1.25, 5), g.Fresno.AlmitasMultiplier(), 1e-9);
        }

        [Test]
        public void LasMaldicionesSoloPesanEnSuZona()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            g.Fresno.ChooseCurses(new[] { "cuervos" });
            Assert.AreEqual(0.15, g.Fresno.RunModifiers().Get(ModKeys.EnemyAtkPct), 1e-9);
            g.Fresno.Run.Zone = 1;
            Assert.AreEqual(0.0, g.Fresno.RunModifiers().Get(ModKeys.EnemyAtkPct), 1e-9);
        }

        [Test]
        public void LaBendicionDelPariaPremiaALasBrujasModestas()
        {
            var g = NewGame();
            g.Fresno.StartRun(new[] { "marisal", "zarigueya", "topo" }, seed: 3);
            g.Fresno.ChooseCurses(new string[0]);
            Assert.AreEqual(5, g.Fresno.GraftChoiceCount(), "tres más una por cada 3★");
            Assert.AreEqual(1.2, g.Fresno.AlmitasMultiplier(), 1e-9, "+10 % por cada 3★");
        }

        [Test]
        public void ElFrutoSeActivaAlJuntarTresDeUnaRama()
        {
            var g = NewGame();
            g.Fresno.StartRun(Team, seed: 3);
            g.Fresno.ChooseCurses(new string[0]);
            var ids = ActiveGrafts(g, Planet.Sol);
            g.Fresno.Run.Grafts.AddRange(ids.Take(2));
            Assert.IsFalse(g.Fresno.FruitActive(Planet.Sol));
            g.Fresno.Run.Grafts.Add(ids[2]);
            Assert.IsTrue(g.Fresno.FruitActive(Planet.Sol));
            foreach (var key in g.Meta.Fresno.Rama(Planet.Sol).Fruit.Mods.Keys)
                Assert.AreNotEqual(0.0, g.Fresno.RunModifiers().Get(key), key);
        }

        // ------------------------------------------------------------------ Tope semanal, Raíces y Ofrenda

        [Test]
        public void PasadoElTopeSemanalLasAlmitasSeVuelvenSavia()
        {
            var g = NewGame();
            long cap = g.Meta.Fresno.Weekly.CapAlmitas;
            long almitas = g.Ledger.Balance(Res.Almitas), savia = g.Ledger.Balance(Res.Savia);
            Assert.AreEqual(cap, g.Fresno.BankAlmitas(cap, "fresno.santuario"));
            Assert.AreEqual(0, g.Fresno.BankAlmitas(1000, "fresno.santuario"));
            Assert.AreEqual(almitas + cap, g.Ledger.Balance(Res.Almitas));
            Assert.AreEqual(savia + 100, g.Ledger.Balance(Res.Savia), "10 % del excedente");
            Assert.Less(g.Meta.Fresno.Weekly.OverflowSaviaRate, g.Meta.Fresno.ManchaReturn, "a una tasa menor");

            g.SetDay(g.Profile.Day + 7);
            Assert.AreEqual(cap, g.Fresno.WeeklyRemaining, "se reinicia cada semana");
        }

        [Test]
        public void NoHayEnergiaLasRunsNuncaSeCortanPorElTope()
        {
            var g = NewGame();
            g.Fresno.BankAlmitas(g.Meta.Fresno.Weekly.CapAlmitas, "fresno.santuario");
            Assert.IsTrue(g.Fresno.StartRun(Team, seed: 3).Ok);
        }

        [Test]
        public void LasRaicesTienenTopeYSePaganConSavia()
        {
            var g = NewGame();
            var root = g.Meta.Fresno.Root("bolsa_raiz");
            int slots = g.Fresno.BeltSlots;
            long savia = g.Ledger.Balance(Res.Savia);
            Assert.IsTrue(g.Fresno.BuyRoot(root.Id).Ok);
            Assert.AreEqual(savia - root.Costs[0], g.Ledger.Balance(Res.Savia));
            Assert.AreEqual(slots + 1, g.Fresno.BeltSlots, "una ranura más de poción");
            Assert.IsFalse(g.Fresno.BuyRoot(root.Id).Ok, "con tope");
            Assert.IsFalse(g.Fresno.BuyRoot("nada").Ok);
        }

        [Test]
        public void LaOfrendaHaceCrecerAlArbolConBonosConTope()
        {
            var g = NewGame();
            int level = g.Fresno.OfrendaLevel;
            long needed = g.Fresno.NextOfrendaThreshold;
            Assert.IsFalse(g.Fresno.Offer(0).Ok);
            Assert.IsTrue(g.Fresno.Offer(needed).Ok);
            Assert.Greater(g.Fresno.OfrendaLevel, level);
            g.Profile.Fresno.OfrendaTotal = long.MaxValue / 4;
            var bonus = g.Fresno.OfrendaBonus();
            foreach (var key in g.Meta.Fresno.Ofrenda.Caps.Keys)
                Assert.LessOrEqual(bonus.Get(key), g.Meta.Fresno.Ofrenda.Caps.Get(key), key);
        }

        // ------------------------------------------------------------------ Runs completas

        [Test]
        public void ElPilotoTerminaRunsCompletasConCualquierSemilla()
        {
            int wins = 0;
            for (ulong seed = 1; seed <= 25; seed++)
            {
                var g = NewGame(seed);
                var rep = new AutoClimber(g).Climb(Team, seed: seed);
                Assert.IsFalse(g.Fresno.InRun);
                Assert.Contains(rep.Outcome, new[] { RunOutcome.Victory, RunOutcome.Defeat });
                Assert.AreEqual(1, g.Profile.Fresno.RunsFinished);
                if (rep.Outcome == RunOutcome.Victory) wins++;
            }
            Assert.Less(wins, 25, "sin Eco ni Raíces, la cima no es un paseo");
        }

        [Test]
        public void LaMismaSemillaJuegaLaMismaRun()
        {
            var a = new AutoClimber(NewGame(11));
            var b = new AutoClimber(NewGame(11));
            var ra = a.Climb(Team, seed: 77);
            var rb = b.Climb(Team, seed: 77);
            Assert.AreEqual(ra.Fingerprint, rb.Fingerprint);
            CollectionAssert.AreEqual(ra.Log, rb.Log);
        }

        [Test]
        public void GuardarYCargarAMediaRunNoCambiaElResultado()
        {
            var straight = NewGame(21);
            var expected = new AutoClimber(straight).Climb(Team, seed: 88);

            var paused = NewGame(21);
            paused.Fresno.StartRun(Team, seed: 88);
            // Avanza nodo por nodo guardando y cargando el perfil entre cada paso (GDD 10.1: se puede pausar en cualquier nodo).
            var store = new MemorySaveStore();
            int nodes = 0;
            var stepper = paused;
            while (stepper.Fresno.Run != null && stepper.Fresno.Run.Phase != RunPhase.EcoSave && nodes < 300)
            {
                new AutoClimber(stepper).StepOnce();
                stepper.Save(store, "s");
                stepper = MetaGame.Load(Meta(), store, "s");
                nodes++;
            }
            Assert.AreEqual(expected.Outcome, stepper.Fresno.Run.Outcome);
            Assert.AreEqual(expected.ReachedNode, stepper.Fresno.Run.NodeId);
            Assert.AreEqual(expected.AlmitasBanked, stepper.Fresno.Run.Banked);
            CollectionAssert.AreEqual(expected.Log, stepper.Fresno.Run.Log);
        }

        [Test]
        public void AceptacionTresRunsSeguidasLaTerceraEsDistinta()
        {
            var g = NewGame(31);
            var climber = new AutoClimber(g);

            // Run 1: se juega de verdad y cae en cuanto lleva almitas (la derrota se fuerza para fijar el caso).
            climber.BeforeBattle = setup =>
            {
                if (g.Fresno.Run.Loose > 0 && g.Fresno.Run.Grafts.Count >= 3) setup.Modifiers.Add(ModKeys.EnemyAtkPct, 100);
            };
            var run1 = climber.Climb(Team, seed: 101);
            Assert.AreEqual(RunOutcome.Defeat, run1.Outcome);
            Assert.Greater(run1.ManchaLeft, 0);
            Assert.IsNotNull(g.Profile.Fresno.Eco[0], "guarda un Eco al caer");
            var eco = g.Profile.Fresno.Eco[0];
            Assert.AreEqual(3, eco.Grafts.Count);

            // Run 2: con Eco; recupera la Mancha y la Savia que entra compra Raíces.
            climber.BeforeBattle = null;
            var run2 = climber.Climb(Team, ecoSlot: 0, seed: 102);
            Assert.AreEqual(run1.ManchaLeft, run2.ManchaRecovered, "recupera todas las almas de la run 1");
            Assert.AreEqual(eco.Name, run2.EcoUsed);

            Assert.IsTrue(g.Fresno.BuyRoot("grano_raiz").Ok, "la Savia compra una Raíz");

            // Run 3: empieza con Eco y con la Raíz activa, y sus modificadores ya no son los de una cuenta nueva.
            Assert.IsTrue(g.Fresno.StartRun(Team, ecoSlot: 0, seed: 103).Ok);
            g.Fresno.ChooseCurses(new string[0]);
            Assert.AreEqual(3, g.Fresno.Run.Grafts.Count, "tres Injertos antes del primer combate");
            Assert.AreEqual(1.0, g.Fresno.RunModifiers().Get(ModKeys.StartGrains), 1e-9, "la Raíz da un Grano");
            var run3 = climber.Continue();
            Assert.Greater(run3.Combats, 0);
            Assert.AreEqual(3, g.Profile.Fresno.History.Count);

            AssertLedgerExplainsEverything(g);
        }

        /// <summary>Cada saldo es la suma de sus movimientos: nada cambió sin pasar por el libro contable.</summary>
        internal static void AssertLedgerExplainsEverything(MetaGame g)
        {
            var net = new Dictionary<string, long>();
            foreach (var kv in g.Ledger.TotalsIn) { var r = kv.Key.Split('|')[0]; net[r] = (net.TryGetValue(r, out var v) ? v : 0) + kv.Value; }
            foreach (var kv in g.Ledger.TotalsOut) { var r = kv.Key.Split('|')[0]; net[r] = (net.TryGetValue(r, out var v) ? v : 0) - kv.Value; }
            foreach (var kv in g.Ledger.Balances) Assert.AreEqual(net.TryGetValue(kv.Key, out var v) ? v : 0, kv.Value, kv.Key);
            long banked = g.Profile.Fresno.History.Sum(h => h.AlmitasBanked);
            Assert.AreEqual(banked, g.Ledger.TotalIn(Res.Almitas, "fresno.santuario") + g.Ledger.TotalIn(Res.Almitas, "fresno.cima"), "las almitas del Fresno cuadran con las runs");
        }
    }
}
