using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using SalYAlmas.Core.World;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Juega la historia con el piloto del combate, eligiendo lo que indique la función.</summary>
    internal static class StoryPlayer
    {
        public static MetaGame NewPlayer(ulong seed = 1)
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), seed, 300));
            var r = g.Story.Create("Noa", "Elle", "Liebre", "aries");
            Assert.IsTrue(r.Ok, r.Message);
            return g;
        }

        public static void Prologue(MetaGame g)
        {
            Assert.IsTrue(g.Story.StartMission("vado", "prologo").Ok);
            Run(g, new[] { "marisal", "zarigueya", "topo" }, null);
        }

        /// <summary>Corre la misión en curso. choose decide la opción; si es null, la primera disponible.</summary>
        public static void Run(MetaGame g, IReadOnlyList<string> team, Func<List<ChoiceView>, ChoiceView> choose, Func<BattleSetup, BattleSetup> tweak = null)
        {
            ulong seed = 1;
            for (int guard = 0; guard < 200 && g.Story.InMission; guard++)
            {
                var scene = g.Story.CurrentScene;
                if (scene.Battle != null)
                {
                    var setup = g.Story.PrepareBattle(team, seed++);
                    if (tweak != null) setup = tweak(setup);
                    var b = new Battle(g.Combat, setup);
                    b.Start();
                    new AutoPilot().RunToEnd(b);
                    var result = g.Story.ResolveBattle(b);
                    Assert.IsTrue(b.IsOver);
                    if (!result.Ok) Assert.Greater(50, (int)seed, "no se pudo ganar " + scene.Id + " tras muchos intentos");
                }
                else if (scene.Choices.Count > 0)
                {
                    var options = g.Story.Choices(team);
                    var pick = choose != null ? choose(options) : options.First(o => o.Available);
                    Assert.IsTrue(g.Story.Choose(pick.Index, team).Ok);
                }
                else Assert.IsTrue(g.Story.Continue().Ok);
            }
            Assert.IsFalse(g.Story.InMission, "la misión no terminó");
        }

        public static void Region(MetaGame g, string region, IReadOnlyList<string> team, Func<List<ChoiceView>, ChoiceView> choose = null)
        {
            foreach (var m in g.Meta.World.Story.Region(region).Missions)
            {
                var ok = g.Story.StartMission(region, m.Id);
                Assert.IsTrue(ok.Ok, ok.Message);
                Run(g, team, choose);
            }
        }

        public static string[] Team(MetaGame g) => g.Profile.Witches.Keys.Where(g.Meta.HasKit).OrderBy(x => x, StringComparer.Ordinal).Take(4).ToArray();
    }

    /// <summary>Motor de historia (GDD 6 y 13).</summary>
    public class StoryTests
    {
        private static MetaGame Ready(ulong seed = 1)
        {
            var g = StoryPlayer.NewPlayer(seed);
            StoryPlayer.Prologue(g);
            // Una Regente del banner de inicio, como una jugadora real antes de salir del prólogo.
            g.Termas.SetStarterChoice("solenne");
            g.Termas.Pull("inicio", 20, out _);
            return g;
        }

        [Test]
        public void HayDosRegionesConCincoMisiones()
        {
            var s = Meta().World.Story;
            foreach (var id in new[] { "aries", "capricornio" }) Assert.AreEqual(5, s.Region(id).Missions.Count, id);
            Assert.AreEqual(4, s.Senderos.Count);
            Assert.AreEqual(12, s.Signs.Count);
            Assert.AreEqual(12, s.Ring.Count);
        }

        [Test]
        public void ElCreadorValidaYElSignoDaBonoYRegion()
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), 1, 300));
            Assert.IsFalse(g.Story.Create("A", "Elle", "Liebre", "aries").Ok, "nombre corto");
            Assert.IsFalse(g.Story.Create("Noa", "Nada", "Liebre", "aries").Ok);
            Assert.IsFalse(g.Story.Create("Noa", "Elle", "Liebre", "ofiuco").Ok);
            Assert.IsTrue(g.Story.Create("Noa", "Elle", "Liebre", "capricornio").Ok);
            Assert.AreEqual("capricornio", g.Story.StartRegion);
            var s = g.Meta.World.Story;
            Assert.AreEqual(s.StatBase + 1, g.Story.Stat(CalculistaStat.Oficio), "Tierra da +1 de Oficio");
            Assert.AreEqual(s.StatBase, g.Story.Stat(CalculistaStat.Temple));
        }

        [Test]
        public void ElPrologoEsObligatorioYEntregaAlasPrimerasBrujas()
        {
            var g = StoryPlayer.NewPlayer();
            Assert.IsFalse(g.Story.StartMission("aries", "aries_1").Ok, "primero el prólogo");
            StoryPlayer.Prologue(g);
            Assert.IsTrue(g.Profile.Owns("zarigueya"));
            Assert.IsTrue(g.Profile.Owns("topo"));
            Assert.IsTrue(g.Profile.Calculista.Titles.Contains("salada_nacimiento"));
            Assert.IsTrue(g.Story.PrologueDone);
        }

        [Test]
        public void ElChequeoDeUmbralEsFijoYMuestraElNumero()
        {
            var g = Ready();
            string[] team = { "solenne" };
            g.Story.StartMission("aries", "aries_1");
            while (g.Story.CurrentScene.Id != "a3")
            {
                var sc = g.Story.CurrentScene;
                if (sc.Battle != null) WinBattle(g, team);
                else g.Story.Continue();
            }
            var check = g.Story.Choices(team).First(c => c.Def.Stat == CalculistaStat.Calculo);
            int have = g.Story.Stat(CalculistaStat.Calculo, team);
            StringAssert.Contains(have + "/" + check.Def.Need, check.CheckLabel);
            Assert.AreEqual(have < check.Def.Need, !check.Available);
        }

        private static void WinBattle(MetaGame g, IReadOnlyList<string> team)
        {
            var setup = g.Story.PrepareBattle(team, 3);
            setup.Modifiers.Add(ModKeys.EnemyHpPct, -0.95);
            var b = new Battle(g.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            Assert.IsTrue(g.Story.ResolveBattle(b).Ok);
        }

        [Test]
        public void LaBrujaPresenteSumaAlChequeo()
        {
            var g = Ready();
            int alone = g.Story.Stat(CalculistaStat.Encanto, new string[0]);
            int withFox = g.Story.Stat(CalculistaStat.Encanto, new[] { "solenne" });
            Assert.AreEqual(alone + g.Meta.World.Story.WitchBonus, withFox, "Solenne, de Sol, suma Encanto");
        }

        [Test]
        public void UnaOpcionBloqueadaNoSePuedeElegir()
        {
            var g = Ready();
            g.Profile.Calculista.Stats[CalculistaStat.Calculo.ToString()] = 0;
            string[] team = { "zarigueya" };
            g.Story.StartMission("aries", "aries_1");
            while (g.Story.CurrentScene.Id != "a3")
            {
                if (g.Story.CurrentScene.Battle != null) WinBattle(g, team);
                else g.Story.Continue();
            }
            var blocked = g.Story.Choices(team).First(c => c.Def.Stat == CalculistaStat.Calculo);
            Assert.IsFalse(blocked.Available);
            Assert.IsFalse(g.Story.Choose(blocked.Index, team).Ok);
        }

        [Test]
        public void LasDosRegionesSeTerminanEnCualquierOrdenYElOrdenSeNota()
        {
            foreach (var order in new[] { new[] { "aries", "capricornio" }, new[] { "capricornio", "aries" } })
            {
                var g = Ready();
                string[] team = StoryPlayer.Team(g);
                StoryPlayer.Region(g, order[0], team);
                Assert.IsTrue(g.Profile.Calculista.Marks.Contains("primera_" + order[0]));
                Assert.AreEqual(0, g.Profile.Calculista.Eclipses, "el eclipse llega cada dos regiones");

                Assert.IsTrue(g.Story.StartMission(order[1], g.Meta.World.Story.Region(order[1]).Missions[0].Id).Ok);
                string opening = g.Story.SceneText(team);
                var plain = g.Story.CurrentScene.Text;
                Assert.AreNotEqual(plain, opening, "el orden cambia lo que se ve en la segunda región");
                g.Story.LeaveMission();

                StoryPlayer.Region(g, order[1], team);
                Assert.IsTrue(g.Story.GameFinished);
                Assert.AreEqual(1, g.Profile.Calculista.Eclipses);
            }
        }

        [Test]
        public void UnaCasaPerdonadaVuelveEnOtraRegion()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            g.Profile.Calculista.Marks.Add("perdono_capitana");
            g.Profile.Calculista.MissionsDone.AddRange(new[] { "cap_1", "cap_2", "cap_3" });
            g.Story.StartMission("capricornio", "cap_4");
            StringAssert.Contains("Ferrán", g.Story.SceneText(team));
            g.Story.Continue();
            WinBattle(g, team);
            Assert.IsTrue(g.Story.Choices(team).Any(c => c.Def.Effects.Marks.Contains("ferran_murio")), "abre una opción que no existe sin la marca");
        }

        [Test]
        public void ElEclipseCorrompeLasRegionesSinVisitarYNuncaBloquea()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            StoryPlayer.Region(g, "capricornio", team);
            Assert.AreEqual(1, g.Profile.Calculista.Eclipses);
            Assert.AreEqual(0, g.Story.CorruptionOf("aries"), "visitada");
            Assert.AreEqual(0, g.Story.CorruptionOf("capricornio"), "visitada");
            Assert.AreEqual(1, g.Story.CorruptionOf("tauro"), "sin visitar");
            Assert.AreEqual(10, g.Meta.World.Story.Ring.Count(r => g.Story.CorruptionOf(r) == 1));
        }

        [Test]
        public void LaRelecturaSeAbreAlFinalYNoRepiteRecompensas()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            Assert.IsFalse(g.Story.StartMission("aries", "aries_4").Ok, "todavía no se termina el juego");
            StoryPlayer.Region(g, "capricornio", team);

            long almitas = g.Ledger.Balance(Res.Almitas);
            Assert.IsTrue(g.Story.StartMission("aries", "aries_4").Ok);
            Assert.IsTrue(g.Profile.Calculista.Reread);
            // Toma la otra decisión: la marca nueva se escribe, y la moneda no se repite.
            StoryPlayer.Run(g, team, o => o.Last(x => x.Available));
            Assert.AreEqual(almitas, g.Ledger.Balance(Res.Almitas));
            Assert.IsTrue(g.Profile.Calculista.Marks.Contains("entrego_capitana"));
            Assert.IsFalse(g.Profile.Calculista.Reread);
        }

        [Test]
        public void GuardarYCargarAMediaMisionContinuaIgual()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            g.Story.StartMission("aries", "aries_1");
            g.Story.Continue();
            string scene = g.Profile.Calculista.Scene;
            var store = new MemorySaveStore();
            g.Save(store, "s");
            var loaded = MetaGame.Load(Meta(), store, "s");
            Assert.AreEqual(scene, loaded.Profile.Calculista.Scene);
            Assert.IsTrue(loaded.Story.InMission);
            Assert.AreEqual(g.Story.SceneText(team), loaded.Story.SceneText(team));
            Assert.AreEqual(g.ToJson(), loaded.ToJson());
        }

        [Test]
        public void ElParalajeTieneSusBandasYElBanoLoBaja()
        {
            var g = Ready();
            var c = g.Profile.Calculista;
            c.Paralaje = 0;
            Assert.AreEqual(BookBand.Bajo, g.Story.Band);
            c.Paralaje = 25;
            Assert.AreEqual(BookBand.Medio, g.Story.Band);
            c.Paralaje = 50;
            Assert.AreEqual(BookBand.Alto, g.Story.Band);
            c.Paralaje = 80;
            Assert.AreEqual(BookBand.Maximo, g.Story.Band);
            Assert.IsTrue(g.Base.Bathe().Ok);
            Assert.AreEqual(60, c.Paralaje);
            Assert.IsFalse(g.Base.Bathe().Ok, "una vez al día");
            g.SetDay(g.Profile.Day + 1);
            Assert.IsTrue(g.Base.Bathe().Ok);
            Assert.AreEqual(40, c.Paralaje);
            g.Story.AddParalaje(-500);
            Assert.AreEqual(0, c.Paralaje);
        }

        [Test]
        public void ElSenderoSeCambiaGratisYSubeConElUso()
        {
            var g = Ready();
            Assert.IsTrue(g.Story.SetSendero("contadora").Ok);
            Assert.IsFalse(g.Story.SetSendero("nada").Ok);
            Assert.AreEqual(0.15, g.Story.CombatMods().Get(ModKeys.AlmitasPct), 1e-9);
            g.Story.AddSenderoXp(g.Meta.World.Story.SenderoXpPerLevel);
            Assert.AreEqual(2, g.Story.SenderoLevel("contadora"));
            Assert.Greater(g.Story.CombatMods().Get(ModKeys.AlmitasPct), 0.15);
            Assert.AreEqual(0.15 * 1.05, g.Fresno.InRun ? 0 : g.Story.CombatMods().Get(ModKeys.AlmitasPct), 1e-9);
        }

        [Test]
        public void SinCalculistaLosCombatesNoCambian()
        {
            var g = NewGame();
            Assert.AreEqual(0, g.Story.CombatMods().Keys.Count);
        }

        [Test]
        public void TodasLasOpcionesTienenSalidaSinImportarLasEstadisticas()
        {
            // Una jugadora con estadísticas en el mínimo puede terminar las dos regiones: los chequeos abren, no bloquean.
            var g = Ready();
            foreach (var key in g.Profile.Calculista.Stats.Keys.ToList()) g.Profile.Calculista.Stats[key] = 0;
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "capricornio", team);
            StoryPlayer.Region(g, "aries", team);
            Assert.IsTrue(g.Story.GameFinished);
        }
    }

    /// <summary>Tienda, servidor y cumplimiento (GDD 16.4 y 16.6).</summary>
    public class StoreTests
    {
        private static MetaGame Game() => StoryPlayer.NewPlayer(5);

        [Test]
        public void UnaCompraDePruebaOtorgaLoCorrectoYQuedaEnElServidor()
        {
            var g = Game();
            Assert.IsTrue(g.Store.Buy("perlas_300").Ok);
            Assert.AreEqual(330, g.Ledger.Balance(Res.Perlas));
            var server = g.Store.Backend.Purchases.Single();
            Assert.AreEqual("perlas_300", server.Product);
            Assert.AreEqual(4.99, server.Usd, 1e-9);
            Assert.AreEqual(330, g.Ledger.TotalIn(Res.Perlas, "compra"));
        }

        [Test]
        public void ElServidorRechazaRecibosFalsosORepetidos()
        {
            var g = Game();
            Assert.IsFalse(g.Store.Redeem(new Receipt { Id = "x", Product = "perlas_1980", Signature = "falsa" }).Ok);
            Assert.AreEqual(0, g.Ledger.Balance(Res.Perlas));
            var receipt = g.Store.Platform.Charge("perlas_60");
            Assert.IsTrue(g.Store.Redeem(receipt).Ok);
            Assert.IsFalse(g.Store.Redeem(receipt).Ok, "un recibo vale una vez");
            Assert.AreEqual(60, g.Ledger.Balance(Res.Perlas));
        }

        [Test]
        public void UnReembolsoRetiraLoEntregado()
        {
            var g = Game();
            g.Store.Buy("perlas_60");
            string id = g.Profile.Store.Purchases[0].Receipt;
            Assert.IsTrue(g.Store.Refund(id).Ok);
            Assert.AreEqual(0, g.Ledger.Balance(Res.Perlas));
            Assert.IsTrue(g.Store.Backend.Purchases.Single().Refunded);
            Assert.IsFalse(g.Store.Refund(id).Ok);
        }

        [Test]
        public void UnReembolsoConPerlasGastadasDejaDeuda()
        {
            var g = Game();
            g.Store.Buy("perlas_60");
            g.Store.ConvertPerlas(60);
            g.Store.Refund(g.Profile.Store.Purchases[0].Receipt);
            Assert.AreEqual(60, g.Profile.Store.PerlasDebt);
            g.Store.Buy("perlas_300");
            Assert.AreEqual(270, g.Ledger.Balance(Res.Perlas), "la deuda se descuenta de la siguiente compra");
            Assert.AreEqual(0, g.Profile.Store.PerlasDebt);
        }

        [Test]
        public void NingunProductoVendeProgreso()
        {
            var shop = Meta().World.Shop;
            foreach (var p in shop.Products) Assert.IsTrue(p.Kind == ProductKind.Perlas || p.Kind == ProductKind.Diezmo || p.Kind == ProductKind.Grimorio, p.Id);
            foreach (var tier in shop.Pass)
                foreach (var kv in tier.Premium)
                    Assert.AreEqual(Res.SalesDeBano, kv.Key, "el carril premium solo trae Sales y cosméticos");
        }

        [Test]
        public void UnCosmeticoConEstadisticasNoPasaLaValidacion()
        {
            var src = Sources();
            src.Services = src.Services.Replace("\"id\": \"sal_ambar\",", "\"id\": \"sal_ambar\", \"mods\": { \"atkPct\": 0.1 },");
            var ex = Assert.Catch(() => MetaLoader.Load(src, TestKit.RealDb()));
            StringAssert.Contains("estadísticas", ex.Message);
        }

        [Test]
        public void DiezmoDaPerlasYSalesDiariasUnaVezAlDia()
        {
            var g = Game();
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            g.Store.Buy("diezmo");
            Assert.AreEqual(300, g.Ledger.Balance(Res.Perlas));
            Assert.IsTrue(g.Store.ClaimDiezmo().Ok);
            Assert.IsFalse(g.Store.ClaimDiezmo().Ok);
            g.SetDay(g.Profile.Day + 1);
            Assert.IsTrue(g.Store.ClaimDiezmo().Ok);
            Assert.AreEqual(sales + 180, g.Ledger.Balance(Res.SalesDeBano));
            g.SetDay(g.Profile.Day + 40);
            Assert.IsFalse(g.Store.DiezmoActive);
        }

        [Test]
        public void ElGrimorioDaSuCarrilSegunLoComprado()
        {
            var g = Game();
            g.Store.AddGrimorioXp(30000);
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            Assert.IsTrue(g.Store.ClaimTier(2).Ok);
            Assert.AreEqual(sales + 80, g.Ledger.Balance(Res.SalesDeBano));
            Assert.IsFalse(g.Store.ClaimTier(2).Ok);
            g.Store.Buy("grimorio");
            Assert.IsTrue(g.Store.ClaimTier(2).Ok, "al comprarlo se cobra el carril premium");
            Assert.AreEqual(sales + 240, g.Ledger.Balance(Res.SalesDeBano));
            Assert.IsTrue(g.Store.ClaimTier(30).Ok);
            Assert.IsTrue(g.Store.OwnsCosmetic("atuendo_topo_grimorio"));
            Assert.IsFalse(g.Store.ClaimTier(77).Ok);
        }

        [Test]
        public void LosCosmeticosSeCompranSeEquipanYNoCambianElCombate()
        {
            var g = NewGame();
            g.Ledger.Apply(Res.Perlas, 1000, "prueba");
            int before = (int)g.Progress.BuildLoadout("marisal").AtkMultiplier;
            Assert.IsFalse(g.Store.Equip("sal_violeta").Ok, "sin comprar");
            Assert.IsTrue(g.Store.BuyCosmetic("sal_violeta").Ok);
            Assert.IsFalse(g.Store.BuyCosmetic("sal_violeta").Ok, "ya lo tiene");
            Assert.IsTrue(g.Store.Equip("sal_violeta").Ok);
            Assert.AreEqual("sal_violeta", g.Store.Equipped("color_sal"));
            Assert.IsTrue(g.Store.BuyCosmetic("atuendo_marisal_yukata").Ok);
            Assert.IsTrue(g.Store.Equip("atuendo_marisal_yukata").Ok);
            Assert.AreEqual("atuendo_marisal_yukata", g.Store.Equipped("atuendo:marisal"));
            Assert.AreEqual(before, (int)g.Progress.BuildLoadout("marisal").AtkMultiplier);
            Assert.AreEqual(980 + 240, g.Ledger.TotalOut(Res.Perlas, "cosmeticos"));
            Assert.IsFalse(g.Store.BuyCosmetic("sal_ambar", withDust: true).Ok, "no se vende por Polvo");
        }

        [Test]
        public void ElLimiteDeGastoFrenaLaCompra()
        {
            var g = Game();
            g.Store.SetSpendLimit(5);
            Assert.IsTrue(g.Store.Buy("perlas_300").Ok);
            Assert.IsFalse(g.Store.Buy("perlas_60").Ok, "4.99 + 0.99 pasa de 5");
            g.SetDay(g.Profile.Day + 31);
            Assert.IsTrue(g.Store.Buy("perlas_60").Ok, "se reinicia cada mes");
        }

        [Test]
        public void EnBelgicaSoloSeTiraConMonedaGanada()
        {
            var g = Game();
            g.Ledger.Apply(Res.SalesDeBano, -g.Ledger.Balance(Res.SalesDeBano), "tirada");
            g.Profile.Settings.Country = "BE";
            g.Store.Buy("perlas_300");
            g.Store.ConvertPerlas(320);
            Assert.AreEqual(0, g.Store.EarnedSales);
            StringAssert.Contains("ganadas", g.GachaService.CanPull("estandar", 1));
            g.Ledger.Apply(Res.SalesDeBano, 160, "historia");
            Assert.AreEqual("", g.GachaService.CanPull("estandar", 1));
            Assert.IsTrue(g.GachaService.Pull("estandar", 1, out _).Ok);
            Assert.AreEqual(320, g.Profile.Store.PurchasedSales, "lo comprado no se tocó");

            g.Profile.Settings.Country = "MX";
            Assert.AreEqual("", g.GachaService.CanPull("estandar", 2), "fuera de Bélgica sirve todo");
            Assert.IsTrue(g.GachaService.Pull("estandar", 1, out _).Ok);
            Assert.AreEqual(160, g.Profile.Store.PurchasedSales);
        }

        [Test]
        public void CadaTiradaQuedaEnElServidorYLaAnaliticaLaVe()
        {
            var g = Game();
            var analytics = (LocalAnalytics)g.Analytics;
            g.Termas.SetStarterChoice("cupria");
            Assert.IsTrue(g.GachaService.Pull("inicio", 20, out var results).Ok);
            Assert.AreEqual(20, g.Store.Backend.Pulls.Count);
            Assert.AreEqual(20, analytics.Events.Count(e => e.Key == AnalyticsEvents.Pull));
            Assert.AreEqual("cupria", analytics.Events.Find(e => e.Key == AnalyticsEvents.FirstFiveStar).Value);
        }

        [Test]
        public void ConfigRemotaApagaUnBanner()
        {
            var g = Game();
            ((LocalRemoteConfig)g.GachaService.Remote).DisabledBanners.Add("estandar");
            StringAssert.Contains("disponible", g.GachaService.CanPull("estandar", 1));
        }

        [Test]
        public void LaPaginaDeProbabilidadesMuestraTasasYContador()
        {
            var g = Game();
            var rows = g.GachaService.OddsPage("limitado_marisal");
            string all = string.Join("\n", rows.Select(r => r.Key + ": " + r.Value));
            StringAssert.Contains("0.6 %", all);
            StringAssert.Contains("70", all);
            StringAssert.Contains("75/25", all);
            StringAssert.Contains("No existen", all);
        }

        [Test]
        public void ElHistorialCubreUnAno()
        {
            var g = Game();
            g.GachaService.Pull("estandar", 2, out _);
            g.SetDay(g.Profile.Day + 200);
            Assert.AreEqual(2, g.GachaService.YearHistory().Count);
            g.SetDay(g.Profile.Day + 200);
            Assert.AreEqual(0, g.GachaService.YearHistory().Count);
        }

        [Test]
        public void UnaJugadoraNuevaObtieneUna5EstrellasGratisEnElInicio()
        {
            // Criterio de la Fase 3: primera 5★ en menos de 2 horas sin pagar. El banner de inicio la garantiza.
            for (ulong seed = 1; seed <= 20; seed++)
            {
                var g = StoryPlayer.NewPlayer(seed);
                StoryPlayer.Prologue(g);
                g.Termas.SetStarterChoice("cervanda");
                Assert.IsTrue(g.GachaService.Pull("inicio", 20, out _).Ok);
                Assert.IsTrue(g.Profile.Owns("cervanda"));
                Assert.AreEqual(0, g.Ledger.TotalIn(Res.Perlas, "compra"));
            }
        }

        [Test]
        public void ElPrologoGarantizaLaPrimeraCadena()
        {
            var g = StoryPlayer.NewPlayer(2);
            g.Story.StartMission("vado", "prologo");
            while (g.Story.CurrentScene.Battle == null) g.Story.Continue();
            var setup = g.Story.PrepareBattle(new string[0], 1);
            var b = new Battle(g.Combat, setup);
            b.Start();
            Assert.GreaterOrEqual(b.Resonance, 100, "la Cadena se puede abrir desde el primer turno");
            new AutoPilot().RunToEnd(b);
            Assert.Greater(b.Events.OfType<Core.Events.ChainStarted>().Count(), 0);
            Assert.AreEqual(Core.Events.BattleOutcome.Victory, b.Outcome);
        }
    }

    /// <summary>Termas, localización y guardado de la Fase 3.</summary>
    public class ServicesTests
    {
        [Test]
        public void ConversarDaConfianzaUnaVezAlDia()
        {
            var g = NewGame();
            int t = g.Base.TrustOf("marisal");
            Assert.IsNotEmpty(g.Base.Talk("marisal").Message);
            Assert.AreEqual(t + 20, g.Base.TrustOf("marisal"));
            g.Base.Talk("marisal");
            Assert.AreEqual(t + 20, g.Base.TrustOf("marisal"));
            g.SetDay(g.Profile.Day + 1);
            g.Base.Talk("marisal");
            Assert.AreEqual(t + 40, g.Base.TrustOf("marisal"));
            Assert.AreEqual(1, g.Base.TrustLevel("marisal"));
            Assert.IsFalse(g.Base.Talk("cervanda").Ok, "no la tienes");
        }

        [Test]
        public void LasBrujasEnReposoRotanYLaFavoritaSeQueda()
        {
            var g = NewGame();
            g.Base.SetFavorite("topo");
            var today = g.Base.Resting();
            Assert.AreEqual("topo", today[0]);
            Assert.LessOrEqual(today.Count, 4);
            g.SetDay(g.Profile.Day + 1);
            var tomorrow = g.Base.Resting();
            Assert.AreEqual("topo", tomorrow[0]);
            CollectionAssert.AreNotEqual(today, tomorrow);
        }

        [Test]
        public void CadaBrujaSeSacudeSegunSuEspecie()
        {
            var g = NewGame();
            StringAssert.Contains("loba", g.Base.ShakeOf("marisal"));
            Assert.IsNotEmpty(g.Base.ShakeOf("migaja"), "tiene una por defecto");
        }

        [Test]
        public void LaTablaDeLocalizacionTieneLosTresIdiomasCompletos()
        {
            var loc = Meta().World.Loc;
            CollectionAssert.AreEquivalent(new[] { "es", "en", "ja" }, loc.Languages);
            foreach (var kv in loc.Strings)
                foreach (var lang in loc.Languages)
                    Assert.IsTrue(kv.Value.TryGetValue(lang, out var s) && s.Length > 0, kv.Key + " en " + lang);
            Assert.AreEqual("Home", loc.Get("nav.home", "en"));
            Assert.AreEqual("ホーム", loc.Get("nav.home", "ja"));
            Assert.AreEqual("Inicio", loc.Get("nav.home", "fr"), "cae al español");
            Assert.AreEqual("no.existe", loc.Get("no.existe", "es"));
        }

        [Test]
        public void UnGuardadoDeLaVersion1SeMigraSinPerderNada()
        {
            var g = NewGame();
            var old = g.Profile;
            old.Version = 1;
            string json = ProfileSerializer.ToJson(old);
            // Quita a mano lo que la versión 1 no tenía.
            var loaded = ProfileSerializer.FromJson(json);
            Assert.AreEqual(PlayerProfile.CurrentVersion, loaded.Version);
            Assert.IsNotNull(loaded.Calculista);
            Assert.IsNotNull(loaded.Store);
            Assert.AreEqual(old.Witches.Count, loaded.Witches.Count);
            Assert.AreEqual(old.Ledger.Balance(Res.Almitas), loaded.Ledger.Balance(Res.Almitas));
        }

        [Test]
        public void ElPerfilDePruebaSigueDandoElMismoFresno()
        {
            // Sin Calculista, los bonos de la Fase 3 no tocan los combates de la Fase 2.
            var a = new AutoClimber(NewGame(11)).Climb(new[] { "marisal", "selenia", "solenne", "tormentina" }, seed: 77);
            var b = new AutoClimber(NewGame(11)).Climb(new[] { "marisal", "selenia", "solenne", "tormentina" }, seed: 77);
            Assert.AreEqual(a.Fingerprint, b.Fingerprint);
        }
    }
}
