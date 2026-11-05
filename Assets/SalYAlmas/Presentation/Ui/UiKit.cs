using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>Curvas de animación. Un solo lugar para que todo el juego se mueva con la misma mano.</summary>
    public static class Ease
    {
        public static float Linear(float t) => t;
        public static float OutCubic(float t) { t = 1 - t; return 1 - t * t * t; }
        public static float InCubic(float t) => t * t * t;
        public static float OutQuint(float t) { t = 1 - t; return 1 - t * t * t * t * t; }
        public static float InOutSine(float t) => 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
        public static float OutBack(float t) { const float s = 1.70158f; t -= 1; return t * t * ((s + 1) * t + s) + 1; }
        public static float OutSoftBack(float t) { const float s = 0.9f; t -= 1; return t * t * ((s + 1) * t + s) + 1; }
    }

    /// <summary>Animaciones por tiempo real. Todas respetan "Reducir movimiento" a través de <see cref="UiTheme.MotionScale"/>.</summary>
    public sealed class Tw : MonoBehaviour
    {
        private sealed class Item
        {
            public float Time, Delay, Duration;
            public Action<float> Step;
            public Func<float, float> Curve;
            public Action Done;
            public object Owner;
            public bool Killed;
        }

        private static Tw _i;
        private readonly List<Item> _items = new List<Item>();
        private readonly List<Item> _adding = new List<Item>();

        private static Tw I
        {
            get
            {
                if (_i != null) return _i;
                var go = new GameObject("Animaciones de interfaz");
                DontDestroyOnLoad(go);
                _i = go.AddComponent<Tw>();
                return _i;
            }
        }

        public static void Run(float duration, Action<float> step, Func<float, float> curve = null, float delay = 0, Action done = null, object owner = null, bool respectMotion = true)
        {
            I._adding.Add(new Item { Duration = Mathf.Max(0.0001f, duration * (respectMotion ? UiTheme.MotionScale : 1f)), Delay = delay, Step = step, Curve = curve ?? Ease.OutCubic, Done = done, Owner = owner });
        }

        public static void After(float seconds, Action done, object owner = null) => Run(seconds, null, Ease.Linear, 0, done, owner, false);

        /// <summary>Cancela lo que esté corriendo de este dueño, para no pelear con la siguiente animación.</summary>
        public static void Kill(object owner)
        {
            if (_i == null || owner == null) return;
            foreach (var t in _i._items) if (ReferenceEquals(t.Owner, owner)) t.Killed = true;
            foreach (var t in _i._adding) if (ReferenceEquals(t.Owner, owner)) t.Killed = true;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_adding.Count > 0) { _items.AddRange(_adding); _adding.Clear(); }
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var t = _items[i];
                if (t.Killed) { _items.RemoveAt(i); continue; }
                if (t.Delay > 0) { t.Delay -= dt; if (t.Delay > 0) continue; }
                t.Time += dt;
                float p = Mathf.Clamp01(t.Time / t.Duration);
                try { t.Step?.Invoke(t.Curve(p)); }
                catch (Exception ex) { Debug.LogException(ex); t.Killed = true; continue; }
                if (p >= 1)
                {
                    _items.RemoveAt(i);
                    try { t.Done?.Invoke(); } catch (Exception ex) { Debug.LogException(ex); }
                }
            }
        }
    }

    /// <summary>Paleta (docs/REFERENCIAS.md), tipografía y arte del kit. Lo genera scripts/ui_kit.py.</summary>
    public static class UiTheme
    {
        public static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
        public static readonly Color Parchment = Hex("#EDE3CC"), ParchmentDim = Hex("#CFBE9A"), Ink = Hex("#2A2018");
        public static readonly Color Abyss = Hex("#0B0B14"), Panel = Hex("#1A1A28"), Gold = Hex("#B8923A"), GoldBright = Hex("#E6C76E");
        public static readonly Color Crimson = Hex("#8E2430"), Salt = Hex("#F6F4EE"), Spark = Hex("#FFF1B8"), Five = Hex("#FFD84D"), Four = Hex("#A25CF5"), Three = Hex("#F2EFE6"), Danger = Hex("#E0533F");

        /// <summary>1 normal; casi 0 con "Reducir movimiento": las transiciones se vuelven cortes suaves.</summary>
        public static float MotionScale = 1f;
        public static bool ReduceFlashes;

        public static Color Rarity(int r) => r >= 5 ? Five : r == 4 ? Four : Three;
        public static string PlanetId(Core.Magic.Planet p) => p.ToString().ToLowerInvariant();

        public static Color Planet(Core.Magic.Planet p) => Presentation.Palette.Planet(p);

        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();
        private static readonly Dictionary<string, TMP_FontAsset> Fonts = new Dictionary<string, TMP_FontAsset>();

        /// <summary>Bordes de 9-slice en píxeles del sprite. Todo lo de chrome/ es @2x (200 píxeles por unidad).</summary>
        private static readonly Dictionary<string, int> Borders = new Dictionary<string, int>
        {
            { "chrome/frame_panel", 112 }, { "chrome/edge_parchment", 96 }, { "chrome/btn_primary", 30 }, { "chrome/btn_secondary", 30 },
            { "chrome/btn_disabled", 30 }, { "chrome/btn_parchment", 30 }, { "chrome/frame_r3", 84 }, { "chrome/frame_r4", 84 }, { "chrome/frame_r5", 84 },
            { "chrome/bar_rail", 13 }, { "chrome/bar_fill", 13 },
        };

        public static Sprite Sp(string path)
        {
            if (Sprites.TryGetValue(path, out var s) && s != null) return s;
            var tex = Resources.Load<Texture2D>("Ui/" + path);
            if (tex == null) { Debug.LogWarning("Falta arte de interfaz: " + path); return null; }
            Borders.TryGetValue(path, out int b);
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), path.StartsWith("chrome/", StringComparison.Ordinal) ? 200 : 100, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
            s.name = path;
            Sprites[path] = s;
            return s;
        }

        public static Texture2D Tex(string path) => Resources.Load<Texture2D>("Ui/" + path);

        public static Material Mat(string shader)
        {
            if (Mats.TryGetValue(shader, out var m) && m != null) return m;
            var sh = Shader.Find("SalYAlmas/UI/" + shader);
            if (sh == null) { Debug.LogWarning("Falta shader de interfaz: " + shader); return null; }
            m = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            if (m.HasProperty("_NoiseTex")) m.SetTexture("_NoiseTex", Tex("fx/noise"));
            Mats[shader] = m;
            return m;
        }

        public static Material NewMat(string shader)
        {
            var b = Mat(shader);
            return b == null ? null : new Material(b) { hideFlags = HideFlags.HideAndDontSave };
        }

        private static TMP_FontAsset Font(string file)
        {
            if (Fonts.TryGetValue(file, out var a) && a != null) return a;
            var f = Resources.Load<Font>("Ui/Fonts/" + file);
            a = f != null ? TMP_FontAsset.CreateFontAsset(f, 72, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true) : null;
            if (a == null) { Debug.LogWarning("Falta fuente: " + file); return TMP_Settings.defaultFontAsset; }
            a.name = file;
            Fonts[file] = a;
            if (!file.StartsWith("Noto", StringComparison.Ordinal))
            {
                a.fallbackFontAssetTable = new List<TMP_FontAsset> { Font("NotoSansSymbols-Regular"), Font("NotoSansSymbols2-Regular") };
            }
            return a;
        }

        /// <summary>Cuerpo: Alegreya. Títulos medianos: Texturina. Solo el logo y los grandes nombres llevan Pirata One.</summary>
        public static TMP_FontAsset Body => Font("Alegreya-Regular");
        public static TMP_FontAsset Bold => Font("Alegreya-Bold");
        public static TMP_FontAsset Italic => Font("Alegreya-Italic");
        public static TMP_FontAsset Title => Font("Texturina-SemiBold");
        public static TMP_FontAsset Gothic => Font("PirataOne-Regular");

        /// <summary>Material de título con sombra de tinta. Sin él, el oro se pierde sobre el atardecer.</summary>
        public static Material Shadowed(TMP_FontAsset font, Color shadow, float softness = 0.35f, float offsetY = -0.9f, float outline = 0.08f)
        {
            string key = "txt|" + font.name + "|" + ColorUtility.ToHtmlStringRGBA(shadow) + softness + offsetY + outline;
            if (Mats.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(font.material) { hideFlags = HideFlags.HideAndDontSave };
            m.EnableKeyword("UNDERLAY_ON");
            m.SetColor("_UnderlayColor", shadow);
            m.SetFloat("_UnderlayOffsetY", offsetY);
            m.SetFloat("_UnderlaySoftness", softness);
            m.SetFloat("_UnderlayDilate", 0.15f);
            if (outline > 0)
            {
                m.EnableKeyword("OUTLINE_ON");
                m.SetFloat("_OutlineWidth", outline);
                m.SetColor("_OutlineColor", new Color(shadow.r, shadow.g, shadow.b, 0.9f));
            }
            Mats[key] = m;
            return m;
        }
    }

    /// <summary>Atajos para construir jerarquías de uGUI por código en el lienzo de 1920×1080.</summary>
    public static class U
    {
        public static RectTransform Node(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Estira a todo el padre con márgenes (izquierda, abajo, derecha, arriba).</summary>
        public static RectTransform Fill(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>Coloca con un ancla (0..1) y posición en píxeles del lienzo. El pivote por defecto coincide con el ancla.</summary>
        public static RectTransform At(RectTransform rt, float ax, float ay, float x, float y, float w, float h, float px = -1, float py = -1)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(px < 0 ? ax : px, py < 0 ? ay : py);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static Image Img(Transform parent, string sprite, Color? color = null, string name = null, bool raycast = false)
        {
            var rt = Node(parent, name ?? sprite ?? "Imagen");
            var i = rt.gameObject.AddComponent<Image>();
            if (sprite != null)
            {
                i.sprite = UiTheme.Sp(sprite);
                if (i.sprite != null && i.sprite.border.sqrMagnitude > 0) i.type = Image.Type.Sliced;
            }
            i.color = color ?? Color.white;
            i.raycastTarget = raycast;
            return i;
        }

        public static TextMeshProUGUI Txt(Transform parent, string text, float size, Color color, TMP_FontAsset font = null, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, bool wrap = false, string name = null)
        {
            var rt = Node(parent, name ?? "Texto");
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font != null ? font : UiTheme.Body;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Superficie que se repite sin estirarse: cuero, pergamino, oro.</summary>
        public static Image Tile(Transform parent, string sprite, Color? color = null, string name = null)
        {
            var i = Img(parent, sprite, color, name ?? sprite);
            i.type = Image.Type.Tiled;
            return i;
        }

        /// <summary>Panel de cuero con marco de oro repujado. El contenido va en el nodo que devuelve.</summary>
        public static RectTransform Panel(Transform parent, string name = "Panel", float darken = 0)
        {
            var root = Node(parent, name);
            var surf = Tile(root, "chrome/tile_leather", Color.Lerp(Color.white, Color.black, darken), "Cuero");
            Fill(surf.rectTransform, 5, 5, 5, 5);
            var vol = Img(root, "fx/grad_v", new Color(1f, 0.82f, 0.68f, 0.07f), "Luz alta");
            Fill(vol.rectTransform, 5, 5, 5, 5);
            var frame = Img(root, "chrome/frame_panel", null, "Marco");
            frame.fillCenter = false;
            Fill(frame.rectTransform);
            return root;
        }

        /// <summary>Hoja de pergamino con borde quemado y rasgado.</summary>
        public static RectTransform Paper(Transform parent, string name = "Pergamino")
        {
            var root = Node(parent, name);
            var surf = Tile(root, "chrome/tile_parchment", null, "Papel");
            Fill(surf.rectTransform, 8, 8, 8, 8);
            var edge = Img(root, "chrome/edge_parchment", null, "Borde");
            Fill(edge.rectTransform, -6, -6, -6, -6);
            return root;
        }

        /// <summary>Nodo con CanvasGroup para desvanecer grupos enteros.</summary>
        public static CanvasGroup Group(RectTransform rt)
        {
            var g = rt.GetComponent<CanvasGroup>();
            return g != null ? g : rt.gameObject.AddComponent<CanvasGroup>();
        }

        public static UiBtn Btn(Transform parent, string sprite, string label, float fontSize = 30, Action click = null, TMP_FontAsset font = null, Color? textColor = null, string name = null)
        {
            var img = Img(parent, sprite, null, name ?? ("Botón " + label), true);
            var b = img.gameObject.AddComponent<UiBtn>();
            b.Background = img;
            b.Click = click;
            if (label != null)
            {
                b.Label = Txt(img.transform, label, fontSize, textColor ?? UiTheme.Parchment, font ?? UiTheme.Bold, TextAlignmentOptions.Center);
                Fill(b.Label.rectTransform, 12, 4, 12, 4);
            }
            return b;
        }

        public static void FadeIn(CanvasGroup g, float dur, float delay = 0, Action done = null, object owner = null)
        {
            g.alpha = 0;
            Tw.Run(dur, p => { if (g != null) g.alpha = p; }, Ease.OutCubic, delay, done, owner);
        }

        /// <summary>Entrada escalonada: sube y aparece. Es el movimiento que une todas las pantallas.</summary>
        public static void Rise(RectTransform rt, float dist = 36, float dur = 0.42f, float delay = 0, object owner = null)
        {
            var g = Group(rt);
            var home = rt.anchoredPosition;
            g.alpha = 0;
            rt.anchoredPosition = home - new Vector2(0, dist);
            Tw.Run(dur, p =>
            {
                if (rt == null) return;
                g.alpha = Mathf.Clamp01(p * 1.6f);
                rt.anchoredPosition = home - new Vector2(0, dist * (1 - p));
            }, Ease.OutQuint, delay, null, owner);
        }

        public static void Slide(RectTransform rt, Vector2 from, float dur = 0.5f, float delay = 0, object owner = null)
        {
            var home = rt.anchoredPosition;
            rt.anchoredPosition = home + from;
            Tw.Run(dur, p => { if (rt != null) rt.anchoredPosition = home + from * (1 - p); }, Ease.OutQuint, delay, null, owner);
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        public static string Compact(long v)
        {
            if (v >= 10000000) return (v / 1000000).ToString("N0") + " M";
            if (v >= 100000) return (v / 1000).ToString("N0") + " k";
            return v.ToString("N0");
        }
    }

    /// <summary>
    /// El botón del juego. Se hunde al tocarlo y rebota al soltarlo, con sonido según su peso. Responde al bajar el dedo
    /// (tacto inmediato), aunque la acción se dispara al soltar dentro, para poder arrepentirse.
    /// </summary>
    public sealed class UiBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Background;
        public TextMeshProUGUI Label;
        public Action Click;
        public string Sound = "tap";
        public float PressScale = 0.955f;
        public string DisabledReason = "";

        private bool _interactable = true;
        private Vector3 _base = Vector3.one;
        private float _scale = 1;
        private bool _started;
        private Image _hover;
        private Sprite _enabledSprite;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable == value) return;
                _interactable = value;
                if (Background != null)
                {
                    if (!value) { _enabledSprite = Background.sprite; Background.sprite = UiTheme.Sp("chrome/btn_disabled"); }
                    else if (_enabledSprite != null) Background.sprite = _enabledSprite;
                }
                if (Label != null) Label.alpha = value ? 1f : 0.55f;
            }
        }

        private void Start() { _base = transform.localScale; _started = true; }
        private void OnDisable()
        {
            Tw.Kill(this);
            if (_started) transform.localScale = _base;
            _scale = 1;
        }

        private void Apply(float s, float dur, Func<float, float> curve)
        {
            if (!_started) return;
            Tw.Kill(this);
            float from = _scale;
            Tw.Run(dur, p =>
            {
                if (this == null) return;
                _scale = Mathf.LerpUnclamped(from, s, p);
                transform.localScale = _base * _scale;
            }, curve, 0, null, this);
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || !_interactable) return;
            Apply(PressScale, 0.07f, Ease.OutCubic);
            if (_interactable) UiSfx.Play(Sound == "tap" ? "tick" : null);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) Apply(1f, 0.28f, Ease.OutBack);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            if (!_interactable)
            {
                UiSfx.Play("deny");
                if (DisabledReason.Length > 0) UiToast.Show(DisabledReason);
                return;
            }
            if (Sound != null && Sound != "tap") UiSfx.Play(Sound);
            Click?.Invoke();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (Input.touchSupported || !_interactable) return;
            if (_hover == null && Background != null)
            {
                _hover = U.Img(transform, null, new Color(1, 0.9f, 0.7f, 0), "Luz");
                U.Fill(_hover.rectTransform);
                _hover.sprite = Background.sprite;
                _hover.type = Background.type;
                _hover.material = UiTheme.Mat("Additive");
                _hover.transform.SetAsFirstSibling();
                _hover.transform.SetSiblingIndex(0);
            }
            if (_hover != null) _hover.color = new Color(1, 0.86f, 0.62f, 0.14f);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_hover != null) _hover.color = new Color(1, 1, 1, 0);
        }
    }

    /// <summary>Mensaje breve del juego sobre un trozo de pergamino.</summary>
    public static class UiToast
    {
        public static RectTransform Layer;
        private static RectTransform _current;

        public static void Show(string text, bool bad = false)
        {
            if (Layer == null || string.IsNullOrEmpty(text)) return;
            if (_current != null) UnityEngine.Object.Destroy(_current.gameObject);
            _current = U.Paper(Layer, "Aviso");
            U.At(_current, 0.5f, 1, 0, -140, Mathf.Min(1300, 110 + text.Length * 13.5f), 92, 0.5f, 1);
            var t = U.Txt(_current, text, 28, bad ? UiTheme.Crimson : UiTheme.Ink, UiTheme.Bold, TextAlignmentOptions.Center, true);
            U.Fill(t.rectTransform, 36, 8, 36, 8);
            var rt = _current;
            var g = U.Group(rt);
            g.blocksRaycasts = false;
            U.Rise(rt, 22, 0.3f);
            Tw.Run(0.35f, p => { if (g != null) g.alpha = 1 - p; }, Ease.Linear, 2.6f, () => { if (rt != null) UnityEngine.Object.Destroy(rt.gameObject); });
        }
    }

    /// <summary>Efectos de sonido de interfaz, sintetizados por scripts/ui_sfx.py. Varias voces para que los toques se encimen.</summary>
    public static class UiSfx
    {
        private static AudioSource[] _voices;
        private static int _next;
        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
        public static float Volume = 0.8f;
        public static bool Muted;

        public static void Play(string id, float gain = 1f, float pitch = 1f)
        {
            if (id == null || Muted) return;
            if (_voices == null)
            {
                var go = new GameObject("Sonidos de interfaz");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _voices = new AudioSource[8];
                for (int i = 0; i < _voices.Length; i++)
                {
                    _voices[i] = go.AddComponent<AudioSource>();
                    _voices[i].playOnAwake = false;
                    _voices[i].ignoreListenerPause = true;
                    _voices[i].spatialBlend = 0;
                }
            }
            if (!Clips.TryGetValue(id, out var c))
            {
                c = Resources.Load<AudioClip>("Ui/sfx/" + id);
                Clips[id] = c;
            }
            if (c == null) return;
            var v = _voices[_next];
            _next = (_next + 1) % _voices.Length;
            v.pitch = pitch;
            v.PlayOneShot(c, Volume * gain);
        }
    }

    /// <summary>Barra de valor con rastro: al bajar o subir deja una estela que la alcanza (lectura de cambio, no solo de estado).</summary>
    public sealed class UiBar : MonoBehaviour
    {
        public Image Fill, Trail;
        private float _value = -1, _trail;

        public static UiBar Make(Transform parent, Color fill, string name = "Barra")
        {
            var rail = U.Img(parent, "chrome/bar_rail", null, name);
            var bar = rail.gameObject.AddComponent<UiBar>();
            bar.Trail = U.Img(rail.transform, "chrome/bar_fill", new Color(1, 0.95f, 0.8f, 0.5f), "Rastro");
            bar.Fill = U.Img(rail.transform, "chrome/bar_fill", fill, "Relleno");
            foreach (var i in new[] { bar.Trail, bar.Fill })
            {
                U.Fill(i.rectTransform, 2, 2, 2, 2);
                i.type = Image.Type.Sliced;
                i.rectTransform.anchorMax = new Vector2(0, 1);
            }
            return bar;
        }

        public void Set(float v, bool snap = false)
        {
            v = Mathf.Clamp01(v);
            if (snap || _value < 0) { _value = _trail = v; Layout(); return; }
            _value = v;
        }

        private void Layout()
        {
            Fill.rectTransform.anchorMax = new Vector2(_value, 1);
            Trail.rectTransform.anchorMax = new Vector2(_trail, 1);
        }

        private void Update()
        {
            if (_value < 0) return;
            float prev = Fill.rectTransform.anchorMax.x;
            float cur = Mathf.Lerp(prev, _value, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 14f));
            _trail = Mathf.Lerp(_trail, _value, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 3.5f));
            Fill.rectTransform.anchorMax = new Vector2(cur, 1);
            Trail.rectTransform.anchorMax = new Vector2(_trail, 1);
        }
    }

    /// <summary>Gira despacio: astrolabios, anillos. Se apaga solo con "Reducir movimiento".</summary>
    public sealed class UiSpin : MonoBehaviour
    {
        public float Speed = 2f;
        private void Update() => transform.Rotate(0, 0, Speed * Time.unscaledDeltaTime * UiTheme.MotionScale);
    }

    /// <summary>Aparta los bordes del notch y de la barra de gestos (Screen.safeArea).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UiSafeArea : MonoBehaviour
    {
        private Rect _last;

        private void Update()
        {
            var s = Screen.safeArea;
            if (s == _last) return;
            _last = s;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(s.xMin / Screen.width, s.yMin / Screen.height);
            rt.anchorMax = new Vector2(s.xMax / Screen.width, s.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>Punto de aviso que dice cuánto hay y lleva directo: un sello de lacre, no un punto rojo mudo.</summary>
    public static class UiBadge
    {
        public static GameObject Make(Transform parent, string text, float size = 40)
        {
            var seal = U.Img(parent, "chrome/seal_wax", null, "Aviso");
            U.At(seal.rectTransform, 1, 1, 6, 6, size, size, 1, 1);
            if (!string.IsNullOrEmpty(text))
            {
                var t = U.Txt(seal.transform, text, size * 0.5f, UiTheme.Salt, UiTheme.Bold, TextAlignmentOptions.Center);
                U.Fill(t.rectTransform);
            }
            return seal.gameObject;
        }
    }
}
