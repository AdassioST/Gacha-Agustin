using System;
using System.Collections;
using System.IO;
using System.Linq;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// Revisión automática de la interfaz: con "-capturas carpeta [pantalla...]" recorre las pantallas, las toca de verdad y
    /// deja una captura por paso. Cualquier excepción queda en errores.txt. Solo existe en compilaciones de desarrollo.
    /// </summary>
    public sealed class UiCapture : MonoBehaviour
    {
        public UiApp App;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private static string _folder;
        private static string[] _only;
        private static bool _checked;
        private string _errors = "";

        public static bool Active
        {
            get
            {
                if (_checked) return _folder != null;
                _checked = true;
                var a = Environment.GetCommandLineArgs();
                for (int i = 0; i + 1 < a.Length; i++)
                    if (a[i] == "-capturas")
                    {
                        _folder = a[i + 1];
                        _only = a.Skip(i + 2).Where(s => !s.StartsWith("-", StringComparison.Ordinal)).ToArray();
                    }
                return _folder != null;
            }
        }

        private bool Want(string n) => _only.Length == 0 || _only.Contains(n);

        private IEnumerator Shot(string name, float wait = 1.0f)
        {
            yield return new WaitForSecondsRealtime(wait);
            ScreenCapture.CaptureScreenshot(Path.Combine(_folder, name + ".png"));
            yield return new WaitForSecondsRealtime(0.25f);
        }

        private void OnEnable() => Application.logMessageReceived += OnLog;
        private void OnDisable() => Application.logMessageReceived -= OnLog;

        private void OnLog(string msg, string stack, LogType t)
        {
            if (t == LogType.Exception || t == LogType.Error || t == LogType.Warning && msg.Contains("Falta "))
            {
                _errors += t + ": " + msg + "\n" + stack + "\n";
                try { File.WriteAllText(Path.Combine(_folder, "errores.txt"), _errors); } catch { }
            }
        }

        private static UiBtn FindBtn(string name)
        {
            foreach (var b in FindObjectsByType<UiBtn>(FindObjectsInactive.Exclude))
                if (b.name == name) return b;
            return null;
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Path.Combine(_folder, "errores.txt"), "");
            App.Hub.DebugTestProfile();
            var g = App.Game.Game;
            g.Ledger.Apply(Core.Economy.Res.SalesDeBano, 60000, "prueba", "Capturas");
            yield return null;

            if (Want("home"))
            {
                App.DebugShow(MetaHub.Screen.Home);
                App.ForceRefresh();
                yield return Shot("home_entrada", 0.25f);
                yield return Shot("home", 1.6f);
                FindBtn("Favorita")?.Click?.Invoke();
                yield return Shot("home_toque", 0.45f);
            }
            if (Want("coven"))
            {
                App.DebugShow(MetaHub.Screen.Coven);
                yield return Shot("coven", 1.5f);
                FindBtn("selenia")?.Click?.Invoke();
                yield return Shot("coven_hoja", 1f);
            }
            if (Want("summon"))
            {
                App.DebugShow(MetaHub.Screen.Summon);
                yield return Shot("summon", 1.4f);
                FindBtn("limitado_marisal")?.Click?.Invoke();
                yield return Shot("summon_limitado", 0.8f);
            }
            if (Want("pull"))
            {
                App.DebugShow(MetaHub.Screen.Summon);
                yield return new WaitForSecondsRealtime(0.4f);
                DemoPull(5);
                yield return Shot("pull_1_caida", 0.75f);
                yield return Shot("pull_2_impactos", 0.75f);
                yield return Shot("pull_3_hervor", 1.4f);
                yield return Shot("pull_4_estallido", 1.05f);
                yield return Shot("pull_5_emerge", 0.7f);
                yield return Shot("pull_6_pie", 1.9f);
                // Tocar pasa a la siguiente bruja: 4★, 4★ y al final la 5★.
                FindBtn("Toque")?.Click?.Invoke();
                yield return Shot("pull_6b_siguiente", 1.6f);
                FindBtn("Toque")?.Click?.Invoke();
                yield return Shot("pull_6c_cinco", 2.0f);
                FindBtn("Saltar")?.Click?.Invoke();
                yield return Shot("pull_7_cuadricula", 2.6f);
                FindBtn("Cerrar")?.Click?.Invoke();
                yield return new WaitForSecondsRealtime(0.8f);
            }
            if (Want("legacy"))
            {
                foreach (var s in new[] { MetaHub.Screen.Forge, MetaHub.Screen.Cauldron, MetaHub.Screen.Fresno, MetaHub.Screen.Story, MetaHub.Screen.Shop, MetaHub.Screen.Settings, MetaHub.Screen.Odds, MetaHub.Screen.Ledger, MetaHub.Screen.Mirror })
                {
                    App.DebugShow(s);
                    yield return Shot("hub_" + s, 0.8f);
                }
                App.DebugShow(MetaHub.Screen.Home);
            }
            Debug.Log("Capturas terminadas.");
            Application.Quit();
        }

        /// <summary>Resultado fabricado para ver la secuencia completa con cada rareza, sin tocar el generador.</summary>
        private void DemoPull(int topRarity)
        {
            var list = new System.Collections.Generic.List<PullResult>
            {
                new PullResult { Rarity = 5, Kind = "bruja", WitchId = "cervanda", Featured = true },
                new PullResult { Rarity = 4, Kind = "bruja", WitchId = "brea" },
                new PullResult { Rarity = 3, Kind = "sal", SaltPlanet = Core.Magic.Planet.Marte, SaltAmount = 40 },
                new PullResult { Rarity = 3, Kind = "polvo", Dust = 120 },
                new PullResult { Rarity = 3, Kind = "sal", SaltPlanet = Core.Magic.Planet.Luna, SaltAmount = 40 },
                new PullResult { Rarity = 4, Kind = "bruja", WitchId = "cempa" },
                new PullResult { Rarity = 3, Kind = "sal", SaltPlanet = Core.Magic.Planet.Venus, SaltAmount = 40 },
                new PullResult { Rarity = 3, Kind = "bruja", WitchId = "axa" },
                new PullResult { Rarity = 3, Kind = "sal", SaltPlanet = Core.Magic.Planet.Sol, SaltAmount = 40 },
                new PullResult { Rarity = 3, Kind = "polvo", Dust = 120 },
            };
            App.DebugPull(list);
        }
#else
        public static bool Active => false;
#endif
    }
}
