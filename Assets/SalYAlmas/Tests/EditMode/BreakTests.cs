using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 9.4: Firmeza, debilidades y Quiebre con el efecto de cada planeta.</summary>
    public class BreakTests
    {
        private static Planet Other(params Planet[] exclude) => Planets.All.First(p => !exclude.Contains(p));

        private static Battle BreakBattle(Planet planet, out WitchUnit witch, out EnemyUnit enemy, bool boss = false)
        {
            var secondary = Other(planet);
            var e = Enemy("e", 50, 10, 10000000, planet, Other(planet, secondary));
            e.IsBoss = boss;
            var db = Db(new[] { Witch("a", planet, secondary, 100) }, new[] { e, Enemy("f", 50, 1000, 10000000, Other(planet, secondary)) });
            var b = Start(db, new[] { "a" });
            witch = W(b, "a");
            enemy = b.Enemies[0];
            return b;
        }

        [TestCase(Planet.Sol, BreakEffect.Quemadura)]
        [TestCase(Planet.Luna, BreakEffect.Encierro)]
        [TestCase(Planet.Marte, BreakEffect.SangradoFuerte)]
        [TestCase(Planet.Mercurio, BreakEffect.VenenoYRetraso)]
        [TestCase(Planet.Venus, BreakEffect.Encanto)]
        [TestCase(Planet.Jupiter, BreakEffect.Electrocucion)]
        [TestCase(Planet.Saturno, BreakEffect.Gravedad)]
        public void QuiebreDeCadaPlaneta(Planet planet, BreakEffect effect)
        {
            var b = BreakBattle(planet, out var a, out var e);
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(a.Id, e.Id)));

            var brk = EventsSince<BreakTriggered>(b, mark).Single();
            Assert.AreEqual(planet, brk.Planet);
            Assert.AreEqual(effect, brk.Effect);
            Assert.IsTrue(e.Broken);
            Assert.AreEqual(0.0, e.Toughness, 1e-9);

            var r = b.Rules.Break;
            double expectedDelay = r.Delay;
            switch (effect)
            {
                case BreakEffect.Quemadura:
                    Assert.IsTrue(e.Has(StatusKind.Burn), "Quemadura que daña cada turno");
                    break;
                case BreakEffect.Encierro:
                    expectedDelay += r.LunaExtraDelay;
                    Assert.AreEqual(-0.10, e.Sum(StatusKind.SpeedPct), 1e-9, "−10 % de Velocidad");
                    break;
                case BreakEffect.SangradoFuerte:
                    Assert.AreEqual(r.MarteBleedCoef, e.Find(StatusKind.Bleed).Value, 1e-9);
                    break;
                case BreakEffect.VenenoYRetraso:
                    expectedDelay += r.MercurioExtraDelay;
                    Assert.IsTrue(e.Has(StatusKind.Poison));
                    break;
                case BreakEffect.Encanto:
                    Assert.AreEqual(r.VenusCharmDamageDown, e.Find(StatusKind.Charm).Value, 1e-9);
                    break;
                case BreakEffect.Electrocucion:
                    Assert.AreEqual(r.JupiterJumpShare, e.Find(StatusKind.Shock).JumpShare, 1e-9, "salta a los enemigos cercanos");
                    break;
                case BreakEffect.Gravedad:
                    expectedDelay += r.SaturnoExtraDelay;
                    break;
            }
            var shift = EventsSince<ActionShifted>(b, mark).Single(s => s.UnitId == e.Id && s.Reason == "Quiebre");
            Assert.AreEqual(-expectedDelay, shift.Percent, 1e-9);
            var res = EventsSince<ResonanceChanged>(b, mark).Single(x => x.Reason == "Quiebre");
            Assert.AreEqual(15, res.New - res.Old, "+15 de Resonancia por Quiebre");
        }

        [Test]
        public void Gravedad_EsElRetrasoMasFuerte()
        {
            var r = new SalYAlmas.Core.Data.BreakRules();
            Assert.Greater(r.SaturnoExtraDelay, r.LunaExtraDelay);
            Assert.Greater(r.SaturnoExtraDelay, r.MercurioExtraDelay);
        }

        [Test]
        public void ElEnemigoQuebradoRecibe10PorCientoMas()
        {
            var b = BreakBattle(Planet.Marte, out var a, out var e);
            Ok(b.Execute(BattleCommand.Spell(a.Id, e.Id)));
            Assert.IsTrue(e.Broken);
            TurnOf(b, "a");
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Basic(a.Id, e.Id)));
            var hit = EventsSince<HitLanded>(b, mark).Single(h => h.TargetId == e.Id);
            // Básico: 1000·1·0.952·0.5·1.1 = 523.6 y 1000·1·0.15·0.5·1.1 = 82.5 → 606.1
            Assert.AreEqual(606, hit.Damage);
        }

        [Test]
        public void SoloLosPlanetasDebilesBajanLaFirmeza_LaBrujaTurbiaUsaSuSecundario()
        {
            // Marte con Saturno contra un enemigo débil solo a Saturno: quita la parte secundaria (1 − p).
            var e = Enemy("e", 50, 1000, 10000000, Planet.Saturno, Planet.Luna);
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { e });
            var b = Start(db, new[] { "a" });
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(W(b, "a").Id, -1)));
            var hit = EventsSince<HitLanded>(b, mark).Single();
            Assert.AreEqual(20 * 0.15, hit.ToughnessDamage, 1e-6);

            // Contra un enemigo sin esas debilidades no quita nada.
            var e2 = Enemy("e2", 50, 1000, 10000000, Planet.Sol, Planet.Luna);
            var b2 = Start(Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { e2 }), new[] { "a" });
            mark = b2.Events.Count;
            Ok(b2.Execute(BattleCommand.Spell(W(b2, "a").Id, -1)));
            Assert.AreEqual(0.0, EventsSince<HitLanded>(b2, mark).Single().ToughnessDamage, 1e-9);
        }

        [Test]
        public void LaFirmezaSeRecuperaEnElTurnoDelEnemigo()
        {
            var b = BreakBattle(Planet.Sol, out var a, out var e);
            Ok(b.Execute(BattleCommand.Spell(a.Id, e.Id)));
            Assert.IsTrue(e.Broken);
            for (int i = 0; i < 10 && e.Broken; i++)
                Ok(b.Execute(b.Pending == DecisionKind.AllyTurn ? BattleCommand.Basic(b.ActiveUnit.Id, b.Enemies[1].Id) : BattleCommand.Advance()));
            Assert.IsFalse(e.Broken);
            Assert.AreEqual(e.MaxToughness, e.Toughness, 1e-9);
            Assert.AreEqual(1, b.Events.OfType<ToughnessRestored>().Count());
        }

        [Test]
        public void QuebrarCancelaElAtaqueQueSeEstaCargando()
        {
            var e = Enemy("e", 200, 10, 10000000, Planet.Marte, Planet.Luna);
            e.Skills.Add(AttackSkill("juicio", 2.0, EnemyTargetRule.All, ultimate: true, charge: true));
            e.Pattern.Clear();
            e.Pattern.Add("juicio");
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { e });
            var b = Start(db, new[] { "a" });
            AdvanceToAlly(b);
            Assert.IsTrue(b.Enemies[0].IsCharging, "el enemigo empezó a cargar");
            Ok(b.Execute(BattleCommand.Spell(W(b, "a").Id, -1)));
            Assert.IsFalse(b.Enemies[0].IsCharging);
            Assert.AreEqual("Quiebre", b.Events.OfType<AttackCanceled>().Single().Reason);
        }
    }
}
