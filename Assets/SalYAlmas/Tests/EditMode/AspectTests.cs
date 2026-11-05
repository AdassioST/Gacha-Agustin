using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 7.3: Aspectos con su prioridad y las cuatro Reacciones Híbridas.</summary>
    public class AspectTests
    {
        [TestCase(Planet.Marte, Planet.Marte, Aspect.Conjuncion)]
        [TestCase(Planet.Sol, Planet.Saturno, Aspect.Oposicion)]
        [TestCase(Planet.Luna, Planet.Saturno, Aspect.Oposicion)]
        [TestCase(Planet.Marte, Planet.Venus, Aspect.Oposicion)]
        [TestCase(Planet.Mercurio, Planet.Jupiter, Aspect.Oposicion)]
        [TestCase(Planet.Mercurio, Planet.Sol, Aspect.Sextil)]
        [TestCase(Planet.Venus, Planet.Mercurio, Aspect.Sextil)]
        [TestCase(Planet.Sol, Planet.Jupiter, Aspect.Trigono)]
        [TestCase(Planet.Luna, Planet.Venus, Aspect.Trigono)]
        [TestCase(Planet.Sol, Planet.Luna, Aspect.Cuadratura)]
        [TestCase(Planet.Marte, Planet.Jupiter, Aspect.Cuadratura)]
        public void LosCincoAspectos(Planet a, Planet b, Aspect expected)
        {
            Assert.AreEqual(expected, AspectRules.Resolve(a, b));
            Assert.AreEqual(expected, AspectRules.Resolve(b, a), "el Aspecto no depende del orden del par");
        }

        [Test]
        public void Prioridad_GanaLaDeArriba()
        {
            // Sol y Saturno son diurnos (Trígono), pero son opuestos: gana Oposición.
            Assert.AreEqual(Aspect.Oposicion, AspectRules.Resolve(Planet.Sol, Planet.Saturno));
            // Luna (nocturna) y Saturno (diurno) serían Cuadratura, pero son opuestos.
            Assert.AreEqual(Aspect.Oposicion, AspectRules.Resolve(Planet.Luna, Planet.Saturno));
            // Mercurio con Mercurio cumple Sextil, pero gana Conjunción.
            Assert.AreEqual(Aspect.Conjuncion, AspectRules.Resolve(Planet.Mercurio, Planet.Mercurio));
            // Mercurio con Júpiter cumple Sextil, pero gana Oposición.
            Assert.AreEqual(Aspect.Oposicion, AspectRules.Resolve(Planet.Mercurio, Planet.Jupiter));
            // Sol con Sol cumple Trígono (misma secta), pero gana Conjunción.
            Assert.AreEqual(Aspect.Conjuncion, AspectRules.Resolve(Planet.Sol, Planet.Sol));
            // Mercurio con Luna cumple Sextil y podría cumplir Trígono o Cuadratura: gana Sextil.
            Assert.AreEqual(Aspect.Sextil, AspectRules.Resolve(Planet.Mercurio, Planet.Luna));
        }

        [TestCase(Planet.Sol, Planet.Saturno, HybridReaction.Eclipse)]
        [TestCase(Planet.Luna, Planet.Saturno, HybridReaction.HeladaNegra)]
        [TestCase(Planet.Marte, Planet.Venus, HybridReaction.EspinasRojas)]
        [TestCase(Planet.Mercurio, Planet.Jupiter, HybridReaction.Tempestad)]
        [TestCase(Planet.Sol, Planet.Luna, HybridReaction.None)]
        public void ReaccionDeCadaOposicion(Planet a, Planet b, HybridReaction reaction)
        {
            Assert.AreEqual(reaction, AspectRules.ReactionFor(a, b));
            Assert.AreEqual(reaction, AspectRules.ReactionFor(b, a));
        }

        // ------------------------------------------------------------------ Efectos dentro de una Cadena

        /// <summary>Cadena de tres brujas x, y, z (en ese orden) con los planetas dados contra un enemigo inofensivo.</summary>
        internal static Battle ChainBattle(Planet px, Planet py, Planet pz, out ChainEnded ended, out int mark,
            System.Action<Battle> before = null, params Planet[] weaknesses)
        {
            var e = Enemy("e", 10, 1000000, 10000000, weaknesses.Length > 0 ? weaknesses : new[] { Planet.Venus, Planet.Mercurio });
            var e2 = Enemy("e2", 10, 1000000, 10000000, Planet.Venus, Planet.Mercurio);
            var db = Db(new[]
            {
                Witch("x", px, Other(px), 130),
                Witch("y", py, Other(py), 120),
                Witch("z", pz, Other(pz), 110),
            }, new[] { e, e2 });
            foreach (var w in db.Witches.Values)
            {
                // Golpes separados para que no haya Spark (24 frames entre eslabones).
                SetChainFrames(w, new[] { 0 }, new[] { 0 }, new[] { 0 });
            }
            var b = Start(db, new[] { "x", "y", "z" });
            b.Resonance = 100;
            before?.Invoke(b);
            mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Chain(b.ActiveUnit.Id, Ids(b, "x", "y", "z"), false, b.Enemies[0].Id)));
            ended = b.Events.OfType<ChainEnded>().Last();
            return b;
        }

        private static Planet Other(Planet p) => p == Planet.Saturno ? Planet.Marte : Planet.Saturno;

        [Test]
        public void Conjuncion_Mas25PorCientoAlEslabonQueLaCompleta()
        {
            var b = ChainBattle(Planet.Marte, Planet.Marte, Planet.Sol, out _, out int mark);
            var asp = EventsSince<AspectFormed>(b, mark);
            Assert.AreEqual(Aspect.Conjuncion, asp[0].Aspect);
            var y = W(b, "y");
            var hit = EventsSince<HitLanded>(b, mark).Single(h => h.AttackerId == y.Id);
            // Enlace M_hab 1.0: (952 + 150) / 2 = 551 base; ×1.25 → 595 + 93.75 = 688.75
            Assert.AreEqual(689, hit.Damage);
            var z = W(b, "z");
            Assert.AreEqual(551, EventsSince<HitLanded>(b, mark).Single(h => h.AttackerId == z.Id).Damage, "el bono no pasa al siguiente par");
        }

        [Test]
        public void Oposicion_DisparaReaccionYSuma20DeSalinidadALasDos()
        {
            var b = ChainBattle(Planet.Sol, Planet.Saturno, Planet.Marte, out _, out int mark);
            var asp = EventsSince<AspectFormed>(b, mark)[0];
            Assert.AreEqual(Aspect.Oposicion, asp.Aspect);
            Assert.AreEqual(HybridReaction.Eclipse, asp.Reaction);
            Assert.AreEqual(1, EventsSince<ReactionTriggered>(b, mark).Count);
            // +5 por eslabón y +20 por la Oposición.
            Assert.AreEqual(25, W(b, "x").Salinity);
            Assert.AreEqual(25, W(b, "y").Salinity);
        }

        [Test]
        public void Sextil_Adelanta15PorCientoALaSegundaBruja()
        {
            var b = ChainBattle(Planet.Mercurio, Planet.Sol, Planet.Saturno, out _, out int mark);
            Assert.AreEqual(Aspect.Sextil, EventsSince<AspectFormed>(b, mark)[0].Aspect);
            var shift = EventsSince<ActionShifted>(b, mark).Single(s => s.Reason == "Sextil");
            Assert.AreEqual(W(b, "y").Id, shift.UnitId);
            Assert.AreEqual(0.15, shift.Percent, 1e-9);
        }

        [Test]
        public void Trigono_UnGranoYCuraDel5PorCiento()
        {
            var b = ChainBattle(Planet.Sol, Planet.Jupiter, Planet.Luna, out _, out int mark, before: bt =>
            {
                W(bt, "x").Hp = 5000;
                W(bt, "y").Hp = 5000;
                bt.Grains = 2;
            });
            Assert.AreEqual(Aspect.Trigono, EventsSince<AspectFormed>(b, mark)[0].Aspect);
            var grains = EventsSince<GrainsChanged>(b, mark).Single(g => g.Reason == "Trígono");
            Assert.AreEqual(1, grains.New - grains.Old);
            var heals = EventsSince<Healed>(b, mark).Where(h => h.Reason == "Trígono").ToList();
            Assert.AreEqual(2, heals.Count);
            Assert.IsTrue(heals.All(h => h.Amount == 500), "5 % de 10 000");
        }

        [Test]
        public void Cuadratura_Mas40PorCientoAFirmezaYMas10DeSalinidadAQuienLaProvoca()
        {
            var b = ChainBattle(Planet.Sol, Planet.Luna, Planet.Sol, out _, out int mark, null, Planet.Luna, Planet.Venus);
            Assert.AreEqual(Aspect.Cuadratura, EventsSince<AspectFormed>(b, mark)[0].Aspect);
            var y = W(b, "y");
            var hit = EventsSince<HitLanded>(b, mark).Single(h => h.AttackerId == y.Id);
            Assert.AreEqual(10 * 0.85 * 1.4, hit.ToughnessDamage, 1e-6);
            Assert.AreEqual(15, y.Salinity, "+5 por eslabón y +10 por provocar la Cuadratura");
            Assert.AreEqual(5, W(b, "x").Salinity);
        }

        [Test]
        public void LaCadenaNoGastaGranosYSuma5DeSalinidadPorEslabon()
        {
            var b = ChainBattle(Planet.Sol, Planet.Luna, Planet.Marte, out _, out int mark, bt => bt.Grains = 3);
            Assert.IsFalse(EventsSince<GrainsChanged>(b, mark).Any(g => g.New < g.Old));
            Assert.AreEqual(5, W(b, "z").Salinity);
        }
    }
}
