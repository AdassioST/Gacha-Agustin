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
                    Assert.GreaterOrEqual(r.xMin, 1032, "no invade las aliadas");
                    Assert.LessOrEqual(r.xMax, 1580, "no invade las órdenes");
                    Assert.GreaterOrEqual(r.yMin, 230, "no invade el rótulo de bando");
                    Assert.LessOrEqual(r.yMax, 800, "no invade las tarjetas de equipo");
                    for (int j = 0; j < i; j++) Assert.IsFalse(r.Overlaps(plates[j]), "placas que se enciman");
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
