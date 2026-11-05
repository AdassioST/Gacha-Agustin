using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Meta;
using static SalYAlmas.Tests.Phase2Kit;

namespace SalYAlmas.Tests
{
    /// <summary>Encargos diarios y semanales, logros y Altura (propuesta de meta-juego 4, 5 y 8).</summary>
    public class QuestTests
    {
        private static MetaGame Fresh(int day = 300)
        {
            var g = new MetaGame(Meta(), MetaGame.NewProfile(Meta(), 1, day));
            Assert.IsTrue(g.Story.Create("Noa", "Elle", "Liebre", "aries").Ok);
            return g;
        }

        [Test]
        public void ElTableroDiarioTieneUnEncargoPorHuecoYCambiaConElDia()
        {
            var g = Fresh();
            var today = g.Quests.DailyBoard();
            Assert.AreEqual(4, today.Count);
            CollectionAssert.AreEqual(new[] { "ancla", "hazana", "termas", "taller" }, today.Select(v => v.Def.Slot).ToArray());
            CollectionAssert.AreEqual(today.Select(v => v.Def.Id).ToArray(), Fresh().Quests.DailyBoard().Select(v => v.Def.Id).ToArray(), "determinista con el día");
            for (int day = 301; day < 320; day++)
            {
                var before = g.Quests.DailyBoard().Where(v => v.Def.Slot != "ancla").Select(v => v.Def.Id).ToList();
                g.SetDay(day);
                var after = g.Quests.DailyBoard().Where(v => v.Def.Slot != "ancla").Select(v => v.Def.Id).ToList();
                Assert.IsEmpty(before.Intersect(after), "ningún encargo que no sea ancla se repite dos días seguidos");
            }
        }

        [Test]
        public void UnEncargoSeCobraUnaVezYLosCuatroDanBono()
        {
            var g = Fresh();
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            var board = g.Quests.DailyBoard();
            foreach (var v in board) Assert.IsFalse(g.Quests.Claim(v.Def.Id).Ok, "sin hacerlo no se cobra");
            foreach (var v in board) g.Report(v.Def.Event, v.Def.Need);
            foreach (var v in board) Assert.IsTrue(g.Quests.Claim(v.Def.Id).Ok);
            Assert.IsFalse(g.Quests.Claim(board[0].Def.Id).Ok, "una sola vez");
            Assert.IsTrue(g.Quests.BonusReady);
            long almitas = g.Ledger.Balance(Res.Almitas);
            Assert.IsTrue(g.Quests.ClaimBonus().Ok);
            Assert.AreEqual(almitas + 10000, g.Ledger.Balance(Res.Almitas));
            Assert.GreaterOrEqual(g.Ledger.Balance(Res.SalesDeBano) - sales, 60, "60 Sales al día (META 5.1)");
            Assert.AreEqual(1, g.Profile.Quests.WeeklyProgress.GetValueOrDefault("w_diarios"), "cuenta para el semanal de cinco días");
        }

        [Test]
        public void LosLogrosCuentanDePorVidaYPaganUnaVez()
        {
            var g = Fresh();
            var first = g.Quests.AchievementBoard().First(v => v.Def.Id == "a_cadena");
            Assert.IsFalse(first.Done);
            g.Report("cadena");
            Assert.IsTrue(g.Quests.AchievementBoard().First(v => v.Def.Id == "a_cadena").Claimable);
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            Assert.IsTrue(g.Quests.Claim("a_cadena").Ok);
            Assert.AreEqual(sales + 80, g.Ledger.Balance(Res.SalesDeBano));
            Assert.IsFalse(g.Quests.Claim("a_cadena").Ok);
            g.SetDay(301);
            Assert.IsTrue(g.Quests.AchievementBoard().First(v => v.Def.Id == "a_cadena").Claimed, "los logros no se reinician con el día");
        }

        [Test]
        public void LaAlturaSubeConElCrecimientoYLosPrimerosNivelesDanSales()
        {
            var g = Fresh();
            Assert.AreEqual(1, g.Quests.Altura);
            long sales = g.Ledger.Balance(Res.SalesDeBano);
            g.Report("prologo");
            Assert.AreEqual(3, g.Quests.Altura, "el prólogo da 1 200 de Crecimiento: dos niveles de 600");
            Assert.AreEqual(sales + 160, g.Ledger.Balance(Res.SalesDeBano), "80 Sales por cada nivel del 2 al 5");
            g.Report("mision", 30);
            Assert.Greater(g.Quests.Altura, 10);
            Assert.AreEqual(sales + 4 * 80, g.Ledger.Balance(Res.SalesDeBano), "después del 5 ya no dan Sales");
        }

        [Test]
        public void JugarLaHistoriaAlimentaLosEncargosYLaAltura()
        {
            var g = Fresh();
            StoryPlayer.Prologue(g);
            g.Termas.SetStarterChoice("solenne");
            g.Termas.Pull("inicio", 20, out _);
            var team = StoryPlayer.Team(g);
            StoryPlayer.Region(g, "aries", team);
            var life = g.Profile.Quests.Lifetime;
            Assert.Greater(life.GetValueOrDefault("seccion"), 5);
            Assert.AreEqual(3, life.GetValueOrDefault("zona"));
            Assert.AreEqual(1, life.GetValueOrDefault("region_signo"));
            Assert.Greater(life.GetValueOrDefault("victoria"), 5);
            Assert.Greater(g.Quests.Altura, 5, "una región entera sube la cuenta");
            Assert.Greater(g.Quests.ClaimAll(), 3, "y deja logros por cobrar");
        }

        [Test]
        public void NingunEncargoPideTirarNiPagar()
        {
            var src = Sources();
            src.Quests = src.Quests.Replace("\"event\": \"salmuera\", \"need\": 1 }", "\"event\": \"tirada\", \"need\": 1 }");
            Assert.Throws<DataValidationException>(() => MetaLoader.Load(src, TestKit.RealDb()));
        }
    }
}
