using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 9.2: AV = 10 000 / Velocidad; avanzar y retrasar mueven un % de 10 000; ciclos de 150 y 100.</summary>
    public class TurnOrderTests
    {
        /// <summary>Juega n turnos (básicos para brujas, Avanzar para enemigos) y devuelve "clave@AV".</summary>
        private static List<string> PlayTurns(Battle b, int n)
        {
            var log = new List<string>();
            for (int i = 0; i < n; i++)
            {
                log.Add(b.ActiveUnit.Key + "@" + b.ElapsedAv.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                Ok(b.Execute(b.Pending == DecisionKind.AllyTurn ? BattleCommand.Basic(b.ActiveUnit.Id, -1) : BattleCommand.Advance()));
            }
            return log;
        }

        [Test]
        public void ElPrimeroEnActuarEsElMasRapido_AvEs10000EntreVelocidad()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 100), Witch("b", Planet.Luna, Planet.Sol, 125) }, new[] { Enemy("e", 80) });
            var b = Start(db, new[] { "a", "b" });
            Assert.AreEqual("b", b.ActiveUnit.Key);
            Assert.AreEqual(80.0, b.ElapsedAv, 1e-9, "10 000 / 125 = 80");
            Assert.AreEqual(100.0 - 80.0, b.ActionValue(W(b, "a")), 1e-9, "a llevaba 80 AV recorridos de 100");
            Assert.AreEqual(125.0 - 80.0, b.ActionValue(b.Enemies[0]), 1e-9);
        }

        [Test]
        public void SecuenciaPorAv_EmpateEntreBrujaYEnemigo_ActuaPrimeroLaBruja()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 200) }, new[] { Enemy("e", 100) });
            var b = Start(db, new[] { "a" });
            var log = PlayTurns(b, 6);
            CollectionAssert.AreEqual(new[] { "a@50", "a@100", "e@100", "a@150", "a@200", "e@200" }, log);
        }

        [Test]
        public void EmpateEntreBrujas_GanaLaMasRapida()
        {
            var db = Db(new[] { Witch("p", Planet.Sol, Planet.Luna, 100), Witch("q", Planet.Luna, Planet.Sol, 200) }, new[] { Enemy("e", 10) });
            var b = Start(db, new[] { "p", "q" });
            var log = PlayTurns(b, 3);
            // q actúa en 50 y vuelve a quedar a 50 AV; p también queda a 50: empate, gana la de mayor Velocidad.
            CollectionAssert.AreEqual(new[] { "q@50", "q@100", "p@100" }, log);
        }

        [Test]
        public void EmpateEntreBrujasIgualDeRapidas_GanaLaDeMenorPosicion()
        {
            var db = Db(new[] { Witch("x", Planet.Sol, Planet.Luna, 100), Witch("y", Planet.Luna, Planet.Sol, 100) }, new[] { Enemy("e", 10) });
            var b = Start(db, new[] { "y", "x" });
            var log = PlayTurns(b, 4);
            CollectionAssert.AreEqual(new[] { "y@100", "x@100", "y@200", "x@200" }, log);
        }

        [Test]
        public void AvanzarYRetrasar_MuevenUnPorcentajeDe10000()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 100) }, new[] { Enemy("e", 50) });
            var b = Start(db, new[] { "a" });
            var e = b.Enemies[0];
            // a actúa en AV 100; el enemigo (Vel. 50) ya recorrió 5 000 de 10 000.
            Assert.AreEqual(5000.0, e.Distance, 1e-9);
            b.ShiftAction(e, -0.20, "prueba");
            Assert.AreEqual(7000.0, e.Distance, 1e-9, "retrasar 20 % suma 2 000");
            Assert.AreEqual(140.0, b.ActionValue(e), 1e-9);
            b.ShiftAction(e, 0.50, "prueba");
            Assert.AreEqual(2000.0, e.Distance, 1e-9, "adelantar 50 % resta 5 000");
            b.ShiftAction(e, 0.50, "prueba");
            Assert.AreEqual(0.0, e.Distance, 1e-9, "nunca baja de cero");
        }

        [Test]
        public void AdelantarALaBrujaEnTurno_SeAplicaCuandoTermina()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 100) }, new[] { Enemy("e", 1) });
            var b = Start(db, new[] { "a" });
            var a = W(b, "a");
            b.ShiftAction(a, 0.30, "prueba");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual(170.0, b.ElapsedAv, 1e-9, "después de actuar en 100, vuelve a actuar 70 AV después");
        }

        [TestCase(0.0, 1)]
        [TestCase(150.0, 1)]
        [TestCase(150.5, 2)]
        [TestCase(250.0, 2)]
        [TestCase(250.5, 3)]
        [TestCase(1000.0, 10)]
        public void Ciclos_ElPrimeroDura150YLosDemas100(double elapsed, int cycle)
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna) }, new[] { Enemy("e") });
            var b = Start(db, new[] { "a" });
            Assert.AreEqual(cycle, b.CycleAt(elapsed));
        }

        [Test]
        public void ElCombateEmiteElInicioDeCadaCiclo()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 100) }, new[] { Enemy("e", 10) });
            var b = Start(db, new[] { "a" });
            PlayTurns(b, 2);
            var cycles = b.Events.OfType<CycleStarted>().ToList();
            Assert.AreEqual(2, cycles.Count, "a actúa en 100 (ciclo 1), 200 (ciclo 2) y 300 (ciclo 3)");
            Assert.AreEqual(2, cycles[0].Cycle);
            Assert.AreEqual(3, cycles[1].Cycle);
        }

        [Test]
        public void VistaPreviaDeLaLineaDeTurnos_NoCambiaElEstado()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 150), Witch("b", Planet.Luna, Planet.Sol, 90) }, new[] { Enemy("e", 120) });
            var b = Start(db, new[] { "a", "b" });
            int events = b.Events.Count;
            var preview = b.TurnOrderPreview(6);
            Assert.AreEqual(events, b.Events.Count);
            Assert.AreEqual(6, preview.Count);
            Assert.AreEqual(b.ActiveUnit, preview[0].Unit);
            for (int i = 1; i < preview.Count; i++) Assert.GreaterOrEqual(preview[i].AvFromNow, preview[i - 1].AvFromNow);
        }

        [Test]
        public void GranosDeSal_EmpiezanEn3_BasicoSumaHechizoResta_TopeDe5()
        {
            var db = Db(new[] { Witch("a", Planet.Sol, Planet.Luna, 100) }, new[] { Enemy("e", 1) });
            var b = Start(db, new[] { "a" });
            var a = W(b, "a");
            Assert.AreEqual(3, b.Grains);
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(2, b.Grains);
            for (int i = 0; i < 5; i++) Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual(5, b.Grains, "máximo 5");
            b.Grains = 0;
            Assert.IsFalse(b.Execute(BattleCommand.Spell(a.Id, -1)).Ok, "sin Granos no hay Hechizo");
        }
    }
}
