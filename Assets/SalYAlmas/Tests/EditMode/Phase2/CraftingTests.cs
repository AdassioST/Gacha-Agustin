using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Crafting (GDD 14 y 15): armas, recetas con estaciones, fusiones, reforja y Salmueras.</summary>
    public class CraftingTests
    {
        [Test]
        public void HayDoceFamiliasCincoNivelesYDosVariantes()
        {
            var meta = Meta();
            Assert.AreEqual(12, meta.Families.Count);
            Assert.AreEqual(120, meta.Weapons.Values.Count(w => !w.Legendary && w.Family != null && w.Tier <= 5 && meta.RecipeFor(w.Id) != null && meta.RecipeFor(w.Id).Generated));
            foreach (var f in meta.Families) Assert.IsNotEmpty(f.Hits, f.Id + " tiene su patrón de frames");
            var patterns = meta.Families.Select(f => string.Join(",", f.Hits.Select(h => h.Frame))).Distinct().Count();
            Assert.Greater(patterns, 6, "las familias se sienten distintas");
        }

        [Test]
        public void UnaRecetaPideEstacionesRecursosYArmas()
        {
            var g = NewGame();
            g.Profile.Stations.Clear();
            var check = g.Forge.Check("fusion_pendulo");
            Assert.AreEqual(2, check.MissingStations.Count, "pide Altar y Yunque a la vez");
            Assert.IsFalse(check.CanCraft);
            g.Profile.Stations.Add("altar");
            Assert.AreEqual(1, g.Forge.Check("fusion_pendulo").MissingStations.Count, "con una sola no alcanza");
            Assert.IsFalse(g.Forge.Craft("fusion_pendulo", out _).Ok);
            g.Profile.Stations.Add("yunque");
            Assert.IsTrue(g.Forge.Check("fusion_pendulo").CanCraft, g.Forge.Check("fusion_pendulo").Why());
        }

        [Test]
        public void LaRecetaDesconocidaNoSePuedeHacer()
        {
            var g = NewGame();
            g.Profile.Recipes.Remove("fusion_pendulo");
            Assert.IsFalse(g.Forge.Craft("fusion_pendulo", out _).Ok);
        }

        [Test]
        public void LaFusionDelGddConsumeSusIngredientes()
        {
            var g = NewGame();
            int weapons = g.Profile.Weapons.Count;
            long almitas = g.Ledger.Balance(Res.Almitas);
            long espectro = g.Ledger.Balance("mat.almita_espectro");
            Assert.IsTrue(g.Forge.Craft("fusion_pendulo", out var pendulo).Ok);
            Assert.AreEqual("pendulo_resonancia", pendulo.DefId);
            Assert.IsFalse(g.Profile.Weapons.Any(w => w.DefId == "daga_1_a"), "se consume la Daga de Sal Oxidada");
            Assert.IsFalse(g.Profile.Weapons.Any(w => w.DefId == "campana_2_b"), "se consume la Campana de Bronce");
            Assert.AreEqual(weapons - 1, g.Profile.Weapons.Count);
            Assert.AreEqual(espectro - 1, g.Ledger.Balance("mat.almita_espectro"), "y una Almita de Espectro");
            Assert.AreEqual(almitas - 15000, g.Ledger.Balance(Res.Almitas));
            Assert.AreEqual(15000, g.Ledger.TotalOut(Res.Almitas, "fusion"));
        }

        [Test]
        public void ElRelojDelEclipsePideElPenduloYCuatroArmasAstrales()
        {
            var g = NewGame();
            Assert.IsFalse(g.Forge.Craft("fusion_reloj", out _).Ok, "sin ingredientes");
            g.Forge.Craft("fusion_pendulo", out _);
            Assert.AreEqual(4, g.Forge.Check("fusion_reloj").MissingWeapons.Count);
            var astral = new[] { Planet.Sol, Planet.Luna, Planet.Marte, Planet.Saturno };
            foreach (var p in astral)
                g.Forge.Grant(g.Meta.Weapons.Values.First(w => w.Tier == 4 && w.Planet == p && !w.Legendary).Id);
            Assert.IsTrue(g.Forge.Craft("fusion_reloj", out var reloj).Ok, g.Forge.Check("fusion_reloj").Why());
            Assert.AreEqual("reloj_eclipse", reloj.DefId);
            Assert.IsTrue(g.Meta.Weapon(reloj.DefId).Legendary);
            Assert.IsFalse(g.Profile.Weapons.Any(w => g.Meta.Weapon(w.DefId).Tier == 4));
        }

        [Test]
        public void UnArmaEquipadaNoSeConsume()
        {
            var g = NewGame();
            var daga = g.Profile.Weapons.First(w => w.DefId == "daga_1_a");
            g.Forge.Equip("marisal", daga.Uid);
            Assert.IsFalse(g.Forge.Check("fusion_pendulo").CanCraft);
            g.Forge.Unequip("marisal");
            Assert.IsTrue(g.Forge.Check("fusion_pendulo").CanCraft);
        }

        [Test]
        public void ElAltarRefinaSalYCobra()
        {
            var g = NewGame();
            long cruda = g.Ledger.Balance(Res.SalCruda(Planet.Sol)), dest = g.Ledger.Balance(Res.SalDestilada(Planet.Sol));
            Assert.IsTrue(g.Forge.Refine(Planet.Sol, 1, 2).Ok);
            Assert.AreEqual(cruda - 10, g.Ledger.Balance(Res.SalCruda(Planet.Sol)));
            Assert.AreEqual(dest + 2, g.Ledger.Balance(Res.SalDestilada(Planet.Sol)));
            g.Profile.Stations.Remove("altar");
            Assert.IsFalse(g.Forge.Refine(Planet.Sol, 1).Ok);
        }

        // ------------------------------------------------------------------ Reforja y Spark

        [Test]
        public void HayUnosVeinticincoPrefijosConLosDelGdd()
        {
            var meta = Meta();
            Assert.That(meta.Prefixes.Count, Is.InRange(25, 26));
            foreach (var id in new[] { "turbio", "salino", "astral", "desquiciado", "ritmico", "pausado" }) Assert.IsNotNull(meta.Prefix(id), id);
            Assert.AreEqual(-2, meta.Prefix("ritmico").Mods.Get(ModKeys.FrameShift), 1e-9);
            Assert.AreEqual(2, meta.Prefix("pausado").Mods.Get(ModKeys.FrameShift), 1e-9);
        }

        [Test]
        public void ReforjarCobraYSoloCambiaElPrefijo()
        {
            var g = NewGame();
            var w = g.Profile.Weapons[0];
            string def = w.DefId;
            var cost = g.Forge.ReforgeCost(w.Uid);
            long almitas = g.Ledger.Balance(Res.Almitas);
            Assert.AreEqual("", w.Prefix, "recién fabricada no tiene prefijo");
            Assert.IsTrue(g.Forge.Reforge(w.Uid).Ok);
            Assert.AreNotEqual("", w.Prefix);
            Assert.AreEqual(def, w.DefId);
            Assert.AreEqual(almitas - cost.Get(Res.Almitas), g.Ledger.Balance(Res.Almitas));
            Assert.AreEqual(cost.Get(Res.Almitas), g.Ledger.TotalOut(Res.Almitas, "reforja"));
        }

        [Test]
        public void LaReforjaRespetaElPesoDeCadaPrefijo()
        {
            var g = NewGame();
            var counts = new Dictionary<string, int>();
            var rng = new SeededRandom(1);
            for (int i = 0; i < 20000; i++)
            {
                var id = g.Forge.RollPrefix(rng).Id;
                counts[id] = (counts.TryGetValue(id, out var v) ? v : 0) + 1;
            }
            Assert.AreEqual(g.Meta.Prefixes.Count, counts.Count, "salen todos");
            double total = g.Meta.Prefixes.Sum(p => p.Weight);
            foreach (var p in g.Meta.Prefixes) Assert.AreEqual(p.Weight / total, counts[p.Id] / 20000.0, 0.01, p.Id);
        }

        private static Battle ChainBattle(MetaGame g, string prefix)
        {
            string[] team = { "marisal", "selenia", "solenne", "tormentina" };
            var uid = g.Forge.Grant("daga_1_a", prefix).Uid;
            g.Forge.Equip("marisal", uid);
            var setup = g.Progress.BuildSetup(team, 1);
            setup.EncounterId = "guardian";
            var b = new Battle(g.Combat, setup);
            b.Start();
            return b;
        }

        private static List<string> SparkTable(Battle b)
        {
            var ids = b.Witches.Select(w => w.Id).ToList();
            var rows = new List<string>();
            foreach (var perm in AutoPilot.Permutations(ids, 3))
            {
                var p = b.PreviewChain(perm, false);
                if (p.IsValid) rows.Add(string.Join(">", perm) + "=" + p.SparkCount);
            }
            return rows;
        }

        [TestCase("ritmico", -2)]
        [TestCase("pausado", 2)]
        public void LosPrefijosDeRitmoMuevenLosFramesYElSpark(string prefix, int shift)
        {
            var plain = ChainBattle(NewGame(), "");
            var moved = ChainBattle(NewGame(), prefix);
            var marisal = moved.Witches.First(w => w.Key == "marisal").Id;
            var ids = moved.Witches.Select(w => w.Id).ToList();
            var order = new[] { marisal }.Concat(ids.Where(i => i != marisal)).Take(3).ToList();
            var a = plain.PreviewChain(order, false);
            var b = moved.PreviewChain(order, false);
            Assert.IsTrue(a.IsValid && b.IsValid);
            var fa = a.Hits.Where(h => h.OwnerId == marisal).Select(h => h.Frame).ToList();
            var fb = b.Hits.Where(h => h.OwnerId == marisal).Select(h => h.Frame).ToList();
            Assert.AreEqual(fa.Count, fb.Count);
            for (int i = 0; i < fa.Count; i++) Assert.AreEqual(fa[i] + shift, fb[i], "golpe " + i);

            CollectionAssert.AreNotEqual(SparkTable(plain), SparkTable(moved), "algún orden de Cadena gana o pierde Spark");
        }

        [Test]
        public void CambiarElArmaCambiaLaCadena()
        {
            var plain = ChainBattle(NewGame(), "");
            var g = NewGame();
            string[] team = { "marisal", "selenia", "solenne", "tormentina" };
            g.Forge.Equip("marisal", g.Forge.Grant("campana_2_b").Uid);
            var setup = g.Progress.BuildSetup(team, 1);
            setup.EncounterId = "guardian";
            var other = new Battle(g.Combat, setup);
            other.Start();
            var ids = plain.Witches.Select(w => w.Id).ToList();
            var ax = plain.PreviewChain(ids.Take(3).ToList(), false);
            var bx = other.PreviewChain(ids.Take(3).ToList(), false);
            Assert.AreNotEqual(string.Join(",", ax.Hits.Select(h => h.Frame)), string.Join(",", bx.Hits.Select(h => h.Frame)));
        }

        // ------------------------------------------------------------------ Salmueras

        [TestCase(0, "salmuera_aurea")]
        [TestCase(1, "salmuera_aurea@")]
        [TestCase(2, "salmuera_aurea@")]
        public void LaPurezaFijaLaPotencia(int purity, string idStart)
        {
            var g = NewGame();
            var r = g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, purity), "ambar_savia");
            Assert.IsTrue(r.Ok, r.Message);
            Assert.IsFalse(r.Turbid);
            StringAssert.StartsWith(idStart, r.PotionId);
            Assert.AreEqual(g.Meta.Brewing.Potency[purity].Name, r.Potency);
            Assert.AreEqual(1, g.Profile.Potions[r.PotionId]);
        }

        [Test]
        public void HayTresPotenciasQueEscalanElEfecto()
        {
            var meta = Meta();
            Assert.AreEqual(3, meta.Brewing.Potency.Count);
            var strong = Cauldron.VariantDef(meta, "balsamo_cuprico@" + meta.Brewing.Potency[2].Name);
            Assert.IsNotNull(strong);
            Assert.Greater(strong.Effects[0].Value, meta.Combat.Potion("balsamo_cuprico").Effects[0].Value);
        }

        [Test]
        public void LaMezclaSinRecetaDaUnaSalmueraTurbia()
        {
            var g = NewGame();
            var r = g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Marte, 0), "ambar_savia");
            Assert.IsTrue(r.Ok, r.Message);
            Assert.IsTrue(r.Turbid);
            StringAssert.StartsWith(Cauldron.TurbidId + "@", r.PotionId);
            Assert.IsEmpty(g.Profile.Recetario, "la Turbia no descubre receta");
            var def = Cauldron.VariantDef(g.Meta, r.PotionId);
            Assert.IsNotNull(def);
            Assert.IsNotEmpty(def.Effects, "se puede beber en combate");
        }

        [Test]
        public void ElRecetarioAnotaLoNuevoUnaSolaVez()
        {
            var g = NewGame();
            Assert.IsTrue(g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 0), "ambar_savia").Discovered);
            Assert.IsFalse(g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 0), "ambar_savia").Discovered);
            Assert.Contains("salmuera_aurea", g.Profile.Recetario.ToList());
        }

        [Test]
        public void HayUnTopeDeCincoPorTipoYCobraSusIngredientes()
        {
            var g = NewGame();
            var cost = g.Cauldron.Cost("agua_termal", BrewSalt.Of(Planet.Sol, 0), "ambar_savia");
            long almitas = g.Ledger.Balance(Res.Almitas);
            for (int i = 0; i < 5; i++) Assert.IsTrue(g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 0), "ambar_savia").Ok);
            Assert.AreEqual(almitas - 5 * cost.Get(Res.Almitas), g.Ledger.Balance(Res.Almitas));
            Assert.IsFalse(g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 0), "ambar_savia").Ok, "GDD 14.3");
            Assert.AreEqual(almitas - 5 * cost.Get(Res.Almitas), g.Ledger.Balance(Res.Almitas), "el intento fallido no cobra");
        }

        [Test]
        public void UnaSalmueraPreparadaFuncionaEnCombate()
        {
            var g = NewGame();
            var brewed = g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 2), "ambar_savia");
            var setup = g.Progress.BuildSetup(new[] { "marisal", "selenia" }, 1);
            setup.EncounterId = "guardian";
            setup.Belt.Add(new BeltSlot(brewed.PotionId, 1));
            setup.CustomPotions = Cauldron.CustomPotionsFor(g.Meta, new[] { brewed.PotionId });
            var b = new Battle(g.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            Assert.IsTrue(b.IsOver);
        }
    }
}
