using System;
using System.Collections.Generic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>Textos de esta capa (Art/Ui/Resources/Ui/ui_loc.json). Si una clave no está aquí, se busca en loc.json del núcleo.</summary>
    public static class UiLoc
    {
        [Serializable] private sealed class Row { public string k, es, en, ja; }
        [Serializable] private sealed class File { public Row[] rows; }

        private static Dictionary<string, Row> _rows;
        public static MetaGame Game;

        public static string Get(string key)
        {
            if (_rows == null)
            {
                _rows = new Dictionary<string, Row>();
                var ta = Resources.Load<TextAsset>("Ui/ui_loc");
                if (ta != null) foreach (var r in JsonUtility.FromJson<File>(ta.text).rows) _rows[r.k] = r;
            }
            string lang = Game?.Profile.Settings.Language ?? "es";
            if (_rows.TryGetValue(key, out var row))
            {
                string s = lang == "en" ? row.en : lang == "ja" ? row.ja : row.es;
                return string.IsNullOrEmpty(s) ? row.es : s;
            }
            return Game != null ? Game.Meta.World.Loc.Get(key, lang) : key;
        }

        public static string F(string key, params object[] args) => string.Format(Get(key), args);
    }

    /// <summary>Lo que necesita saber cada pantalla nueva.</summary>
    public abstract class UiScreen
    {
        public RectTransform Root;
        protected UiApp App;
        protected MetaGame G => App.Game.Game;

        public void Init(UiApp app, RectTransform root) { App = app; Root = root; }
        public abstract void Build();
        public virtual void Refresh() { }
        public virtual void Tick(float dt) { }
        public virtual void OnShown() { }
        public virtual void OnHidden() { }
    }

    /// <summary>
    /// La capa de interfaz: Home, Manantial, Aquelarre y la pantalla de título en uGUI, con transiciones, audio y partículas.
    /// Las demás pantallas siguen en el hub IMGUI (MetaHub) con la misma piel; todo se navega por MetaHub.Current, así que el
    /// hub sigue siendo la fuente de verdad de a qué pantalla vas.
    /// </summary>
    public sealed class UiApp : MonoBehaviour
    {
        public SalYAlmasGame Game;
        public MetaHub Hub => Game.Hub;
        public RectTransform Screens, Safe, Overlay;
        public UiShell Shell;

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private readonly Dictionary<MetaHub.Screen, UiScreen> _built = new Dictionary<MetaHub.Screen, UiScreen>();
        private UiScreen _shown;
        private MetaHub.Screen _shownKey;
        private UiTitle _title;
        private Image _wipe;
        private Material _wipeMat;
        private bool _wiping;
        public PullSequence_ Pull;

        /// <summary>Marca a la tirada corriendo para que Update no cambie de pantalla debajo.</summary>
        public sealed class PullSequence_ { public UiPullSequence Seq; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var game = FindAnyObjectByType<SalYAlmasGame>();
            if (game == null || game.Hub == null) return;
            var go = new GameObject("Interfaz");
            DontDestroyOnLoad(go);
            var app = go.AddComponent<UiApp>();
            app.Game = game;
            try { app.Build(); }
            catch (Exception ex)
            {
                // Si esta capa falla, el hub IMGUI sigue siendo jugable: nunca dejamos al jugador sin juego.
                Debug.LogException(ex);
                game.Hub.ExternalScreen = null;
                game.Hub.ExternalChrome = null;
                game.Hub.BackBlocked = null;
                Destroy(go);
            }
        }

        private static bool Owned(MetaHub.Screen s) => s == MetaHub.Screen.Home || s == MetaHub.Screen.Termas || s == MetaHub.Screen.Summon || s == MetaHub.Screen.Coven;
        private static MetaHub.Screen Canon(MetaHub.Screen s) => s == MetaHub.Screen.Termas ? MetaHub.Screen.Home : s;

        /// <summary>Pantallas de contenido del hub: su contenido es IMGUI, pero fondo y barras son de esta capa.</summary>
        private bool Framed(MetaHub.Screen s) => Game.InHub && !Owned(s) && s != MetaHub.Screen.Creator && s != MetaHub.Screen.Scene && !(s == MetaHub.Screen.Fresno && Game.Game.Fresno.InRun);

        private RectTransform _plain;

        private void Build()
        {
            UiLoc.Game = Game.Game;
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;
            _scaler = gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.layer = 5;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("Eventos", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            var rootRt = (RectTransform)transform;
            Screens = U.Fill(U.Node(rootRt, "Pantallas"));
            Safe = U.Fill(U.Node(rootRt, "Zona segura"));
            Safe.gameObject.AddComponent<UiSafeArea>();
            Shell = new UiShell(this, Safe);
            Overlay = U.Fill(U.Node(rootRt, "Encima"));
            UiToast.Layer = U.Fill(U.Node(rootRt, "Avisos"));

            _wipe = U.Img(rootRt, null, new Color(0.03f, 0.02f, 0.05f, 1), "Tinta", true);
            U.Fill(_wipe.rectTransform);
            _wipeMat = UiTheme.NewMat("Wipe");
            _wipe.material = _wipeMat;
            _wipe.gameObject.SetActive(false);

            // El hub IMGUI no se dibuja cuando manda esta capa.
            Hub.ExternalScreen = s => Game.InHub && Owned(s);
            Hub.ExternalChrome = Framed;
            Hub.BackBlocked = () => _wiping || _title != null || (Pull != null && Pull.Seq != null);
            UiTheme.ReduceFlashes = Game.Game.Profile.Settings.ReduceFlashes;
            SyncSettings();

            // El título solo se ve al abrir el juego, y nunca si la revisión pide una pantalla concreta.
            if (!UiCapture.Active && Owned(Hub.Current))
            {
                _title = new UiTitle();
                var r = U.Fill(U.Node(Overlay, "Título"));
                _title.Init(this, r);
                _title.Build();
                _title.OnShown();
            }
            else if (UiCapture.Active) gameObject.AddComponent<UiCapture>().App = this;

            Sync(true);
        }

        public void TitleDone(Action swap)
        {
            Transition(() =>
            {
                if (_title != null) { Destroy(_title.Root.gameObject); _title = null; }
                swap?.Invoke();
                _shown?.OnShown();
                Shell.Enter();
            });
        }

        private UiScreen Make(MetaHub.Screen key)
        {
            if (_built.TryGetValue(key, out var s)) return s;
            switch (key)
            {
                case MetaHub.Screen.Home: s = new UiHome(); break;
                case MetaHub.Screen.Summon: s = new UiSummon(); break;
                case MetaHub.Screen.Coven: s = new UiCoven(); break;
                default: return null;
            }
            var root = U.Fill(U.Node(Screens, key.ToString()));
            root.SetAsFirstSibling();
            s.Init(this, root);
            s.Build();
            _built[key] = s;
            return s;
        }

        /// <summary>Pone la pantalla que el hub dice. Sin animación: Transition es la que decide cuándo se ve.</summary>
        private void Sync(bool initial = false)
        {
            bool mine = Game.InHub && Owned(Hub.Current);
            bool framed = Framed(Hub.Current);
            bool canvasOn = mine || framed || _title != null || _wiping;
            if (_canvas.enabled != canvasOn) _canvas.enabled = canvasOn;
            if (_plain == null)
            {
                _plain = U.Fill(U.Node(Screens, "Fondo de contenido"));
                _plain.SetAsFirstSibling();
                var bg = U.Img(_plain, "bg/abyss", null, "Fondo");
                UiBackdrop.Cover(bg.rectTransform);
                var wheel = U.Img(_plain, "fx/astrolabe", new Color(1f, 0.82f, 0.55f, 0.09f), "Rueda");
                wheel.material = UiTheme.Mat("Additive");
                U.At(wheel.rectTransform, 0.5f, 0.5f, 0, 0, 1300, 1300, 0.5f, 0.5f);
                wheel.gameObject.AddComponent<UiSpin>().Speed = 0.9f;
                UiAmbience.Almitas(_plain, new Rect(-1000, -540, 2000, 1000), 1.2f);
            }
            if (_plain.gameObject.activeSelf != framed) _plain.gameObject.SetActive(framed);
            if (!mine)
            {
                if (_shown != null)
                {
                    _shown.OnHidden();
                    _shown.Root.gameObject.SetActive(false);
                    _shown = null;
                }
                _shownKey = (MetaHub.Screen)(-1);
                if (framed != Shell.Visible)
                {
                    Shell.Show(framed);
                    if (framed) { Shell.Refresh(); }
                }
                if (framed) { Shell.Mark(Hub.Current); }
                return;
            }
            var key = Canon(Hub.Current);
            if (_shown != null && _shownKey == key) return;
            if (_shown != null) { _shown.OnHidden(); _shown.Root.gameObject.SetActive(false); }
            var next = Make(key);
            _shown = next;
            _shownKey = key;
            next.Root.gameObject.SetActive(true);
            next.Refresh();
            Shell.Show(true);
            Shell.Mark(key);
            Shell.Refresh();
            if (_title == null) { next.OnShown(); if (!initial) Shell.Enter(); }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float aspect = Screen.width / Mathf.Max(1f, (float)Screen.height);
            _scaler.matchWidthOrHeight = aspect >= 16f / 9f ? 1f : 0f;
            if (!_wiping && (Pull == null || Pull.Seq == null)) Sync();
            _shown?.Tick(dt);
            _title?.Tick(dt);
            Pull?.Seq?.Tick(dt);
            if (_shown != null || Shell.Visible) Shell.Tick(dt);
            if (Shell.Visible && Time.frameCount % 20 == 0) Shell.Refresh();
            if (Time.frameCount % 15 == 0) SyncSettings();
        }

        /// <summary>
        /// Ajustes que la capa uGUI respeta: Ahorro de batería baja las partículas, Reducir destellos apaga los flashes y
        /// la tecla N (efectos) también silencia los sonidos de interfaz. Reducir movimiento detiene la decoración.
        /// </summary>
        private void SyncSettings()
        {
            var s = Game?.Game?.Profile.Settings;
            if (s == null) return;
            UiParticles.Density = s.BatterySaver ? 0.45f : 1f; // TUNING
            UiTheme.ReduceFlashes = s.ReduceFlashes;
            UiTheme.MotionScale = s.ReduceMotion ? 0f : 1f;
            Shader.SetGlobalFloat("_UiMotion", UiTheme.MotionScale);
            UiSfx.Muted = Game.SfxMuted;
        }

        // -------------------------------------------------------------------------- Navegación

        /// <summary>Navegar a cualquier pantalla: la tinta cubre, se cambia y se descubre. Se siente como pasar de folio.</summary>
        public void Go(MetaHub.Screen screen)
        {
            if (Hub.Current == screen || _wiping) return;
            UiSfx.Play("page");
            bool toLegacy = !Owned(screen);
            Transition(() =>
            {
                Hub.Open(screen);
                Sync();
            }, toLegacy);
        }

        public void OpenWitch(string id)
        {
            if (_wiping) return;
            UiSfx.Play("page");
            Transition(() => { Hub.OpenWitch(id); Sync(); }, true);
        }

        private void Transition(Action middle, bool toLegacy = false)
        {
            if (_wiping) return;
            _wiping = true;
            _canvas.enabled = true;
            _wipe.transform.SetAsLastSibling();
            _wipe.gameObject.SetActive(true);
            float scale = UiTheme.MotionScale;
            Tw.Run(0.28f, p => _wipeMat.SetFloat("_Cover", p), Ease.InCubic, 0, () =>
            {
                try { middle(); }
                catch (Exception ex) { Debug.LogException(ex); }
                Tw.Run(0.34f, p => _wipeMat.SetFloat("_Cover", 1 - p), Ease.OutCubic, 0.04f, () =>
                {
                    _wiping = false;
                    _wipe.gameObject.SetActive(false);
                });
            });
        }

        public void ForceRefresh()
        {
            _shown?.Refresh();
            Shell.Refresh();
        }

        public void StartPull(string banner, int count)
        {
            if (Pull != null && Pull.Seq != null) return;
            var r = Game.Game.GachaService.Pull(banner, count, out var results);
            if (!r.Ok) { UiToast.Show(r.Message, true); UiSfx.Play("deny"); return; }
            Hub.SaveNow();
            Pull = new PullSequence_();
            var root = U.Fill(U.Node(Overlay, "Tirada"));
            Pull.Seq = new UiPullSequence();
            Pull.Seq.Begin(this, root, results, banner, count, () =>
            {
                Pull.Seq = null;
                Destroy(root.gameObject);
                ForceRefresh();
            });
        }

        // -------------------------------------------------------------------------- Para las capturas de revisión

        public void DebugPull(List<Core.Gacha.PullResult> results)
        {
            Pull = new PullSequence_();
            var root = U.Fill(U.Node(Overlay, "Tirada"));
            Pull.Seq = new UiPullSequence();
            Pull.Seq.Begin(this, root, results, "estandar", 10, () =>
            {
                Pull.Seq = null;
                Destroy(root.gameObject);
                ForceRefresh();
            });
        }

        public void DebugShow(MetaHub.Screen s) { Hub.Open(s); Sync(); }
        public UiScreen Shown => _shown;
    }

    /// <summary>Pantalla de título: es la primera impresión. Logo, agua, vapor y la rueda natal girando.</summary>
    public sealed class UiTitle : UiScreen
    {
        private bool _gone;
        private TextMeshProUGUI _tap;
        private float _t;
        private RectTransform _logo;
        private Image _flash;

        public override void Build()
        {
            UiBackdrop.Scene(Root, 1f, out _);
            var dim = U.Img(Root, "fx/vignette", new Color(0, 0, 0, 0.8f), "Viñeta");
            U.Fill(dim.rectTransform);

            var wheel = U.Img(Root, "fx/astrolabe", new Color(1, 0.86f, 0.6f, 0.42f), "Rueda natal");
            wheel.material = UiTheme.Mat("Additive");
            U.At(wheel.rectTransform, 0.5f, 0.56f, 0, 0, 900, 900, 0.5f, 0.5f);
            wheel.gameObject.AddComponent<UiSpin>().Speed = -2.2f;
            UiAmbience.Almitas(Root, new Rect(-1000, -540, 2000, 900), 3);
            UiAmbience.Steam(Root, new Rect(-1100, -600, 2200, 150), 3f, new Color(1, 0.9f, 0.84f, 0.13f), 600);

            var logo = U.Img(Root, "logo/logo", null, "Logo");
            logo.material = UiTheme.NewMat("Shimmer");
            _logo = logo.rectTransform;
            U.At(_logo, 0.5f, 0.58f, 0, 0, 1190, 442, 0.5f, 0.5f);

            _tap = U.Txt(Root, UiLoc.Get("title.tap"), 34, UiTheme.Parchment, UiTheme.Italic, TextAlignmentOptions.Center);
            _tap.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Italic, new Color(0.05f, 0.02f, 0.05f, 0.95f));
            U.At(_tap.rectTransform, 0.5f, 0.17f, 0, 0, 900, 60, 0.5f, 0.5f);
            var ver = U.Txt(Root, UiLoc.Get("title.version"), 20, new Color(1, 0.95f, 0.85f, 0.6f), UiTheme.Body, TextAlignmentOptions.BottomRight);
            U.At(ver.rectTransform, 1, 0, -28, 20, 600, 30, 1, 0);

            var hit = U.Img(Root, null, new Color(0, 0, 0, 0), "Toque", true);
            U.Fill(hit.rectTransform);
            var b = hit.gameObject.AddComponent<UiBtn>();
            b.PressScale = 1;
            b.Sound = "enter";
            b.Click = Leave;

            _flash = U.Img(Root, "fx/soft_dot", new Color(1, 0.9f, 0.7f, 0), "Destello");
            _flash.material = UiTheme.Mat("Additive");
            U.At(_flash.rectTransform, 0.5f, 0.58f, 0, 0, 2400, 1500, 0.5f, 0.5f);
        }

        public override void OnShown()
        {
            U.FadeIn(U.Group(_logo), 1.4f, 0.25f);
            _logo.localScale = Vector3.one * 0.94f;
            Tw.Run(1.8f, p => { if (_logo != null) _logo.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, p); }, Ease.OutCubic, 0.25f);
            U.FadeIn(U.Group(_tap.rectTransform), 0.8f, 1.3f);
        }

        public override void Tick(float dt)
        {
            _t += dt;
            if (_tap != null) _tap.alpha = 0.55f + 0.45f * Mathf.Sin(_t * 2.1f) * Mathf.Sin(_t * 2.1f);
        }

        private void Leave()
        {
            if (_gone) return;
            _gone = true;
            float maxA = UiTheme.ReduceFlashes ? 0.2f : 0.7f;
            Tw.Run(0.35f, p => { if (_flash != null) _flash.color = new Color(1, 0.9f, 0.7f, maxA * p); }, Ease.OutCubic);
            App.TitleDone(null);
        }
    }

    /// <summary>Fondos compartidos: el paisaje por capas y su parallax. Se escalan "cubriendo": jamás se deforman.</summary>
    public static class UiBackdrop
    {
        /// <summary>Hace que un nodo cubra a su padre conservando la proporción 16:9 del arte.</summary>
        public static void Cover(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var f = rt.gameObject.AddComponent<AspectRatioFitter>();
            f.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            f.aspectRatio = 16f / 9f;
        }

        /// <summary>Atardecer con estrellas, sierras, salina y terraza. Devuelve las capas para el parallax.</summary>
        public static RectTransform Scene(RectTransform parent, float bright, out RectTransform[] layers)
        {
            var root = U.Fill(U.Node(parent, "Paisaje"));
            string[] files = { "bg/home_sky", "bg/home_stars", "bg/home_far", "bg/home_mid", "bg/home_near" };
            layers = new RectTransform[files.Length];
            var tint = new Color(bright, bright, bright, 1);
            for (int i = 0; i < files.Length; i++)
            {
                var im = U.Img(root, files[i], tint, files[i]);
                layers[i] = im.rectTransform;
                Cover(im.rectTransform);
                // Margen para que el parallax no descubra los bordes.
                im.transform.localScale = Vector3.one * 1.04f;
                if (i == 1) im.material = UiTheme.Mat("Additive");
                if (i == 3 && UiTheme.Mat("Water") != null)
                {
                    im.material = UiTheme.NewMat("Water");
                    im.material.SetFloat("_Strength", 0.003f);
                    im.material.SetColor("_Glow", new Color(1f, 0.82f, 0.65f, 0.1f));
                }
                if (i == 4)
                {
                    im.material = UiTheme.NewMat("Water");
                    im.material.SetFloat("_Strength", 0f);
                    im.material.SetColor("_Glow", new Color(1f, 0.8f, 0.62f, 0.05f));
                }
            }
            return root;
        }
    }
}
