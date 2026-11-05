using System.Collections.Generic;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Colores de la paleta del proyecto (docs/REFERENCIAS.md, sección de paleta).</summary>
    public static class Palette
    {
        public static readonly Color Parchment = Hex("#EDE3CC");
        public static readonly Color ParchmentShadow = Hex("#CFBE9A");
        public static readonly Color Ink = Hex("#2A2018");
        public static readonly Color Abyss = Hex("#0B0B14");
        public static readonly Color Panel = Hex("#1A1A28");
        public static readonly Color Gold = Hex("#B8923A");
        public static readonly Color GoldBright = Hex("#E6C76E");
        public static readonly Color Crimson = Hex("#8E2430");
        public static readonly Color SaltWhite = Hex("#F6F4EE");
        public static readonly Color Spark = Hex("#FFF1B8");
        public static readonly Color FiveStar = Hex("#FFD84D");
        public static readonly Color Danger = Hex("#E0533F");

        public static Color Planet(Planet p)
        {
            if (Theme.ColorSafe)
            {
                // Paleta de Okabe e Ito, distinguible con los daltonismos comunes. El nombre siempre acompaña al color.
                switch (p)
                {
                    case Core.Magic.Planet.Sol: return Hex("#E69F00");
                    case Core.Magic.Planet.Luna: return Hex("#F0F0F0");
                    case Core.Magic.Planet.Marte: return Hex("#D55E00");
                    case Core.Magic.Planet.Mercurio: return Hex("#56B4E9");
                    case Core.Magic.Planet.Venus: return Hex("#009E73");
                    case Core.Magic.Planet.Jupiter: return Hex("#0072B2");
                    default: return Hex("#CC79A7");
                }
            }
            switch (p)
            {
                case Core.Magic.Planet.Sol: return Hex("#F5B02E");
                case Core.Magic.Planet.Luna: return Hex("#E2F0FA");
                case Core.Magic.Planet.Marte: return Hex("#C7301A");
                case Core.Magic.Planet.Mercurio: return Hex("#1CC8EE");
                case Core.Magic.Planet.Venus: return Hex("#86A775");
                case Core.Magic.Planet.Jupiter: return Hex("#6262EA");
                default: return Hex("#85608A");
            }
        }

        public static Color Band(SalinityBand b)
        {
            switch (b)
            {
                case SalinityBand.Estable: return Hex("#3EA48E");
                case SalinityBand.Salobre: return Hex("#E6E9BB");
                case SalinityBand.Sobresalada: return Hex("#E59200");
                case SalinityBand.CristalizacionTurbia: return Hex("#CC3A5C");
                default: return Hex("#D5A4F4");
            }
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static string ToHex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }

    /// <summary>
    /// Utilidades de dibujo IMGUI. Toda la interfaz se dibuja sobre un lienzo virtual de 1920×1080
    /// escalado a la pantalla real (horizontal, como pide el GDD).
    /// </summary>
    public static class GuiKit
    {
        public const float Width = 1920f;
        public const float Height = 1080f;

        private static Texture2D _white;
        private static Texture2D _circle;
        private static Texture2D _diamond;
        private static readonly Dictionary<string, GUIStyle> Styles = new Dictionary<string, GUIStyle>();

        public static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                }
                return _white;
            }
        }

        public static Texture2D Circle => _circle != null ? _circle : (_circle = MakeShape(64, false));
        public static Texture2D DiamondTex => _diamond != null ? _diamond : (_diamond = MakeShape(64, true));

        private static Texture2D MakeShape(int size, bool diamond)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    float d = diamond ? (Mathf.Abs(dx) + Mathf.Abs(dy)) : Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(r - d);
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Escala el lienzo virtual a la pantalla manteniendo la proporción (con franjas si hace falta).</summary>
        public static void BeginScreen()
        {
            float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
            float ox = (Screen.width - Width * scale) / 2f;
            float oy = (Screen.height - Height * scale) / 2f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        }

        /// <summary>Convierte un punto de pantalla real (por ejemplo de WorldToScreenPoint) al lienzo virtual.</summary>
        public static Vector2 ScreenToCanvas(Vector3 screen)
        {
            float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
            float ox = (Screen.width - Width * scale) / 2f;
            float oy = (Screen.height - Height * scale) / 2f;
            return new Vector2((screen.x - ox) / scale, (Screen.height - screen.y - oy) / scale);
        }

        public static GUIStyle Style(int size, Color color, TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false, bool wrap = false)
        {
            string key = size + "|" + ColorUtility.ToHtmlStringRGBA(color) + "|" + anchor + "|" + bold + "|" + wrap;
            if (Styles.TryGetValue(key, out var s) && s != null) return s;
            s = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                alignment = anchor,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                wordWrap = wrap,
                richText = true,
                clipping = TextClipping.Clip,
            };
            s.normal.textColor = color;
            s.padding = new RectOffset(0, 0, 0, 0);
            Styles[key] = s;
            return s;
        }

        public static GUIStyle ButtonStyle(int size)
        {
            string key = "btn" + size;
            if (Styles.TryGetValue(key, out var s) && s != null) return s;
            s = new GUIStyle(Theme.ButtonBase()) { fontSize = size, richText = true, wordWrap = true };
            Styles[key] = s;
            return s;
        }

        public static void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, White);
            GUI.color = old;
        }

        public static void Frame(Rect r, Color c, float t = 2f)
        {
            Fill(new Rect(r.x, r.y, r.width, t), c);
            Fill(new Rect(r.x, r.yMax - t, r.width, t), c);
            Fill(new Rect(r.x, r.y, t, r.height), c);
            Fill(new Rect(r.xMax - t, r.y, t, r.height), c);
        }

        public static void PanelBox(Rect r, float alpha = 0.88f)
        {
            Theme.GoldPanel(r, alpha);
        }

        public static void Bar(Rect r, float fraction, Color fill, Color back)
        {
            Fill(r, back);
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(fraction), r.height), fill);
        }

        public static void Dot(Vector2 center, float radius, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), Circle);
            GUI.color = old;
        }

        public static void Diamond(Vector2 center, float radius, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), DiamondTex);
            GUI.color = old;
        }

        public static void Line(Vector2 a, Vector2 b, float width, Color c)
        {
            var old = GUI.matrix;
            var oldColor = GUI.color;
            Vector2 d = b - a;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, a);
            GUI.color = c;
            GUI.DrawTexture(new Rect(a.x, a.y - width / 2f, d.magnitude, width), White);
            GUI.matrix = old;
            GUI.color = oldColor;
        }

        /// <summary>Anillo de puntos: los primeros fraction·segments van encendidos.</summary>
        public static void Ring(Vector2 center, float radius, float fraction, Color on, Color off, int segments = 36, float dot = 4f)
        {
            int lit = Mathf.RoundToInt(Mathf.Clamp01(fraction) * segments);
            for (int i = 0; i < segments; i++)
            {
                float ang = (-90f + 360f * i / segments) * Mathf.Deg2Rad;
                var p = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
                Dot(p, dot, i < lit ? on : off);
            }
        }

        public static bool Button(Rect r, string text, bool enabled = true, int size = 22)
        {
            bool old = GUI.enabled;
            GUI.enabled = old && enabled;
            bool pressed = GUI.Button(r, text, ButtonStyle(size));
            GUI.enabled = old;
            return pressed;
        }

        public static void Label(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false, bool wrap = false)
        {
            if (Theme.LargeText) size = Mathf.RoundToInt(size * 1.12f);
            GUI.Label(r, text, Style(size, color, anchor, bold, wrap));
        }

        /// <summary>Etiqueta de planeta: color más nombre, porque el color nunca va solo (REFERENCIAS, reglas de color).</summary>
        public static void PlanetPill(Rect r, Planet p, bool bold = false)
        {
            Fill(r, Palette.WithAlpha(Palette.Abyss, 0.9f));
            Fill(new Rect(r.x, r.y, 6, r.height), Palette.Planet(p));
            Label(new Rect(r.x + 10, r.y, r.width - 12, r.height), Planets.DisplayName(p), Mathf.RoundToInt(r.height * 0.62f), Palette.Planet(p), TextAnchor.MiddleLeft, bold);
        }
    }
}
