using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Progress;
using SalYAlmas.Core.Save;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Progreso de brujas (GDD 11.3 y 12): curvas, grados, Juicios, Vía, Dignidad y Talentos.</summary>
    public class ProgressTests
    {
        [TestCase(2, 283L)]
        [TestCase(10, 3162L)]
        [TestCase(40, 25298L)]
        [TestCase(80, 71554L)]
        public void LaCurvaDeNivelEsLaDelGdd(int n, long cost) => Assert.AreEqual(cost, Meta().Progression.LevelCost(n));

        [Test]
        public void SubirDeNivelCobraLaSumaDeLaCurva()
        {
            var g = NewGame();
            var wp = g.Progress.Get("topo");
            wp.Level = 10;
            long before = g.Ledger.Balance(Res.Almitas);
            long expected = Enumerable.Range(11, 5).Sum(n => Meta().Progression.LevelCost(n));
            Assert.AreEqual(expected, g.Progress.LevelUpCost("topo", 5));
            Assert.IsTrue(g.Progress.LevelUp("topo", 5).Ok);
            Assert.AreEqual(15, wp.Level);
            Assert.AreEqual(before - expected, g.Ledger.Balance(Res.Almitas));
            Assert.AreEqual(expected, g.Ledger.TotalOut(Res.Almitas, "nivel"));
        }

        [Test]
        public void ElNivelTopaEnOchenta()
        {
            var g = NewGame();
            var wp = g.Progress.Get("topo");
            wp.Level = 80;
            Assert.IsFalse(g.Progress.LevelUp("topo").Ok);
        }

        [Test]
        public void LaCurvaDeGradosEsLaDelGdd()
        {
            var steps = Meta().Progression.Grades;
            CollectionAssert.AreEqual(new[] { Grade.C, Grade.B, Grade.A, Grade.S, Grade.SS }, steps.Select(s => s.Grade));
            CollectionAssert.AreEqual(new long[] { 20000, 36000, 64800, 116640, 209952 }, steps.Select(s => s.Almitas));
            Assert.AreEqual(447392, steps.Sum(s => s.Almitas), "GDD 12.3: total");
            Assert.IsTrue(steps.All(s => s.SaltAmount > 0), "con sal refinada");
            Assert.IsTrue(steps.Zip(steps.Skip(1), (a, b) => a.MinLevel < b.MinLevel).All(x => x), "con nivel mínimo creciente");
        }

        // ------------------------------------------------------------------ Juicios

        private static readonly string[] Allies = { "marisal", "selenia", "solenne" };

        private static Battle Fight(MetaGame g, TrialPlan plan, AutoPilot pilot = null)
        {
            var b = new Battle(g.Combat, plan.Setup);
            b.Start();
            (pilot ?? new AutoPilot()).RunToEnd(b);
            return b;
        }

        [Test]
        public void ElAscendenteUsaElKitDeSuBruja()
        {
            var g = NewGame();
            g.Progress.Get("topo").Level = 40;
            Assert.IsTrue(g.Trials.Prepare("topo", Allies, 1, out var plan).Ok);
            var def = g.Combat.Witch("topo");
            Assert.IsTrue(plan.Ascendant.IsBoss);
            Assert.AreEqual(plan.Step.TrialLevel, plan.Ascendant.Level);
            Assert.IsTrue(plan.Ascendant.Skills.Any(s => s.Name == def.Closing.Name), "su propio Cierre");
            Assert.IsTrue(plan.Ascendant.Skills.Any(s => s.Name == def.Ascensions[0].Name && s.Charge), "su Ascensión, telegrafiada");
        }

        [Test]
        public void ElJuicioExigeNivelYPoderPagar()
        {
            var g = NewGame();
            g.Progress.Get("topo").Level = 10;
            Assert.IsFalse(g.Trials.Prepare("topo", Allies, 1, out _).Ok, "nivel mínimo");
            g.Progress.Get("topo").Level = 40;
            g.Ledger.Apply(Res.Almitas, -g.Ledger.Balance(Res.Almitas), "nivel");
            Assert.IsFalse(g.Trials.Prepare("topo", Allies, 1, out _).Ok, "almitas");
        }

        [Test]
        public void ElJuicioRechazaEquiposInvalidos()
        {
            var g = NewGame();
            g.Progress.Get("topo").Level = 40;
            Assert.IsFalse(g.Trials.Prepare("topo", new[] { "marisal", "marisal" }, 1, out _).Ok, "aliada repetida");
            Assert.IsFalse(g.Trials.Prepare("topo", new[] { "marisal", "selenia", "solenne", "tormentina" }, 1, out _).Ok, "más de tres aliadas");
            Assert.IsFalse(g.Trials.Prepare("cervanda", Allies, 1, out _).Ok, "sin kit de combate");
        }

        [Test]
        public void PerderUnJuicioNoCuestaNadaYSePuedeReintentar()
        {
            var g = NewGame();
            g.Progress.Get("topo").Level = 40;
            string before = string.Join(",", g.Ledger.Balances.OrderBy(k => k.Key).Select(k => k.Key + k.Value));
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(g.Trials.Prepare("topo", Allies, (ulong)i, out var plan).Ok);
                plan.Setup.Modifiers.Add(ModKeys.EnemyAtkPct, 100);
                var result = g.Trials.Resolve(plan, Fight(g, plan));
                Assert.IsFalse(result.Passed);
                Assert.IsFalse(result.GradeUp.Ok);
            }
            Assert.AreEqual(Grade.D, g.Progress.Get("topo").Grade);
            Assert.AreEqual(before, string.Join(",", g.Ledger.Balances.OrderBy(k => k.Key).Select(k => k.Key + k.Value)));
            Assert.AreEqual(3, g.Progress.Get("topo").TrialAttempts);
        }

        [Test]
        public void AceptacionUnaBrujaSubeDeDaBPorSusJuicios()
        {
            var g = NewGame();
            const string id = "zarigueya";
            var wp = g.Progress.Get(id);
            Assert.AreEqual(Grade.D, wp.Grade);
            long start = g.Ledger.Balance(Res.Almitas);

            Assert.IsTrue(g.Trials.Prepare(id, Allies, 1, out var c).Ok);
            Assert.AreEqual(Grade.C, c.Target);
            Assert.IsEmpty(c.Conditions, "el Juicio I no tiene condición");
            var rc = g.Trials.Resolve(c, Fight(g, c));
            Assert.IsTrue(rc.Passed);
            Assert.AreEqual(Grade.C, wp.Grade);
            Assert.AreEqual(1, wp.Memories.Count, "cada Juicio revela un recuerdo");

            wp.Level = 50;
            Assert.IsTrue(g.Trials.Prepare(id, Allies, 2, out var b, Via.Menor).Ok);
            Assert.AreEqual(Grade.B, b.Target);
            Assert.AreEqual(1, b.Conditions.Count, "el Juicio II tiene una condición");
            var rb = g.Trials.Resolve(b, Fight(g, b));
            Assert.IsTrue(rb.Passed, string.Join(" · ", rb.Conditions.Select(x => x.Detail)));
            Assert.AreEqual(Grade.B, wp.Grade);
            Assert.AreEqual(Via.Menor, wp.Via, "en B elige Vía");
            Assert.IsTrue(wp.ViaChosen);
            Assert.AreEqual(56000, start - g.Ledger.Balance(Res.Almitas), "20 000 + 36 000");
        }

        [Test]
        public void GanarSinCumplirLaCondicionNoSubeElGrado()
        {
            var g = NewGame();
            var wp = g.Progress.Get("topo");
            wp.Level = 50;
            wp.Grade = Grade.C;
            Assert.IsTrue(g.Trials.Prepare("topo", new[] { "marisal" }, 3, out var plan).Ok);
            plan.Conditions = new List<TrialConditionDef> { new TrialConditionDef { Type = TrialConditionType.MinChains, Value = 999, Text = "imposible" } };
            var r = g.Trials.Resolve(plan, Fight(g, plan));
            Assert.IsTrue(r.Victory);
            Assert.IsFalse(r.Passed);
            Assert.AreEqual(Grade.C, wp.Grade);
        }

        private static Battle ReferenceTrial(MetaGame g, AutoPilot pilot, out TrialPlan plan)
        {
            g.Progress.Get("topo").Level = 50;
            g.Progress.Get("topo").Grade = Grade.C;
            Assert.IsTrue(g.Trials.Prepare("topo", new[] { "marisal", "zarigueya" }, 8, out plan).Ok);
            return Fight(g, plan, pilot);
        }

        private static bool Eval(Battle b, TrialConditionType type, int value) =>
            TrialService.Evaluate(new[] { new TrialConditionDef { Type = type, Value = value, Text = "" } }, b, "topo", "Topo")[0].Passed;

        [Test]
        public void CadaCondicionDeJuicioSeEvaluaContraElCombate()
        {
            var g = NewGame();
            var b = ReferenceTrial(g, null, out _);
            int cycle = b.Cycle;
            Assert.IsTrue(Eval(b, TrialConditionType.MaxCycles, cycle));
            Assert.IsFalse(Eval(b, TrialConditionType.MaxCycles, cycle - 1));
            Assert.IsTrue(Eval(b, TrialConditionType.MinSparks, b.TotalSparks));
            Assert.IsFalse(Eval(b, TrialConditionType.MinSparks, b.TotalSparks + 1));
            Assert.IsTrue(Eval(b, TrialConditionType.MaxTeam, 3));
            Assert.IsFalse(Eval(b, TrialConditionType.MaxTeam, 2));
            Assert.IsFalse(Eval(b, TrialConditionType.AlliesRarity, 3), "Marisal es 5★");
            Assert.IsTrue(Eval(b, TrialConditionType.MinChains, 0));
            Assert.IsFalse(Eval(b, TrialConditionType.MinChains, 999));
            Assert.IsTrue(Eval(b, TrialConditionType.SalinityAbove, -1));
            Assert.IsFalse(Eval(b, TrialConditionType.SalinityAbove, 200));
        }

        [Test]
        public void LasCondicionesDeRenunciaDistinguenSiSeUsoElRecurso()
        {
            var g = NewGame();
            var clean = ReferenceTrial(g, new AutoPilot { UseAscensions = false, UsePotions = false }, out _);
            Assert.IsTrue(Eval(clean, TrialConditionType.NoAscensions, 0));
            Assert.IsTrue(Eval(clean, TrialConditionType.NoPotions, 0));

            var g2 = NewGame();
            var full = ReferenceTrial(g2, new AutoPilot(), out _);
            int asc = full.Events.OfType<Core.Events.AscensionCast>().Count();
            Assert.AreEqual(asc == 0, Eval(full, TrialConditionType.NoAscensions, 0));
            int falls = full.Events.OfType<Core.Events.UnitDefeated>().Count(d => d.Side == Side.Ally);
            Assert.AreEqual(falls == 0, Eval(full, TrialConditionType.NoFalls, 0));
        }

        [Test]
        public void LosCincoJuiciosTienenLasCondicionesDelGdd()
        {
            var rules = Meta().Progression;
            Assert.AreEqual(0, rules.ConditionsFor("marisal", Grade.C).Count);
            Assert.AreEqual(1, rules.ConditionsFor("marisal", Grade.B).Count);
            Assert.AreEqual(1, rules.ConditionsFor("marisal", Grade.A).Count);
            Assert.AreEqual(1, rules.ConditionsFor("marisal", Grade.S).Count);
            Assert.AreEqual(2, rules.ConditionsFor("marisal", Grade.SS).Count);
        }

        // ------------------------------------------------------------------ Vía, Dignidad y Talentos

        [Test]
        public void LaViaTieneUnCambioGratisYDespuesCuesta()
        {
            var g = NewGame();
            var wp = g.Progress.Get("marisal");
            wp.Via = Via.Mayor;
            long before = g.Ledger.Balance(Res.Almitas);
            Assert.IsFalse(g.Progress.ChangeVia("marisal", Via.Mayor).Ok, "ya la sigue");
            Assert.IsTrue(g.Progress.ChangeVia("marisal", Via.Menor).Ok);
            Assert.AreEqual(before, g.Ledger.Balance(Res.Almitas), "el primero es gratis");
            long cost = g.Progress.ViaChangeCost("marisal");
            Assert.Greater(cost, 0);
            Assert.IsTrue(g.Progress.ChangeVia("marisal", Via.Mayor).Ok);
            Assert.AreEqual(before - cost, g.Ledger.Balance(Res.Almitas));
            Assert.IsFalse(g.Progress.ChangeVia("topo", Via.Menor).Ok, "antes de B no hay Vía");
        }

        [Test]
        public void LaDignidadTieneCincoNivelesYSubeElDanoDelAscendente()
        {
            var g = NewGame();
            Assert.AreEqual(5, Meta().Progression.Dignity.Names.Count);
            Assert.AreEqual("Peregrina", g.Progress.DignityName("marisal"));
            for (int i = 0; i < 4; i++) Assert.IsTrue(g.Progress.DignityUp("marisal").Ok);
            Assert.AreEqual("Domicilio", g.Progress.DignityName("marisal"));
            Assert.IsFalse(g.Progress.DignityUp("marisal").Ok, "tope");
            Assert.AreEqual(0.16, g.Progress.BuildLoadout("marisal").Mods.Get(ModKeys.AscendantDamagePct), 1e-9);
        }

        [Test]
        public void LosTalentosDeCadenaVanDelUnoAlDiez()
        {
            var g = NewGame();
            var wp = g.Progress.Get("marisal");
            Assert.AreEqual(1, wp.ChainTalents[(int)ChainRole.Cierre]);
            var before = g.Progress.BuildLoadout("marisal").Mods.Get(ModKeys.ChainMove(ChainRole.Cierre));
            Assert.IsTrue(g.Progress.ChainTalentUp("marisal", ChainRole.Cierre).Ok);
            Assert.AreEqual(before + 0.04, g.Progress.BuildLoadout("marisal").Mods.Get(ModKeys.ChainMove(ChainRole.Cierre)), 1e-9);
            wp.ChainTalents[(int)ChainRole.Cierre] = 10;
            Assert.IsFalse(g.Progress.ChainTalentUp("marisal", ChainRole.Cierre).Ok, "tope 10");
        }

        [Test]
        public void SubirDeNivelHaceMasFuerteALaBrujaEnCombate()
        {
            var g = NewGame();
            int Atk() => (int)new Battle(g.Combat, g.Progress.BuildSetup(new[] { "topo" }, 1) .WithEncounter("guardian")).Witches[0].BaseAtk;
            int low = Atk();
            g.Progress.Get("topo").Level = 70;
            Assert.Greater(Atk(), low);
        }
    }

    internal static class SetupExtensions
    {
        public static BattleSetup WithEncounter(this BattleSetup s, string id) { s.EncounterId = id; return s; }
    }
}
