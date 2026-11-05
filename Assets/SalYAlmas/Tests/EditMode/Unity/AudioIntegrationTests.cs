#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SalYAlmas.Presentation;
using SalYAlmas.Ui;
using UnityEditor;
using UnityEngine;

namespace SalYAlmas.Tests
{
    public class AudioIntegrationTests
    {
        private const string SfxPath = "Assets/SalYAlmas/Audio/Resources/SfxTemporal";
        private const string UiSfxPath = "Assets/SalYAlmas/Art/Ui/Resources/Ui/sfx";

        [Test]
        public void LaInterfazConservaLosDieciseisEfectosSinCompresionConPerdida()
        {
            var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { UiSfxPath }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Assert.That(paths.Length, Is.EqualTo(16));
            foreach (var path in paths)
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.defaultSampleSettings.compressionFormat, Is.EqualTo(AudioCompressionFormat.PCM), path);
                Assert.That(importer.defaultSampleSettings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad), path);
                Assert.That(importer.defaultSampleSettings.sampleRateSetting, Is.EqualTo(AudioSampleRateSetting.PreserveSampleRate), path);
            }
        }

        [TestCase("drop")]
        [TestCase("page")]
        public void LosAtaquesPulidosComienzanEnCero(string id)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(UiSfxPath + "/" + id + ".wav");
            var samples = new float[clip.samples];
            Assert.That(clip.GetData(samples, 0), Is.True);
            Assert.That(samples[0], Is.EqualTo(0f).Within(1f / 32768));
            Assert.That(samples[samples.Length - 1], Is.EqualTo(0f).Within(1f / 32768));
        }

        [Test]
        public void ElRiserTerminaConUnaColaSuave()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(UiSfxPath + "/riser.wav");
            var samples = new float[clip.samples];
            Assert.That(clip.GetData(samples, 0), Is.True);
            double tail = samples.Skip(samples.Length - clip.frequency / 100).Average(x => (double)x * x);
            Assert.That(tail, Is.LessThan(0.0001), "Los últimos 10 ms deben caer por debajo de −40 dBFS RMS.");
        }

        [Test]
        public void LaInterfazSilenciadaNoCreaVocesNiReduceLaMusica()
        {
            var go = new GameObject("Prueba de silencio de interfaz");
            try
            {
                var player = go.AddComponent<TempSfxPlayer>();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                if (typeof(TempSfxPlayer).GetField("_voices", flags).GetValue(player) == null)
                    typeof(TempSfxPlayer).GetMethod("Awake", flags).Invoke(player, null);
                typeof(TempSfxPlayer).GetProperty("Muted").SetValue(player, true);
                int sources = go.GetComponentsInChildren<AudioSource>().Length, requests = 0;
                player.MusicDuckRequested += (seconds, factor) => requests++;
                UiSfx.Bind(player);
                UiSfx.Play("reveal5");
                UiSfx.Scroll();
                Assert.That(UiSfx.Muted, Is.True);
                Assert.That(requests, Is.Zero);
                Assert.That(go.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(sources));
                Assert.That(go.GetComponentsInChildren<AudioSource>().Any(x => x.isPlaying), Is.False);
                UiSfx.Unbind(player);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void LosEfectosSilenciadosNoReducenElVolumenDeLaMusica()
        {
            var go = new GameObject("Prueba de silencio independiente");
            try
            {
                var player = go.AddComponent<TempSfxPlayer>();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                if (typeof(TempSfxPlayer).GetField("_voices", flags).GetValue(player) == null)
                    typeof(TempSfxPlayer).GetMethod("Awake", flags).Invoke(player, null);
                typeof(TempSfxPlayer).GetProperty("Muted").SetValue(player, true);
                int requests = 0;
                player.MusicDuckRequested += (seconds, factor) => requests++;
                player.NotifyPull(5);
                Assert.That(requests, Is.Zero);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void LaRevisionDelHubNoReemplazaUnPerfilIncompatible()
        {
            var store = new SalYAlmas.Core.Save.MemorySaveStore();
            var profile = Phase2Kit.NewGame().Profile;
            profile.Version = SalYAlmas.Core.Save.PlayerProfile.CurrentVersion + 1;
            string json = SalYAlmas.Core.Save.ProfileSerializer.ToJson(profile);
            store.Save("perfil", json);
            Assert.Throws<System.InvalidOperationException>(() => new MetaHub(Phase2Kit.Meta(), store, null));
            Assert.That(store.Load("perfil"), Is.EqualTo(json));
        }

        [Test]
        public void ElCatalogoCompletoConservaPcmYElPresupuesto()
        {
            var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { SfxPath }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Assert.That(paths.Length, Is.EqualTo(470));
            Assert.That(paths.Sum(p => new FileInfo(p).Length), Is.LessThanOrEqualTo(30000000));
            foreach (var path in paths)
            {
                var settings = ((AudioImporter)AssetImporter.GetAtPath(path)).defaultSampleSettings;
                Assert.That(settings.compressionFormat, Is.EqualTo(AudioCompressionFormat.PCM), path);
                Assert.That(settings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad), path);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                Assert.That(clip.frequency, Is.EqualTo(48000), path);
                Assert.That(clip.channels, Is.EqualTo(1), path);
            }
        }

        [Test]
        public void VariacionesSinRepeticionNiConsumoDelAzarDeUnity()
        {
            var go = new GameObject("Prueba de resolución de audio");
            var state = Random.state;
            try
            {
                var player = go.AddComponent<TempSfxPlayer>();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                if (typeof(TempSfxPlayer).GetField("_voices", flags).GetValue(player) == null)
                    typeof(TempSfxPlayer).GetMethod("Awake", flags).Invoke(player, null);
                var resolve = typeof(TempSfxPlayer).GetMethod("Resolve", flags);
                string previous = null;
                for (int i = 0; i < 40; i++)
                {
                    var next = (string)resolve.Invoke(player, new object[] { "sfx_ui_tocar", new string[0] });
                    Assert.That(next, Is.Not.Null);
                    Assert.That(next, Is.Not.EqualTo(previous));
                    previous = next;
                }
                // Entre dos disparos admitidos puede haber muchos intentos descartados por el cooldown.
                var heard = (System.Collections.Generic.Dictionary<string, string>)typeof(TempSfxPlayer).GetField("_lastHeardVariant", flags).GetValue(player);
                heard["sfx_ui_tocar"] = previous;
                for (int i = 0; i < 40; i++)
                    Assert.That(resolve.Invoke(player, new object[] { "sfx_ui_tocar", new string[0] }), Is.Not.EqualTo(previous));
                Assert.That(JsonUtility.ToJson(Random.state), Is.EqualTo(JsonUtility.ToJson(state)));
                Assert.That(resolve.Invoke(player, new object[] { "sfx_partitura_nota", new[] { "luna", "cierre" } }), Does.StartWith("sfx_partitura_nota__luna__cierre"));
                Assert.That(resolve.Invoke(player, new object[] { "sfx_ui_tocar__v2", new string[0] }), Is.EqualTo("sfx_ui_tocar__v2"));
            }
            finally { Random.state = state; Object.DestroyImmediate(go); }
        }
    }
}
#endif
