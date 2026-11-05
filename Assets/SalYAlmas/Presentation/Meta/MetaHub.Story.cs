using System.Linq;
using System.Text;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Progress;
using SalYAlmas.Core.World;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private string _region = "";

        // ------------------------------------------------------------------ Mapa de regiones

        private void DrawStoryMap(Rect a)
        {
            if (Game.Story.OdysseyEnabled) { DrawOdysseyMap(a); return; }
            var body = Header(a, L("dest.story"), $"Eclipses: {Game.Profile.Calculista.Eclipses}  ·  Almas digeridas: {Game.Profile.Calculista.Souls:N0}");
            var s = _meta.World.Story;
            var story = Game.Story;
            var regions = s.Regions.Where(r => !r.Prologue).ToList();
            if (_region.Length == 0 || s.Region(_region) == null || s.Region(_region).Prologue) _region = story.StartRegion.Length > 0 ? story.StartRegion : regions[0].Id;

            // El anillo de las doce regiones con su corrupción: el reloj de eclipses a la vista (GDD 6).
            var ring = new Rect(body.x, body.y, 620, body.height);
            Theme.GoldPanel(ring, 0.9f);
            Vector2 c = ring.center + new Vector2(0, -50);
            GuiKit.Label(new Rect(ring.x, ring.y + 8, ring.width, 36), "El anillo de Halia", 26, Palette.GoldBright, TextAnchor.MiddleCenter, true);
            Theme.TreeGlyph(c, 60, Palette.Gold, 4f);
            for (int i = 0; i < s.Ring.Count; i++)
            {
                string id = s.Ring[i];
                float ang = (-90 + i * 30) * Mathf.Deg2Rad;
                var p = c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 240;
                bool playable = s.Region(id) != null;
                int cor = story.CorruptionOf(id);
                Color col = playable ? (story.RegionDone(id) ? Palette.FiveStar : Palette.GoldBright) : cor > 1 ? Palette.Danger : cor == 1 ? Palette.Hex("#E59200") : Palette.WithAlpha(Palette.ParchmentShadow, 0.5f);
                GuiKit.Dot(p, playable ? 34 : 24, col);
                GuiKit.Label(new Rect(p.x - 50, p.y - 14, 100, 28), s.Sign(id)?.Name.Substring(0, 3), 17, Palette.Ink, TextAnchor.MiddleCenter, true);
                if (playable && _region == id) GuiKit.Ring(p, 42, 1f, Palette.SaltWhite, Color.clear, 24, 2.2f);
                if (playable && GUI.Button(new Rect(p.x - 34, p.y - 34, 68, 68), GUIContent.none, GUIStyle.none)) _region = id;
            }
            GuiKit.Label(new Rect(ring.x + 20, ring.yMax - 72, ring.width - 40, 64), "Las regiones que no visitas se corrompen un poco más en cada eclipse. Nunca te bloquean.", 17, Palette.ParchmentShadow, TextAnchor.UpperCenter, false, true);

            var region = s.Region(_region);
            var right = new Rect(ring.xMax + 10, body.y, body.width - ring.width - 10, body.height);
            Theme.GoldPanel(right, 0.9f);
            Theme.Title(new Rect(right.x + 24, right.y + 8, right.width - 48, 54), region.Name, 38, Palette.GoldBright, TextAnchor.MiddleLeft);
            GuiKit.Label(new Rect(right.x + 24, right.y + 60, right.width - 48, 30), $"{region.Theme}  ·  eco de {region.Echo}", 19, Palette.Parchment);
            float y = right.y + 100;
            foreach (var m in region.Missions)
            {
                bool done = story.MissionDone(m.Id);
                string why = story.CanStart(region.Id, m.Id);
                var r = new Rect(right.x + 24, y, right.width - 48, 84);
                string state = done ? (story.GameFinished ? "Relectura" : "Terminada") : why.Length > 0 ? "Bloqueada" : "Disponible";
                if (GuiKit.Button(r, $"{m.Name}  ·  <size=16>{state}</size>\n<size=16>{m.Description}</size>", why.Length == 0 && !story.InMission, 23))
                {
                    var res = story.StartMission(region.Id, m.Id);
                    Note(res.Message, res.Ok);
                    if (res.Ok) { Save(); Go(Screen.Scene); }
                }
                if (done) GuiKit.Label(new Rect(r.xMax - 60, r.y, 50, r.height), "✓", 36, Palette.FiveStar, TextAnchor.MiddleCenter, true);
                y += 92;
            }
            GuiKit.Label(new Rect(right.x + 24, y + 6, right.width - 48, 34), "Sendero de la Calculista: se cambia gratis fuera de combate", 19, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 42;
            for (int i = 0; i < s.Senderos.Count; i++)
            {
                var sen = s.Senderos[i];
                var b = new Rect(right.x + 24 + i * ((right.width - 48) / 4), y, (right.width - 48) / 4 - 8, 74);
                if (GuiKit.Button(b, $"{sen.Name} {story.SenderoLevel(sen.Id)}\n<size=13>{sen.Description}</size>", true, 17)) Apply(story.SetSendero(sen.Id));
                if (Game.Profile.Calculista.Sendero == sen.Id) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
        }

        // ------------------------------------------------------------------ Escena: novela visual con decisiones

        private void DrawScene(Rect a)
        {
            var story = Game.Story;
            var scene = story.CurrentScene;
            if (scene == null)
            {
                // Terminó la misión. El prólogo lleva directo al primer banner.
                _history.Clear();
                if (!_afterPrologue && story.PrologueDone && !Game.Profile.Gacha.StarterFiveStar && Game.Profile.Gacha.StarterPulls == 0)
                {
                    _afterPrologue = true;
                    _banner = "inicio";
                    _history.Push(Screen.Home);
                    Current = Screen.Summon;
                    Note("Te recibieron con Sales de Baño. Elige a tu Regente y déjalas caer.");
                }
                else Current = Screen.Story;
                Save();
                return;
            }

            var party = Party();
            // El fondo de la zona en su capa: la orilla, el subsuelo, el corazón (ODISEA 4).
            var backdrop = CurrentBackdrop(out var backTint);
            if (backdrop != null) PortraitBank.Fill(a, backdrop, 0.5f, backTint);
            GuiKit.Fill(a, Palette.WithAlpha(Palette.Abyss, backdrop != null ? 0.18f : 0.35f));
            if (story.CurrentZone != null)
                GuiKit.Label(new Rect(30, 20, 900, 40), $"{story.CurrentRegion.Name} · {story.CurrentZone.Name} · nivel {story.ZoneLevel(story.CurrentRegion, story.CurrentZone)}", 22, Palette.Parchment, TextAnchor.MiddleLeft, true);
            string portrait = scene.Portrait;
            if (portrait.Length > 0)
            {
                var body = _art.Body(portrait);
                var box = new Rect(90, 60, 640, 1000);
                if (body != null) PortraitBank.Fit(box, body);
                else if (BustFor(portrait) != null) PortraitBank.Fit(new Rect(150, 300, 520, 520), BustFor(portrait));
                else PortraitBank.Silhouette(new Rect(150, 300, 380, 520), Palette.Planet(_meta.Roster[portrait].Primary));
            }

            bool wide = scene.Choices.Count > 2;
            var text = new Rect(460, wide ? 560 : 640, 1420, wide ? 490 : 410);
            Theme.Panel(text, Palette.Hex("#EDE3CC"), Palette.Hex("#CFBE9A"), Palette.Gold);
            if (scene.Speaker.Length > 0)
            {
                var plate = new Rect(text.x + 30, text.y - 34, 400, 60);
                Theme.Panel(plate, Palette.Hex("#6A5230"), Palette.Hex("#2A1E14"), Palette.GoldBright);
                Theme.Title(plate, scene.Speaker, 28, Palette.Parchment);
            }
            bool book = scene.Speaker == "Libro Mayor";
            string body2 = story.SceneText(party);
            if (book || story.Band >= BookBand.Alto) body2 = Glitch(body2, book ? 0 : 1);
            GuiKit.Label(new Rect(text.x + 34, text.y + 34, text.width - 68, 180), body2, Theme.LargeText ? 30 : 26, Palette.Ink, TextAnchor.UpperLeft, false, true);

            float y = text.y + 218;
            if (scene.Battle != null)
            {
                bool tutorial = scene.Battle.Tutorial;
                int sections = story.SectionCount;
                if (sections > 1)
                {
                    SectionTrack(new Rect(text.x + 840, y + 4, text.width - 880, 34), story.SectionNumber, sections, scene.Battle.Boss);
                    GuiKit.Label(new Rect(text.x + 840, y + 40, text.width - 880, 30), $"Sección {story.SectionNumber} de {sections} · la vida y la Salinidad se arrastran", 16, Palette.Ink, TextAnchor.MiddleCenter);
                }
                LootStrip(new Rect(text.x + 34, text.yMax - 64, text.width - 68, 50));
                if (GuiKit.Button(new Rect(text.x + 34, y, 460, 76), sections > 1 ? $"Pelear · {story.SectionNumber}/{sections}" : "Pelear", true, 34))
                {
                    var setup = story.PrepareBattle(party, NewSeed());
                    string hint = tutorial ? "Abre la Cadena y ordena a las brujas: hay un orden donde dos golpes caen juntos." : "";
                    StartBattle?.Invoke(setup, b =>
                    {
                        var r = story.ResolveBattle(b);
                        Note(r.Message, r.Ok);
                        foreach (var e in b.Events)
                            if (e is Core.Events.ChainStarted && !Game.Profile.Store.FirstChainDone)
                            {
                                Game.Profile.Store.FirstChainDone = true;
                                Game.Analytics.Track(AnalyticsEvents.FirstChain);
                                break;
                            }
                        Save();
                    }, hint);
                }
                if (GuiKit.Button(new Rect(text.x + 510, y, 300, 76), "Combate automático", !tutorial, 20)) AutoStoryBattle(party);
            }
            else if (scene.Choices.Count > 0)
            {
                var options = story.Choices(party);
                for (int i = 0; i < options.Count; i++)
                {
                    var o = options[i];
                    string label = o.Def.Text + (o.CheckLabel.Length > 0 ? $"   <size=17>[{o.CheckLabel}]</size>" : "");
                    var b = new Rect(text.x + 34 + (i % 2) * ((text.width - 68) / 2), y + (i / 2) * 82, (text.width - 68) / 2 - 10, 76);
                    if (GuiKit.Button(b, label, o.Available, 21))
                    {
                        Apply(story.Choose(o.Index, party));
                        break;
                    }
                }
            }
            else if (GuiKit.Button(new Rect(text.xMax - 300, text.yMax - 90, 270, 70), L("menu.continue"), true, 28)) Apply(story.Continue());

            if (GuiKit.Button(new Rect(1700, 20, 200, 56), "Salir de la misión", true, 18))
            {
                story.LeaveMission();
                Save();
                Current = Screen.Story;
            }
            if (Game.Profile.Calculista.Paralaje > 0) GuiKit.Label(new Rect(1400, 22, 290, 50), $"{L("top.parallax")} {Game.Profile.Calculista.Paralaje}", 20, BandColor(story.Band), TextAnchor.MiddleRight, true);
        }

        private bool _afterPrologue;

        private void AutoStoryBattle(System.Collections.Generic.List<string> party)
        {
            var setup = Game.Story.PrepareBattle(party, NewSeed());
            var b = new Battle(Game.Combat, setup);
            b.Start();
            new AutoPilot().RunToEnd(b);
            var r = Game.Story.ResolveBattle(b);
            Note(r.Ok ? "Victoria en combate automático." : r.Message, r.Ok);
            Save();
        }

        // ------------------------------------------------------------------ Ventanas del Libro Mayor (GDD 13.1 y 6.5)

        private const string Glyphs = "ᚠᚢᚦᚨᚱᚲᛇᛈᛉᛊᛏᛒ";

        /// <summary>Con Paralaje alto, las ventanas fallan: números y letras sustituidos por glifos en Salmódico. Solo es presentación.</summary>
        private string Glitch(string text, int level)
        {
            int paralaje = Game.Profile.Calculista.Paralaje;
            if (paralaje < _meta.World.Story.ParalajeBands[1] || Game.Profile.Settings.ReduceFlashes) return text;
            int every = paralaje >= _meta.World.Story.ParalajeBands[2] ? 5 : 11;
            int tick = (int)(Time.unscaledTime * 3);
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                bool swap = char.IsLetterOrDigit(ch) && (char.IsDigit(ch) || (i + tick) % every == 0);
                sb.Append(swap ? Glyphs[(i + tick) % Glyphs.Length] : ch);
            }
            return sb.ToString();
        }

        private void DrawFolio(Rect r, string id)
        {
            var wp = Game.Profile.Witches[id];
            var e = _meta.Roster[id];
            var frame = Game.Store.Equipped("marco");
            var marco = _meta.World.Shop.Cosmetic(frame);
            Color rim = marco != null && ColorUtility.TryParseHtmlString(marco.Color, out var mc) ? mc : Palette.Gold;
            Theme.Panel(r, Palette.Hex("#EDE3CC"), Palette.Hex("#CFBE9A"), rim);
            int folio = 100 + _meta.RosterOrder.IndexOf(id) * 17;
            string atk = "—", hp = "—", spd = "—";
            string sign = "";
            if (_meta.HasKit(id))
            {
                var d = Game.Combat.Witch(id);
                var lo = Game.Progress.BuildLoadout(id);
                atk = (d.Atk * lo.AtkMultiplier).ToString("N0");
                hp = (d.MaxHp * lo.HpMultiplier).ToString("N0");
                spd = (d.Speed * lo.SpeedMultiplier).ToString("0");
                sign = d.AscendantSign;
            }
            string text = $"<b>LIBRO MAYOR · FOLIO {folio:0000}</b>\n"
                + $"Nombre:  {e.Name}\nEspecie:  {e.Species}\n"
                + $"Carta:  {Planets.DisplayName(e.Primary)} y {Planets.DisplayName(e.Secondary)}{(sign.Length > 0 ? " · Asc. " + sign : "")}\n"
                + $"Grado:  {wp.Grade}{(wp.ViaChosen ? " · Vía " + wp.Via : "")}\nNivel:  {wp.Level}\n"
                + $"ATQ {atk} · VIDA {hp} · VEL {spd}\n"
                + $"Confianza:  {Game.Base.TrustLevel(id)}";
            GuiKit.Label(new Rect(r.x + 18, r.y + 12, r.width - 36, r.height - 24), Glitch(text, 0), 17, Palette.Ink, TextAnchor.UpperLeft, false, true);
        }
    }
}
