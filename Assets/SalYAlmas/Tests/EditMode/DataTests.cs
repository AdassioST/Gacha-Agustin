using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using static SalYAlmas.Tests.TestKit;

namespace SalYAlmas.Tests
{
    /// <summary>Los JSON de Assets/SalYAlmas/Data se cargan y se validan en el núcleo.</summary>
    public class DataTests
    {
        [Test]
        public void LasSeisBrujasDeLaFase1()
        {
            var db = RealDb();
            CollectionAssert.AreEqual(new[] { "marisal", "selenia", "solenne", "tormentina", "zarigueya", "topo" }, db.WitchOrder);
            foreach (var w in db.Witches.Values)
            {
                Assert.AreEqual(Grade.B, w.Grade, w.Id + ": pureza de grado B");
                Assert.AreNotEqual(w.Primary, w.Secondary);
                Assert.IsNotNull(w.Basic, w.Id);
                Assert.IsNotNull(w.Spell, w.Id);
                foreach (var m in new[] { w.Opening, w.Link, w.Closing })
                {
                    Assert.IsNotNull(m, w.Id);
                    Assert.IsNotEmpty(m.Hits, w.Id + ": los movimientos de Cadena declaran sus frames");
                }
                Assert.IsTrue(w.Ascensions.All(a => a != null), w.Id + ": Ascensión I, II y III");
                Assert.IsFalse(string.IsNullOrEmpty(w.Regencia.Name), w.Id + ": Regencia");
                if (w.Rarity == 3) Assert.AreEqual(SelfCastForm.None, w.SelfCast, w.Id + ": 3★ sin Autohechizo (GDD 12.1)");
                else Assert.AreNotEqual(SelfCastForm.None, w.SelfCast, w.Id);
            }
        }

        [Test]
        public void MezclasYFormasDeAutohechizoDelGdd()
        {
            var db = RealDb();
            void Check(string id, Planet p, Planet s, SelfCastForm form)
            {
                Assert.AreEqual(p, db.Witch(id).Primary, id);
                Assert.AreEqual(s, db.Witch(id).Secondary, id);
                Assert.AreEqual(form, db.Witch(id).SelfCast, id);
            }
            // GDD 18.2 y 17
            Check("marisal", Planet.Marte, Planet.Saturno, SelfCastForm.AgujaYSello);
            Check("selenia", Planet.Luna, Planet.Saturno, SelfCastForm.Hilos);
            Check("solenne", Planet.Sol, Planet.Mercurio, SelfCastForm.FrenesiSalNegra);
            Check("tormentina", Planet.Jupiter, Planet.Mercurio, SelfCastForm.Hilos);
            Check("zarigueya", Planet.Saturno, Planet.Luna, SelfCastForm.None);
            Check("topo", Planet.Saturno, Planet.Marte, SelfCastForm.None);
            Assert.AreEqual(0.5, db.Witch("zarigueya").PassiveAtkMultiplier, 1e-9, "su Ascendente se hace el muerto: −50 % de ataque");
            Assert.IsTrue(db.Witch("topo").DealsNoDamage, "el Topo no hace daño");
            // Ficha del Libro Mayor (GDD 13.1)
            var m = db.Witch("marisal");
            Assert.AreEqual((52, 9300, 1840.0, 112.0), (m.Level, m.MaxHp, m.Atk, m.Speed));
        }

        [Test]
        public void TresEnemigosNormalesYElJefe()
        {
            var db = RealDb();
            Assert.AreEqual(3, db.Enemies.Values.Count(e => !e.IsBoss));
            var boss = db.Enemy("guardian");
            Assert.IsTrue(boss.IsBoss);
            Assert.AreEqual("Guardián Corrupto del Fresno", boss.Name);
            foreach (var e in db.Enemies.Values)
            {
                Assert.That(e.Weaknesses.Count, Is.InRange(2, 3), e.Id + ": de dos a tres planetas débiles");
                Assert.Greater(e.Toughness, 0);
                Assert.IsNotEmpty(e.Pattern, e.Id + ": IA por patrones");
            }
            Assert.IsTrue(boss.Skills.Any(s => s.IsUltimate && s.Charge), "el jefe tiene un ataque definitivo telegrafiado");
            Assert.IsNotNull(boss.Phase2);
        }

        [Test]
        public void ReglasDelJson_RespetanElGdd()
        {
            var fromJson = RulesLoader.Load(ReadData("rules.json"));
            var canon = new CombatRules();
            int checkedFields = 0;
            foreach (var section in typeof(CombatRules).GetFields())
            {
                var a = section.GetValue(fromJson);
                var b = section.GetValue(canon);
                foreach (var f in section.FieldType.GetFields())
                {
                    if (f.GetCustomAttribute<GddAttribute>() == null) continue;
                    Assert.AreEqual(f.GetValue(b), f.GetValue(a), $"{section.Name}.{f.Name} viene del GDD {f.GetCustomAttribute<GddAttribute>().Section}");
                    checkedFields++;
                }
            }
            Assert.Greater(checkedFields, 50);
        }

