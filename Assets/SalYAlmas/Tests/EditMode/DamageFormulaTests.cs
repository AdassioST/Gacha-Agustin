using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>
    /// GDD 9.8: Daño = ATQ · M_hab · (1 + ΣB) · M_p · C · F_DEF · F_RES · V · S · A,
    /// F_DEF = (N_a + 20) / ((N_e + 20)(1 − R_DEF) + N_a + 20), F_RES = 1 − (RES_e − PEN).
    /// Los cinco casos están calculados a mano (el desarrollo está en cada comentario).
    /// </summary>
    public class DamageFormulaTests
    {
        private static DamageInput Base() => DamageInput.Neutral(1000, 1.0, 50, 50);

        [Test]
        public void Caso1_Neutral()
        {
            // F_DEF = 70 / (70·1 + 70) = 0.5 → 1000 · 1 · 1 · 1 · 1 · 0.5 · 1 = 500
            var i = Base();
            Assert.AreEqual(500.0, DamageFormula.Compute(i), 1e-9);
            Assert.AreEqual(500, DamageFormula.Round(DamageFormula.Compute(i)));
        }

        [Test]
        public void Caso2_TodoJunto_SobresaladaPurezaCriticoQuiebreSparkYConjuncion()
        {
            // Marisal (ATQ 1 840, nivel 52) con M_hab 2.4, Sobresalada (ΣB 0.2), parte dominante en B
            // (M_p = 0.85 · (1 + 0.8·0.15) = 0.952), crítico (C = 1.5), contra nivel 52 con RES 0.2,
            // quebrado (V 1.1), Spark (S 1.5) y Conjunción (A 1.25).
            // F_DEF = 72 / (72 + 72) = 0.5 · F_RES = 0.8
            // 1840 · 2.4 = 4416 · 1.2 = 5299.2 · 0.952 = 5044.8384 · 1.5 = 7567.2576 · 0.5 = 3783.6288
            // · 0.8 = 3026.90304 · 1.1 = 3329.593344 · 1.5 = 4994.390016 · 1.25 = 6242.98752 → 6 243
            var i = new DamageInput
            {
                Atk = 1840, SkillMultiplier = 2.4, BonusSum = 0.2,
                PurityMultiplier = PurityMath.DominantShare(0.85, 0.8, 0.7), CritMultiplier = 1.5,
                AttackerLevel = 52, TargetLevel = 52, DefenseReduction = 0, TargetRes = 0.2, Penetration = 0,
                Vulnerability = 1.1, Spark = 1.5, Aspect = 1.25,
            };
            Assert.AreEqual(6242.98752, DamageFormula.Compute(i), 1e-6);
            Assert.AreEqual(6243, DamageFormula.Round(DamageFormula.Compute(i)));
        }

        [Test]
        public void Caso3_ParteSecundariaConDefensaRotaYDebilidad()
        {
            // Parte secundaria en B: M_p = 1 − 0.85 = 0.15. ATQ 1 500, nivel 50 contra nivel 60 con
            // R_DEF 0.15 y RES −0.1 (débil): F_DEF = 70 / (80·0.85 + 70) = 70 / 138 = 0.50724637…
            // F_RES = 1 − (−0.1) = 1.1 → 1500 · 0.15 = 225 · 0.50724637 = 114.130435 · 1.1 = 125.543478 → 126
            var i = new DamageInput
            {
                Atk = 1500, SkillMultiplier = 1.0, BonusSum = 0, PurityMultiplier = PurityMath.SecondaryShare(0.85),
                CritMultiplier = 1, AttackerLevel = 50, TargetLevel = 60, DefenseReduction = 0.15, TargetRes = -0.1,
                Penetration = 0, Vulnerability = 1, Spark = 1, Aspect = 1,
            };
            Assert.AreEqual(125.5434783, DamageFormula.Compute(i), 1e-6);
            Assert.AreEqual(126, DamageFormula.Round(DamageFormula.Compute(i)));
        }

        [Test]
        public void Caso4_EnemigoContraBrujaConFrenesiYSobresalada()
        {
            // El Guardián (ATQ 1 500, nivel 52, Savia Negra ΣB 0.2) con Raíz Punzante (M_hab 1.7) contra una
            // bruja de nivel 50 en Frenesí (R_DEF 0.5) y Sobresalada (R_DEF 0.15): R_DEF = 0.65.
            // F_DEF = 72 / (70·0.35 + 72) = 72 / 96.5 = 0.74611399
            // 1500 · 1.7 = 2550 · 1.2 = 3060 · 0.74611399 = 2283.108808 → 2 283
            var i = new DamageInput
            {
                Atk = 1500, SkillMultiplier = 1.7, BonusSum = 0.2, PurityMultiplier = 1, CritMultiplier = 1,
                AttackerLevel = 52, TargetLevel = 50, DefenseReduction = 0.65, TargetRes = 0, Penetration = 0,
                Vulnerability = 1, Spark = 1, Aspect = 1,
            };
            Assert.AreEqual(2283.108808, DamageFormula.Compute(i), 1e-6);
            Assert.AreEqual(2283, DamageFormula.Round(DamageFormula.Compute(i)));
        }

        [Test]
        public void Caso5_EncantoYPenetracion()
        {
            // ATQ 1 200, M_hab 0.85, ΣB = 0.25 − 0.4 (Encanto) = −0.15, nivel 52 contra 50.
            // F_DEF = 72 / (70 + 72) = 0.50704225 · F_RES = 1 − (0.3 − 0.1) = 0.8
            // 1200 · 0.85 = 1020 · 0.85 = 867 · 0.50704225 = 439.605634 · 0.8 = 351.684507 → 352
            var i = new DamageInput
            {
                Atk = 1200, SkillMultiplier = 0.85, BonusSum = 0.25 - 0.4, PurityMultiplier = 1, CritMultiplier = 1,
                AttackerLevel = 52, TargetLevel = 50, DefenseReduction = 0, TargetRes = 0.3, Penetration = 0.1,
                Vulnerability = 1, Spark = 1, Aspect = 1,
            };
            Assert.AreEqual(351.684507, DamageFormula.Compute(i), 1e-6);
            Assert.AreEqual(352, DamageFormula.Round(DamageFormula.Compute(i)));
        }

        [TestCase(0.70, 1.0)]
        [TestCase(0.85, 1.12)]
        [TestCase(0.999, 1.2392)]
        public void MultiplicadorDePureza(double p, double expected)
        {
            // GDD 7.2: con p = 0.7 vale 1.0; con p = 0.999 vale 1.24.
            Assert.AreEqual(expected, PurityMath.DominantMultiplier(p, 0.8, 0.7), 1e-9);
        }

        [Test]
        public void RepartoDeDanoEntrePlanetas()
        {
            Assert.AreEqual(0.7, PurityMath.DominantShare(0.7, 0.8, 0.7), 1e-12, "D: 70/30 sin multiplicador");
            Assert.AreEqual(0.3, PurityMath.SecondaryShare(0.7), 1e-12);
            Assert.AreEqual(0.952, PurityMath.DominantShare(0.85, 0.8, 0.7), 1e-12);
            Assert.AreEqual(0.15, PurityMath.SecondaryShare(0.85), 1e-12);
        }

        [Test]
        public void EnCombate_ElGolpeUsaLaFormulaPorPlaneta()
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { Enemy("e", 50, 1000000, 10000000, Planet.Sol, Planet.Luna) });
            var b = Start(db, new[] { "a" });
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(W(b, "a").Id, -1)));
            var hit = EventsSince<HitLanded>(b, mark).Single();
            // Hechizo M_hab 2.0, ATQ 1 000, F_DEF 0.5: Marte 1000·2·0.952·0.5 = 952; Saturno 1000·2·0.15·0.5 = 150.
            Assert.AreEqual(952, hit.DominantDamage);
            Assert.AreEqual(150, hit.SecondaryDamage);
            Assert.AreEqual(1102, hit.Damage);
            Assert.AreEqual(Planet.Marte, hit.Planet);
        }

        [Test]
        public void EnCombate_ViaMenorCambiaElPlanetaDominante()
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100) }, new[] { Enemy("e", 50, 1000000, 10000000, Planet.Sol, Planet.Luna) });
            var b = Start(db, new[] { "a" }, configure: s => s.ViaOverrides["a"] = Via.Menor);
            Assert.AreEqual(Planet.Saturno, W(b, "a").Dominant);
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(W(b, "a").Id, -1)));
            Assert.AreEqual(Planet.Saturno, EventsSince<HitLanded>(b, mark).Single().Planet);
        }

        [Test]
        public void EnCombate_CriticoConAzarInyectado()
        {
            var w = Witch("a", Planet.Marte, Planet.Saturno, 100);
            w.CritRate = 0.5;
            var db = Db(new[] { w }, new[] { Enemy("e", 50, 1000000, 10000000, Planet.Sol, Planet.Luna) });
            var b = Start(db, new[] { "a" }, rng: new FixedRandom(0.1));
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(W(b, "a").Id, -1)));
            var hit = EventsSince<HitLanded>(b, mark).Single();
            Assert.IsTrue(hit.IsCrit);
            Assert.AreEqual(1653, hit.Damage, "1102 · 1.5 = 1653");
        }
    }
}
