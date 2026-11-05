using System;
using System.Collections;
using System.IO;
using System.Linq;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Presentation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// Revisión automática de la interfaz: con "-capturas carpeta [pantalla...]" recorre las pantallas e invoca sus acciones.
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
        private int _manualShot;
        private bool _manual;

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
                        _folder = Path.GetFullPath(a[i + 1]);
                        _only = a.Skip(i + 2).Where(s => !s.StartsWith("-", StringComparison.Ordinal)).ToArray();
                    }
                return _folder != null;
            }
        }

        private bool Want(string n) => _only.Length == 0 || _only.Contains(n);

        private void Update()
        {
            if (!_manual || !Input.GetKeyDown(KeyCode.F10)) return;
            string file = Path.Combine(_folder, "interaccion_" + (++_manualShot).ToString("00"));
            ScreenCapture.CaptureScreenshot(file + ".png");
            var game = App.Game;
            File.WriteAllText(file + ".txt", game.DebugTargetState());
        }

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

        private static void Tap(string name)
        {
            var b = FindBtn(name);
            if (b == null) { Debug.LogError("Auditoría: falta el botón " + name); return; }
            var rect = (RectTransform)b.transform;
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var data = new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<UiBtn>() != b)
            {
                Debug.LogError("Auditoría: centro del botón bloqueado: " + name);
                return;
            }
            ExecuteEvents.Execute(b.gameObject, data, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(b.gameObject, data, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(b.gameObject, data, ExecuteEvents.pointerClickHandler);
            Debug.Log("Auditoría: raycast y toque correctos: " + name);
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Path.Combine(_folder, "errores.txt"), "");
            App.Hub.DebugTestProfile();
            App.Hub.DebugAccessibility(_only.Contains("reducido"), _only.Contains("grande"));
            _manual = _only.Contains("manual");
            var g = App.Game.Game;
            g.Ledger.Apply(Core.Economy.Res.SalesDeBano, 60000, "prueba", "Capturas");
            yield return null;

            if (Want("home"))
            {
                App.DebugShow(MetaHub.Screen.Home);
                App.ForceRefresh();
                yield return Shot("home_entrada", 0.25f);
                yield return Shot("home", 1.6f);
                Tap("Favorita");
                yield return Shot("home_toque", 0.45f);
            }
            if (Want("coven"))
            {
                App.DebugShow(MetaHub.Screen.Coven);
                yield return Shot("coven", 1.5f);
                Tap("selenia");
                yield return Shot("coven_hoja", 1f);
            }
            if (Want("summon"))
            {
                App.DebugShow(MetaHub.Screen.Summon);
                yield return Shot("summon", 1.4f);
                Tap("limitado_marisal");
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
            if (Want("combate"))
            {
                // Run del Fresno en la primera zona y su primer combate: vista lateral, bioma de Raíces.
                App.Hub.DebugRun();
                yield return Shot("fresno_run", 1f);
                App.Hub.DebugBattle();
                yield return Shot("combate_1_inicio", 2.2f);
                if (_manual) yield break; // Permite probar entradas reales; F10 conserva imagen y estado.
                var game = FindAnyObjectByType<SalYAlmasGame>();
                if (game != null && game.Director != null)
                {
                    // Gestos: el abanico abierto, un Hechizo arrastrado hasta el segundo enemigo y lo que llegó al núcleo.
                    var log = new System.Text.StringBuilder();
                    log.AppendLine("pin: " + game.DebugHud("pin"));
                    yield return Shot("combate_1b_abanico", 0.6f);
                    log.AppendLine("close: " + game.DebugHud("close"));
                    log.AppendLine("hold: " + game.DebugHud("hold:Spell:1"));
                    yield return Shot("combate_1c_apuntar", 0.5f);
                    log.AppendLine("release: " + game.DebugHud("release"));
                    yield return Shot("combate_1d_hechizo", 0.9f);
                    log.AppendLine(game.DebugTargetState());
                    File.WriteAllText(Path.Combine(_folder, "combate_gestos.txt"), log.ToString());
                }
                if (game != null && game.Director != null)
                {
                    Debug.Log("Capturas: combate en automático.");
                    game.Director.Auto = true;
                    game.Director.Speed = 2f;
                    StartCoroutine(Trace(game));
                    yield return Shot("combate_2_golpes", 3.5f);
                    yield return Shot("combate_3_avance", 6f);
                    float limit = Time.realtimeSinceStartup + 90f;
                    while (!game.Director.ResultReady && Time.realtimeSinceStartup < limit) yield return null;
                    yield return Shot("combate_4_resultado", 1.2f);
                }
            }
            if (Want("escala"))
            {
                // Tamaños: coloso con escolta, dos gigantes lado a lado, jefe gigante solo y una oleada mixta.
                var casos = new[]
                {
                    ("escala_1_coloso", new[] { "madre_tenaza", "cangrejo_salinero", "gaviota_hueca" }),
                    ("escala_2_dos_gigantes", new[] { "carnero_hierro", "medusa_cielo" }),
                    ("escala_3_jefe_gigante", new[] { "guardian" }),
                    ("escala_4_mixta", new[] { "gota_obsidiana", "figura_sal", "soldada_crisol" }),
                };
                foreach (var (name, enemies) in casos)
                {
                    App.Hub.DebugSizedBattle(enemies);
                    yield return Shot(name, 2.2f);
                    var game = FindAnyObjectByType<SalYAlmasGame>();
                    if (game != null && game.Director != null)
                    {
                        // El primer enemigo apuntado: el corchete debe abrazar su silueta entera.
                        File.AppendAllText(Path.Combine(_folder, "escala.txt"), name + ": " + game.DebugHud("hold:Spell:0") + "\n");
                        yield return Shot(name + "_apuntar", 0.5f);
                        game.DebugHud("close");
                    }
                    App.Hub.DebugEndBattle();
                    yield return new WaitForSecondsRealtime(0.4f);
                }
            }
            if (_manual) yield break;
            Debug.Log("Capturas terminadas.");
            Application.Quit();
        }

        /// <summary>Deja en el log el estado del combate cada segundo, para ubicar un atasco.</summary>
        private IEnumerator Trace(SalYAlmasGame game)
        {
            while (game != null && game.Director != null && !game.Director.ResultReady)
            {
                var b = game.Director.Battle;
                Debug.Log($"Capturas: turno {b.TurnNumber}, pendiente {b.Pending}, eventos {b.Events.Count}, reproduciendo {game.Director.IsPlaying}");
                yield return new WaitForSecondsRealtime(1f);
            }
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