        [Test]
        public void ValoresClaveDelGdd()
        {
            var r = new CombatRules();
            Assert.AreEqual(10000, r.TurnOrder.AvBase);
            Assert.AreEqual(150, r.TurnOrder.FirstCycleAv);
            Assert.AreEqual(100, r.TurnOrder.CycleAv);
            Assert.AreEqual((5, 3), (r.Grains.Max, r.Grains.Start));
            Assert.AreEqual(150, r.Salinity.Max);
            Assert.AreEqual(300, r.Anima.Max);
            Assert.AreEqual(100, r.Resonance.Max);
            Assert.AreEqual(24, r.Chain.LinkSpacingFrames);
            Assert.AreEqual(2, r.Chain.SparkWindowFrames);
            Assert.AreEqual(0.5, r.Chain.SparkDamageBonus);
            Assert.AreEqual(20, r.Resonance.SparkCapPerChain);
            Assert.AreEqual(3, r.Potions.BeltSlots);
        }

        [Test]
        public void ElValidadorJuntaLosErrores()
        {
            const string witches = @"{ ""witches"": [ { ""id"": ""x"", ""name"": ""X"", ""rarity"": 3,
                ""mix"": { ""primary"": ""marte"", ""secondary"": ""marte"" },
                ""stats"": { ""level"": 50, ""hp"": 100, ""atk"": 10, ""spd"": 100 },
                ""selfCast"": { ""form"": ""hilos"", ""name"": ""h"" },
                ""moves"": {
                  ""basic"": { ""id"": ""b"", ""name"": ""b"", ""multiplier"": 1, ""hits"": [[0, 0.5], [4, 0.2]] },
                  ""spell"": { ""id"": ""s"", ""name"": ""s"" },
                  ""opening"": { ""id"": ""o"", ""name"": ""o"", ""hits"": [200] },
                  ""link"": { ""id"": ""l"", ""name"": ""l"", ""hits"": [] },
                  ""closing"": { ""id"": ""c"", ""name"": ""c"", ""hits"": [0] },
                  ""ascension1"": { ""id"": ""a1"", ""name"": ""a1"", ""hits"": [0] },
                  ""ascension2"": { ""id"": ""a2"", ""name"": ""a2"", ""hits"": [0] },
                  ""ascension3"": { ""id"": ""a3"", ""name"": ""a3"", ""hits"": [0] } },
                ""regencia"": { ""name"": ""r"", ""description"": ""d"" } } ] }";
            const string enemies = @"{ ""enemies"": [ { ""id"": ""e"", ""name"": ""E"", ""level"": 50, ""hp"": 10, ""atk"": 1, ""spd"": 100,
                ""toughness"": 10, ""weaknesses"": [""sol""], ""attackPlanet"": ""luna"", ""skills"": [], ""pattern"": [""nada""] } ] }";
            var src = new DataSources
            {
                Rules = @"{ ""grains"": { ""maxx"": 4 } }",
                Witches = witches,
                Enemies = enemies,
                Encounters = @"{ ""encounters"": [ { ""id"": ""z"", ""name"": ""Z"", ""enemies"": [""fantasma""] } ] }",
                Potions = @"{ ""potions"": [] }",
            };
            var ex = Assert.Throws<DataValidationException>(() => DataLoader.Load(src));
            string all = string.Join("\n", ex.Errors);
            StringAssert.Contains("regla desconocida: maxx", all);
            StringAssert.Contains("dos planetas distintos", all);
            StringAssert.Contains("Autohechizo", all);
            StringAssert.Contains("deben sumar 1", all);
            StringAssert.Contains("1.8 s", all);
            StringAssert.Contains("frames de golpe", all);
            StringAssert.Contains("GDD 9.4", all);
            StringAssert.Contains("habilidad que no existe", all);
            StringAssert.Contains("enemigo desconocido", all);
            Assert.GreaterOrEqual(ex.Errors.Count, 9);
        }

        [Test]
        public void ElLectorJsonAceptaComentariosYReportaLaLinea()
        {
            var n = Json.Parse("// encabezado\n{ \"a\": 1, /* nota */ \"b\": [1, 2,], // TUNING\n \"c\": \"ñ\\u00e1\" }");
            Assert.AreEqual(1.0, n.Get("a").AsNumber);
            Assert.AreEqual(2, n.Get("b").Items.Count);
            Assert.AreEqual("ñá", n.Get("c").AsString);
            var ex = Assert.Throws<JsonParseException>(() => Json.Parse("{\n\"a\": 1,\n\"a\": 2 }", "x.json"));
            StringAssert.Contains("x.json:3", ex.Message);
        }

        [Test]
        public void LosDatosMarcanLosValoresPropuestosConTuning()
        {
            foreach (var file in DataSources.FileNames)
            {
                if (file == "encounters.json") continue;
                StringAssert.Contains("// TUNING", ReadData(file), file);
            }
        }

        [Test]
        public void LosFramesDeCadenaCabenEnLosTiemposDelGdd()
        {
            var db = RealDb();
            foreach (var w in db.Witches.Values)
                foreach (var m in new[] { w.Opening, w.Link, w.Closing })
                    Assert.LessOrEqual(m.LastHitFrame, db.Rules.Chain.MaxLinkFrames, $"{w.Id}.{m.Id}: un eslabón dura máximo 1.8 s");
        }
    }
}
