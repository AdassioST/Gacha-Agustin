using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.World;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// La Odisea en pantalla (docs/propuestas/ODISEA.md): el anillo desde tu signo, las cuatro zonas de cada región,
    /// la pista de secciones con su botín, las Expediciones con Marea elegible y el tablero de encargos y logros.
    /// Los textos de reglas salen del núcleo; esta capa solo los acomoda.
    /// </summary>
    public sealed partial class MetaHub
    {
        private string _zone = "";
        private int _expTide = -1;
        private int _questTab;
        private Vector2 _questScroll;
        private float _questScrollDistance;
        private bool _questScrollInput;

        // ------------------------------------------------------------------ Arte de las zonas

        private static string LayerSuffix(SkyLayer l) => l == SkyLayer.Abismo ? "_abismo" : l == SkyLayer.Grieta ? "_grieta" : "";

        /// <summary>Tinte de la capa cuando no hay arte propio de Grieta o Abismo: el cielo se acerca.</summary>
        private static Color LayerTint(SkyLayer l) =>
            l == SkyLayer.Abismo ? new Color(0.62f, 0.5f, 0.72f) : l == SkyLayer.Grieta ? new Color(0.86f, 0.82f, 0.98f) : Color.white;

        private Texture2D ZoneArt(ZoneDef zone, SkyLayer layer, out Color tint)
        {
            tint = Color.white;
            if (zone == null || _art == null) return null;
            var own = _art.Get(zone.Backdrop + LayerSuffix(layer));
            if (own != null) return own;
            tint = LayerTint(layer);
            return _art.Get(zone.Backdrop);
        }

        /// <summary>Fondo del combate en curso: la zona de la Expedición o de la misión, en su capa.</summary>
        public Texture2D CurrentBackdrop(out Color tint)
        {
            tint = Color.white;
            if (Game == null) return null;
            var story = Game.Story;
            if (story.InExpedition && Current == Screen.Expedition)
                return ZoneArt(story.ExpeditionZone, StoryService.LayerOf(ExpeditionTideShown), out tint);
            if (story.InMission)
            {
                var region = story.CurrentRegion;
                if (region == null) return null;
                if (region.Prologue) return _art?.Get("amb_vado");
                return ZoneArt(story.CurrentZone, story.Layer(region.Id), out tint);
            }
            return null;
        }

        private int ExpeditionTideShown => Mathf.Min(_meta.World.Story.Odyssey.MaxTide, Game.Profile.Calculista.ExpTide + (Game.Story.ExpeditionZone != null && Game.Story.ExpeditionZone.IsEx ? 2 : 0));

        private static string KindName(ZoneKind k) => k == ZoneKind.Orilla ? "La orilla" : k == ZoneKind.Subsuelo ? "El subsuelo" : k == ZoneKind.Corazon ? "El corazón" : "El cielo (EX)";

        // ------------------------------------------------------------------ El anillo

        private void DrawOdysseyMap(Rect a)
        {
            var s = _meta.World.Story;
            var story = Game.Story;
            var c = Game.Profile.Calculista;
            string start = story.StartRegion;
            string desc = story.Descendant;
            string sub = $"Marea de Sal {story.CurrentTide} · {StoryService.LayerName(StoryService.LayerOf(story.CurrentTide))}  ·  Eclipses {c.Eclipses}"
                + (start.Length > 0 ? $"  ·  Tu región: {s.Region(start)?.Name}" : "")
                + (desc.Length > 0 ? $"  ·  Tu Descendente: {s.Sign(desc)?.Name}" : "");
            var body = Header(a, L("dest.story"), sub);
            var ring = story.BuiltRing;
            if (_region.Length == 0 || s.Region(_region) == null || s.Region(_region).Prologue)
                _region = start.Length > 0 && s.Region(start) != null ? start : ring[0].Id;

            // ---------- Izquierda: el anillo de Halia sobre el mapa pintado.
            var left = new Rect(body.x, body.y, 760, body.height);
            Theme.GoldPanel(left, 0.92f);
            var map = _art?.Get("mapa_anillo");
            if (map != null) PortraitBank.Fill(new Rect(left.x + 6, left.y + 6, left.width - 12, left.height - 12), map, 0.5f, new Color(1f, 1f, 1f, 0.55f));
            Vector2 center = new Vector2(left.center.x, left.y + left.height * 0.48f);
            float radius = Mathf.Min(left.width, left.height) * 0.38f;
            Theme.TreeGlyph(center, 56, Palette.WithAlpha(Palette.GoldBright, 0.8f), 4f);
            var pos = new Dictionary<string, Vector2>();
            for (int i = 0; i < s.Ring.Count; i++)
            {
                float ang = (-90 + i * 30) * Mathf.Deg2Rad;
                pos[s.Ring[i]] = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
            }
            // El arco recorrido: las regiones visitadas, unidas por el camino de la Tortuga.
            for (int i = 0; i < ring.Count && ring.Count > 1; i++)
            {
                var r0 = ring[i];
                var r1 = ring[(i + 1) % ring.Count];
                bool walked = c.RegionsVisited.Contains(r0.Id) && c.RegionsVisited.Contains(r1.Id);
                GuiKit.Line(pos[r0.Id], pos[r1.Id], walked ? 5f : 2f, walked ? Palette.GoldBright : Palette.WithAlpha(Palette.ParchmentShadow, 0.35f));
            }
            foreach (var id in s.Ring)
            {
                var p = pos[id];
                var region = s.Region(id);
                int tide = story.Tide(id);
                if (region == null)
                {
                    // Región que todavía no sale: solo se ve cómo la corrompe el cielo.
                    Color dim = tide >= 4 ? Palette.Danger : tide >= 2 ? Palette.Hex("#8E6BB0") : Palette.WithAlpha(Palette.ParchmentShadow, 0.55f);
                    GuiKit.Dot(p, 20, dim);
                    GuiKit.Label(new Rect(p.x - 40, p.y - 12, 80, 24), s.Sign(id)?.Name.Substring(0, 3), 14, Palette.Parchment, TextAnchor.MiddleCenter, true);
                    continue;
                }
                bool done = story.RegionDone(id), visited = c.RegionsVisited.Contains(id);
                bool open = !visited && story.RingBlock(id).Length == 0 && story.PrologueDone;
                Color col = done ? Palette.FiveStar : visited ? Palette.GoldBright : open ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.7f);
                float pulse = open ? 4f * Mathf.Sin(Time.unscaledTime * 3f) : 0f;
                GuiKit.Dot(p, 40 + pulse, Palette.WithAlpha(Palette.Abyss, 0.85f));
                GuiKit.Dot(p, 34 + pulse, col);
                GuiKit.Label(new Rect(p.x - 60, p.y - 15, 120, 30), s.Sign(id)?.Name, 17, Palette.Ink, TextAnchor.MiddleCenter, true);
                string tag = done ? "✓" : visited ? $"nv {story.RegionLevel(id)}" : open ? story.Direction(id) : "";
                if (id == start && !visited) tag = "Tu signo";
                if (tag.Length > 0) GuiKit.Label(new Rect(p.x - 80, p.y + 34, 160, 26), tag, 17, done ? Palette.FiveStar : Palette.SaltWhite, TextAnchor.MiddleCenter, true);
                if (id == desc) GuiKit.Ring(p, 50, 1f, Palette.Danger, Color.clear, 28, 2f);
                if (_region == id) GuiKit.Ring(p, 46, 1f, Palette.SaltWhite, Color.clear, 28, 2.6f);
                if (GUI.Button(new Rect(p.x - 40, p.y - 40, 80, 80), GUIContent.none, GUIStyle.none)) { _region = id; _zone = ""; _expTide = -1; }
            }
            GuiKit.Label(new Rect(left.x + 24, left.yMax - 84, left.width - 48, 76),
                "El lugar lo eliges tú: la Tortuga sigue el anillo desde tu signo, en sentido Directo o Retrógrado. El cielo avanza solo: cada dos regiones hay un eclipse, y lo que visitas después es más fuerte y más extraño.",
                16, Palette.Parchment, TextAnchor.UpperCenter, false, true);

            // ---------- Derecha: la región elegida.
            var rr = new Rect(left.xMax + 12, body.y, body.xMax - left.xMax - 12, body.height);
            DrawRegionPanel(rr, s.Region(_region));
        }

        private void DrawRegionPanel(Rect r, RegionDef region)
        {
            var story = Game.Story;
            var c = Game.Profile.Calculista;
            Theme.GoldPanel(r, 0.94f);
            if (region.Zones.Count == 0) { GuiKit.Label(r, region.Name, 30, Palette.Parchment, TextAnchor.MiddleCenter); return; }
            if (_zone.Length == 0 || region.Zone(_zone) == null)
            {
                var firstOpen = region.Zones.FirstOrDefault(z => !z.IsEx && z.Missions.Any(m => !story.MissionDone(m)));
                _zone = (firstOpen ?? region.Zones[0]).Id;
            }
            var zone = region.Zone(_zone);
            var layer = zone.IsEx ? StoryService.LayerOf(Mathf.Min(_meta.World.Story.Odyssey.MaxTide, story.Tide(region.Id) + 2)) : story.Layer(region.Id);

            // Banda superior: el fondo de la zona, con la región y su nivel.
            var banner = new Rect(r.x + 8, r.y + 8, r.width - 16, 300);
            var art = ZoneArt(zone, layer, out var tint);
            if (art != null) PortraitBank.Fill(banner, art, 0.5f, tint);
            else GuiKit.Fill(banner, Palette.WithAlpha(Palette.Planet(region.Planet), 0.35f));
            GuiKit.Fill(new Rect(banner.x, banner.yMax - 120, banner.width, 120), Palette.WithAlpha(Palette.Abyss, 0.72f));
            Theme.Title(new Rect(banner.x + 24, banner.yMax - 118, banner.width - 48, 54), region.Name, 40, Palette.GoldBright, TextAnchor.MiddleLeft);
            int lv = story.RegionLevel(region.Id);
            string level = c.RegionsVisited.Contains(region.Id) || story.RingBlock(region.Id).Length == 0 ? $"Nivel {lv} a {lv + 8}" : "Nivel al llegar";
            GuiKit.Label(new Rect(banner.x + 24, banner.yMax - 62, banner.width - 48, 30),
                $"{region.Theme}  ·  eco de {region.Echo}  ·  {level}  ·  Marea {story.Tide(region.Id)}, {StoryService.LayerName(story.Layer(region.Id))}", 18, Palette.Parchment);
            var native = _meta.Enemy(region.Native);
            if (native != null)
                GuiKit.Label(new Rect(banner.x + 24, banner.yMax - 32, banner.width - 48, 28), $"Nativo: {native.Name}. {native.Traits.Hint}", 16, Palette.SaltWhite, TextAnchor.MiddleLeft, false, true);

            string block = story.RingBlock(region.Id);
            float y = banner.yMax + 12;
            if (block.Length > 0 && !c.RegionsVisited.Contains(region.Id))
            {
                GuiKit.Label(new Rect(r.x + 24, y, r.width - 48, 60), block, 22, Palette.Danger, TextAnchor.MiddleLeft, true, true);
                y += 66;
            }

            // Pestañas de las cuatro zonas.
            float tw = (r.width - 48 - 3 * 10) / 4f;
            for (int i = 0; i < region.Zones.Count && i < 4; i++)
            {
                var z = region.Zones[i];
                var tab = new Rect(r.x + 24 + i * (tw + 10), y, tw, 78);
                int done = z.Missions.Count(story.MissionDone);
                string state = z.IsEx ? (c.ZonesDone.Contains(z.Id) ? "✓ vencida" : story.RegionDone(region.Id) ? "abierta" : "al terminar") : $"{done}/{z.Missions.Count}" + (c.ZonesDone.Contains(z.Id) ? " ✓" : "");
                if (GuiKit.Button(tab, $"{KindName(z.Kind)}\n<size=15>{z.Name} · {state}</size>", true, 19)) { _zone = z.Id; _expTide = -1; }
                if (z.Id == _zone) GuiKit.Frame(tab, Palette.FiveStar, 3f);
            }
            y += 92;

            GuiKit.Label(new Rect(r.x + 24, y, r.width - 48, 30), $"{zone.Description}  ·  Nivel {story.ZoneLevel(region, zone)}  ·  {(zone.IsEx ? story.ExpeditionSections(zone) + " secciones y jefe" : zone.Sections + " secciones antes de cada combate")}", 18, Palette.Parchment, TextAnchor.MiddleLeft, false, true);
            y += 40;

            if (story.InMission)
            {
                var cm = story.CurrentMission;
                if (GuiKit.Button(new Rect(r.x + 24, y, r.width - 48, 64), $"Misión en pausa: {cm?.Name}  ·  continuar", true, 22)) Go(Screen.Scene);
                y += 72;
            }

            // Misiones de la zona.
            foreach (var mid in zone.Missions)
            {
                var m = region.Missions.Find(x => x.Id == mid);
                bool done = story.MissionDone(m.Id);
                string why = story.CanStart(region.Id, m.Id);
                string state = done ? (story.GameFinished ? "Relectura" : "Terminada") : why.Length > 0 ? "Bloqueada" : "Disponible";
                var b = new Rect(r.x + 24, y, r.width - 48, 66);
                if (GuiKit.Button(b, $"{m.Name}  ·  <size=15>{state}</size>\n<size=15>{m.Description}</size>", why.Length == 0 && !story.InMission && !story.InExpedition, 21))
                {
                    var res = story.StartMission(region.Id, m.Id);
                    Note(res.Message, res.Ok);
                    if (res.Ok) { Save(); Go(Screen.Scene); }
                }
                if (done) GuiKit.Label(new Rect(b.xMax - 56, b.y, 46, b.height), "✓", 32, Palette.FiveStar, TextAnchor.MiddleCenter, true);
                y += 72;
            }

            // Expedición con Marea elegible.
            int min = story.MinTide(region.Id), max = story.MaxTideFor(region.Id);
            if (_expTide < min || _expTide > max) _expTide = max;
            var row = new Rect(r.x + 24, r.yMax - 96, r.width - 48, 80);
            Theme.Panel(row, Palette.Hex("#3A2638"), Palette.Hex("#170F1C"), Palette.Gold, 0.95f);
            GuiKit.Label(new Rect(row.x + 16, row.y + 4, 360, 34), "Expedición", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(row.x + 16, row.y + 38, 520, 34), $"Botín: almitas, {_meta.Catalog.NameOf(Core.Economy.Res.Mat(region.Material))} y sal de {Planets.DisplayName(region.Planet)}", 16, Palette.Parchment);
            for (int t = min; t <= max; t++)
            {
                var tb = new Rect(row.x + 560 + (t - min) * 74, row.y + 14, 66, 52);
                if (GuiKit.Button(tb, "M" + t, true, 20)) _expTide = t;
                if (t == _expTide) GuiKit.Frame(tb, Palette.FiveStar, 3f);
            }
            string can = story.CanExpedition(region.Id, zone.Id, _expTide);
            if (GuiKit.Button(new Rect(row.xMax - 300, row.y + 10, 284, 60), can.Length == 0 ? "Salir de Expedición" : "Expedición cerrada", can.Length == 0, 21))
            {
                var res = story.StartExpedition(region.Id, zone.Id, _expTide);
                Note(res.Message, res.Ok);
                if (res.Ok) { Save(); Go(Screen.Expedition); }
            }
            if (can.Length > 0) GuiKit.Label(new Rect(row.x + 560 + (max - min + 1) * 74 + 10, row.y + 10, row.width - 880 - (max - min + 1) * 74, 60), can, 15, Palette.ParchmentShadow, TextAnchor.MiddleLeft, false, true);
        }

        // ------------------------------------------------------------------ La pista de secciones

        /// <summary>Puntos de las secciones: ganadas, la actual y el jefe al final.</summary>
        private void SectionTrack(Rect r, int current, int count, bool bossAtEnd)
        {
            if (count <= 1) return;
            float step = Mathf.Min(90f, r.width / count);
            float x0 = r.center.x - step * (count - 1) / 2f;
            for (int i = 0; i < count; i++)
            {
                var p = new Vector2(x0 + i * step, r.center.y);
                if (i < count - 1) GuiKit.Line(p, new Vector2(p.x + step, p.y), 3f, i < current - 1 ? Palette.GoldBright : Palette.WithAlpha(Palette.ParchmentShadow, 0.5f));
            }
            for (int i = 0; i < count; i++)
            {
                var p = new Vector2(x0 + i * step, r.center.y);
                bool boss = bossAtEnd && i == count - 1;
                Color col = i < current - 1 ? Palette.FiveStar : i == current - 1 ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.7f);
                if (boss) GuiKit.Diamond(p, 20, i == current - 1 ? Palette.Danger : col);
                else GuiKit.Dot(p, i == current - 1 ? 16 : 12, col);
            }
        }

        private void LootStrip(Rect r)
        {
            var loot = Game.Profile.Calculista.LastLoot;
            if (loot.Count == 0) return;
            Theme.Panel(r, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), Palette.Gold, 0.92f);
            GuiKit.Label(new Rect(r.x + 16, r.y, r.width - 32, r.height), "Botín  " + string.Join("   ", loot), 18, Palette.FiveStar, TextAnchor.MiddleLeft, true, true);
        }

        // ------------------------------------------------------------------ Expedición

        private void DrawExpedition(Rect a)
        {
            var story = Game.Story;
            if (!story.InExpedition) { Current = Screen.Story; return; }
            var region = story.ExpeditionRegion;
            var zone = story.ExpeditionZone;
            var layer = StoryService.LayerOf(ExpeditionTideShown);
            var art = ZoneArt(zone, layer, out var tint);
            if (art != null) PortraitBank.Fill(a, art, 0.5f, tint);
            var panel = new Rect(a.x + 40, a.yMax - 330, a.width - 80, 310);
            Theme.GoldPanel(panel, 0.94f);
            Theme.Title(new Rect(panel.x + 30, panel.y + 12, panel.width - 60, 54), $"{zone.Name} · {region.Name}", 38, Palette.GoldBright, TextAnchor.MiddleLeft);
            int sections = story.ExpeditionSectionCount;
            GuiKit.Label(new Rect(panel.x + 30, panel.y + 66, panel.width - 60, 30),
                $"Expedición · Marea {Game.Profile.Calculista.ExpTide} · {StoryService.LayerName(layer)} · sección {story.ExpeditionSectionNumber} de {sections}" + (story.ExpeditionBossNext ? " · ¡el jefe!" : ""), 20, Palette.Parchment);
            SectionTrack(new Rect(panel.x + 30, panel.y + 104, panel.width - 60, 50), story.ExpeditionSectionNumber, sections, true);
            LootStrip(new Rect(panel.x + 30, panel.y + 160, panel.width - 60, 50));
            var party = Party();
            if (GuiKit.Button(new Rect(panel.x + 30, panel.yMax - 88, 420, 72), story.ExpeditionBossNext ? "Enfrentar al jefe" : "Pelear la sección", party.Count > 0, 28))
            {
                var setup = story.PrepareExpeditionBattle(party, NewSeed());
                StartBattle?.Invoke(setup, b =>
                {
                    var res = story.ResolveExpeditionBattle(b);
                    Note(res.Message, res.Ok);
                    Save();
                    if (!story.InExpedition) Current = Screen.Story;
                }, story.ExpeditionBossNext ? "Lee la placa del jefe antes de abrir la Cadena." : "");
            }
            if (GuiKit.Button(new Rect(panel.x + 470, panel.yMax - 88, 300, 72), "Combate automático", party.Count > 0, 20))
            {
                var b = new Battle(Game.Combat, story.PrepareExpeditionBattle(party, NewSeed()));
                b.Start();
                new AutoPilot().RunToEnd(b);
                var res = story.ResolveExpeditionBattle(b);
                Note(res.Ok ? "Victoria en combate automático. " + res.Message : res.Message, res.Ok);
                Save();
                if (!story.InExpedition) Current = Screen.Story;
            }
            if (GuiKit.Button(new Rect(panel.xMax - 290, panel.yMax - 88, 260, 72), "Retirarse", true, 22))
            {
                story.LeaveExpedition();
                Save();
                Current = Screen.Story;
            }
        }

        // ------------------------------------------------------------------ Encargos, logros y Altura

        private void DrawQuests(Rect a)
        {
            var q = Game.Quests;
            var (into, need) = q.AlturaProgress();
            var body = Header(a, L("dest.quests"), $"Altura {q.Altura} · Crecimiento {q.Growth:N0}");
            StatusBar(new Rect(body.x + 20, body.y, 900, 30), "Altura " + q.Altura, need > 0 ? into / (float)need : 0f, Theme.Ember, $"{into:N0}/{need:N0}");
            if (GuiKit.Button(new Rect(body.xMax - 300, body.y - 6, 280, 46), $"Cobrar todo ({q.Pending})", q.Pending > 0, 20))
            {
                int n = q.ClaimAll();
                Note(n > 0 ? $"Cobraste {n} recompensas." : "No hay nada que cobrar.", n > 0);
                Save();
            }
            string[] tabs = { "Diarios", "Semanales", "Logros" };
            for (int i = 0; i < tabs.Length; i++)
            {
                var t = new Rect(body.x + 20 + i * 230, body.y + 44, 220, 56);
                if (GuiKit.Button(t, tabs[i], true, 22)) { _questTab = i; _questScroll = Vector2.zero; _questScrollDistance = 0f; _questScrollInput = false; }
                if (_questTab == i) GuiKit.Frame(t, Palette.FiveStar, 3f);
            }
            var list = _questTab == 0 ? q.DailyBoard() : _questTab == 1 ? q.WeeklyBoard() : q.AchievementBoard().OrderBy(v => v.Claimed ? 2 : v.Claimable ? 0 : 1).ToList();
            var area = new Rect(body.x + 20, body.y + 112, body.width - 40, body.height - 112);
            Theme.GoldPanel(area, 0.9f);
            float rowH = 86;
            var view = new Rect(0, 0, area.width - 40, list.Count * (rowH + 8) + (_questTab == 0 ? rowH + 8 : 0));
            var previousScroll = _questScroll;
            var inputType = Event.current.rawType;
            if (inputType == EventType.ScrollWheel || inputType == EventType.MouseDrag || inputType == EventType.MouseDown) _questScrollInput = true;
            _questScroll = GUI.BeginScrollView(new Rect(area.x + 10, area.y + 10, area.width - 20, area.height - 20), _questScroll, view);
            if (_questScrollInput)
            {
                _questScrollDistance += Vector2.Distance(previousScroll, _questScroll);
                if (_questScrollDistance >= 28f) { PullSequence.Sfx?.Invoke("sfx_ui_deslizar"); _questScrollDistance = 0f; }
            }
            // IMGUI puede aplicar la rueda en el siguiente pase de Layout/Repaint.
            if (inputType == EventType.Repaint) _questScrollInput = false;
            float y = 0;
            foreach (var v in list)
            {
                var row = new Rect(0, y, view.width, rowH);
                Theme.Panel(row, Palette.Hex("#3A2E26"), Palette.Hex("#1B1512"), v.Claimed ? Palette.WithAlpha(Palette.Gold, 0.4f) : v.Claimable ? Palette.FiveStar : Palette.Gold, 0.95f);
                GuiKit.Label(new Rect(row.x + 18, row.y + 6, row.width - 380, 36), v.Def.Name, 22, v.Claimed ? Palette.ParchmentShadow : Palette.Parchment, TextAnchor.MiddleLeft, true);
                string reward = RewardText(v.Def);
                GuiKit.Label(new Rect(row.x + 18, row.y + 44, row.width - 380, 32), (v.Def.Description.Length > 0 ? v.Def.Description + "  ·  " : "") + reward, 16, Palette.ParchmentShadow, TextAnchor.MiddleLeft, false, true);
                GuiKit.Bar(new Rect(row.xMax - 350, row.y + 30, 170, 18), Mathf.Clamp01(v.Progress / (float)v.Def.Need), Palette.FiveStar, Palette.WithAlpha(Palette.Abyss, 0.8f));
                GuiKit.Label(new Rect(row.xMax - 350, row.y + 50, 170, 26), $"{v.Progress:N0}/{v.Def.Need:N0}", 15, Palette.Parchment, TextAnchor.MiddleCenter);
                if (GuiKit.Button(new Rect(row.xMax - 166, row.y + 14, 150, 58), v.Claimed ? "Cobrado" : "Cobrar", v.Claimable, 20)) { Apply(q.Claim(v.Def.Id)); }
                y += rowH + 8;
            }
            if (_questTab == 0)
            {
                var row = new Rect(0, y, view.width, rowH);
                Theme.Panel(row, Palette.Hex("#4A3A20"), Palette.Hex("#1B1512"), q.BonusReady ? Palette.FiveStar : Palette.Gold, 0.95f);
                GuiKit.Label(new Rect(row.x + 18, row.y, row.width - 200, rowH), "Bono por los cuatro encargos: 10 000 almitas y sal cruda", 21, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                if (GuiKit.Button(new Rect(row.xMax - 166, row.y + 14, 150, 58), "Cobrar", q.BonusReady, 20)) Apply(q.ClaimBonus());
            }
            GUI.EndScrollView();
        }

        private string RewardText(QuestDef def)
        {
            var r = _meta.World.Quests;
            var items = def.Rewards.Count > 0 ? def.Rewards : r.Daily.Contains(def) ? r.DailyReward : r.WeeklyReward;
            return string.Join(", ", items.Select(kv => $"{kv.Value:N0} {_meta.Catalog.NameOf(kv.Key)}"));
        }
    }
}
