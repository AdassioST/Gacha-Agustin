using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Los JSON de la Fase 2 se cargan, se validan y respetan los números del GDD.</summary>
    public class MetaDataTests
    {
        [Test]
        public void LosDatosDeLaFase2CarganSinErrores()
        {
            var meta = Meta();
            Assert.AreEqual(24, meta.Roster.Count, "elenco de lanzamiento: 24 brujas (GDD 18)");
            Assert.AreEqual(6, meta.Roster.Values.Count(w => w.Rarity == 5));
            Assert.AreEqual(10, meta.Roster.Values.Count(w => w.Rarity == 4));
            Assert.AreEqual(8, meta.Roster.Values.Count(w => w.Rarity == 3));
            Assert.AreEqual(6, meta.Roster.Values.Count(w => w.HasKit), "las seis brujas con kit de la Fase 1");
        }

        [Test]
        public void TodosLosRecursosDe11_1TienenFuenteYSumidero()
        {
            var cat = Meta().Catalog;
            string[] families = { "almitas", "sal_cruda", "sal_destilada", "quintaesencia", "sal_negra", "savia", "sales_de_bano", "perlas", "polvo_astral", "mat" };
            CollectionAssert.AreEqual(families, cat.Families);
            foreach (var r in cat.All())
            {
                Assert.IsNotEmpty(r.Sources, r.Id + ": fuente");
                Assert.IsNotEmpty(r.Sinks, r.Id + ": sumidero");
                if (r.Id != Res.Perlas)
                {
                    Assert.IsFalse(r.Sold, r.Id + ": nada que dé poder se vende (GDD 11)");
                    Assert.IsFalse(r.Sources.Contains("compra"), r.Id + ": solo las Perlas se compran con dinero real");
                }
            }
            foreach (var p in Planets.All)
            {
                Assert.IsTrue(cat.Has(Res.SalCruda(p)));
                Assert.IsTrue(cat.Has(Res.SalDestilada(p)));
                Assert.IsTrue(cat.Has(Res.Quintaesencia(p)));
            }
        }

        [Test]
        public void DoceFamiliasCincoNivelesDosVariantes_120Armas()
        {
            var meta = Meta();
            Assert.AreEqual(12, meta.Families.Count);
            Assert.AreEqual(5, meta.Tiers.Count);
            var normal = meta.Weapons.Values.Where(w => w.Variant >= 0).ToList();
            Assert.AreEqual(120, normal.Count, "12 familias × 5 niveles × 2 variantes (GDD 15.5)");
            Assert.AreEqual(8, meta.Weapons.Values.Count(w => w.Legendary), "más 8 legendarias");
            foreach (var f in meta.Families)
            {
                Assert.IsNotEmpty(f.Hits, f.Id + ": cada familia tiene su patrón de golpes");
                Assert.AreEqual(1.0, f.Hits.Sum(h => h.Share), 0.02, f.Id);
            }
            // Los patrones son distintos entre familias.
            var patterns = meta.Families.Select(f => string.Join(",", f.Hits.Select(h => h.Frame))).ToList();
            Assert.AreEqual(patterns.Count, patterns.Distinct().Count(), "cada familia con su propio patrón de golpes");
            Assert.AreEqual("Daga de Sal Oxidada", meta.Weapon("daga_1_a").Name);
            Assert.AreEqual("Campana de Bronce", meta.Weapon("campana_2_b").Name);
            Assert.GreaterOrEqual(meta.Prefixes.Count, 25, "unos 25 prefijos (GDD 15.4)");
        }

        [Test]
        public void SieteRamasDe15Injertos_5ActivosY10Pendientes()
        {
            var f = Meta().Fresno;
            Assert.AreEqual(7, f.Ramas.Count);
            foreach (var rama in f.Ramas)
            {
                Assert.AreEqual(15, rama.Grafts.Count, rama.Name);
                Assert.AreEqual(5, rama.Grafts.Count(g => !g.Pending), rama.Name + ": cinco Injertos con efecto real");
                Assert.AreEqual(10, rama.Grafts.Count(g => g.Pending), rama.Name + ": diez pendientes");
                Assert.Greater(rama.Fruit.Mods.Count, 0, rama.Name + ": su Fruto tiene efecto");
            }
            Assert.AreEqual(0.20, f.Rama(Planet.Saturno).Fruit.Mods.Get("delayDamageCoef"), 1e-9, "GDD 10.3: el Fruto de Saturno");
            var savia = f.Mutation("savia_negra");
            Assert.AreEqual(0.30, savia.Mods.Get("damagePct"), 1e-9);
            Assert.AreEqual(-0.30, savia.Mods.Get("healingPct"), 1e-9);
            Assert.AreEqual(5, f.MaxCursesPerZone);
            Assert.AreEqual(1.25, f.CurseAlmitasMultiplier, 1e-9);
            Assert.AreEqual(0.30, f.ManchaReturn, 1e-9);
            Assert.AreEqual(3, f.EcoSlots);
            Assert.AreEqual(3, f.EcoGrafts);
            Assert.AreEqual(1, f.EcoTrainedMoves);
        }

        [Test]
        public void ElGachaUsaLasTasasDelGdd()
        {
            var g = Meta().Gacha;
            Assert.AreEqual(160, g.CostPerPull);
            Assert.AreEqual(0.006, g.Base5, 1e-12);
            Assert.AreEqual(60, g.SoftPityStart);
            Assert.AreEqual(0.07, g.SoftPityStep, 1e-12);
            Assert.AreEqual(70, g.HardPity);
            Assert.AreEqual(0.06, g.Base4, 1e-12);
            Assert.AreEqual(10, g.Guarantee4Every);
            Assert.AreEqual(0.75, g.FeaturedRate, 1e-12);
            Assert.AreEqual(300, g.SelectorAt);
            Assert.AreEqual(0.006, g.Rate5(59), 1e-12);
            Assert.AreEqual(0.076, g.Rate5(60), 1e-12, "la tirada 60 ya lleva +7 %");
            Assert.AreEqual(1.0, g.Rate5(70), 1e-12);
        }

        [Test]
        public void CadaSalmueraTieneRecetaYTresPotencias()
        {
            var meta = Meta();
            Assert.AreEqual(11, meta.Brewing.Recipes.Count);
            Assert.AreEqual(3, meta.Brewing.Potency.Count);
            Assert.AreEqual(new[] { "agua_termal", "vinagre", "leche_lunar" }, meta.Brewing.Bases.Take(3).Select(b => b.Id).ToArray(), "las tres bases del GDD 14.1");
        }

        [Test]
        public void ElValidadorRechazaModificadoresDesconocidos()
        {
            var src = Sources();
            src.Fresno = src.Fresno.Replace("\"dotPct\": 0.25", "\"dotPorciento\": 0.25");
            var ex = Assert.Throws<DataValidationException>(() => MetaLoader.Load(src, TestKit.RealDb()));
            StringAssert.Contains("modificador desconocido: dotPorciento", ex.Message);
        }
    }
}
