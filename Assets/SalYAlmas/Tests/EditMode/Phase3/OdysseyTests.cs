using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.World;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>La Odisea: anillo libre, Marea de Sal, secciones, botín y Expediciones (docs/propuestas/ODISEA.md).</summary>
    public class OdysseyTests
    {
        private static MetaGame Ready(string sign = "aries", ulong seed = 1)
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), seed, 300));
            Assert.IsTrue(g.Story.Create("Noa", "Elle", "Liebre", sign).Ok);
            StoryPlayer.Prologue(g);
            g.Termas.SetStarterChoice("solenne");
            g.Termas.Pull("inicio", 20, out _);
            return g;
        }

        private static Battle Win(BattleSetup setup, MetaGame g)
        {
            setup.Modifiers.Add(ModKeys.EnemyHpPct, -0.95);
            var b = new Battle(g.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            Assert.AreEqual(BattleOutcome.Victory, b.Outcome);
            return b;
        }

        /// <summary>Avanza la misión en curso hasta su primer combate.</summary>
        private static void ToBattle(MetaGame g, string[] team)
        {
            for (int guard = 0; guard < 30 && g.Story.CurrentScene.Battle == null; guard++)
            {
                var s = g.Story.CurrentScene;
                if (s.Choices.Count > 0) g.Story.Choose(g.Story.Choices(team).First(c => c.Available).Index, team);
                else g.Story.Continue();
            }
            Assert.IsNotNull(g.Story.CurrentScene.Battle);
        }

        [Test]
        public void CadaSignoEmpiezaEnLaCardinalDeSuElemento()
        {
            var expected = new Dictionary<string, string>
            {
                { "aries", "aries" }, { "leo", "aries" }, { "sagitario", "aries" },
                { "tauro", "capricornio" }, { "virgo", "capricornio" }, { "capricornio", "capricornio" },
                { "geminis", "libra" }, { "libra", "libra" }, { "acuario", "libra" },
                { "cancer", "cancer" }, { "escorpio", "cancer" }, { "piscis", "cancer" },
            };
            foreach (var kv in expected) Assert.AreEqual(kv.Value, Meta().World.Story.Sign(kv.Key).Region, kv.Key);
            var ring = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), 1, 300)).Story.BuiltRing.Select(r => r.Id).ToArray();
            CollectionAssert.AreEqual(new[] { "aries", "cancer", "libra", "capricornio" }, ring, "el anillo construido sigue el zodiaco");
        }

        [Test]
        public void ElNivelDependeDeCuandoLlegasNoDeLaRegionNiDelEquipo()
        {
            var g = Ready("cancer");
            string[] team = StoryPlayer.Team(g);
            Assert.AreEqual(5, g.Story.RegionLevel("cancer"), "tu región es la sencilla");
            Assert.AreEqual(SkyLayer.Superficie, g.Story.Layer("cancer"));

            // El mismo combate da el mismo nivel de enemigo con un equipo de nivel 1 o de nivel 60.
            Assert.IsTrue(g.Story.StartMission("cancer", "can_1").Ok);
            ToBattle(g, team);
            int low = g.Story.PrepareBattle(team, 1).CustomEnemies[0].Level;
            foreach (var id in team) g.Profile.Witches[id].Level = 60;
            int high = g.Story.PrepareBattle(team, 1).CustomEnemies[0].Level;
            Assert.AreEqual(low, high, "los enemigos ya no escalan con el equipo");
            Assert.AreEqual(5, low, "orilla de la primera región: nivel 5");
            g.Story.LeaveMission();

            StoryPlayer.Region(g, "cancer", team);
            Assert.AreEqual(1, g.Story.ArrivalIndex("aries"));
            Assert.AreEqual(11, g.Story.RegionLevel("aries"), "la segunda región llega al nivel 11, sea cual sea");
        }

        [Test]
        public void UnCombateDeHistoriaEsUnaCadenaDeSeccionesQueArrastraLaVida()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            Assert.IsTrue(g.Story.StartMission("aries", "aries_1").Ok);
            ToBattle(g, team);
            Assert.AreEqual(2, g.Story.SectionCount, "orilla: una sección y el combate de la escena");
            string scene = g.Story.CurrentScene.Id;
            long almitas = g.Ledger.Balance(Res.Almitas);

            var setup = g.Story.PrepareBattle(team, 1);
            var b = new Battle(g.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            Assert.AreEqual(BattleOutcome.Victory, b.Outcome);
            var r = g.Story.ResolveBattle(b);
            Assert.IsTrue(r.Ok, r.Message);
            Assert.AreEqual(scene, g.Story.CurrentScene.Id, "sigue en la misma escena: falta la última sección");
            Assert.Greater(g.Ledger.Balance(Res.Almitas), almitas, "la sección da almitas");
            Assert.IsNotEmpty(g.Profile.Calculista.LastLoot);

            var next = g.Story.PrepareBattle(team, 2);
            foreach (var w in b.Witches.Where(w => !w.IsGuest))
                Assert.AreEqual(w.IsAlive ? System.Math.Max(0.10, w.Hp / (double)w.MaxHp) : 0.10, next.StartHp[w.Key], 1e-9, "la vida se arrastra");
            Win(next, g);
            Assert.IsTrue(g.Story.ResolveBattle(new Battle(g.Combat, next)).Ok == false, "un combate sin terminar no cuenta");
        }

        [Test]
        public void PerderRepiteLaSeccionSinCosto()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            Assert.IsTrue(g.Story.StartMission("aries", "aries_1").Ok);
            ToBattle(g, team);
            var setup = g.Story.PrepareBattle(team, 1);
            setup.Modifiers.Add(ModKeys.EnemyHpPct, 50);
            var b = new Battle(g.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            Assert.AreNotEqual(BattleOutcome.Victory, b.Outcome);
            Assert.IsFalse(g.Story.ResolveBattle(b).Ok);
            Assert.AreEqual(0, g.Profile.Calculista.Section, "la sección se reintenta");
            Assert.IsTrue(g.Story.InMission);
        }

        [Test]
        public void TerminarUnaZonaDaSalesUnaVezYLaRegionAbreSuEstacion()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            StoryPlayer.Region(g, "aries", team);
            Assert.That(g.Profile.Calculista.ZonesDone, Is.SupersetOf(new[] { "aries_orilla", "aries_subsuelo", "aries_corazon" }));
            // 400 de las misiones (80 + 80 + 80 + 160) más 100 por cada una de las tres zonas.
            Assert.AreEqual(sales + 400 + 300, g.Ledger.Balance(Res.SalesDeBano));
            Assert.IsTrue(g.Profile.Stations.Contains("fragua_hierro_rojo"), "Aries abre la Fragua de Hierro Rojo");
            Assert.Greater(g.Ledger.Balance(Res.Mat("hierro_rojo")), 0, "las secciones dan el material de la región");
        }

        [Test]
        public void LaExpedicionPideLaZonaTerminadaYRespetaLaMarea()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            Assert.IsNotEmpty(g.Story.CanExpedition("aries", "aries_orilla", 0), "antes de su historia no");
            StoryPlayer.Region(g, "aries", team);
            Assert.AreEqual("", g.Story.CanExpedition("aries", "aries_orilla", 0));
            Assert.IsNotEmpty(g.Story.CanExpedition("aries", "aries_orilla", 1), "la Marea de hoy todavía es 0");
            Assert.AreEqual("", g.Story.CanExpedition("aries", "aries_cielo", 0), "la EX se abre al terminar la región");

            Assert.IsTrue(g.Story.StartExpedition("aries", "aries_cielo", 0).Ok);
            int sections = g.Story.ExpeditionSectionCount;
            Assert.AreEqual(5, sections, "cuatro secciones y el jefe");
            for (int i = 0; i < sections; i++)
            {
                if (i == sections - 1) Assert.IsTrue(g.Story.ExpeditionBossNext);
                var b = Win(g.Story.PrepareExpeditionBattle(team, (ulong)(10 + i)), g);
                Assert.IsTrue(g.Story.ResolveExpeditionBattle(b).Ok);
            }
            Assert.IsFalse(g.Story.InExpedition);
            Assert.IsTrue(g.Profile.Calculista.ZonesDone.Contains("aries_cielo"));
        }

        [Test]
        public void UnaMisionEnPausaSobreviveAUnaExpedicion()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            Assert.IsTrue(g.Story.StartMission("cancer", "can_1").Ok);
            ToBattle(g, team);
            string scene = g.Story.CurrentScene.Id;
            Assert.IsTrue(g.Story.StartExpedition("aries", "aries_orilla", 0).Ok);
            Assert.IsFalse(g.Story.StartMission("libra", "lib_1").Ok);
            Assert.IsTrue(g.Story.LeaveExpedition().Ok);
            Assert.AreEqual(scene, g.Story.CurrentScene.Id, "la misión sigue donde estaba");
        }

        [Test]
        public void ElBotinDeExpedicionBajaDespuesDelTopeDiario()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            var o = Meta().World.Story.Odyssey;
            g.Profile.Calculista.DailyDay = g.Profile.Day;
            g.Profile.Calculista.DailySections = o.DailyFull;
            Assert.IsTrue(g.Story.StartExpedition("aries", "aries_orilla", 0).Ok);
            long before = g.Ledger.Balance(Res.Almitas);
            var b = Win(g.Story.PrepareExpeditionBattle(team, 3), g);
            Assert.IsTrue(g.Story.ResolveExpeditionBattle(b).Ok);
            long full = (long)System.Math.Round(o.SectionAlmitas * System.Math.Pow(g.Story.RegionLevel("aries"), o.SectionExponent));
            Assert.AreEqual((long)System.Math.Round(full * o.AfterCap), g.Ledger.Balance(Res.Almitas) - before, "después del tope: ×0.25");
        }

        [Test]
        public void DesdeLaGrietaEntranLosInvasores()
        {
            var g = Ready();
            string[] team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            g.Profile.Calculista.Eclipses = 3;
            g.Profile.Calculista.Corruption["aries"] = 0;
            Assert.AreEqual(3, g.Story.MaxTideFor("aries"));
            bool invader = false;
            for (int run = 0; run < 12 && !invader; run++)
            {
                Assert.IsTrue(g.Story.StartExpedition("aries", "aries_orilla", 3).Ok);
                var setup = g.Story.PrepareExpeditionBattle(team, (ulong)run);
                invader = setup.CustomEnemies.Any(e => e.Id.StartsWith("cristalizado"));
                Assert.Greater(setup.CustomEnemies[0].Level, g.Story.RegionLevel("aries"), "más Marea, más nivel");
                g.Profile.Calculista.SectionsWon++;
                g.Story.LeaveExpedition();
            }
            Assert.IsTrue(invader, "en la Grieta aparecen Cristalizados");
        }

        [Test]
        public void TodaRegionTieneSusZonasYSusEnemigosConPista()
        {
            var s = Meta().World.Story;
            Assert.IsTrue(s.Odyssey.Enabled);
            foreach (var region in s.Regions.Where(r => !r.Prologue))
            {
                Assert.AreEqual(4, region.Zones.Count, region.Id);
                Assert.IsTrue(region.Zones.Last().IsEx, region.Id + ": la cuarta zona es la EX");
                foreach (var z in region.Zones)
                    foreach (var id in z.Pool.Concat(new[] { z.Boss }).Where(x => x.Length > 0))
                        Assert.IsNotEmpty(Meta().Enemy(id).Traits.Hint, id + " necesita una pista en su placa");
            }
        }

        // ------------------------------------------------------------------ Rasgos de enemigos y trucos de brujas

        private static Battle Fight(string[] team, EnemyDef enemy, params EnemyDef[] more)
        {
            var enc = new EncounterDef { Id = "prueba", Name = "Prueba" };
            var setup = new BattleSetup { Team = team.ToList(), Seed = 4, CustomEncounter = enc };
            foreach (var e in new[] { enemy }.Concat(more)) { enc.Enemies.Add(e.Id); setup.CustomEnemies.Add(e); }
            var b = new Battle(Meta().Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            return b;
        }

        [Test]
        public void ElProtectorBajaElDanoDeSusCompaneros()
        {
            var crab = Meta().Enemy("cangrejo_salinero");
            var slime = Meta().Enemy("medusa_salmuera");
            var guarded = Fight(new[] { "marisal" }, Core.Fresno.FresnoService.Scaled(slime, "m1", 3, 0.01, 0, ""), Core.Fresno.FresnoService.Scaled(crab, "c1", 3, 0.01, 0, ""));
            var plainCrab = Core.Fresno.FresnoService.Scaled(crab, "c2", 3, 0.01, 0, "");
            plainCrab.Traits = new EnemyTraits();
            var alone = Fight(new[] { "marisal" }, Core.Fresno.FresnoService.Scaled(slime, "m2", 3, 0.01, 0, ""), plainCrab);
            double Avg(Battle b, string prefix) => b.Events.OfType<HitLanded>().Where(h => h.TargetName == Meta().Enemy("medusa_salmuera").Name && h.Damage > 1).Select(h => (double)h.Damage).DefaultIfEmpty(0).Average();
            Assert.Less(Avg(guarded, "m1"), Avg(alone, "m2") * 0.8, "con el cangrejo vivo, la medusa recibe menos daño");
        }

        [Test]
        public void LosSinCartaIgnoranLosAspectos()
        {
            var b = Fight(new[] { "marisal", "selenia", "solenne", "tormentina" }, Core.Fresno.FresnoService.Scaled(Meta().Enemy("sin_carta"), "s1", 4, 0.01, 0, ""));
            Assert.IsTrue(b.Events.OfType<ReactionTriggered>().Any(r => r.Detail.Contains("no tiene carta")), "el registro dice que el Aspecto se pierde");
        }

        [Test]
        public void ElReflejoDevuelveDanoAQuienGolpea()
        {
            var b = Fight(new[] { "marisal" }, Core.Fresno.FresnoService.Scaled(Meta().Enemy("duquesa_fiel"), "d1", 0.3, 0.01, 0, ""));
            Assert.IsTrue(b.Events.OfType<DotTicked>().Any(d => d.StatusName.StartsWith("Reflejo")));
        }

        [Test]
        public void LaSoldadaCreceCadaTurno()
        {
            var b = Fight(new[] { "topo" }, Core.Fresno.FresnoService.Scaled(Meta().Enemy("soldada_crisol"), "s1", 2, 0.01, 0, ""));
            Assert.Greater(b.Events.OfType<StatusApplied>().Count(s => s.StatusName == "Crece y olvida"), 1);
        }

        [Test]
        public void AxaReviveUnaVezYSalitreEmpiezaIntoxicada()
        {
            var w = Meta().Combat;
            Assert.AreEqual(0.30, w.Witch("axa").ReviveOnceHp, 1e-9);
            Assert.Greater(w.Witch("salitre").StartPoison, 0);
            var b = Fight(new[] { "axa" }, Core.Fresno.FresnoService.Scaled(Meta().Enemy("ferran"), "f1", 5, 3, 0, ""));
            Assert.AreEqual(1, b.Events.OfType<UnitRevived>().Count(), "Axa vuelve del agua una sola vez");
            var s = Fight(new[] { "salitre" }, Core.Fresno.FresnoService.Scaled(Meta().Enemy("figura_sal"), "x", 0.2, 0.01, 0, ""));
            Assert.IsTrue(s.Events.OfType<StatusApplied>().Any(e => e.StatusName == "Intoxicada"));
        }
    }
}
