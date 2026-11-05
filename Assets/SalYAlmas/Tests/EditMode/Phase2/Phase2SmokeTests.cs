using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>
    /// Pruebas de humo de la Fase 2 (trabajo en curso). Comprueban que cada sistema arranca y responde;
    /// las pruebas mínimas completas del prompt quedan pendientes (ver docs/FASE2_PROGRESO.md).
    /// </summary>
    public class Phase2SmokeTests
    {
        private static MetaGame TestGame() => new MetaGame(Meta(), MetaGame.TestProfile(Meta(), 7, 300));

        [Test]
        public void ElPerfilSeGuardaYSeCargaSinPerdida()
        {
            var game = TestGame();
            var store = new MemorySaveStore();
            game.Save(store, "slot");
            var loaded = MetaGame.Load(Meta(), store, "slot");
            Assert.AreEqual(game.ToJson(), loaded.ToJson());
        }

        [Test]
        public void LaSimulacionDelGachaDaLosPromediosDelGdd()
        {
            var sim = GachaSimulation.Run(Meta().Gacha, 1000000, 2026);
            Assert.That(sim.AveragePerFive, Is.InRange(52.0, 54.0), "GDD 16.2: 53 tiradas por 5★");
            Assert.That(sim.AveragePerFeatured, Is.InRange(65.0, 67.0), "GDD 16.2: 66 tiradas por la destacada");
            Assert.LessOrEqual(sim.MaxPullsForFive, 70);
            Assert.LessOrEqual(sim.MaxPullsForFeatured, 140);
            Assert.LessOrEqual(sim.MaxGapFourStar, 10);
        }

        [Test]
        public void ElMapaDelFresnoEsDeterministaConLaMismaSemilla()
        {
            var a = MapGenerator.Generate(Meta().Fresno, 42);
            var b = MapGenerator.Generate(Meta().Fresno, 42);
            var c = MapGenerator.Generate(Meta().Fresno, 43);
            Assert.AreEqual(a.Fingerprint(), b.Fingerprint());
            Assert.AreNotEqual(a.Fingerprint(), c.Fingerprint());
            Assert.AreEqual(4, a.Zones.Count);
            Assert.IsTrue(a.Zones.All(z => z.Count == 10));
        }

        [Test]
        public void UnJuicioSeGeneraYSeJuegaConElPilotoAutomatico()
        {
            var game = TestGame();
            var ok = game.Trials.Prepare("zarigueya", new[] { "marisal", "solenne", "selenia" }, 11, out var plan);
            Assert.IsTrue(ok.Ok, ok.Message);
            var battle = new Battle(game.Combat, plan.Setup);
            battle.Start();
            new AutoPilot().RunToEnd(battle);
            Assert.IsTrue(battle.IsOver);
        }

        [Test]
        public void UnaRunDelFresnoArrancaYLlegaAlPrimerCombate()
        {
            var game = TestGame();
            var r = game.Fresno.StartRun(new[] { "marisal", "selenia", "solenne", "tormentina" }, seed: 5);
            Assert.IsTrue(r.Ok, r.Message);
            Assert.IsTrue(game.Fresno.ChooseCurses(new string[0]).Ok);
            var node = game.Fresno.AvailableNodes().Single();
            Assert.IsTrue(game.Fresno.EnterNode(node.Lane).Ok);
            var setup = game.Fresno.PrepareBattle();
            var battle = new Battle(game.Combat, setup);
            battle.Start();
            new AutoPilot().RunToEnd(battle);
            Assert.IsTrue(game.Fresno.ResolveBattle(battle).Ok);
        }
    }
}
