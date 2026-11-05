using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Save;
using SalYAlmas.Core.World;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private int _carousel;
        private float _carouselShown;
        private float _dragStart = -1;
        private int _covenPage;
        private int _covenFilter; // 0 todas, 3, 4 o 5
        private int _creatorSign;
        private int _creatorSpecies;
        private int _creatorPronoun;
        private string _creatorName = "";

        private static readonly (Screen screen, string key)[] Destinations =
        {
            (Screen.Story, "dest.story"), (Screen.Quests, "dest.quests"), (Screen.Fresno, "dest.fresno"), (Screen.Summon, "dest.summon"), (Screen.Coven, "dest.trials"), (Screen.Ledger, "dest.ledger"),
        };

        // ------------------------------------------------------------------ Home

        private void DrawHome(Rect a)
        {
            float cardsWidth = 1120f;
            var party = Party();
            float w = cardsWidth / Mathf.Max(4, party.Count);
            for (int i = 0; i < 4; i++)
            {
                var r = new Rect(a.x + i * w, a.y, w - 8, a.height);
                if (i < party.Count) PartyCard(r, party[i], i == 0);
                else
                {
                    Theme.GoldPanel(r, 0.7f);
                    if (GuiKit.Button(new Rect(r.center.x - 90, r.center.y - 40, 180, 80), "+ Bruja", true, 24)) GoRoot(Screen.Coven);
                }
            }

            var side = new Rect(a.x + cardsWidth + 10, a.y, a.width - cardsWidth - 10, a.height);
            float bw = (side.width - 24) / 3f;
            if (ShortcutButton(new Rect(side.x, side.y, bw, 72), L("menu.settings"))) Go(Screen.Settings);
            if (ShortcutButton(new Rect(side.x + bw + 12, side.y, bw, 72), L("menu.odds"))) Go(Screen.Odds);
            if (ShortcutButton(new Rect(side.x + (bw + 12) * 2, side.y, bw, 72), L("base.mirror"))) Go(Screen.Mirror);

            DrawCarousel(new Rect(side.x, side.y + 84, side.width, side.height - 84));
        }

        private bool ShortcutButton(Rect r, string text) => GuiKit.Button(r, text, true, 20);

        private void PartyCard(Rect r, string id, bool leader)
        {
            var wp = Game.Profile.Witches[id];
            var entry = _meta.Roster[id];
            var planet = wp.Via == Via.Mayor ? entry.Primary : entry.Secondary;
            Color color = Palette.Planet(planet);
            Theme.Panel(r, Color.Lerp(color, Palette.Abyss, 0.35f), Color.Lerp(color, Palette.Abyss, 0.85f), Palette.GoldBright);
            var inner = new Rect(r.x + 8, r.y + 8, r.width - 16, r.height - 16);
            var body = _art.Body(id);
            if (body != null) PortraitBank.Fill(inner, body, 1f);
            else if (BustFor(id) != null) PortraitBank.Fit(inner, BustFor(id));
            else PortraitBank.Silhouette(new Rect(inner.x + 30, inner.y + 80, inner.width - 60, inner.height - 240), color);

            var shade = new Rect(inner.x, inner.yMax - 130, inner.width, 130);
            GuiKit.Fill(shade, Palette.WithAlpha(Palette.Abyss, 0.72f));
            Theme.Title(new Rect(inner.x + 10, shade.y + 4, inner.width - 20, 36), Short(id), 26, Palette.Parchment, TextAnchor.MiddleLeft);
            GuiKit.Label(new Rect(inner.x + 10, shade.y + 40, inner.width - 20, 28), Stars(entry.Rarity), 22, entry.Rarity == 5 ? Palette.FiveStar : entry.Rarity == 4 ? Palette.Hex("#C9A6F2") : Palette.Parchment, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(inner.x + 10, shade.y + 70, inner.width - 70, 26), $"Nivel {wp.Level}  ·  Grado {wp.Grade}", 18, Palette.Parchment);
            GuiKit.Label(new Rect(inner.x + 10, shade.y + 96, inner.width - 70, 26), Planets.DisplayName(planet), 17, color);
            PlanetOrb(new Vector2(inner.xMax - 34, shade.yMax - 36), 28, planet);
            if (leader)
            {
                var ribbon = new Rect(r.x + 16, r.y + 14, r.width - 32, 36);
                Theme.Panel(ribbon, Palette.GoldBright, Palette.Gold, Palette.SaltWhite);
                GuiKit.Label(ribbon, L("top.leader"), 18, Palette.Ink, TextAnchor.MiddleCenter, true);
            }
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { _witch = id; _trialResult = null; Go(Screen.Witch); }
        }

        private string Short(string id) => _meta.HasKit(id) ? Game.Combat.Witch(id).ShortName : WitchName(id);

        /// <summary>El busto que corresponde a su grado. El modo recatado se queda con la primera fase (GDD 23.3).</summary>
        private Texture2D BustFor(string id)
        {
            bool third = Game.Profile.Owns(id) && Game.Profile.Witches[id].Grade >= Grade.S && !Game.Profile.Settings.ModestMode;
            return _art.Bust(id, third);
        }

        private static void PlanetOrb(Vector2 c, float radius, Planet p)
        {
            GuiKit.Dot(c, radius + 3, Palette.GoldBright);
            GuiKit.Dot(c, radius, Color.Lerp(Palette.Planet(p), Palette.Abyss, 0.25f));
            GuiKit.Dot(new Vector2(c.x - radius * 0.3f, c.y - radius * 0.35f), radius * 0.3f, Palette.WithAlpha(Color.white, 0.55f));
            // La inicial del planeta: el color nunca va solo (REFERENCIAS, reglas de color).
            GuiKit.Label(new Rect(c.x - radius, c.y - radius, radius * 2, radius * 2), Planets.DisplayName(p).Substring(0, 1), Mathf.RoundToInt(radius), Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        private void DrawCarousel(Rect r)
        {
            _carouselShown = Mathf.Lerp(_carouselShown, _carousel, 0.18f);
            var e = Event.current;
            float cardW = r.width * 0.72f;
            float cx = r.center.x;
            for (int i = -1; i <= 1; i++)
            {
                int idx = ((_carousel + i) % Destinations.Length + Destinations.Length) % Destinations.Length;
                float offset = (i + (_carousel - _carouselShown)) * (cardW * 0.92f);
                bool center = i == 0;
                float scale = center ? 1f : 0.82f;
                var card = new Rect(cx + offset - cardW * scale / 2, r.center.y - (r.height - 70) * scale / 2 - 16, cardW * scale, (r.height - 70) * scale);
                // Las vecinas se asoman por los lados, sin salirse de la columna.
                if (!center)
                {
                    if (i < 0) { float over = r.x - card.x; if (over > 0) { card.x += over; card.width -= over; } }
                    else { float over = card.xMax - r.xMax; if (over > 0) card.width -= over; }
                    if (card.width < 30) continue;
                }
                DestinationCard(card, idx, center);
            }

            for (int i = 0; i < Destinations.Length; i++)
            {
                var c = new Vector2(r.center.x + (i - (Destinations.Length - 1) / 2f) * 36, r.yMax - 22);
                GuiKit.Dot(c, 10, i == _carousel ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.45f));
            }
            if (GuiKit.Button(new Rect(r.x, r.center.y - 56, 56, 92), "◀", true, 26)) _carousel = (_carousel + Destinations.Length - 1) % Destinations.Length;
            if (GuiKit.Button(new Rect(r.xMax - 56, r.center.y - 56, 56, 92), "▶", true, 26)) _carousel = (_carousel + 1) % Destinations.Length;

            // Deslizar con el dedo.
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) _dragStart = e.mousePosition.x;
            if (e.type == EventType.MouseUp && _dragStart >= 0)
            {
                float dx = e.mousePosition.x - _dragStart;
                if (Mathf.Abs(dx) > 70) _carousel = (_carousel + (dx < 0 ? 1 : Destinations.Length - 1)) % Destinations.Length;
                _dragStart = -1;
            }
        }

        private void DestinationCard(Rect card, int index, bool center)
        {
            var (screen, key) = Destinations[index];
            Color[] tops = { Palette.Hex("#8C4A30"), Palette.Hex("#5A4A7A"), Palette.Hex("#3E6A4E"), Palette.Hex("#3E6A8E"), Palette.Hex("#7A3058"), Palette.Hex("#6E5A30") };
            Theme.Panel(card, tops[index], Color.Lerp(tops[index], Palette.Abyss, 0.85f), center ? Palette.GoldBright : Palette.Gold, center ? 1f : 0.65f);
            var c = new Vector2(card.center.x, card.y + card.height * 0.38f);
            float s = Mathf.Min(card.width, card.height) * 0.26f;
            Color gc = center ? Palette.GoldBright : Palette.WithAlpha(Palette.GoldBright, 0.5f);
            if (card.width > 140)
                switch (index)
                {
                    case 0: Theme.BookGlyph(c, s, gc); break;
                    case 1: GuiKit.Diamond(c, s * 0.8f, gc); GuiKit.Diamond(c, s * 0.4f, Palette.Abyss); break;
                    case 2: Theme.TreeGlyph(c, s, gc, 5f); break;
                    case 3: Theme.SpringGlyph(c, s, gc); break;
                    case 4: Theme.FlameGlyph(c, s, gc); break;
                    default: Theme.BookGlyph(c, s * 0.9f, gc); break;
                }
            if (!center) return;

            Theme.Title(new Rect(card.x, card.yMax - card.height * 0.34f, card.width, 64), L(key), 50, Palette.FiveStar);
            GuiKit.Label(new Rect(card.x + 16, card.yMax - card.height * 0.34f + 66, card.width - 32, 56), L(key + ".sub"), 20, Palette.Parchment, TextAnchor.UpperCenter, false, true);
            int badge = screen == Screen.Fresno && F.InRun ? 1 : screen == Screen.Story && (Game.Story.InMission || Game.Story.InExpedition) ? 1 : screen == Screen.Quests ? Game.Quests.Pending : 0;
            if (badge > 0) Badge(new Vector2(card.xMax - 28, card.y + 28), badge);
            if (GUI.Button(card, GUIContent.none, GUIStyle.none))
            {
                if (screen == Screen.Coven) { _covenFilter = 0; GoRoot(Screen.Coven); }
                else Go(screen);
            }
        }

        // ------------------------------------------------------------------ Aquelarre: la grilla de brujas

        private void DrawCoven(Rect a)
        {
            var body = Header(a, L("nav.coven"), $"{Game.Profile.Witches.Count}/{_meta.RosterOrder.Count} brujas");
            string[] names = { "Todas", "5★", "4★", "3★" };
            int[] vals = { 0, 5, 4, 3 };
            for (int i = 0; i < 4; i++)
            {
                var b = new Rect(body.x + 1180 + i * 170, a.y, 160, 56);
                if (GuiKit.Button(b, names[i], true, 22)) { _covenFilter = vals[i]; _covenPage = 0; }
                if (_covenFilter == vals[i]) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            Theme.GoldPanel(body, 0.55f);
            var owned = _meta.RosterOrder.Where(id => Game.Profile.Owns(id) && (_covenFilter == 0 || _meta.Roster[id].Rarity == _covenFilter))
                .OrderByDescending(id => Game.Profile.Party.Contains(id)).ThenByDescending(id => _meta.Roster[id].Rarity).ThenBy(id => id, StringComparer.Ordinal).ToList();
            const float tile = 180, gap = 12;
            int cols = Mathf.FloorToInt((body.width - 20) / (tile + gap));
            int rows = Mathf.Max(1, Mathf.FloorToInt((body.height - 66) / (tile + 30 + gap)));
            int per = cols * rows;
            int pages = Mathf.Max(1, Mathf.CeilToInt(owned.Count / (float)per));
            _covenPage = Mathf.Clamp(_covenPage, 0, pages - 1);
            for (int i = 0; i < per && _covenPage * per + i < owned.Count; i++)
            {
                string id = owned[_covenPage * per + i];
                UnitTile(new Rect(body.x + 14 + (i % cols) * (tile + gap), body.y + 12 + (i / cols) * (tile + 30 + gap), tile, tile + 30), id);
            }
            if (pages > 1)
            {
                float y = body.yMax - 48;
                if (GuiKit.Button(new Rect(body.x + 14, y, 90, 42), "◀", _covenPage > 0, 20)) _covenPage--;
                GuiKit.Label(new Rect(body.x + 110, y, 120, 42), $"{_covenPage + 1} / {pages}", 20, Palette.Parchment, TextAnchor.MiddleCenter);
                if (GuiKit.Button(new Rect(body.x + 236, y, 90, 42), "▶", _covenPage < pages - 1, 20)) _covenPage++;
            }
        }

        private void UnitTile(Rect r, string id)
        {
            var entry = _meta.Roster[id];
            var w = Game.Profile.Witches[id];
            bool inParty = Game.Profile.Party.Contains(id);
            Color planet = Palette.Planet(entry.Primary);
            var face = new Rect(r.x, r.y, r.width, r.width);
            Theme.Panel(face, Color.Lerp(planet, Palette.Abyss, 0.3f), Color.Lerp(planet, Palette.Abyss, 0.8f), entry.Rarity == 5 ? Palette.FiveStar : entry.Rarity == 4 ? Palette.Hex("#A77BEA") : Palette.ParchmentShadow);
            var bust = BustFor(id);
            if (bust != null) GUI.DrawTexture(new Rect(face.x + 6, face.y + 6, face.width - 12, face.height - 12), bust, ScaleMode.ScaleToFit, true);
            else PortraitBank.Silhouette(new Rect(face.x + 30, face.y + 20, face.width - 60, face.height - 50), planet);
            GuiKit.Label(new Rect(face.x + 8, face.yMax - 30, face.width - 16, 24), Stars(entry.Rarity), 18, entry.Rarity == 5 ? Palette.FiveStar : Palette.Parchment, TextAnchor.MiddleCenter, true);
            if (!_meta.HasKit(id)) GuiKit.Label(new Rect(face.x + 6, face.y + 6, 110, 22), "sin kit", 14, Palette.ParchmentShadow);
            GuiKit.Label(new Rect(r.x, face.yMax + 2, r.width, 28), $"Nv {w.Level} · {w.Grade}", 17, Palette.Parchment, TextAnchor.MiddleCenter);
            if (inParty) Theme.Title(new Rect(face.x, face.y + 2, face.width, 28), "EQUIPO", 19, Palette.FiveStar);

            // El punto de arriba a la derecha pone o quita del equipo; el resto abre la ficha.
            var toggle = new Rect(face.xMax - 42, face.y + 6, 36, 36);
            GuiKit.Dot(toggle.center, 15, inParty ? Palette.FiveStar : Palette.WithAlpha(Palette.Abyss, 0.8f));
            GuiKit.Dot(toggle.center, 11, inParty ? Palette.Gold : Palette.WithAlpha(Palette.ParchmentShadow, 0.5f));
            if (_meta.HasKit(id) && GUI.Button(toggle, GUIContent.none, GUIStyle.none))
            {
                var p = Game.Profile.Party;
                if (inParty) p.Remove(id);
                else if (p.Count < 4) p.Add(id);
                else Note("El equipo lleva cuatro brujas.", false);
                Save();
            }
            else if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { _witch = id; _trialResult = null; _allies.Clear(); Go(Screen.Witch); }
        }

        // ------------------------------------------------------------------ Creador de la Calculista

        private void DrawCreator(Rect a)
        {
            var s = _meta.World.Story;
            var panel = new Rect(200, 50, 1520, 980);
            Theme.GoldPanel(panel);
            Theme.Title(new Rect(panel.x, panel.y + 10, panel.width, 60), "¿Quién cayó del Río Lácteo?", 40, Palette.GoldBright);
            float x = panel.x + 40, y = panel.y + 84, w = panel.width - 80;
            GuiKit.Label(new Rect(x, y, 200, 44), "Tu nombre", 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            GUI.SetNextControlName("nombre");
            _creatorName = GUI.TextField(new Rect(x + 200, y, 460, 44), _creatorName, 12, GuiKit.ButtonStyle(24));
            y += 58;
            GuiKit.Label(new Rect(x, y, 200, 44), "Pronombres", 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < s.Pronouns.Count; i++)
            {
                var b = new Rect(x + 200 + i * 160, y, 150, 44);
                if (GuiKit.Button(b, s.Pronouns[i], true, 20)) _creatorPronoun = i;
                if (_creatorPronoun == i) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            y += 58;
            GuiKit.Label(new Rect(x, y, 200, 44), "Especie", 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < s.Species.Count; i++)
            {
                var b = new Rect(x + 200 + (i % 6) * 200, y + (i / 6) * 50, 190, 44);
                if (GuiKit.Button(b, s.Species[i], true, 20)) _creatorSpecies = i;
                if (_creatorSpecies == i) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            y += 112;
            GuiKit.Label(new Rect(x, y, w, 44), "Tu signo ascendente define dónde empiezas", 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            y += 48;
            for (int i = 0; i < s.Signs.Count; i++)
            {
                var b = new Rect(x + (i % 4) * (w / 4), y + (i / 4) * 92, w / 4 - 10, 86);
                var sign = s.Signs[i];
                if (GuiKit.Button(b, $"{sign.Name}  ·  {sign.Element}\n<size=15>{sign.Phrase}</size>", true, 21)) _creatorSign = i;
                if (_creatorSign == i) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            y += 92 * 3 + 12;
            var chosen = s.Signs[_creatorSign];
            GuiKit.Label(new Rect(x, y, w - 460, 60), $"+1 de {chosen.Bonus}. Empiezas en {s.Region(chosen.Region)?.Name}.", 21, Palette.GoldBright, TextAnchor.MiddleLeft, false, true);
            if (GuiKit.Button(new Rect(x + w - 440, y, 440, 76), "Despertar", _creatorName.Trim().Length >= 2, 34))
            {
                var r = Game.Story.Create(_creatorName, s.Pronouns[_creatorPronoun], s.Species[_creatorSpecies], chosen.Id);
                Note(r.Message, r.Ok);
                if (r.Ok)
                {
                    Apply(Game.Story.StartMission("vado", "prologo"));
                    Current = Screen.Scene;
                }
            }
            if (GuiKit.Button(new Rect(panel.xMax - 300, panel.yMax - 54, 280, 44), "Perfil de prueba", true, 16)) NewTestProfile();
        }
    }
}
