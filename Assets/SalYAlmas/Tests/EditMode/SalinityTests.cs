using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 7.4: Salinidad de 0 a 150 con cinco bandas.</summary>
    public class SalinityTests
    {
        private static Battle SoloBattle(double enemySpeed = 50)
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100), Witch("b", Planet.Luna, Planet.Saturno, 90) },
                        new[] { Enemy("e", enemySpeed, 1000000, 10000000, Planet.Venus, Planet.Mercurio) });
            return Start(db, new[] { "a", "b" });
        }

        [TestCase(0, SalinityBand.Estable)]
        [TestCase(39, SalinityBand.Estable)]
        [TestCase(40, SalinityBand.Salobre)]
        [TestCase(69, SalinityBand.Salobre)]
        [TestCase(70, SalinityBand.Sobresalada)]
        [TestCase(99, SalinityBand.Sobresalada)]
        [TestCase(100, SalinityBand.CristalizacionTurbia)]
        [TestCase(149, SalinityBand.CristalizacionTurbia)]
        [TestCase(150, SalinityBand.ExtasisAbisal)]
        public void LimitesDeLasBandas(int value, SalinityBand band)
        {
            Assert.AreEqual(band, SalinityBands.FromValue(value, new SalinityRules()));
        }

        [Test]
        public void Estable_SinEfecto()
        {
            var b = SoloBattle();
            var a = W(b, "a");
            a.Salinity = 20;
            Assert.AreEqual(0.0, b.CritRate(a), 1e-9);
            Assert.AreEqual(0.0, b.DamageBonus(a, true), 1e-9);
            Assert.AreEqual(0.0, b.DefenseReduction(a), 1e-9);
        }

        [Test]
        public void Salobre_Mas10PorCientoDeCritico()
        {
            var b = SoloBattle();
            var a = W(b, "a");
            a.Salinity = 40;
            Assert.AreEqual(0.10, b.CritRate(a), 1e-9);
            Assert.AreEqual(0.0, b.DamageBonus(a, false), 1e-9);
        }

        [Test]
        public void Sobresalada_Mas20DeDanoYMenos15DeDefensa()
        {
            var b = SoloBattle();
            var a = W(b, "a");
            a.Salinity = 70;
            Assert.AreEqual(0.20, b.DamageBonus(a, false), 1e-9);
            Assert.AreEqual(0.15, b.DefenseReduction(a), 1e-9);
            Assert.AreEqual(0.0, b.CritRate(a), 1e-9);
        }

        [Test]
        public void CristalizacionTurbia_Mas40DelAscendente_YPierde5PorCientoPorTurno()
        {
            var b = SoloBattle();
            var a = W(b, "a");
            var w2 = W(b, "b");
            a.Salinity = 100;
            Assert.AreEqual(0.40, b.DamageBonus(a, byAscendant: true), 1e-9, "Hechizo, Cadena, Ascensión");
            Assert.AreEqual(0.0, b.DamageBonus(a, byAscendant: false), 1e-9, "el ataque básico es de la bruja, no del Ascendente");

            w2.Salinity = 100;
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual("b", b.ActiveUnit.Key);
            var loss = EventsSince<DotTicked>(b, mark).Single(d => d.UnitId == w2.Id);
            Assert.AreEqual("Cristalización Turbia", loss.StatusName);
            Assert.AreEqual(500, loss.Damage, "5 % de 10 000");
            Assert.AreEqual(9500, w2.Hp);
        }

        [Test]
        public void CristalizacionTurbia_NoMata()
        {
            var b = SoloBattle();
            var w2 = W(b, "b");
            w2.Salinity = 120;
            w2.Hp = 100;
            Ok(b.Execute(BattleCommand.Basic(W(b, "a").Id, -1)));
            Assert.AreEqual(1, w2.Hp, "// TUNING: la Cristalización deja 1 de vida");
            Assert.IsTrue(w2.IsAlive);
        }

        [Test]
        public void ExtasisAbisal_DosTurnosDelAscendenteConDanoDoble_LuegoBajaA60YQuedaDesalada()
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { Enemy("e", 50, 1000000, 10000000, Planet.Venus, Planet.Mercurio) });
            var b = Start(db, new[] { "a" });
            var a = W(b, "a");
            a.Salinity = 145;

            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(150, a.Salinity);
            Assert.IsTrue(a.InEcstasy);
            Assert.AreEqual(1, b.Events.OfType<EcstasyStarted>().Count());

            int autoTurns = 0;
            for (int guard = 0; guard < 20 && a.InEcstasy; guard++)
            {
                if (b.Pending == DecisionKind.AutoTurn && b.ActiveUnit == a) autoTurns++;
                Assert.AreNotEqual(DecisionKind.AllyTurn, b.Pending, "en Éxtasis la bruja no obedece");
                Ok(b.Execute(BattleCommand.Advance()));
            }
            Assert.AreEqual(2, autoTurns, "el Ascendente toma el control dos turnos");

            var ecstasyHits = b.Events.OfType<HitLanded>().Where(h => h.Source == HitSource.Ecstasy).ToList();
            Assert.AreEqual(2, ecstasyHits.Count);
            // Cierre: M_hab 1.0 ×2. Dominante 1000·2·0.952·0.5 = 952; secundario 1000·2·0.15·0.5 = 150.
            Assert.AreEqual(1102, ecstasyHits[0].Damage);

            Assert.AreEqual(60, a.Salinity, "después la Salinidad baja a 60");
            Assert.IsTrue(a.IsDesalada);
            TestKit.AdvanceToAlly(b);
            Assert.AreEqual(a, b.ActiveUnit);
            Assert.IsFalse(b.Execute(BattleCommand.Spell(a.Id, -1)).Ok, "Desalada: sin magia ese turno");
            Assert.IsFalse(b.Execute(BattleCommand.Ascension(a.Id, 1, -1)).Ok);
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.IsFalse(a.IsDesalada, "Desalada dura un turno");
            Assert.AreEqual(1, b.Events.OfType<EcstasyEnded>().Count());
        }

        [Test]
        public void ElExtasisNuncaDanaALasAliadas()
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100), Witch("b", Planet.Luna, Planet.Sol, 60) },
                        new[] { Enemy("e", 50, 1000000, 10000000, Planet.Venus, Planet.Mercurio) });
            var b = Start(db, new[] { "a", "b" });
            var a = W(b, "a");
            a.Salinity = 149;
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            for (int i = 0; i < 10 && !b.IsOver; i++)
            {
                if (b.Pending == DecisionKind.AllyTurn) Ok(b.Execute(BattleCommand.Basic(b.ActiveUnit.Id, -1)));
                else Ok(b.Execute(BattleCommand.Advance()));
            }
            Assert.IsTrue(b.Events.OfType<HitLanded>().Where(h => h.Source == HitSource.Ecstasy).All(h => h.TargetId >= Battle.FirstEnemyId));
            Assert.AreEqual(W(b, "b").MaxHp, W(b, "b").Hp);
        }

        [Test]
        public void CambiosPorAccion_HechizoSuma10_BasicoResta5MasTurnoSinMagia10()
        {
            var b = SoloBattle(1);
            var a = W(b, "a");
            a.Salinity = 50;
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(60, a.Salinity, "Hechizo +10");
            TurnOf(b, "a");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual(45, a.Salinity, "básico −5 y turno sin magia −10");
        }

        [Test]
        public void CambioDeBanda_EmiteEvento()
        {
            var b = SoloBattle();
            var a = W(b, "a");
            a.Salinity = 35;
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            var change = EventsSince<SalinityBandChanged>(b, mark).Single();
            Assert.AreEqual(SalinityBand.Estable, change.OldBand);
            Assert.AreEqual(SalinityBand.Salobre, change.NewBand);
            Assert.AreEqual(45, change.Value);
        }
    }
}
