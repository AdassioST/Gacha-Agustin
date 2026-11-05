using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Gacha (GDD 12.2 y 16): tasas, pity, garantías, exclusión de duplicados y banners.</summary>
    public class GachaTests
    {
        private static MetaGame Fresh(ulong seed = 1)
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), seed, 300));
            g.Ledger.Apply(Res.SalesDeBano, 160 * 20000, "prueba");
            return g;
        }

        [Test]
        public void LaCurvaDeProbabilidadEsLaDelGdd()
        {
            var r = Meta().Gacha;
            Assert.AreEqual(0.006, r.Rate5(1), 1e-9);
            Assert.AreEqual(0.006, r.Rate5(59), 1e-9);
            Assert.AreEqual(0.076, r.Rate5(60), 1e-9);
            Assert.AreEqual(0.146, r.Rate5(61), 1e-9);
            Assert.AreEqual(1.0, r.Rate5(70), 1e-9);
        }

        [TestCase(2026UL)]
        [TestCase(7UL)]
        public void UnMillonDeTiradasCaeDentroDeUnaTiradaDelGdd(ulong seed)
        {
            var sim = GachaSimulation.Run(Meta().Gacha, 1000000, seed);
            Assert.AreEqual(53.0, sim.AveragePerFive, 1.0, "por 5★");
            Assert.AreEqual(66.0, sim.AveragePerFeatured, 1.0, "por la destacada");
            Assert.AreEqual(0.0189, sim.ConsolidatedRate, 0.001, "tasa consolidada");
            Assert.AreEqual(70, sim.MaxPullsForFive, "pity duro");
            Assert.LessOrEqual(sim.MaxPullsForFeatured, 140);
            Assert.LessOrEqual(sim.MaxGapFourStar, 10, "4★ cada 10");
            Assert.AreEqual(0.2, sim.LostFeatured / (double)sim.FiveStars, 0.01, "75/25 con garantía: pierde una de cada cinco 5★");
        }

        [Test]
        public void PerderEl75GarantizaLaSiguienteYElContadorNoExpira()
        {
            var r = Meta().Gacha;
            var pity = new GachaPity();
            var rng = new SeededRandom(3);
            bool lost = false;
            for (int i = 0; i < 200000; i++)
            {
                var roll = Termas.Roll(r, pity, rng, true);
                if (roll.Rarity != 5) continue;
                if (lost) Assert.IsTrue(roll.Featured, "tras perder, garantía");
                lost = roll.LostFeatured;
            }
        }

        [Test]
        public void ElContadorSeCompartePorTipoDeBanner()
        {
            var g = Fresh();
            g.Termas.Pull("limitado_marisal", 5, out _);
            int since = g.Termas.Pity("limitado_marisal").Since5;
            Assert.AreEqual(since, g.Termas.Pity("limitado_selenia").Since5, "se comparte entre limitados");
            Assert.AreEqual(0, g.Termas.Pity("estandar").Since5, "el estándar lleva el suyo");
            Assert.AreEqual(0, g.Termas.Pity("inicio").Since5, "HUECOS C-24: el de inicio lleva el suyo");
        }

        [Test]
        public void LaTiradaCobraSalesDeBanoYRespetaElSaldo()
        {
            var g = Fresh();
            long before = g.Ledger.Balance(Res.SalesDeBano);
            Assert.IsTrue(g.Termas.Pull("estandar", 10, out var results).Ok);
            Assert.AreEqual(10, results.Count);
            Assert.AreEqual(before - 1600, g.Ledger.Balance(Res.SalesDeBano));
            g.Ledger.Apply(Res.SalesDeBano, -g.Ledger.Balance(Res.SalesDeBano), "tirada");
            Assert.IsFalse(g.Termas.Pull("estandar", 1, out _).Ok);
        }

        // ------------------------------------------------------------------ Exclusión (GDD 12.2 y HUECOS C-23)

        [Test]
        public void UnaBrujaQueYaTienesNuncaSale()
        {
            var g = Fresh();
            for (int i = 0; i < 1500; i++)
            {
                int owned = g.Profile.Witches.Count;
                Assert.IsTrue(g.Termas.Pull(i % 2 == 0 ? "estandar" : "limitado_marisal", 1, out var r).Ok);
                if (r[0].Kind == "bruja") Assert.AreEqual(owned + 1, g.Profile.Witches.Count, r[0].WitchId + " salió repetida");
                else Assert.AreEqual(owned, g.Profile.Witches.Count);
            }
        }

        private static void OwnAll(MetaGame g, int rarity)
        {
            foreach (var id in g.Meta.RosterOrder) if (g.Meta.Roster[id].Rarity == rarity) g.AddWitch(id);
        }

        [Test]
        public void ConTodasLasBrujasElResultadoDaPolvoAstral()
        {
            var g = Fresh();
            OwnAll(g, 5);
            OwnAll(g, 4);
            OwnAll(g, 3);
            long dust = g.Ledger.Balance(Res.PolvoAstral);
            Assert.IsTrue(g.Termas.Pull("estandar", 200, out var results).Ok);
            Assert.IsTrue(results.All(r => r.Kind != "bruja"));
            var fives = results.Where(r => r.Rarity == 5).ToList();
            var fours = results.Where(r => r.Rarity == 4).ToList();
            Assert.IsNotEmpty(fives);
            Assert.IsTrue(fives.Concat(fours).All(r => r.Kind == "polvo"));
            Assert.AreEqual(dust + fives.Count * g.Meta.Gacha.Dust5 + fours.Count * g.Meta.Gacha.Dust4, g.Ledger.Balance(Res.PolvoAstral));
        }

        [Test]
        public void ConLasTresEstrellasCompletasDaRecetasYSalCruda()
        {
            var g = Fresh();
            OwnAll(g, 3);
            g.Profile.Recipes.Clear();
            Assert.IsTrue(g.Termas.Pull("estandar", 40, out var results).Ok);
            var threes = results.Where(r => r.Rarity == 3).ToList();
            Assert.IsNotEmpty(threes);
            Assert.IsTrue(threes.All(r => r.Kind == "receta" && r.RecipeId != null && r.SaltAmount > 0));
            foreach (var r in threes) Assert.IsTrue(g.Profile.Recipes.Contains(r.RecipeId));
            Assert.Greater(g.Ledger.TotalIn(Res.SalCruda(Planet.Sol), "gacha.sal") + g.Ledger.TotalIn(Res.SalCruda(Planet.Luna), "gacha.sal")
                + g.Ledger.TotalIn(Res.SalCruda(Planet.Marte), "gacha.sal") + g.Ledger.TotalIn(Res.SalCruda(Planet.Mercurio), "gacha.sal")
                + g.Ledger.TotalIn(Res.SalCruda(Planet.Venus), "gacha.sal") + g.Ledger.TotalIn(Res.SalCruda(Planet.Jupiter), "gacha.sal")
                + g.Ledger.TotalIn(Res.SalCruda(Planet.Saturno), "gacha.sal"), 0);
        }

        [Test]
        public void C23SiYaTienesTodoElEstandarNoPuedesPerderEl75()
        {
            var g = Fresh();
            foreach (var id in g.Termas.StandardFiveStars()) g.AddWitch(id);
            Assert.IsFalse(g.Profile.Owns("marisal"));
            for (int i = 0; i < 80 && !g.Profile.Owns("marisal"); i++)
            {
                g.Termas.Pull("limitado_marisal", 1, out var r);
                if (r[0].Rarity == 5)
                {
                    Assert.IsFalse(r[0].LostFeatured);
                    Assert.AreEqual("marisal", r[0].WitchId);
                }
            }
            Assert.IsTrue(g.Profile.Owns("marisal"), "el pity duro la entrega");
            Assert.IsFalse(g.Termas.Pity("limitado_marisal").Guaranteed);
        }

        [Test]
        public void C23SiYaTienesALaDestacadaLa5EstrellasSaleDelEstandar()
        {
            var g = Fresh();
            g.AddWitch("marisal");
            for (int i = 0; i < 70; i++)
            {
                g.Termas.Pull("limitado_marisal", 1, out var r);
                if (r[0].Rarity != 5) continue;
                Assert.IsFalse(r[0].Featured);
                Assert.IsFalse(r[0].LostFeatured, "no cuenta como pérdida");
                CollectionAssert.Contains(g.Termas.StandardFiveStars(), r[0].WitchId);
                Assert.IsFalse(g.Termas.Pity("limitado_marisal").Guaranteed, "no deja garantía");
                return;
            }
            Assert.Fail("El pity duro debía entregar una 5★.");
        }

        [Test]
        public void LasLimitadasNoSalenDelEstandar()
        {
            var g = Fresh();
            Assert.IsFalse(g.Termas.StandardFiveStars().Contains("marisal"));
            Assert.IsFalse(g.Termas.StandardFiveStars().Contains("selenia"));
            Assert.AreEqual(4, g.Termas.StandardFiveStars().Count);
        }

        // ------------------------------------------------------------------ Banners

        [Test]
        public void ElBannerDeInicioDaVeinteTiradasConLaRegenteElegida()
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), 12, 300));
            Assert.IsFalse(g.Termas.SetStarterChoice("marisal").Ok, "no está entre las cuatro");
            Assert.IsTrue(g.Termas.SetStarterChoice("cupria").Ok);
            Assert.IsTrue(g.Termas.Pull("inicio", 20, out var results).Ok, "alcanza con la bienvenida");
            Assert.IsTrue(g.Profile.Owns("cupria"), "garantizada");
            Assert.IsFalse(g.Termas.Pull("inicio", 1, out _).Ok, "solo veinte tiradas");
            Assert.AreEqual(results.Select(r => r.WitchId).Where(x => x != null).Count(), results.Select(r => r.WitchId).Where(x => x != null).Distinct().Count());
        }

        [Test]
        public void ElSelectorSeAbreALasTrescientasDelEstandar()
        {
            var g = Fresh();
            Assert.IsFalse(g.Termas.ClaimSelector("cervanda").Ok);
            g.Termas.Pull("estandar", 299, out _);
            Assert.IsFalse(g.Termas.SelectorAvailable);
            g.Termas.Pull("estandar", 1, out _);
            Assert.IsTrue(g.Termas.SelectorAvailable);
            var unowned = g.Termas.StandardFiveStars().Find(id => !g.Profile.Owns(id));
            if (unowned != null)
            {
                Assert.IsFalse(g.Termas.ClaimSelector("marisal").Ok, "solo del estándar");
                Assert.IsTrue(g.Termas.ClaimSelector(unowned).Ok);
                Assert.IsTrue(g.Profile.Owns(unowned));
                Assert.IsFalse(g.Termas.SelectorAvailable, "una sola vez");
            }
        }

        [Test]
        public void ElTermometroYElHistorialReflejanCadaTirada()
        {
            var g = Fresh(9);
            for (int i = 0; i < 30; i++) g.Termas.Pull("estandar", 1, out _);
            var pity = g.Termas.Pity("estandar");
            Assert.AreEqual(pity.Since5 / 70.0, g.Termas.Thermometer("estandar"), 1e-9);
            Assert.AreEqual(30, g.Profile.Gacha.History.Count);
            Assert.AreEqual(Enumerable.Range(1, 30).Select(x => (long)x), g.Profile.Gacha.History.Select(h => h.Seq));
            pity.Since5 = 59;
            Assert.IsTrue(g.Termas.Boiling("estandar"), "hierve desde el pity suave");
            pity.Since5 = 10;
            Assert.IsFalse(g.Termas.Boiling("estandar"));
        }

        [TestCase(5, "dorado")]
        [TestCase(4, "violeta")]
        [TestCase(3, "blanco")]
        public void ElVaporCambiaConLaRareza(int rarity, string vapor) => Assert.AreEqual(vapor, new PullResult { Rarity = rarity }.Vapor);

        [Test]
        public void ElGachaEsDeterministaConLaMismaSemilla()
        {
            var a = Fresh(44);
            var b = Fresh(44);
            a.Termas.Pull("estandar", 300, out _);
            b.Termas.Pull("estandar", 300, out _);
            Assert.AreEqual(a.ToJson(), b.ToJson());
        }
    }

    /// <summary>Economía y guardado (GDD 11 y 21): libro contable y carga sin pérdida.</summary>
    public class EconomyAndSaveTests
    {
        [Test]
        public void ElLibroRechazaFlujosNoDeclaradosYSaldosNegativos()
        {
            var g = NewGame();
            Assert.Throws<LedgerException>(() => g.Ledger.Apply(Res.Almitas, 10, "de_la_nada"));
            Assert.Throws<LedgerException>(() => g.Ledger.Apply(Res.Almitas, -10, "de_la_nada"));
            Assert.Throws<LedgerException>(() => g.Ledger.Apply("oro", 10, "prueba"));
            Assert.Throws<LedgerException>(() => g.Ledger.Apply(Res.Almitas, -(g.Ledger.Balance(Res.Almitas) + 1), "nivel"));
        }

        [Test]
        public void CadaMovimientoLlevaFuenteOSumideroYTrySpendEsTodoONada()
        {
            var g = NewGame();
            var e = g.Ledger.Apply(Res.Almitas, 100, "prueba", "nota");
            Assert.AreEqual(100, e.Delta);
            Assert.AreEqual("prueba", e.Flow);
            Assert.AreEqual(g.Ledger.Balance(Res.Almitas), e.Balance);

            long almitas = g.Ledger.Balance(Res.Almitas);
            var cost = new Bundle().Add(Res.Almitas, 10).Add(Res.Perlas, long.MaxValue / 2);
            Assert.IsFalse(g.Ledger.TrySpend(cost, "nivel"));
            Assert.AreEqual(almitas, g.Ledger.Balance(Res.Almitas));
        }

        [Test]
        public void TodoRecursoDeLaTabla11TieneFuenteYSumidero()
        {
            foreach (var r in Meta().Catalog.All())
            {
                Assert.IsNotEmpty(r.Sources, r.Id);
                Assert.IsNotEmpty(r.Sinks, r.Id);
            }
        }

        [Test]
        public void NingunRecursoDePoderSeVende()
        {
            var sold = Meta().Catalog.All().Where(r => r.Sold).Select(r => r.Id).ToList();
            CollectionAssert.AreEquivalent(new[] { Res.Perlas }, sold, "GDD 16.4: nunca se vende progreso");
        }

        [Test]
        public void ElLibroCuadraTrasTresRunsYTrabajoDeCrafting()
        {
            var g = NewGame(5);
            var climber = new AutoClimber(g);
            string[] team = { "marisal", "selenia", "solenne", "tormentina" };
            for (int i = 0; i < 3; i++)
            {
                climber.Climb(team, g.Profile.Fresno.Eco[0] != null ? 0 : -1, (ulong)(50 + i));
                foreach (var root in g.Meta.Fresno.Roots) g.Fresno.BuyRoot(root.Id);
            }
            g.Forge.Craft("fusion_pendulo", out _);
            g.Termas.Pull("estandar", 10, out _);
            g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 1), "ambar_savia");
            FresnoTests.AssertLedgerExplainsEverything(g);
        }

        private static MetaGame BusyGame()
        {
            var g = NewGame(13);
            string[] team = { "marisal", "selenia", "solenne", "tormentina" };
            new AutoClimber(g).Climb(team, seed: 9);
            g.Forge.Craft("fusion_pendulo", out var p);
            g.Forge.Reforge(p.Uid);
            g.Forge.Equip("marisal", p.Uid);
            g.Termas.Pull("estandar", 25, out _);
            g.Cauldron.Brew("agua_termal", BrewSalt.Of(Planet.Sol, 2), "ambar_savia");
            g.Progress.DignityUp("marisal");
            g.Progress.ChainTalentUp("marisal", ChainRole.Enlace);
            g.Fresno.Offer(1000);
            return g;
        }

        [Test]
        public void GuardarYCargarNoPierdeNada()
        {
            var g = BusyGame();
            var store = new MemorySaveStore();
            g.Save(store, "a");
            var loaded = MetaGame.Load(Meta(), store, "a");
            Assert.AreEqual(g.ToJson(), loaded.ToJson());
            Assert.AreEqual(g.Ledger.Balance(Res.Almitas), loaded.Ledger.Balance(Res.Almitas));
            Assert.AreEqual(g.Profile.Fresno.History.Count, loaded.Profile.Fresno.History.Count);
            Assert.AreEqual(g.Profile.Witches["marisal"].ChainTalents[1], loaded.Profile.Witches["marisal"].ChainTalents[1]);
            Assert.AreEqual(g.Profile.Gacha.History.Count, loaded.Profile.Gacha.History.Count);
            CollectionAssert.AreEquivalent(g.Profile.Recipes, loaded.Profile.Recipes);
        }

        [Test]
        public void ElAzarSigueIgualDespuesDeCargar()
        {
            var g = BusyGame();
            var store = new MemorySaveStore();
            g.Save(store, "a");
            var loaded = MetaGame.Load(Meta(), store, "a");
            g.Termas.Pull("estandar", 50, out var ra);
            loaded.Termas.Pull("estandar", 50, out var rb);
            Assert.AreEqual(ra.Select(r => r.WitchId + r.Rarity), rb.Select(r => r.WitchId + r.Rarity));
            Assert.AreEqual(g.ToJson(), loaded.ToJson());
        }

        [Test]
        public void ElArchivoGuardaYCargaEnDisco()
        {
            string dir = Path.Combine(Path.GetTempPath(), "salyalmas_" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = new FileSaveStore(dir);
                Assert.IsFalse(store.Exists("x"));
                Assert.IsNull(store.Load("x"));
                var g = BusyGame();
                g.Save(store, "x");
                g.Save(store, "x");
                Assert.IsTrue(store.Exists("x"));
                Assert.AreEqual(g.ToJson(), MetaGame.Load(Meta(), store, "x").ToJson());
                store.Delete("x");
                Assert.IsFalse(store.Exists("x"));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void UnGuardadoDeUnaVersionMasNuevaSeRechaza()
        {
            var profile = NewGame().Profile;
            profile.Version = PlayerProfile.CurrentVersion + 1;
            var json = ProfileSerializer.ToJson(profile);
            Assert.Throws<InvalidOperationException>(() => ProfileSerializer.FromJson(json));
        }
    }
}
