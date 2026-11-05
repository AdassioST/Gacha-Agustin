using System.Collections.Generic;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// La piel del juego: oro viejo sobre tinta y ciruela, como un folio del Libro Mayor al atardecer
    /// (docs/REFERENCIAS.md). Todo se genera por código hasta que llegue el arte final de interfaz.
    /// </summary>
    public static class Theme
    {
        public static readonly Color Plum = Palette.Hex("#2A1830");
        public static readonly Color Dusk = Palette.Hex("#5A2438");
        public static readonly Color Ember = Palette.Hex("#A8562E");
        public static readonly Color GoldDark = Palette.Hex("#6E5420");

        /// <summary>Ajustes de accesibilidad que afectan a toda la interfaz.</summary>
        public static bool LargeText;
        public static bool ReduceFlashes;

        private static Texture2D _sky;
        private static readonly Dictionary<string, Texture2D> Frames = new Dictionary<string, Texture2D>();

        /// <summary>Cielo de atardecer para el fondo del home y de las pantallas.</summary>
        public static Texture2D Sky
        {
            get
            {
                if (_sky != null) return _sky;
                _sky = Resources.Load<Texture2D>("Ui/bg/abyss");
                if (_sky != null) return _sky;
                const int w = 256, h = 144;
                _sky = new Texture2D(w, h, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                for (int y = 0; y < h; y++)
                {
                    float t = y / (float)(h - 1); // 0 abajo, 1 arriba
                    Color c = t < 0.35f ? Color.Lerp(Palette.Hex("#6E3A2C"), Dusk, t / 0.35f)
                        : t < 0.7f ? Color.Lerp(Dusk, Plum, (t - 0.35f) / 0.35f)
                        : Color.Lerp(Plum, Palette.Abyss, (t - 0.7f) / 0.3f);
                    for (int x = 0; x < w; x++)
                    {
                        float dx = (x - w * 0.5f) / w, dy = (t - 0.12f);
                        float glow = Mathf.Clamp01(0.55f - Mathf.Sqrt(dx * dx * 1.6f + dy * dy));
                        _sky.SetPixel(x, y, Color.Lerp(c, Ember, glow * 0.55f));
                    }
                }
                _sky.Apply();
                return _sky;
            }
        }

        /// <summary>Marco biselado de oro con interior en degradado, listo para estirarse con bordes de 14 px.</summary>
        public static Texture2D Bevel(Color top, Color bottom, Color rim)
        {
            string key = ColorUtility.ToHtmlStringRGBA(top) + ColorUtility.ToHtmlStringRGBA(bottom) + ColorUtility.ToHtmlStringRGBA(rim);
            if (Frames.TryGetValue(key, out var tex) && tex != null) return tex;
            const int s = 48, r = 10, b = 4;
            tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = RoundedDistance(x + 0.5f, y + 0.5f, s, r);
                    float alpha = Mathf.Clamp01(0.5f - d);
                    float v = y / (float)(s - 1);
                    Color inner = Color.Lerp(bottom, top, v);
                    // Luz arriba y sombra abajo en el aro de oro, para que parezca metal.
                    Color metal = Color.Lerp(Color.Lerp(rim, Color.black, 0.45f), Color.Lerp(rim, Color.white, 0.35f), v);
                    float ring = Mathf.Clamp01(d + b + 0.5f);
                    Color c = Color.Lerp(inner, metal, ring);
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            Frames[key] = tex;
            return tex;
        }

        private static float RoundedDistance(float x, float y, int s, int r)
        {
            float qx = Mathf.Abs(x - s / 2f) - (s / 2f - r);
            float qy = Mathf.Abs(y - s / 2f) - (s / 2f - r);
            float ox = Mathf.Max(qx, 0), oy = Mathf.Max(qy, 0);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
        }

        /// <summary>Dibuja un panel con marco de oro.</summary>
        public static void Panel(Rect r, Color top, Color bottom, Color rim, float alpha = 1f)
        {
            var old = GUI.color;
            var leather = Kit("chrome/tile_leather");
            if (leather == null)
            {
                GUI.color = new Color(1, 1, 1, alpha);
                GUI.Box(r, GUIContent.none, FrameStyle(top, bottom, rim));
                GUI.color = old;
                return;
            }
            // Cuero repetido sin estirarse, teñido por el color que pidió el panel, con luz de arriba y marco repujado.
            Color tint = Color.Lerp(top, bottom, 0.35f);
            float lum = tint.r * 0.3f + tint.g * 0.59f + tint.b * 0.11f;
            GUI.color = new Color(Mathf.Clamp01(0.75f + tint.r * 0.9f - lum * 0.5f), Mathf.Clamp01(0.75f + tint.g * 0.9f - lum * 0.5f), Mathf.Clamp01(0.75f + tint.b * 0.9f - lum * 0.5f), alpha);
            var inner = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
            GUI.DrawTextureWithTexCoords(inner, leather, new Rect(r.x / 256f, r.y / 256f, inner.width / 256f, inner.height / 256f));
            var grad = Kit("chrome/shade_up");
            if (grad != null)
            {
                GUI.color = new Color(1, 0.86f, 0.74f, 0.1f * alpha);
                GUI.DrawTexture(new Rect(inner.x, inner.y, inner.width, Mathf.Min(inner.height, 90)), Kit("fx/grad_v"), ScaleMode.StretchToFill);
            }
            GUI.color = new Color(1, 1, 1, alpha * (rim.a <= 0 ? 0 : 1));
            float small = Mathf.Min(r.width, r.height);
            if (small >= 96) GUI.Box(r, GUIContent.none, KitStyle("chrome/frame_panel_1x", 56));
            else if (small >= 34) GUI.Box(r, GUIContent.none, KitStyle("chrome/btn_secondary_1x", 15));
            else GuiKit.Frame(r, Palette.WithAlpha(rim, alpha * 0.85f), 1f);
            GUI.color = old;
        }

        private static readonly Dictionary<string, Texture2D> KitTex = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, GUIStyle> KitStyles = new Dictionary<string, GUIStyle>();

        /// <summary>Texturas del kit de interfaz (scripts/ui_kit.py). Sin kit, el hub conserva su dibujo por código.</summary>
        public static Texture2D Kit(string path)
        {
            if (KitTex.TryGetValue(path, out var t)) return t;
            t = Resources.Load<Texture2D>("Ui/" + path);
            KitTex[path] = t;
            return t;
        }

        private static GUIStyle KitStyle(string path, int border)
        {
            if (KitStyles.TryGetValue(path, out var s) && s != null) return s;
            s = new GUIStyle { border = new RectOffset(border, border, border, border) };
            s.normal.background = Kit(path);
            KitStyles[path] = s;
            return s;
        }

        private static Font _body, _bold;
        public static Font BodyFont => _body != null ? _body : (_body = Resources.Load<Font>("Ui/Fonts/Alegreya-Regular"));
        public static Font BoldFont => _bold != null ? _bold : (_bold = Resources.Load<Font>("Ui/Fonts/Alegreya-Bold"));

        public static void GoldPanel(Rect r, float alpha = 0.96f) => Panel(r, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), Palette.Gold, alpha);

        private static readonly Dictionary<Texture2D, GUIStyle> FrameStyles = new Dictionary<Texture2D, GUIStyle>();

        private static GUIStyle FrameStyle(Color top, Color bottom, Color rim)
        {
            var tex = Bevel(top, bottom, rim);
            if (FrameStyles.TryGetValue(tex, out var s) && s != null) return s;
            s = new GUIStyle { border = new RectOffset(14, 14, 14, 14) };
            s.normal.background = tex;
            FrameStyles[tex] = s;
            return s;
        }

        private static GUIStyle _button;

        /// <summary>El botón del juego: placa de oro sobre tinta. También lo usan el HUD y la Cadena de la Fase 1.</summary>
        public static GUIStyle ButtonBase()
        {
            if (_button != null) return _button;
            _button = new GUIStyle(GUI.skin.button) { border = new RectOffset(14, 14, 14, 14), alignment = TextAnchor.MiddleCenter, padding = new RectOffset(10, 10, 4, 4), richText = true, wordWrap = true };
            Set(_button.normal, Bevel(Palette.Hex("#4A3248"), Palette.Hex("#241830"), Palette.Gold), Palette.Parchment);
            Set(_button.hover, Bevel(Palette.Hex("#6A4458"), Palette.Hex("#34223C"), Palette.GoldBright), Palette.SaltWhite);
            Set(_button.active, Bevel(Palette.Hex("#241830"), Palette.Hex("#4A3248"), Palette.GoldBright), Palette.GoldBright);
            Set(_button.focused, Bevel(Palette.Hex("#4A3248"), Palette.Hex("#241830"), Palette.Gold), Palette.Parchment);
            if (Kit("chrome/btn_secondary_1x") != null)
            {
                _button.border = new RectOffset(15, 15, 15, 15);
                Set(_button.normal, Kit("chrome/btn_secondary_1x"), Palette.Parchment);
                Set(_button.hover, Kit("chrome/btn_primary_1x"), Palette.SaltWhite);
                Set(_button.active, Kit("chrome/btn_primary_1x"), Palette.Spark);
                Set(_button.focused, Kit("chrome/btn_secondary_1x"), Palette.Parchment);
                if (BoldFont != null) _button.font = BoldFont;
                _button.fontStyle = FontStyle.Normal;
            }
            return _button;
        }

        private static void Set(GUIStyleState state, Texture2D tex, Color text)
        {
            state.background = tex;
            state.textColor = text;
        }

        /// <summary>Texto grande con sombra, para títulos y carteles.</summary>
        public static void Title(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GuiKit.Label(new Rect(r.x + 2, r.y + 3, r.width, r.height), text, size, Palette.WithAlpha(Color.black, 0.75f), anchor, true);
            GuiKit.Label(r, text, size, color, anchor, true);
        }

        /// <summary>Colores de planeta seguros para daltonismo (docs/paleta.svg), cuando el ajuste está activo.</summary>
        public static bool ColorSafe;

        public static void Tint(Rect r, Color c) => GuiKit.Fill(r, c);

        // ------------------------------------------------------------------ Dibujos de prueba para los iconos

        /// <summary>El Fresno: tronco, raíces y ramas en líneas de oro. Es el emblema del juego.</summary>
        public static void TreeGlyph(Vector2 c, float s, Color color, float width = 3f)
        {
            GuiKit.Line(new Vector2(c.x, c.y + s), new Vector2(c.x, c.y - s * 0.2f), width * 1.6f, color);
            for (int i = -1; i <= 1; i += 2)
            {
                GuiKit.Line(new Vector2(c.x, c.y + s), new Vector2(c.x + i * s * 0.6f, c.y + s * 1.1f), width, color);
                GuiKit.Line(new Vector2(c.x, c.y + s * 0.6f), new Vector2(c.x + i * s * 0.5f, c.y + s * 1.05f), width * 0.7f, color);
                GuiKit.Line(new Vector2(c.x, c.y), new Vector2(c.x + i * s * 0.75f, c.y - s * 0.55f), width, color);
                GuiKit.Line(new Vector2(c.x, c.y - s * 0.2f), new Vector2(c.x + i * s * 0.4f, c.y - s * 0.95f), width, color);
                GuiKit.Line(new Vector2(c.x + i * s * 0.45f, c.y - s * 0.3f), new Vector2(c.x + i * s * 0.95f, c.y - s * 0.3f), width * 0.7f, color);
            }
            GuiKit.Line(new Vector2(c.x, c.y - s * 0.2f), new Vector2(c.x, c.y - s), width, color);
        }

        public static void BookGlyph(Vector2 c, float s, Color color)
        {
            GuiKit.Line(new Vector2(c.x, c.y - s * 0.6f), new Vector2(c.x, c.y + s * 0.7f), 4, color);
            for (int i = -1; i <= 1; i += 2)
            {
                GuiKit.Line(new Vector2(c.x, c.y - s * 0.6f), new Vector2(c.x + i * s, c.y - s * 0.75f), 4, color);
                GuiKit.Line(new Vector2(c.x, c.y + s * 0.7f), new Vector2(c.x + i * s, c.y + s * 0.55f), 4, color);
                GuiKit.Line(new Vector2(c.x + i * s, c.y - s * 0.75f), new Vector2(c.x + i * s, c.y + s * 0.55f), 4, color);
                for (int k = 0; k < 3; k++)
                    GuiKit.Line(new Vector2(c.x + i * s * 0.2f, c.y - s * 0.35f + k * s * 0.32f), new Vector2(c.x + i * s * 0.8f, c.y - s * 0.45f + k * s * 0.32f), 2, Palette.WithAlpha(color, 0.7f));
            }
        }

        public static void SpringGlyph(Vector2 c, float s, Color color)
        {
            for (int i = 0; i < 3; i++) GuiKit.Ring(new Vector2(c.x, c.y + s * 0.3f), s * (0.35f + i * 0.32f), 1f, Palette.WithAlpha(color, 1f - i * 0.28f), Color.clear, 24 + i * 8, 2.4f);
            for (int i = -1; i <= 1; i++)
                for (int k = 0; k < 3; k++)
                    GuiKit.Dot(new Vector2(c.x + i * s * 0.32f + Mathf.Sin(k * 1.7f + i) * s * 0.07f, c.y - s * 0.1f - k * s * 0.3f), 3.4f - k * 0.6f, Palette.WithAlpha(color, 0.9f - k * 0.2f));
        }

        public static void FlameGlyph(Vector2 c, float s, Color color)
        {
            // Una balanza con una llama: el Juicio de Pureza.
            GuiKit.Line(new Vector2(c.x - s, c.y - s * 0.4f), new Vector2(c.x + s, c.y - s * 0.4f), 4, color);
            GuiKit.Line(new Vector2(c.x, c.y - s * 0.7f), new Vector2(c.x, c.y + s * 0.9f), 4, color);
            GuiKit.Line(new Vector2(c.x - s * 0.5f, c.y + s * 0.9f), new Vector2(c.x + s * 0.5f, c.y + s * 0.9f), 5, color);
            for (int i = -1; i <= 1; i += 2)
            {
                GuiKit.Line(new Vector2(c.x + i * s, c.y - s * 0.4f), new Vector2(c.x + i * s * 0.65f, c.y + s * 0.3f), 2, color);
                GuiKit.Line(new Vector2(c.x + i * s, c.y - s * 0.4f), new Vector2(c.x + i * s * 1.35f, c.y + s * 0.3f), 2, color);
                GuiKit.Line(new Vector2(c.x + i * s * 0.6f, c.y + s * 0.3f), new Vector2(c.x + i * s * 1.4f, c.y + s * 0.3f), 4, color);
            }
            GuiKit.Diamond(new Vector2(c.x, c.y - s * 0.75f), s * 0.2f, Palette.FiveStar);
        }

        public static void AnvilGlyph(Vector2 c, float s, Color color)
        {
            GuiKit.Fill(new Rect(c.x - s, c.y - s * 0.45f, s * 2, s * 0.35f), color);
            GuiKit.Fill(new Rect(c.x - s * 0.4f, c.y - s * 0.1f, s * 0.8f, s * 0.55f), color);
            GuiKit.Fill(new Rect(c.x - s * 0.7f, c.y + s * 0.45f, s * 1.4f, s * 0.3f), color);
        }

        public static void PotGlyph(Vector2 c, float s, Color color)
        {
            GuiKit.Dot(new Vector2(c.x, c.y + s * 0.2f), s * 0.75f, color);
            GuiKit.Fill(new Rect(c.x - s * 0.8f, c.y - s * 0.5f, s * 1.6f, s * 0.22f), color);
            for (int k = 0; k < 3; k++) GuiKit.Dot(new Vector2(c.x - s * 0.3f + k * s * 0.3f, c.y - s * 0.75f - (k % 2) * s * 0.15f), 3, Palette.WithAlpha(color, 0.8f));
        }

        public static void CovenGlyph(Vector2 c, float s, Color color)
        {
            for (int i = -1; i <= 1; i++)
            {
                float x = c.x + i * s * 0.7f, y = c.y + (i == 0 ? -s * 0.1f : s * 0.1f);
                GuiKit.Dot(new Vector2(x, y - s * 0.3f), s * 0.28f, color);
                GuiKit.Fill(new Rect(x - s * 0.28f, y - s * 0.02f, s * 0.56f, s * 0.7f), color);
            }
        }

        public static void ShopGlyph(Vector2 c, float s, Color color)
        {
            GuiKit.Diamond(c, s * 0.75f, color);
            GuiKit.Diamond(c, s * 0.38f, Palette.WithAlpha(Color.white, 0.55f));
        }

        public static void HomeGlyph(Vector2 c, float s, Color color)
        {
            // El caparazón de la Tortuga.
            GuiKit.Dot(new Vector2(c.x, c.y + s * 0.1f), s * 0.8f, color);
            GuiKit.Fill(new Rect(c.x - s, c.y + s * 0.15f, s * 2, s * 0.8f), Palette.WithAlpha(Palette.Abyss, 0.65f));
            GuiKit.Fill(new Rect(c.x - s * 0.9f, c.y + s * 0.2f, s * 1.8f, s * 0.14f), color);
            GuiKit.Dot(new Vector2(c.x + s * 0.95f, c.y + s * 0.05f), s * 0.2f, color);
        }
    }

    /// <summary>
    /// Retratos del elenco. Busto de 160 px (art de Codex) y cuerpo recortado. La escena los trae asignados; en el Editor,
    /// si la escena es vieja, se buscan directo en la carpeta de arte.
    /// </summary>
    public sealed class PortraitBank
    {
        private readonly Dictionary<string, Texture2D> _byName = new Dictionary<string, Texture2D>();

        /// <summary>Los datos usan los IDs de la Fase 1 hasta que corra la Fase 1.5 (decisión D-03); el arte ya usa los nuevos.</summary>
        public static string ArtId(string id) => id == "zarigueya" ? "muertita" : id == "topo" ? "topacia" : id;

        public PortraitBank(IEnumerable<Texture2D> textures)
        {
            if (textures == null) return;
            foreach (var t in textures) if (t != null) _byName[t.name] = t;
        }

        public Texture2D Bust(string id, bool thirdPhase = false)
        {
            string art = ArtId(id);
            if (thirdPhase && _byName.TryGetValue(art + "_f3_neutral", out var f3)) return f3;
            return _byName.TryGetValue(art + "_f1_neutral", out var f1) ? f1 : null;
        }

        public Texture2D Body(string id) => _byName.TryGetValue(ArtId(id) + "_f1_cuerpo", out var b) ? b : null;

        /// <summary>Dibuja una imagen recortándola para llenar el rectángulo. vAnchor 1 alinea arriba.</summary>
        public static void Fill(Rect r, Texture2D tex, float vAnchor = 1f, Color? tint = null)
        {
            if (tex == null) return;
            float ra = r.width / r.height, ta = tex.width / (float)tex.height;
            var uv = new Rect(0, 0, 1, 1);
            if (ta > ra) { uv.width = ra / ta; uv.x = (1 - uv.width) / 2; }
            else { uv.height = ta / ra; uv.y = (1 - uv.height) * vAnchor; }
            var old = GUI.color;
            if (tint.HasValue) GUI.color = tint.Value;
            GUI.DrawTextureWithTexCoords(r, tex, uv, true);
            GUI.color = old;
        }

        /// <summary>Figura de reemplazo para las brujas cuyo arte todavía no existe.</summary>
        public static void Silhouette(Rect r, Color color)
        {
            Color c = Color.Lerp(color, Palette.Abyss, 0.45f);
            float head = Mathf.Min(r.width, r.height) * 0.24f;
            GuiKit.Dot(new Vector2(r.center.x, r.y + head), head, c);
            GuiKit.Fill(new Rect(r.center.x - head * 1.2f, r.y + head * 1.9f, head * 2.4f, r.height - head * 1.9f), c);
        }

        /// <summary>Dibuja la imagen entera dentro del rectángulo, sin deformarla y apoyada abajo.</summary>
        public static void Fit(Rect r, Texture2D tex, Color? tint = null)
        {
            if (tex == null) return;
            float ta = tex.width / (float)tex.height;
            float h = r.height, w = h * ta;
            if (w > r.width) { w = r.width; h = w / ta; }
            var box = new Rect(r.center.x - w / 2, r.yMax - h, w, h);
            var old = GUI.color;
            if (tint.HasValue) GUI.color = tint.Value;
            GUI.DrawTexture(box, tex, ScaleMode.StretchToFill, true);
            GUI.color = old;
        }
    }
}
