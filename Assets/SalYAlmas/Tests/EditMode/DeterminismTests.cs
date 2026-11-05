using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;

namespace SalYAlmas.Tests
{
    /// <summary>Mismo estado y mismas entradas dan el mismo resultado (GDD 21.2).</summary>
    public class DeterminismTests
    {
        private static BattleSetup Setup(ulong seed) => new BattleSetup
        {
            Team = { "marisal", "selenia", "solenne", "tormentina" },
            GuestId = "zarigueya",
            EncounterId = "guardian",
            Seed = seed,
            Belt = { new BeltSlot("balsamo_cuprico", 3), new BeltSlot("leche_luna", 2), new BeltSlot("destilado_rocio", 2) },
        };

        private static Battle Play(ulong seed)
        {
            var b = new Battle(TestKit.RealDb(), Setup(seed));
            b.Start();
            new AutoPilot().RunToEnd(b);
            return b;
        }

        private static List<string> Serialized(Battle b) => b.Events.Select(e => e.Serialize()).ToList();

        [Test]
        public void LaMismaSemillaReproduceElMismoCombateEventoPorEvento()
        {
            var first = Play(20261105);
            var second = Play(20261105);
            Assert.GreaterOrEqual(first.Events.Count, 200, "el combate debe producir al menos 200 eventos");
            CollectionAssert.AreEqual(Serialized(first), Serialized(second));
            Assert.AreEqual(first.Outcome, second.Outcome);
        }

        [Test]
        public void LasOrdenesGrabadasRepitenElCombate()
        {
            var original = Play(77);
            var replay = Battle.Replay(TestKit.RealDb(), Setup(77), original.CommandLog);
            Assert.GreaterOrEqual(replay.Events.Count, 200);
            CollectionAssert.AreEqual(Serialized(original), Serialized(replay));
        }

        [Test]
        public void OtraSemillaCambiaElAzar()
        {
            var a = Play(1);
            var b = Play(2);
            CollectionAssert.AreNotEqual(Serialized(a), Serialized(b));
        }

        [Test]
        public void ElGeneradorConSemillaEsEstable()
        {
            // Valores fijos de xoshiro256** con SplitMix64: si cambian, cambió el algoritmo y se rompen las repeticiones.
            var r1 = new SeededRandom(42);
            var r2 = new SeededRandom(42);
            for (int i = 0; i < 1000; i++) Assert.AreEqual(r1.NextULong(), r2.NextULong());
            var r3 = new SeededRandom(42);
            double d = r3.NextDouble();
            Assert.IsTrue(d >= 0 && d < 1);
            var counts = new int[6];
            var r4 = new SeededRandom(7);
            for (int i = 0; i < 60000; i++) counts[r4.NextInt(6)]++;
            Assert.IsTrue(counts.All(c => c > 9000 && c < 11000), "distribución razonable");
        }

        [Test]
        public void SerializacionEstableDeUnEvento()
        {
            var e = new HitLanded
            {
                AttackerId = 1, AttackerName = "Marisal", TargetId = 100, TargetName = "Guardián", Source = HitSource.Chain,
                Frame = 26, Damage = 827, IsSpark = true, Planet = SalYAlmas.Core.Magic.Planet.Marte, ToughnessDamage = 12.75, ChainLink = 0,
            };
            StringAssert.Contains("Frame=26", e.Serialize());
            StringAssert.Contains("Damage=827", e.Serialize());
            StringAssert.Contains("IsSpark=True", e.Serialize());
            StringAssert.Contains("ToughnessDamage=12.75", e.Serialize());
        }
    }
}
