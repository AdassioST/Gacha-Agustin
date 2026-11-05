using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 8.2 y 9.7: Autohechizo, Vislumbre, Ánima y Ascensión.</summary>
    public class AscendantTests
    {
        private static Battle Solo(SelfCastForm form, out WitchUnit a, double enemySpeed = 50, int blackSalt = 2)
        {
            var w = Witch("a", Planet.Marte, Planet.Saturno, 100);
            w.SelfCast = form;
            var db = Db(new[] { w, Witch("b", Planet.Luna, Planet.Sol, 60) }, new[] { Enemy("e", enemySpeed, 1000000, 10000000, Planet.Sol, Planet.Luna) });
            var b = Start(db, new[] { "a", "b" }, configure: s => s.BlackSalt = blackSalt);
            a = W(b, "a");
            return b;
        }

        [Test]
        public void AgujaYSello_Mas30DeAtaqueDosTurnos_Cuesta2GranosY25DeSalinidad()
        {
            var b = Solo(SelfCastForm.AgujaYSello, out var a);
            double atk = b.Atk(a);
            Ok(b.Execute(BattleCommand.SelfCast(a.Id)));
            Assert.AreEqual(1, b.Grains);
            Assert.AreEqual(25, a.Salinity);
            Assert.AreEqual(atk * 1.3, b.Atk(a), 1e-9);
            Assert.AreEqual(a, b.ActiveUnit, "acción libre: no termina el turno");
            Assert.IsFalse(b.Execute(BattleCommand.SelfCast(a.Id)).Ok, "una vez por turno");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            TurnOf(b, "a");
            Assert.AreEqual(atk * 1.3, b.Atk(a), 1e-9, "dura su segundo turno");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual(atk, b.Atk(a), 1e-9, "y vence al final de ese turno");
        }

        [Test]
        public void Hilos_ActuaDeNuevoACambioDel10PorCientoDeSuVida()
        {
            var b = Solo(SelfCastForm.Hilos, out var a);
            Ok(b.Execute(BattleCommand.SelfCast(a.Id)));
            Assert.AreEqual(9000, a.Hp);
            Assert.AreEqual(25, a.Salinity);
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            var turn = EventsSince<TurnStarted>(b, mark).First();
            Assert.AreEqual(a.Id, turn.UnitId);
            Assert.IsTrue(turn.IsExtra, "actúa de nuevo de inmediato");
            Assert.AreEqual(a, b.ActiveUnit);
            Assert.IsFalse(b.Execute(BattleCommand.SelfCast(a.Id)).Ok, "los Hilos no se encadenan");
        }

        [Test]
        public void FrenesiDeSalNegra_Mas100DeDanoMenos50DeDefensa_Mas40DeSalinidad()
        {
            var b = Solo(SelfCastForm.FrenesiSalNegra, out var a);
            Ok(b.Execute(BattleCommand.SelfCast(a.Id)));
            Assert.AreEqual(1, b.BlackSalt, "traga una Sal Negra");
            Assert.AreEqual(40, a.Salinity);
            Assert.AreEqual(1.0, b.DamageBonus(a, false), 1e-9);
            Assert.AreEqual(0.5, b.DefenseReduction(a), 1e-9);
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(2204, EventsSince<HitLanded>(b, mark).Single().Damage, "1102 · 2");
            Assert.AreEqual(0.0, b.DamageBonus(a, false) - 0.0, 1e-9, "el daño dura un turno");
            Assert.AreEqual(0.5, b.DefenseReduction(a), 1e-9, "el precio sigue durante los turnos enemigos");
        }

        [Test]
        public void LasAprendizasNoTienenAutohechizo()
        {
            var b = Solo(SelfCastForm.None, out var a);
            Assert.IsFalse(b.Execute(BattleCommand.SelfCast(a.Id)).Ok);
        }

        [Test]
        public void Vislumbre_DosTurnosConElGradoSiguiente_UnaVezPorCombate()
        {
            var b = Solo(SelfCastForm.None, out var a);
            Assert.AreEqual(0.85, b.Purity(a), 1e-9);
            double atk = b.Atk(a);
            Ok(b.Execute(BattleCommand.Vislumbre(a.Id)));
            Assert.AreEqual(1, b.BlackSalt);
            Assert.AreEqual(30, a.Salinity);
            Assert.AreEqual(Grade.A, a.EffectiveGrade);
            Assert.AreEqual(0.92, b.Purity(a), 1e-9);
            Assert.AreEqual(atk * 1.16 / 1.10, b.Atk(a), 1e-9);
            Assert.IsFalse(b.Execute(BattleCommand.Vislumbre(a.Id)).Ok, "una vez por combate");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            TurnOf(b, "a");
            Assert.IsTrue(a.InVislumbre);
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.IsFalse(a.InVislumbre, "dura dos turnos");
            Assert.AreEqual(0.85, b.Purity(a), 1e-9);
        }

        [Test]
        public void Vislumbre_CuestaSalNegra_ElFiltroDePurezaLoHaceGratis()
        {
            var b = Solo(SelfCastForm.None, out var a, blackSalt: 0);
            Assert.IsFalse(b.Execute(BattleCommand.Vislumbre(a.Id)).Ok);
            b.ApplyEffect(new EffectDef { Type = EffectType.FreeVislumbre }, a, a, "Filtro de Pureza");
            Ok(b.Execute(BattleCommand.Vislumbre(a.Id)));
            Assert.AreEqual(0, b.BlackSalt);
        }

        [Test]
        public void Ascension_CuestaYGasta100_200o300DeAnima_SinTerminarElTurno()
        {
            var b = Solo(SelfCastForm.None, out var a);
            Assert.IsFalse(b.Execute(BattleCommand.Ascension(a.Id, 1, -1)).Ok, "sin Ánima no hay Ascensión");
            a.Anima = 300;
            Ok(b.Execute(BattleCommand.Ascension(a.Id, 1, -1)));
            Assert.AreEqual(200, a.Anima);
            Ok(b.Execute(BattleCommand.Ascension(a.Id, 2, -1)));
            Assert.AreEqual(0, a.Anima);
            Assert.AreEqual(a, b.ActiveUnit, "la Ascensión no gasta el turno");
            Assert.AreEqual(2, b.Events.OfType<AscensionCast>().Count());
        }

        [Test]
        public void AscensionIII_RequiereGradoS()
        {
            var b = Solo(SelfCastForm.None, out var a);
            a.Anima = 300;
            Assert.IsFalse(b.Execute(BattleCommand.Ascension(a.Id, 3, -1)).Ok, "en grado B no");

            var w = Witch("s", Planet.Sol, Planet.Luna, 100);
            var db = Db(new[] { w }, new[] { Enemy("e", 50) });
            var bs = Start(db, new[] { "s" }, configure: s => s.GradeOverrides["s"] = Grade.S);
            W(bs, "s").Anima = 300;
            Ok(bs.Execute(BattleCommand.Ascension(W(bs, "s").Id, 3, -1)));
            Assert.AreEqual(3, bs.Events.OfType<AscensionCast>().Single().Level);
        }

        [Test]
        public void AnimaTope300_YSeLlenaAlActuarYAlRecibirDano()
        {
            var e = Enemy("e", 200, 1000000, 10000000, Planet.Sol, Planet.Luna);
            e.Skills.Add(AttackSkill("golpe", 1.0));
            e.Pattern.Clear();
            e.Pattern.Add("golpe");
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { e });
            var b = Start(db, new[] { "a" });
            var a = W(b, "a");
            AdvanceToAlly(b);
            Assert.AreEqual(8, a.Anima, "+8 al recibir un golpe (TUNING)");
            int before = a.Anima;
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            Assert.AreEqual(before + 20, a.Anima);
            a.Anima = 295;
            AdvanceToAlly(b);
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(300, a.Anima, "tope de 300");
        }

        [Test]
        public void LaAscensionInterrumpeElOrden_SePuedeLanzarEnElTurnoEnemigo()
        {
            var e = Enemy("e", 200, 1000000, 10000000, Planet.Sol, Planet.Luna);
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { e });
            var b = Start(db, new[] { "a" });
            Assert.AreEqual(DecisionKind.EnemyTurn, b.Pending);
            var a = W(b, "a");
            a.Anima = 100;
            Ok(b.Execute(BattleCommand.Ascension(a.Id, 1, -1)));
            Assert.AreEqual(DecisionKind.EnemyTurn, b.Pending, "sigue siendo el turno del enemigo");
        }

        [Test]
        public void AscensionesSeguidasEnLaMismaRonda_FormanAspecto_YSiSeEncimanHacenSpark()
        {
            var x = Witch("x", Planet.Sol, Planet.Luna, 100);
            var y = Witch("y", Planet.Saturno, Planet.Luna, 90);
            x.Ascensions[0] = Move("x_asc1", MoveKind.Ascension, 3.0, 30, 10, 30);
            y.Ascensions[0] = Move("y_asc1", MoveKind.Ascension, 3.0, 30, 12, 60);
            var db = Db(new[] { x, y }, new[] { Enemy("e", 10, 1000000, 10000000, Planet.Venus, Planet.Mercurio) });
            var b = Start(db, new[] { "x", "y" });
            W(b, "x").Anima = 100;
            W(b, "y").Anima = 100;
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Ascension(W(b, "x").Id, 1, -1)));
            Ok(b.Execute(BattleCommand.Ascension(W(b, "y").Id, 1, -1)));
            var asp = EventsSince<AspectFormed>(b, mark).Single();
            Assert.AreEqual("Ascensión", asp.Context);
            Assert.AreEqual(Aspect.Oposicion, asp.Aspect);
            Assert.AreEqual(HybridReaction.Eclipse, asp.Reaction);
            var yHits = EventsSince<HitLanded>(b, mark).Where(h => h.AttackerId == W(b, "y").Id).ToList();
            Assert.IsTrue(yHits[0].IsSpark, "frame 12 contra el 10 de x: encimadas");
            Assert.IsFalse(yHits[1].IsSpark);
        }
    }
}
