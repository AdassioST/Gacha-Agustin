using System;
using System.Collections.Generic;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// La tirada de las Termas (GDD 16.1): sales que caen, vapor por rareza, reflejo de la constelación para 5★, la bruja que
    /// emerge y se sacude según su especie, su tarjeta y el resumen. El resultado ya viene decidido por el núcleo: esto solo
    /// lo muestra, y se puede saltar en cualquier punto. En una tirada de diez, solo las 4★ y 5★ hacen la emergencia completa.
    /// Los tiempos son // TUNING. El GDD pide Timeline: esto queda como secuencia por código hasta que haya arte 3D.
    /// </summary>
    public sealed class PullSequence
    {
        private enum Step { Fall, Vapor, Reflect, Emerge, Card, Summary }

        private static readonly float[] Durations = { 1.0f, 1.3f, 1.2f, 1.6f, 0f, 0f }; // TUNING

        /// <summary>Lo conecta la escena: reproduce un efecto de audio por su ID.</summary>
        public static Action<string> Sfx;

        private readonly MetaGame _g;
        private readonly PortraitBank _art;
        private readonly List<PullResult> _results;
        private readonly Color _water;
        private readonly Action _done;
        private readonly bool _ten;
        private readonly bool _surprise;

        private Step _step;
        private float _time;
        private int _index;
        private float _startTime;

        public PullSequence(MetaGame game, PortraitBank art, List<PullResult> results, Color water, Action done)
        {
            _g = game;
            _art = art;
            _results = results;
            _water = water;
            _done = done;
            _ten = results.Count > 1;
            _startTime = Time.unscaledTime;
            // Subida sorpresa: a veces el vapor pasa de violeta a dorado. Es presentación: el resultado ya está decidido.
            _surprise = UnityEngine.Random.value < 0.35f; // TUNING
            _index = NextFull(-1);
            _step = Step.Fall;
            Sfx?.Invoke("sfx_tirada_sales_caer");
            if (game.Profile.Settings.SkipCinematics) _step = Step.Summary;
        }

        private int NextFull(int from)
        {
            for (int i = from + 1; i < _results.Count; i++)
                if (!_ten || _results[i].Rarity >= 4) return i;
            return -1;
        }

        private int BestRarity
        {
            get
            {
                int best = 3;
                foreach (var r in _results) best = Mathf.Max(best, r.Rarity);
                return best;
            }
        }

        private static Color VaporColor(int rarity) =>
            rarity == 5 ? Palette.FiveStar : rarity == 4 ? Palette.Hex("#A77BEA") : Palette.SaltWhite;

        public void Draw()
        {
            float dt = Time.unscaledDeltaTime;
            _time += dt;

            GUI.DrawTexture(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Theme.Sky, ScaleMode.StretchToFill);
            var center = new Vector2(GuiKit.Width / 2, 760);
            DrawWater(center);

            float total = Durations[(int)_step];
            float t = total > 0 ? Mathf.Clamp01(_time / total) : 1f;
            switch (_step)
            {
                case Step.Fall: DrawFall(center, t); break;
                case Step.Vapor: DrawVapor(center, t); break;
                case Step.Reflect: DrawReflect(center, t); break;
                case Step.Emerge: DrawEmerge(center, t); break;
                case Step.Card: DrawCard(); break;
                case Step.Summary: DrawSummary(); break;
            }

            if (total > 0 && _time >= total) Advance();
            if (_step != Step.Summary && GuiKit.Button(new Rect(GuiKit.Width - 230, 24, 200, 66), "Saltar", true, 26))
            {
                _step = Step.Summary;
                Sfx?.Invoke("sfx_tirada_saltar");
            }
            // Tocar la pantalla avanza a la siguiente parte.
            if (Event.current.type == EventType.MouseDown && Event.current.mousePosition.y > 110 && _step != Step.Summary && _step != Step.Card && total > 0)
            {
                Advance();
                Event.current.Use();
            }
        }

        private void Advance()
        {
            _time = 0;
            switch (_step)
            {
                case Step.Fall:
                    _step = Step.Vapor;
                    int best = BestRarity;
                    Sfx?.Invoke(best == 5 ? (_surprise ? "mus_tirada_subida_dorada" : "mus_tirada_vapor_dorado") : best == 4 ? "mus_tirada_vapor_violeta" : "mus_tirada_vapor_blanco");
                    break;
                case Step.Vapor:
                    if (_index < 0) _step = Step.Summary;
                    else EnterReveal();
                    break;
                case Step.Reflect:
                    _step = Step.Emerge;
                    Sfx?.Invoke("sfx_tirada_emergencia");
                    break;
                case Step.Emerge:
                    _step = Step.Card;
                    Sfx?.Invoke("sfx_tirada_tarjeta");
                    break;
                case Step.Card:
                    _index = NextFull(_index);
                    if (_index < 0) _step = Step.Summary;
                    else EnterReveal();
                    break;
            }
        }

        private void EnterReveal()
        {
            bool five = _results[_index].Rarity == 5;
            _step = five ? Step.Reflect : Step.Emerge;
            Sfx?.Invoke(five ? "sfx_tirada_constelacion" : "sfx_tirada_emergencia");
        }

        private void DrawWater(Vector2 c)
        {
            GuiKit.Dot(new Vector2(c.x, c.y + 80), 470, Palette.Hex("#2A2418"));
            GuiKit.Dot(new Vector2(c.x, c.y + 80), 440, _water);
            GuiKit.Ring(new Vector2(c.x, c.y + 80), 446, 1f, Palette.WithAlpha(Palette.SaltWhite, 0.6f), Color.clear, 90, 2f);
            for (int i = 0; i < 3; i++)
            {
                float k = (Time.unscaledTime * 0.25f + i / 3f) % 1f;
                GuiKit.Ring(new Vector2(c.x, c.y + 80), 60 + k * 340, 1f, Palette.WithAlpha(Palette.SaltWhite, 0.25f * (1 - k)), Color.clear, 60, 1.5f);
            }
        }

        private void DrawFall(Vector2 c, float t)
        {
            int count = _ten ? 30 : 12;
            for (int i = 0; i < count; i++)
            {
                float offset = i * 0.61803f % 1f;
                float k = Mathf.Clamp01(t * 1.5f - offset * 0.5f);
                var p = new Vector2(c.x - 180 + offset * 360, Mathf.Lerp(90, c.y + 40, k * k));
                GuiKit.Diamond(p, 7, Palette.SaltWhite);
            }
            Theme.Title(new Rect(0, 140, GuiKit.Width, 60), _ten ? "Diez puñados de sal al agua" : "Un puñado de sal al agua", 34, Palette.Parchment);
        }

        private void DrawVapor(Vector2 c, float t)
        {
            int best = BestRarity;
            // Con la subida sorpresa, una 5★ empieza en violeta.
            Color color = VaporColor(best);
            if (best == 5 && _surprise) color = Color.Lerp(VaporColor(4), VaporColor(5), Mathf.Clamp01((t - 0.55f) * 3f));
            if (Theme.ReduceFlashes) color = Color.Lerp(color, Palette.ParchmentShadow, 0.3f);
            for (int i = 0; i < 26; i++)
            {
                float k = (i * 0.37f + t * 0.8f) % 1f;
                float x = c.x + Mathf.Sin(i * 2.1f) * (70 + k * 200);
                GuiKit.Dot(new Vector2(x, c.y + 60 - k * 560), 40 + k * 70, Palette.WithAlpha(color, 0.2f * (1 - k)));
            }
            Theme.Title(new Rect(0, 140, GuiKit.Width, 60), "El vapor sube…", 34, color);
        }

        private void DrawReflect(Vector2 c, float t)
        {
            // La constelación de su Ascendente se dibuja en el reflejo, como pista.
            var r = _results[_index];
            ulong seed = (ulong)(r.WitchId?.Length ?? 7);
            var pts = new List<Vector2>();
            for (int i = 0; i < 7; i++)
            {
                float a = (i * 137.5f + seed * 41) * Mathf.Deg2Rad;
                pts.Add(new Vector2(c.x + Mathf.Cos(a) * (70 + i * 34), c.y + 90 + Mathf.Sin(a) * (40 + i * 18)));
            }
            int lit = Mathf.CeilToInt(t * pts.Count);
            for (int i = 0; i < lit; i++)
            {
                GuiKit.Dot(pts[i], 8, Palette.FiveStar);
                if (i > 0) GuiKit.Line(pts[i - 1], pts[i], 2.5f, Palette.WithAlpha(Palette.FiveStar, 0.8f));
            }
            Theme.Title(new Rect(0, 140, GuiKit.Width, 60), "El agua se aquieta y refleja el cielo", 34, Palette.FiveStar);
        }

        private void DrawEmerge(Vector2 c, float t)
        {
            var r = _results[_index];
            if (r.Kind != "bruja") { DrawResultIcon(c, r); return; }
            var body = _art.Body(r.WitchId);
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.5f));
            float shake = t > 0.65f && !Theme.ReduceFlashes && !Theme.ReduceMotion ? Mathf.Sin(Time.unscaledTime * 40f) * (1 - t) * 40f : 0f;
            var box = new Rect(c.x - 340 + shake, Mathf.Lerp(c.y + 60, 100, rise), 680, 840);
            var clip = new Rect(0, 0, GuiKit.Width, c.y + 140);
            GUI.BeginGroup(clip);
            if (body != null) PortraitBank.Fit(box, body);
            else if (_art.Bust(r.WitchId) != null) PortraitBank.Fit(box, _art.Bust(r.WitchId));
            else PortraitBank.Silhouette(new Rect(box.x + 170, box.y + 120, 340, 640), Palette.Planet(_g.Meta.Witch(r.WitchId).Primary));
            GUI.EndGroup();
            if (t > 0.65f)
                GuiKit.Label(new Rect(GuiKit.Width / 2 - 600, 950, 1200, 60), _g.Base.ShakeOf(r.WitchId), 28, Palette.Parchment, TextAnchor.MiddleCenter, false, true);
        }

        private void DrawResultIcon(Vector2 c, PullResult r)
        {
            GuiKit.Diamond(new Vector2(c.x, c.y - 160), 100, VaporColor(r.Rarity));
            Theme.Title(new Rect(0, 880, GuiKit.Width, 60), r.Kind == "polvo" ? $"{r.Dust} de Polvo Astral" : $"{r.SaltAmount} de Sal Cruda", 34, Palette.Parchment);
        }

        private void DrawCard()
        {
            var r = _results[_index];
            var card = new Rect(GuiKit.Width / 2 - 620, 110, 1240, 860);
            Color rim = VaporColor(r.Rarity);
            Theme.Panel(card, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), rim);
            if (r.Kind == "bruja")
            {
                var entry = _g.Meta.Witch(r.WitchId);
                var body = _art.Body(r.WitchId);
                var view = new Rect(card.x + 20, card.y + 20, 520, card.height - 40);
                GuiKit.Fill(view, Color.Lerp(Palette.Planet(entry.Primary), Palette.Abyss, 0.65f));
                if (body != null) PortraitBank.Fit(view, body);
                else if (_art.Bust(r.WitchId) != null) PortraitBank.Fit(view, _art.Bust(r.WitchId));
                else PortraitBank.Silhouette(new Rect(view.x + 90, view.y + 120, view.width - 180, view.height - 200), Palette.Planet(entry.Primary));
                float x = view.xMax + 30, w = card.xMax - x - 30, y = card.y + 40;
                Theme.Title(new Rect(x, y, w, 64), entry.Name, 44, rim, TextAnchor.MiddleLeft);
                y += 70;
                GuiKit.Label(new Rect(x, y, w, 40), new string('★', entry.Rarity), 36, rim, TextAnchor.MiddleLeft, true);
                y += 52;
                GuiKit.Label(new Rect(x, y, w, 34), entry.Species + "  ·  " + entry.Role, 24, Palette.Parchment);
                y += 50;
                GuiKit.PlanetPill(new Rect(x, y, 240, 40), entry.Primary, true);
                GuiKit.PlanetPill(new Rect(x + 256, y, 240, 40), entry.Secondary, true);
                y += 66;
                string line = _g.Base.PullLineOf(r.WitchId);
                if (line.Length > 0) GuiKit.Label(new Rect(x, y, w, 120), "«" + line + "»", 28, Palette.GoldBright, TextAnchor.UpperLeft, false, true);
                y += 130;
                if (r.Featured) GuiKit.Label(new Rect(x, y, w, 40), "Destacada", 26, Palette.FiveStar, TextAnchor.MiddleLeft, true);
                if (r.LostFeatured) GuiKit.Label(new Rect(x, y, w, 60), "Perdió el 75/25: la siguiente 5★ es la destacada.", 24, Palette.Parchment, TextAnchor.MiddleLeft, false, true);
            }
            else DrawResultIcon(new Vector2(GuiKit.Width / 2, 560), r);
            if (GuiKit.Button(new Rect(card.xMax - 330, card.yMax - 96, 300, 76), "Continuar", true, 28)) Advance();
        }

        private void DrawSummary()
        {
            var panel = new Rect(200, 70, 1520, 940);
            Theme.GoldPanel(panel);
            Theme.Title(new Rect(panel.x, panel.y + 10, panel.width, 60), "Lo que salió del agua", 38, Palette.GoldBright);
            const float tile = 260, gap = 16;
            int cols = 5;
            float x0 = panel.center.x - (Mathf.Min(_results.Count, cols) * (tile + gap) - gap) / 2f;
            for (int i = 0; i < _results.Count; i++)
            {
                var r = _results[i];
                var box = new Rect(x0 + (i % cols) * (tile + gap), panel.y + 90 + (i / cols) * (tile + 90), tile, tile + 70);
                Color rim = VaporColor(r.Rarity);
                Theme.Panel(box, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), rim);
                if (r.Kind == "bruja")
                {
                    var bust = _art.Bust(r.WitchId);
                    if (bust != null) GUI.DrawTexture(new Rect(box.x + 10, box.y + 10, tile - 20, tile - 20), bust, ScaleMode.ScaleToFit, true);
                    else PortraitBank.Silhouette(new Rect(box.x + 50, box.y + 20, tile - 100, tile - 40), Palette.Planet(_g.Meta.Witch(r.WitchId).Primary));
                    GuiKit.Label(new Rect(box.x + 8, box.yMax - 66, tile - 16, 30), _g.Meta.Witch(r.WitchId).Name, 18, Palette.Parchment, TextAnchor.MiddleCenter, true);
                }
                else
                {
                    GuiKit.Diamond(new Vector2(box.center.x, box.y + 120), 60, rim);
                    GuiKit.Label(new Rect(box.x + 8, box.yMax - 66, tile - 16, 30), r.Kind == "polvo" ? $"{r.Dust} Polvo Astral" : $"{r.SaltAmount} Sal Cruda", 18, Palette.Parchment, TextAnchor.MiddleCenter);
                }
                GuiKit.Label(new Rect(box.x, box.yMax - 36, tile, 30), new string('★', r.Rarity), 22, rim, TextAnchor.MiddleCenter, true);
            }
            if (GuiKit.Button(new Rect(panel.center.x - 200, panel.yMax - 96, 400, 78), "Volver", true, 30)) _done();
        }
    }
}
