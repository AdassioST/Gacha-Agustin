using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 7.3: las cuatro Reacciones Híbridas.</summary>
    public class ReactionTests
    {
        [Test]
        public void Eclipse_QuitaDosMejorasYRetrasa30()
        {
            var b = AspectTests.ChainBattle(Planet.Sol, Planet.Saturno, Planet.Marte, out _, out int mark, bt =>
            {
                var e = bt.Enemies[0];
                bt.AddStatus(e, StatusKind.ResAll, 0.25, 3, "Corteza", isBuff: true);
                bt.AddStatus(e, StatusKind.DamagePct, 0.20, 3, "Savia", isBuff: true);
                bt.AddStatus(e, StatusKind.AtkPct, 0.20, 3, "Furia", isBuff: true);
            });
            var e0 = b.Enemies[0];
            Assert.AreEqual(HybridReaction.Eclipse, EventsSince<ReactionTriggered>(b, mark).Single().Reaction);
            Assert.AreEqual(1, e0.Statuses.Count(s => s.IsBuff), "quedó una de tres mejoras");
            Assert.AreEqual(2, EventsSince<StatusRemoved>(b, mark).Count(s => s.Reason == "Eclipse"));
            var shift = EventsSince<ActionShifted>(b, mark).Single(s => s.Reason == "Eclipse");
            Assert.AreEqual(-0.30, shift.Percent, 1e-9);
        }

        [Test]
        public void HeladaNegra_ElEnemigoPierdeSuSiguienteTurno()
        {
            var b = AspectTests.ChainBattle(Planet.Luna, Planet.Saturno, Planet.Marte, out _, out int mark);
            var e = b.Enemies[0];
            Assert.IsTrue(e.Has(StatusKind.Frozen));
            for (int i = 0; i < 300 && !b.Events.OfType<TurnSkipped>().Any(); i++)
                Ok(b.Execute(b.Pending == DecisionKind.AllyTurn ? BattleCommand.Basic(b.ActiveUnit.Id, b.Enemies[1].Id) : BattleCommand.Advance()));
            var skipped = b.Events.OfType<TurnSkipped>().Single();
            Assert.AreEqual(e.Id, skipped.UnitId);
            Assert.IsFalse(e.Has(StatusKind.Frozen));
        }

        [Test]
        public void HeladaNegra_ElJefePierde50PorCientoDeVelocidadUnTurno()
        {
            var e = Enemy("jefe", 100, 1000000, 10000000, Planet.Venus, Planet.Mercurio);
            e.IsBoss = true;
            var db = Db(new[] { Witch("x", Planet.Luna, Planet.Marte, 130), Witch("y", Planet.Saturno, Planet.Marte, 120), Witch("z", Planet.Sol, Planet.Marte, 110) }, new[] { e });
            var b = Start(db, new[] { "x", "y", "z" });
            b.Resonance = 100;
            var boss = b.Enemies[0];
            double before = b.Speed(boss);
            Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, Ids(b, "x", "y", "z"), false, boss.Id)));
            Assert.IsFalse(boss.Has(StatusKind.Frozen), "el jefe no pierde el turno");
            Assert.AreEqual(before * 0.5, b.Speed(boss), 1e-9);
            Assert.AreEqual(1, boss.Find(StatusKind.SpeedPct).TurnsLeft);
        }

        [Test]
        public void EspinasRojas_SangradoQueCuraAlEquipoCon30PorCientoDelDano()
        {
            var b = AspectTests.ChainBattle(Planet.Marte, Planet.Venus, Planet.Sol, out _, out int mark, bt =>
            {
                foreach (var w in bt.Witches) w.Hp = 5000;
            });
            var e = b.Enemies[0];
            var bleed = e.Statuses.Single(s => s.Name == "Espinas Rojas");
            Assert.AreEqual(0.30, bleed.HealShare, 1e-9);

            for (int i = 0; i < 300 && !b.Events.OfType<DotTicked>().Any(d => d.StatusName == "Espinas Rojas"); i++)
                Ok(b.Execute(b.Pending == DecisionKind.AllyTurn ? BattleCommand.Basic(b.ActiveUnit.Id, b.Enemies[1].Id) : BattleCommand.Advance()));
            var tick = b.Events.OfType<DotTicked>().First(d => d.StatusName == "Espinas Rojas");
            int tickIndex = tick.Index;
            int expectedHeal = (int)System.Math.Round(tick.Damage * 0.30, System.MidpointRounding.AwayFromZero);
            var heals = b.Events.Skip(tickIndex).OfType<Healed>().Where(h => h.Reason == "Espinas Rojas").ToList();
            Assert.AreEqual(3, heals.Count, "cura a cada bruja");
            Assert.IsTrue(heals.All(h => h.Amount == expectedHeal));
        }

        [Test]
        public void Tempestad_RayoATodosConDosGolpesExtra()
        {
            var b = AspectTests.ChainBattle(Planet.Mercurio, Planet.Jupiter, Planet.Sol, out _, out int mark);
            var hits = EventsSince<HitLanded>(b, mark).Where(h => h.Source == HitSource.Reaction).ToList();
            Assert.AreEqual(4, hits.Count, "2 golpes extra × 2 enemigos");
            CollectionAssert.AreEquivalent(new[] { 24 + 10, 24 + 10, 24 + 24, 24 + 24 }, hits.Select(h => h.Frame));
            Assert.IsTrue(hits.All(h => h.AttackerId == W(b, "y").Id));

            // La Partitura los muestra antes de confirmar.
            var b2 = AspectTests.ChainBattle(Planet.Mercurio, Planet.Jupiter, Planet.Sol, out _, out _, bt =>
            {
                var preview = bt.PreviewChain(Ids(bt, "x", "y", "z"), false);
                Assert.AreEqual(2, preview.Hits.Count(h => h.IsReaction));
            });
        }

        [Test]
        public void Tempestad_FacilitaElSpark()
        {
            // El golpe extra en el frame 24 + 24 coincide con el primer golpe del tercer eslabón (frame 48).
            var b = AspectTests.ChainBattle(Planet.Mercurio, Planet.Jupiter, Planet.Sol, out var ended, out int mark);
            Assert.Greater(ended.Sparks, 0);
            Assert.IsTrue(EventsSince<HitLanded>(b, mark).Any(h => h.Source == HitSource.Reaction && h.IsSpark));
        }
    }
}
