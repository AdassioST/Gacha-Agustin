using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 9.5 y 9.6: la Cadena, la Partitura y el Spark.</summary>
    public class ChainTests
    {
        /// <summary>a abre con un golpe en su frame 26; b enlaza con un golpe en su frame bFrame (absoluto 24 + bFrame).</summary>
        private static Battle SparkBattle(int bFrame, out WitchUnit a, out WitchUnit b2)
        {
            var wa = Witch("a", Planet.Marte, Planet.Saturno, 130);
            var wb = Witch("b", Planet.Venus, Planet.Saturno, 120);
            var wc = Witch("c", Planet.Luna, Planet.Saturno, 110);
            SetChainFrames(wa, new[] { 26 }, new[] { 0 }, new[] { 0 });
            SetChainFrames(wb, new[] { 0 }, new[] { bFrame }, new[] { 0 });
            SetChainFrames(wc, new[] { 0 }, new[] { 0 }, new[] { 80 });
            var db = Db(new[] { wa, wb, wc }, new[] { Enemy("e", 10, 1000000, 10000000, Planet.Sol, Planet.Mercurio) });
            var b = Start(db, new[] { "a", "b", "c" });
            b.Resonance = 100;
            a = W(b, "a");
            b2 = W(b, "b");
            return b;
        }

        [Test]
        public void Spark_ConDosFramesDeDistanciaSi()
        {
            var b = SparkBattle(4, out var a, out var wb); // 24 + 4 = 28; |28 − 26| = 2
            var preview = b.PreviewChain(Ids(b, "a", "b", "c"), false);
            Assert.AreEqual(2, preview.SparkCount, "los dos golpes que coinciden son Spark");
            Assert.IsTrue(preview.Hits.Single(h => h.OwnerId == a.Id).IsSpark);
            Assert.IsTrue(preview.Hits.Single(h => h.OwnerId == wb.Id).IsSpark);
        }

        [Test]
        public void Spark_ConTresFramesDeDistanciaNo()
        {
            var b = SparkBattle(5, out _, out _); // 29 − 26 = 3
            Assert.AreEqual(0, b.PreviewChain(Ids(b, "a", "b", "c"), false).SparkCount);
        }

        [Test]
        public void Spark_LaMismaBrujaNoHaceSparkConsigoMisma()
        {
            var hits = new List<ChainHitPlan>
            {
                new ChainHitPlan { OwnerId = 1, Frame = 10 },
                new ChainHitPlan { OwnerId = 1, Frame = 11 },
                new ChainHitPlan { OwnerId = 2, Frame = 40 },
            };
            Assert.AreEqual(0, ChainPlanner.MarkSparks(hits, 2));
        }

        [Test]
        public void Spark_Mas50PorCientoAEseGolpe_Mas2DeAnimaYUnaAlmita()
        {
            var b = SparkBattle(4, out var a, out var wb);
            int mark = b.Events.Count;
            int animaA = a.Anima, animaB = wb.Anima;
            Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, Ids(b, "a", "b", "c"), false, -1)));
            var hits = EventsSince<HitLanded>(b, mark);
            var hitA = hits.Single(h => h.AttackerId == a.Id);
            Assert.IsTrue(hitA.IsSpark);
            // Apertura M_hab 1.0: 551 sin Spark; ×1.5 → 714 + 112.5 = 826.5
            Assert.AreEqual(827, hitA.Damage);
            var hitC = hits.Single(h => h.AttackerId == W(b, "c").Id);
            Assert.IsFalse(hitC.IsSpark);
            Assert.AreEqual(551, hitC.Damage);
            // +10 por eslabón (TUNING) y +2 por su Spark.
            Assert.AreEqual(animaA + 10 + 2, a.Anima);
            Assert.AreEqual(animaB + 10 + 2, wb.Anima);
            var ended = EventsSince<ChainEnded>(b, mark).Single();
            Assert.AreEqual(2, ended.Sparks);
            Assert.AreEqual(2, ended.Almitas);
            Assert.AreEqual(2, b.Almitas);
        }

        [Test]
        public void TopeDeResonancia_ElSparkDaMaximo20PorCadena()
        {
            var wa = Witch("a", Planet.Marte, Planet.Saturno, 130);
            var wb = Witch("b", Planet.Venus, Planet.Saturno, 120);
            var wc = Witch("c", Planet.Luna, Planet.Saturno, 110);
            // Muchos golpes encimados: cada golpe que coincide con otra bruja es un Spark.
            SetChainFrames(wa, new[] { 24, 30, 36, 42 }, new[] { 0 }, new[] { 0 });
            SetChainFrames(wb, new[] { 0 }, new[] { 0, 6, 12, 18 }, new[] { 0 });
            SetChainFrames(wc, new[] { 0 }, new[] { 0 }, new[] { 0 });
            var db = Db(new[] { wa, wb, wc }, new[] { Enemy("e", 10, 1000000, 10000000, Planet.Sol, Planet.Mercurio) });
            var b = Start(db, new[] { "a", "b", "c" });
            b.Resonance = 100;
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, Ids(b, "a", "b", "c"), false, -1)));
            var ended = EventsSince<ChainEnded>(b, mark).Single();
            Assert.AreEqual(8, ended.Sparks);
            Assert.AreEqual(20, ended.ResonanceFromSparks, "tope de +20 por Cadena");
            Assert.AreEqual(20, b.Resonance, "100 − 100 de la Cadena + 20 de Spark");
            var sparkGains = EventsSince<ResonanceChanged>(b, mark).Where(r => r.Reason == "Spark").Sum(r => r.New - r.Old);
            Assert.AreEqual(20, sparkGains);
        }

        [Test]
        public void ElEslabonNEmpieza24PorNMenos1FramesDespues()
        {
            var db = TestKit.RealDb();
            var b = new Battle(db, new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 3 });
            b.Start();
            var preview = b.PreviewChain(b.Witches.Select(w => w.Id).ToList(), true);
            Assert.IsTrue(preview.IsValid);
            Assert.AreEqual(5, preview.Links.Count, "cuatro brujas y la Invitada como Cierre");
            CollectionAssert.AreEqual(new[] { 0, 24, 48, 72, 96 }, preview.Links.Select(l => l.StartFrame));
            CollectionAssert.AreEqual(new[] { ChainRole.Apertura, ChainRole.Enlace, ChainRole.Enlace, ChainRole.Enlace, ChainRole.Cierre },
                preview.Links.Select(l => l.Role));
            Assert.IsTrue(preview.Links[4].IsGuest);
            Assert.LessOrEqual(preview.TotalFrames, b.Rules.Chain.MaxChainFrames, "la Cadena completa dura 6 s como máximo");
        }

        [Test]
        public void ValidacionDeLaCadena()
        {
            var db = TestKit.RealDb();
            var b = new Battle(db, new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 3 });
            b.Start();
            AdvanceToAlly(b);
            int opener = b.ActiveUnit.Id;
            var ids = b.Witches.Select(w => w.Id).ToList();

            Assert.IsFalse(b.Execute(BattleCommand.Chain(opener, ids.Take(3), false, -1)).Ok, "sin 100 de Resonancia no hay Cadena");
            b.Resonance = 100;
            Assert.IsFalse(b.Execute(BattleCommand.Chain(opener, ids.Take(2), false, -1)).Ok, "mínimo 3 brujas");
            Assert.IsFalse(b.Execute(BattleCommand.Chain(opener, new[] { ids[0], ids[0], ids[1] }, false, -1)).Ok, "sin repetir");
            Assert.IsFalse(b.Execute(BattleCommand.Chain(opener, new[] { ids[0], ids[1], Battle.GuestId }, false, -1)).Ok, "la Invitada no entra como bruja del campo");
            Ok(b.Execute(BattleCommand.Chain(opener, ids.Take(3), true, -1)));
            Assert.IsTrue(b.GuestClosingUsed);
            Assert.AreEqual(1, b.Events.OfType<ChainStarted>().Count());

            AdvanceToAlly(b);
            b.Resonance = 100;
            Assert.IsFalse(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, ids.Take(3), true, -1)).Ok, "la Invitada cierra una vez por combate");
            Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, ids, false, -1)));
        }

        [Test]
        public void PreviewChainEsIgualALaEjecucionReal()
        {
            var db = TestKit.RealDb();
            var setup = new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 11 };
            var probe = new Battle(db, setup);
            probe.Start();
            var ids = probe.Witches.Select(w => w.Id).ToList();

            int checkedOrders = 0;
            foreach (var size in new[] { 3, 4 })
            foreach (var order in AutoPilot.Permutations(ids, size).Where((perm, i) => i % 3 == 0))
            foreach (var guest in new[] { false, true })
            {
                var b = new Battle(db, setup);
                b.Start();
                AdvanceToAlly(b);
                b.Resonance = 100;
                var preview = b.PreviewChain(order, guest);
                Assert.IsTrue(preview.IsValid, preview.Error);

                int mark = b.Events.Count;
                Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, order, guest, -1)));
                var events = b.Events.Skip(mark).ToList();
                int start = events.FindIndex(e => e is ChainStarted);
                int end = events.FindIndex(e => e is ChainEnded);
                var executed = events.Skip(start).Take(end - start).OfType<HitLanded>()
                    .Select(h => (h.AttackerId, h.Frame, h.IsSpark)).ToList();
                var predicted = preview.Hits.Select(h => (h.OwnerId, h.Frame, h.IsSpark)).ToList();

                CollectionAssert.AreEqual(predicted, executed, "orden " + string.Join(",", order) + " invitada " + guest);
                Assert.AreEqual(preview.SparkCount, ((ChainEnded)events[end]).Sparks);
                checkedOrders++;
            }
            Assert.Greater(checkedOrders, 20);
        }

        [Test]
        public void PreviewChainNoTocaElEstado()
        {
            var db = TestKit.RealDb();
            var setup = new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 5 };
            var withPreviews = new Battle(db, setup);
            var without = new Battle(db, setup);
            withPreviews.Start();
            without.Start();
            var ids = withPreviews.Witches.Select(w => w.Id).ToList();

            var pilot = new AutoPilot();
            for (int step = 0; step < 40 && !withPreviews.IsOver; step++)
            {
                int events = withPreviews.Events.Count, res = withPreviews.Resonance, grains = withPreviews.Grains;
                foreach (var order in AutoPilot.Permutations(ids, 3)) withPreviews.PreviewChain(order, true);
                Assert.AreEqual(events, withPreviews.Events.Count);
                Assert.AreEqual(res, withPreviews.Resonance);
                Assert.AreEqual(grains, withPreviews.Grains);

                var cmd = pilot.Next(without);
                Ok(without.Execute(cmd));
                Ok(withPreviews.Execute(cmd));
            }
            CollectionAssert.AreEqual(without.Events.Select(e => e.Serialize()), withPreviews.Events.Select(e => e.Serialize()),
                "previsualizar no consume azar ni cambia el resultado");
        }

        [Test]
        public void ElOrdenImporta_ElMismoEquipoDaDistintosSpark()
        {
            var db = TestKit.RealDb();
            var b = new Battle(db, new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 1 });
            b.Start();
            var ids = b.Witches.Select(w => w.Id).ToList();
            var counts = AutoPilot.Permutations(ids, 4).Select(o => b.PreviewChain(o, true).SparkCount).Distinct().ToList();
            Assert.GreaterOrEqual(counts.Count, 3, "distintos órdenes deben dar distintos Spark");
            Assert.Contains(0, AutoPilot.Permutations(ids, 3).Select(o => b.PreviewChain(o, false).SparkCount).ToList());
        }
    }
}
