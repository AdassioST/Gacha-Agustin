#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.EditorTools;
using SalYAlmas.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SalYAlmas.Tests
{
    /// <summary>Pruebas que solo tienen sentido dentro de Unity: los TextAsset, la escena y el runtime de Unity.</summary>
    public class UnityIntegrationTests
    {
        private static string Text(string file) => AssetDatabase.LoadAssetAtPath<TextAsset>(BattleSceneBuilder.DataDir + "/" + file).text;

        [Test]
        public void LosJsonSeImportanComoTextAssetYSeValidan()
        {
            var db = DataLoader.Load(new DataSources
            {
                Rules = Text("rules.json"),
                Witches = Text("witches.json"),
                Enemies = Text("enemies.json"),
                Encounters = Text("encounters.json"),
                Potions = Text("potions.json"),
            });
            Assert.AreEqual(6, db.Witches.Count);
            Assert.IsTrue(db.Enemies.ContainsKey("guardian"));
        }

        [Test]
        public void ElConstructorArmaLaEscenaConTodoAsignado()
        {
            // En modo batch la escena activa no tiene título y Unity no deja abrir otra aditiva junto a ella.
            bool alone = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.Length == 0;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, alone ? NewSceneMode.Single : NewSceneMode.Additive);
            try
            {
                var game = BattleSceneBuilder.Populate(scene, new Material[7], null, null);
                Assert.IsNotNull(game.BattleCamera);
                Assert.IsNotNull(game.RulesJson);
                Assert.IsNotNull(game.WitchesJson);
                Assert.IsNotNull(game.EnemiesJson);
                Assert.IsNotNull(game.EncountersJson);
                Assert.IsNotNull(game.PotionsJson);
                Assert.IsNotNull(game.EconomyJson);
                Assert.IsNotNull(game.RosterJson);
                Assert.IsNotNull(game.ProgressionJson);
                Assert.IsNotNull(game.WeaponsJson);
                Assert.IsNotNull(game.BrewingJson);
                Assert.IsNotNull(game.GachaJson);
                Assert.IsNotNull(game.FresnoJson);
                Assert.IsNotNull(game.FresnoEnemiesJson);
                Assert.AreEqual(7, game.PlanetMaterials.Length);
                Assert.IsTrue(scene.GetRootGameObjects().Any(g => g.GetComponent<Light>() != null));
            }
            finally
            {
                if (!alone) EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ElMismoCombateDaLoMismoEnElRuntimeDeUnity()
        {
            // El runtime de Unity (Mono) debe dar exactamente los mismos eventos que dos corridas seguidas.
            var setup = new BattleSetup { Team = { "marisal", "selenia", "solenne", "tormentina" }, GuestId = "zarigueya", EncounterId = "guardian", Seed = 99 };
            var a = new Battle(TestKit.RealDb(), setup);
            var b = new Battle(TestKit.RealDb(), setup);
            a.Start();
            b.Start();
            new AutoPilot().RunToEnd(a);
            new AutoPilot().RunToEnd(b);
            CollectionAssert.AreEqual(a.Events.Select(e => e.Serialize()), b.Events.Select(e => e.Serialize()));
        }

        [Test]
        public void HayColorParaCadaPlanetaYBanda()
        {
            foreach (var p in SalYAlmas.Core.Magic.Planets.All) Assert.AreNotEqual(Color.clear, Palette.Planet(p));
            foreach (SalYAlmas.Core.Magic.SalinityBand band in System.Enum.GetValues(typeof(SalYAlmas.Core.Magic.SalinityBand)))
                Assert.AreNotEqual(Color.clear, Palette.Band(band));
        }
    }
}
#endif
