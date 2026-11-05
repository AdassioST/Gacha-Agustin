using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>GDD 14: cinturón de tres ranuras, una poción por turno, las once Salmueras.</summary>
    public class PotionTests
    {
        private static Battle WithBelt(out WitchUnit a, params string[] potions)
        {
            var db = Db(new[] { Witch("a", Planet.Marte, Planet.Saturno, 100), Witch("b", Planet.Luna, Planet.Sol, 60) },
                        new[] { Enemy("e", 50, 1000, 10000000, Planet.Marte, Planet.Luna) });
            var b = Start(db, new[] { "a", "b" }, configure: s =>
            {
                foreach (var p in potions) s.Belt.Add(new BeltSlot(p, 2));
                s.BlackSalt = 0;
            });
            a = W(b, "a");
            return b;
        }

        [Test]
        public void ElCatalogoTieneLasOnceSalmueras()
        {
            var ids = RealDb().Potions.Keys.ToList();
            CollectionAssert.AreEquivalent(new[]
            {
                "salmuera_aurea", "destilado_rocio", "elixir_sal_negra", "esencia_alma", "filtro_pureza", "leche_luna",
                "vinagre_marte", "tonico_azogue", "balsamo_cuprico", "plomo_liquido", "agua_olvido",
            }, ids);
        }

        [Test]
        public void CinturonDeTresRanuras_UnaPorTurno_SinGastarElTurno()
        {
            Assert.Throws<System.ArgumentException>(() => WithBelt(out _, "salmuera_aurea", "esencia_alma", "leche_luna", "tonico_azogue"));
            var b = WithBelt(out var a, "esencia_alma", "tonico_azogue", "leche_luna");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(a, b.ActiveUnit, "acción libre");
            Assert.IsFalse(b.Execute(BattleCommand.Potion(a.Id, 1, a.Id)).Ok, "una por turno");
            Assert.AreEqual(1, b.Belt[0].Charges);
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            TurnOf(b, "a");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 1, a.Id)));
        }

        [Test]
        public void SalmueraAurea()
        {
            var b = WithBelt(out var a, "salmuera_aurea");
            double atk = b.Atk(a);
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(4, b.Grains, "+1 Grano");
            Assert.AreEqual(atk * 1.1, b.Atk(a), 1e-9, "+10 % de ataque");
            Assert.AreEqual(2, a.Find(StatusKind.AtkPct).TurnsLeft);
        }

        [Test]
        public void DestiladoDeRocio()
        {
            var b = WithBelt(out var a, "destilado_rocio");
            a.Salinity = 100;
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(60, a.Salinity, "−40 de Salinidad");
            Ok(b.Execute(BattleCommand.Basic(a.Id, -1)));
            TurnOf(b, "a");
            Assert.AreEqual(170.0, b.ElapsedAv, 1e-9, "actuó en 100 y adelantó 30 %: vuelve a los 70 AV y no a los 100");
        }

        [Test]
        public void ElixirDeSalNegra()
        {
            var b = WithBelt(out var a, "elixir_sal_negra");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            int mark = b.Events.Count;
            a.Hp = 5000;
            b.Heal(a, 1000, "prueba");
            Assert.AreEqual(4000, a.Hp, "las curaciones se invierten");
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(2204, EventsSince<HitLanded>(b, mark).Single().Damage, "el siguiente Golpe Astral hace el doble");
        }

        [Test]
        public void EsenciaDeAlma()
        {
            var b = WithBelt(out var a, "esencia_alma");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(30, a.Anima);
        }

        [Test]
        public void FiltroDePureza()
        {
            var b = WithBelt(out var a, "filtro_pureza");
            Assert.IsFalse(b.Execute(BattleCommand.Vislumbre(a.Id)).Ok, "sin Sal Negra");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Ok(b.Execute(BattleCommand.Vislumbre(a.Id)));
        }

        [Test]
        public void LecheDeLuna()
        {
            var b = WithBelt(out var a, "leche_luna");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(2000, a.ShieldAmount, 1e-9, "20 % de 10 000");
            Assert.AreEqual(2000, W(b, "b").ShieldAmount, 1e-9, "a todas");
            int dealt = b.ApplyDamage(a, 2500, out int absorbed);
            Assert.AreEqual(2000, absorbed);
            Assert.AreEqual(500, dealt);
        }

        [Test]
        public void VinagreDeMarte()
        {
            var b = WithBelt(out var a, "vinagre_marte");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            int mark = b.Events.Count;
            Ok(b.Execute(BattleCommand.Spell(a.Id, -1)));
            Assert.AreEqual(20 * 0.85 * 1.5, EventsSince<HitLanded>(b, mark).Single().ToughnessDamage, 1e-6);
            Assert.IsFalse(a.Has(StatusKind.NextToughness), "solo el siguiente golpe");
        }

        [Test]
        public void TonicoDeAzogue()
        {
            var b = WithBelt(out var a, "tonico_azogue");
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(120.0, b.Speed(a), 1e-9);
        }

        [Test]
        public void BalsamoCuprico()
        {
            var b = WithBelt(out var a, "balsamo_cuprico");
            a.Hp = 5000;
            b.AddStatus(a, StatusKind.DefenseDown, 0.2, 2, "Herida", isDebuff: true);
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)));
            Assert.AreEqual(7500, a.Hp, "cura 25 %");
            Assert.IsFalse(a.Has(StatusKind.DefenseDown), "limpia un estado negativo");
        }

        [Test]
        public void PlomoLiquido()
        {
            var b = WithBelt(out var a, "plomo_liquido");
            var e = b.Enemies[0];
            double before = e.Distance;
            Ok(b.Execute(BattleCommand.Potion(a.Id, 0, e.Id)));
            Assert.AreEqual(before + 2000, e.Distance, 1e-9, "retrasa 20 %");
        }

        [Test]
        public void AguaDeOlvido_SoloFueraDeCombate()
        {
            var b = WithBelt(out var a, "agua_olvido");
            Assert.IsFalse(b.Execute(BattleCommand.Potion(a.Id, 0, a.Id)).Ok);
            Assert.AreEqual(2, b.Belt[0].Charges);
        }
    }
}
