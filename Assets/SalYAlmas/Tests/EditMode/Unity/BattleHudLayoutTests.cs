#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Presentation;
using UnityEngine;

namespace SalYAlmas.Tests
{
    public class BattleHudLayoutTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EnemigosAmontonadosConservanPlacasSeparadasYFueraDeLosControles(int count)
        {
            // Cinco es el máximo validado por DataLoader. Las cabezas coincidentes reproducen
            // la peor oclusión; también probamos proyecciones que salen del lienzo.
            foreach (var head in new[] { new Vector2(1200, 490), new Vector2(-500, -100), new Vector2(3000, 2000) })
            {
                var plates = BattleHudLayout.ResolveEnemyPlates(Enumerable.Repeat(head, count).ToArray());
                Assert.AreEqual(count, plates.Count);
                for (int i = 0; i < plates.Count; i++)
                {
                    var r = plates[i];
                    // El HUD vive en las orillas: arriba los turnos y controles, abajo las cartas y el abanico de órdenes.
                    Assert.GreaterOrEqual(r.xMin, 980, "no invade las aliadas");
                    Assert.LessOrEqual(r.xMax, 1900, "no sale de la pantalla");
                    Assert.GreaterOrEqual(r.yMin, 112, "no invade la línea de turnos ni los controles");
                    Assert.LessOrEqual(r.yMax, 640, "no tapa las figuras ni el abanico de órdenes");
                    for (int j = 0; j < i; j++) Assert.IsFalse(r.Overlaps(plates[j]), "placas que se enciman");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void BarrasDeJefeQuedanArribaSinTaparCabezasNiControles(int count)
        {
            var bars = BattleHudLayout.BossBars(count);
            Assert.AreEqual(Mathf.Min(count, BattleHudLayout.MaxBossBars), bars.Count);
            for (int i = 0; i < bars.Count; i++)
            {
                var r = bars[i];
                Assert.GreaterOrEqual(r.xMin, 540, "no invade la línea de turnos");
                Assert.LessOrEqual(r.xMax, 1440, "no invade los controles de la derecha");
                Assert.GreaterOrEqual(r.yMin, 108, "no tapa la Resonancia");
                Assert.LessOrEqual(r.yMax, BattleStage.HeadLimit, "ninguna cabeza de gigante queda debajo");
                Assert.Greater(r.width, 300, "cabe el nombre, la intención y la vida");
                for (int j = 0; j < i; j++) Assert.IsFalse(r.Overlaps(bars[j]), "barras que se enciman");
            }
        }

        [TestCase(1)]
        [TestCase(4)]
        public void ConBarraDeJefeLasPlacasDeLaEscoltaBajanDeElla(int count)
        {
            float top = BattleHudLayout.PlatesTop(true);
            var bars = BattleHudLayout.BossBars(2);
            foreach (var head in new[] { new Vector2(1100, 150), new Vector2(1700, 600), new Vector2(-500, -100) })
            {
                var plates = BattleHudLayout.ResolveEnemyPlates(Enumerable.Repeat(head, count).ToArray(), top);
                foreach (var r in plates)
                {
                    Assert.GreaterOrEqual(r.yMin, top);
                    Assert.LessOrEqual(r.yMax, 640);
                    Assert.IsFalse(bars.Any(b => b.Overlaps(r)), "una placa tapa la barra de jefe");
                }
            }
        }

        [Test]
        public void ProyeccionesMixtasNoEncimanPlacasEnMilDistribuciones()
        {
            var random = new System.Random(20261105);
            for (int run = 0; run < 1000; run++)
            {
                var heads = new List<Vector2>();
                for (int i = 0; i < 1 + run % 5; i++) heads.Add(new Vector2(800 + random.Next(1000), 200 + random.Next(700)));
                var plates = BattleHudLayout.ResolveEnemyPlates(heads);
                for (int i = 0; i < plates.Count; i++)
                    for (int j = i + 1; j < plates.Count; j++) Assert.IsFalse(plates[i].Overlaps(plates[j]), $"distribución {run}");
            }
        }
    }
}
#endif
