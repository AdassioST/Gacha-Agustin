using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using SalYAlmas.Core.World;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// El juego fuera del combate: todo sale del home y vuelve a él, con la barra superior de la cuenta y la barra inferior
    /// de navegación siempre a la mano, como en un gacha moderno. Solo manda órdenes al núcleo y dibuja su estado.
    /// Las pantallas están en los otros archivos de esta clase.
    /// </summary>
    public sealed partial class MetaHub
    {
        public enum Screen { Creator, Home, Coven, Witch, Forge, Cauldron, Termas, Summon, Shop, Mirror, Story, Scene, Fresno, Ledger, Settings, Odds, Expedition, Quests }

        private const string Slot = "perfil";
        private static readonly DateTime Epoch = new DateTime(2026, 1, 1);
        public const float TopHeight = 118f;
        public const float NavHeight = 122f;

        private readonly MetaDatabase _meta;
        private readonly ISaveStore _store;
        private readonly PortraitBank _art;
        private string _note = "";
        private Color _noteColor = Palette.Parchment;
        private float _noteTime;

        public Screen Current { get; private set; } = Screen.Home;
        private readonly Stack<Screen> _history = new Stack<Screen>();

        /// <summary>Pide a la escena un combate con el HUD real; al terminar devuelve el Battle.</summary>
        public Action<BattleSetup, Action<Battle>, string> StartBattle;
        public Action OpenFormation;
        public Action ApplySettings;

        public MetaGame Game { get; private set; }

        public MetaHub(MetaDatabase meta, ISaveStore store, PortraitBank art)
        {
            _meta = meta;
            _store = store;
            _art = art;
            LoadOrCreate();
        }

        /// <summary>El reloj real solo se lee aquí, en la presentación; el núcleo recibe el día como entrada.</summary>
        private static int Today() => (DateTime.Now.Date - Epoch).Days;

        private void LoadOrCreate()
        {
            try
            {
                Game = _store.Exists(Slot) ? MetaGame.Load(_meta, _store, Slot) : null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo cargar el perfil: " + ex.Message);
                Game = null;
            }
            if (Game == null) NewPlayer();
            else
            {
                Game.SetDay(Today());
                Current = Game.Story.Created ? Screen.Home : Screen.Creator;
                if (Game.Story.InMission) Current = Screen.Scene;
                ApplyAccessibility();
            }
        }

        private static ulong NewSeed() => (ulong)(DateTime.Now.Ticks % 1000000007L);

        /// <summary>Una cuenta nueva empieza por crear a la Calculista.</summary>
        private void NewPlayer()
        {
            Game = new MetaGame(_meta, MetaGame.NewProfile(_meta, NewSeed(), Today()));
            ResetSelections();
            _history.Clear();
            Current = Screen.Creator;
            Save();
            ApplyAccessibility();
        }

        /// <summary>Perfil de prueba: seis brujas, recetas y recursos de sobra, para revisar sistemas sin jugar la historia.</summary>
        private void NewTestProfile()
        {
            Game = new MetaGame(_meta, MetaGame.TestProfile(_meta, NewSeed(), Today()));
            Game.Story.Create("Noa", "Elle", _meta.World.Story.Species[0], "capricornio");
            Game.Profile.Party.AddRange(new[] { "marisal", "selenia", "solenne", "tormentina" });
            Game.Profile.Calculista.MissionsDone.Add("prologo");
            Game.Profile.Calculista.RegionsDone.Add("vado");
            foreach (var w in Game.Profile.Witches.Values) if (Game.Meta.HasKit(w.Id) && !Game.Profile.Party.Contains(w.Id) && Game.Profile.Party.Count < 4) Game.Profile.Party.Add(w.Id);
            ResetSelections();
            _history.Clear();
            Current = Screen.Home;
            Save();
            ApplyAccessibility();
        }

        private void Save()
        {
            try { Game.Save(_store, Slot); }
            catch (Exception ex) { Debug.LogException(ex); Note("No se pudo guardar: " + ex.Message, false); }
        }

        public void Note(string text, bool ok = true)
        {
            _note = text ?? "";
            _noteColor = ok ? Palette.Parchment : Palette.Danger;
            _noteTime = Time.unscaledTime;
        }

        private void Apply(OpResult r)
        {
            Note(r.Message, r.Ok);
            if (r.Ok) Save();
        }

        private void ResetSelections()
        {
            _curses.Clear();
            _ecoGrafts.Clear();
            _ecoTrained = -1;
            _trialResult = null;
            _lastPull.Clear();
            _selectedWeapon = 0;
        }

        private void ApplyAccessibility()
        {
            var s = Game.Profile.Settings;
            Theme.LargeText = s.LargeText;
            Theme.ReduceFlashes = s.ReduceFlashes;
            Theme.ColorSafe = s.ColorSafe;
            ApplySettings?.Invoke();
        }

        private string L(string key) => _meta.World.Loc.Get(key, Game.Profile.Settings.Language);

        // ------------------------------------------------------------------ Navegación

        /// <summary>Abre una pantalla desde fuera del hub (la capa de interfaz nueva).</summary>
        public void Open(Screen screen) => Go(screen);

        public void Go(Screen screen, bool remember = true)
        {
            if (screen == Current) return;
            if (remember) _history.Push(Current);
            Current = screen;
            _note = "";
        }

        /// <summary>La barra inferior reinicia el historial, como en los gachas: cada pestaña es una raíz.</summary>
        private void GoRoot(Screen screen)
        {
            _history.Clear();
            if (screen != Screen.Home) _history.Push(Screen.Home);
            Current = screen;
            _note = "";
        }

        private void Back()
        {
            Current = _history.Count > 0 ? _history.Pop() : Screen.Home;
            _note = "";
        }

        // ------------------------------------------------------------------ Marco

        public void Draw()
        {
            GUI.DrawTexture(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Theme.Sky, ScaleMode.StretchToFill);
            DrawWheel();

            if (Current == Screen.Creator) { DrawCreator(new Rect(0, 0, GuiKit.Width, GuiKit.Height)); DrawNote(); return; }
            if (Current == Screen.Scene) { DrawScene(new Rect(0, 0, GuiKit.Width, GuiKit.Height)); DrawNote(); return; }
            if (_pull != null) { _pull.Draw(); return; }

            bool inRun = Current == Screen.Fresno && F.InRun;
            DrawTopBar();
            var area = new Rect(14, TopHeight + 8, GuiKit.Width - 28, GuiKit.Height - TopHeight - (inRun ? 18 : NavHeight + 18));
            switch (Current)
            {
                case Screen.Home: DrawHome(area); break;
                case Screen.Coven: DrawCoven(area); break;
                case Screen.Witch: DrawWitch(area); break;
                case Screen.Forge: DrawForge(area); break;
                case Screen.Cauldron: DrawCauldron(area); break;
                case Screen.Termas: DrawBase(area); break;
                case Screen.Summon: DrawTermas(area); break;
                case Screen.Shop: DrawShop(area); break;
                case Screen.Mirror: DrawMirror(area); break;
                case Screen.Story: DrawStoryMap(area); break;
                case Screen.Fresno: DrawFresno(area); break;
                case Screen.Ledger: DrawLedger(area); break;
                case Screen.Settings: DrawSettings(area); break;
                case Screen.Odds: DrawOdds(area); break;
                case Screen.Expedition: DrawExpedition(area); break;
                case Screen.Quests: DrawQuests(area); break;
            }
            if (!inRun) DrawNav();
            DrawNote();
        }

        private void DrawNote()
        {
            if (_note.Length == 0 || Time.unscaledTime - _noteTime > 6f) return;
            var size = new Rect(GuiKit.Width / 2 - 620, GuiKit.Height - NavHeight - 58, 1240, 50);
            Theme.Panel(size, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), _noteColor == Palette.Danger ? Palette.Danger : Palette.Gold, 0.96f);
            GuiKit.Label(size, _note, 20, _noteColor, TextAnchor.MiddleCenter, false, true);
        }

        /// <summary>La rueda natal de fondo. Con Paralaje alto, las constelaciones se redibujan (GDD 6.5).</summary>
        private void DrawWheel()
        {
            var c = new Vector2(GuiKit.Width / 2, GuiKit.Height * 0.55f);
            Color gold = Palette.WithAlpha(Palette.Gold, 0.13f);
            GuiKit.Ring(c, 430, 1f, gold, Color.clear, 120, 1.6f);
            GuiKit.Ring(c, 380, 1f, gold, Color.clear, 100, 1.3f);
            for (int i = 0; i < 12; i++)
            {
                float a = (i * 30f + (Game.Story.Band >= BookBand.Medio ? Time.unscaledTime * 0.6f : 0f)) * Mathf.Deg2Rad;
                GuiKit.Line(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 380, c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 430, 1.5f, gold);
            }
            Theme.TreeGlyph(c, 280, Palette.WithAlpha(Palette.Gold, 0.09f), 7f);
        }

        private void DrawTopBar()
        {
            var story = Game.Story;
            var c = Game.Profile.Calculista;
            var l = Game.Ledger;

            var left = new Rect(8, 6, 640, TopHeight - 8);
            Theme.GoldPanel(left);
            GuiKit.Label(new Rect(left.x + 20, left.y + 6, 420, 32), c.Name.Length > 0 ? c.Name : "Calculista", 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            string sendero = _meta.World.Story.Sendero(c.Sendero)?.Name ?? "";
            GuiKit.Label(new Rect(left.x + 300, left.y + 6, 325, 32), sendero, 18, Palette.ParchmentShadow, TextAnchor.MiddleRight);
            // Altura: el nivel de cuenta sale del Crecimiento por todo lo que juegas (META 4), no solo de la Ofrenda.
            var (into, need) = Game.Quests.AlturaProgress();
            StatusBar(new Rect(left.x + 20, left.y + 40, 600, 28), L("top.level") + " " + Game.Quests.Altura, need > 0 ? into / (float)need : 0f, Theme.Ember, $"{into:N0}/{need:N0}");
            float p = c.Paralaje / (float)Mathf.Max(1, _meta.World.Story.ParalajeMax);
            StatusBar(new Rect(left.x + 20, left.y + 72, 600, 28), L("top.parallax"), p, BandColor(story.Band), $"{c.Paralaje}/{_meta.World.Story.ParalajeMax}  ·  {story.Band}");

            DrawCrest(new Rect(GuiKit.Width / 2 - 300, 0, 600, TopHeight + 6));

            var right = new Rect(GuiKit.Width - 648, 6, 640, TopHeight - 8);
            Theme.GoldPanel(right);
            Wallet(new Rect(right.x + 20, right.y + 10, 296, 44), Palette.Hex("#BFD8F0"), "Perlas", l.Balance(Res.Perlas));
            Wallet(new Rect(right.x + 326, right.y + 10, 296, 44), Palette.SaltWhite, "Sales de Baño", l.Balance(Res.SalesDeBano));
            Wallet(new Rect(right.x + 20, right.y + 58, 296, 44), Palette.FiveStar, "Almitas", l.Balance(Res.Almitas));
            Wallet(new Rect(right.x + 326, right.y + 58, 296, 44), Palette.Planet(Planet.Venus), "Savia", l.Balance(Res.Savia));
        }

        private static Color BandColor(BookBand b) =>
            b == BookBand.Bajo ? Palette.Hex("#3EA48E") : b == BookBand.Medio ? Palette.Hex("#A77BEA") : b == BookBand.Alto ? Palette.Hex("#E59200") : Palette.Hex("#CC3A5C");

        private static void StatusBar(Rect r, string label, float frac, Color color, string value)
        {
            Theme.Panel(r, Palette.Hex("#0E0B14"), Palette.Hex("#1A1420"), Theme.GoldDark);
            var fill = new Rect(r.x + 110, r.y + 5, (r.width - 118) * Mathf.Clamp01(frac), r.height - 10);
            GuiKit.Fill(fill, color);
            GuiKit.Fill(new Rect(fill.x, fill.y, fill.width, 3), Palette.WithAlpha(Color.white, 0.35f));
            GuiKit.Label(new Rect(r.x + 12, r.y, 100, r.height), label, 15, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x + 110, r.y, r.width - 124, r.height), value, 14, Palette.SaltWhite, TextAnchor.MiddleRight);
        }

        private static void Wallet(Rect r, Color gem, string name, long amount)
        {
            Theme.Panel(r, Palette.Hex("#0E0B14"), Palette.Hex("#1A1420"), Theme.GoldDark);
            GuiKit.Diamond(new Vector2(r.x + 24, r.center.y), 13, gem);
            GuiKit.Label(new Rect(r.x + 44, r.y, 130, r.height), name, 14, Palette.ParchmentShadow);
            GuiKit.Label(new Rect(r.x + 110, r.y, r.width - 124, r.height), amount.ToString("N0"), 20, Palette.SaltWhite, TextAnchor.MiddleRight, true);
        }

        private void DrawCrest(Rect r)
        {
            var c = new Vector2(r.center.x, r.y + 56);
            for (int i = -1; i <= 1; i += 2)
            {
                for (int k = 0; k < 7; k++)
                {
                    float t = k / 6f;
                    var a = new Vector2(c.x + i * (70 + t * 200), c.y - 36 + t * t * 80);
                    var b = new Vector2(c.x + i * (70 + (t + 0.16f) * 200), c.y - 36 + (t + 0.16f) * (t + 0.16f) * 80);
                    if (k < 6) GuiKit.Line(a, b, 7 - k * 0.7f, Palette.Gold);
                }
            }
            GuiKit.Diamond(new Vector2(c.x, c.y - 42), 22, Palette.Hex("#D9709E"));
            GuiKit.Diamond(new Vector2(c.x, c.y - 42), 10, Palette.SaltWhite);
            var plate = new Rect(c.x - 150, c.y - 28, 300, 74);
            Theme.Panel(plate, Palette.Hex("#3A2638"), Palette.Hex("#170F1C"), Palette.GoldBright);
            Theme.Title(new Rect(plate.x, plate.y + 2, plate.width, 46), "Sal y Almas", 34, Palette.GoldBright);
            GuiKit.Label(new Rect(plate.x, plate.y + 44, plate.width, 22), "La Odisea del Fresno", 15, Palette.Parchment, TextAnchor.MiddleCenter);
        }

        // ------------------------------------------------------------------ Barra inferior

        private int BadgeFor(Screen s)
        {
            switch (s)
            {
                case Screen.Home: return (Game.Story.PrologueDone ? 0 : 1) + Game.Quests.Pending;
                case Screen.Termas:
                    int talk = 0;
                    foreach (var id in Game.Base.Resting()) if (!Game.Base.Talked(id)) talk++;
                    return talk;
                case Screen.Forge: return Game.Forge.Craftable().Count;
                case Screen.Shop:
                    return Game.Store.DiezmoActive && Game.Profile.Store.DiezmoClaimedDay != Game.Profile.Day ? 1 : 0;
                default: return 0;
            }
        }

        private void DrawNav()
        {
            var bar = new Rect(0, GuiKit.Height - NavHeight, GuiKit.Width, NavHeight);
            Theme.Panel(bar, Palette.Hex("#2C2234"), Palette.Hex("#120D18"), Palette.Gold, 0.98f);
            var items = new[]
            {
                (Screen.Home, "nav.home"), (Screen.Coven, "nav.coven"), (Screen.Forge, "nav.forge"),
                (Screen.Cauldron, "nav.cauldron"), (Screen.Termas, "nav.termas"), (Screen.Shop, "nav.shop"),
            };
            float w = 280f, x0 = (GuiKit.Width - w * items.Length) / 2f;
            for (int i = 0; i < items.Length; i++)
            {
                var tile = new Rect(x0 + i * w + 6, bar.y + 8, w - 12, NavHeight - 16);
                var (screen, key) = items[i];
                bool active = Current == screen || (screen == Screen.Coven && Current == Screen.Witch) || (screen == Screen.Termas && (Current == Screen.Summon || Current == Screen.Mirror));
                Theme.Panel(tile, active ? Palette.Hex("#7A4A40") : Palette.Hex("#4A3248"), active ? Palette.Hex("#3A2030") : Palette.Hex("#1E1426"), active ? Palette.FiveStar : Palette.Gold);
                var icon = new Vector2(tile.x + 66, tile.center.y);
                Color gc = active ? Palette.FiveStar : Palette.GoldBright;
                switch (screen)
                {
                    case Screen.Home: Theme.HomeGlyph(icon, 34, gc); break;
                    case Screen.Coven: Theme.CovenGlyph(icon, 34, gc); break;
                    case Screen.Forge: Theme.AnvilGlyph(icon, 34, gc); break;
                    case Screen.Cauldron: Theme.PotGlyph(icon, 34, gc); break;
                    case Screen.Termas: Theme.SpringGlyph(icon, 36, gc); break;
                    case Screen.Shop: Theme.ShopGlyph(icon, 34, gc); break;
                }
                Theme.Title(new Rect(tile.x + 112, tile.y, tile.width - 118, tile.height), L(key), 25, active ? Palette.FiveStar : Palette.Parchment, TextAnchor.MiddleLeft);
                int badge = BadgeFor(screen);
                if (badge > 0) Badge(new Vector2(tile.xMax - 20, tile.y + 18), badge);
                if (GUI.Button(tile, GUIContent.none, GUIStyle.none))
                {
                    if (F.InRun && screen == Screen.Fresno) Go(Screen.Fresno);
                    else GoRoot(screen);
                }
            }
        }

        private static void Badge(Vector2 c, int count)
        {
            GuiKit.Dot(c, 19, Palette.SaltWhite);
            GuiKit.Dot(c, 16.5f, Palette.Danger);
            GuiKit.Label(new Rect(c.x - 20, c.y - 16, 40, 32), count > 99 ? "99" : count.ToString(), 18, Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        /// <summary>Encabezado de una pantalla: Volver y su título.</summary>
        private Rect Header(Rect a, string title, string subtitle = "")
        {
            if (GuiKit.Button(new Rect(a.x, a.y, 160, 56), L("menu.back"), true, 24)) { Back(); }
            var plate = new Rect(a.x + 168, a.y, 760, 56);
            Theme.Panel(plate, Palette.Hex("#6A5230"), Palette.Hex("#2A1E14"), Palette.Gold);
            Theme.Title(new Rect(plate.x + 20, plate.y, plate.width - 40, plate.height), title, 28, Palette.Parchment, TextAnchor.MiddleLeft);
            if (subtitle.Length > 0) GuiKit.Label(new Rect(plate.xMax + 16, a.y, a.xMax - plate.xMax - 16, 56), subtitle, 18, Palette.ParchmentShadow);
            return new Rect(a.x, a.y + 64, a.width, a.height - 64);
        }

        // ------------------------------------------------------------------ Ayudas

        private static string Stars(int rarity) => new string('★', rarity);

        private string WitchName(string id) => _meta.Roster.TryGetValue(id, out var r) ? r.Name : id;

        private List<string> Party()
        {
            var party = Game.Profile.Party.Where(id => Game.Profile.Owns(id) && _meta.HasKit(id)).ToList();
            foreach (var id in _meta.RosterOrder)
            {
                if (party.Count >= 4) break;
                if (Game.Profile.Owns(id) && _meta.HasKit(id) && !party.Contains(id)) party.Add(id);
            }
            return party;
        }

        /// <summary>Lista paginada: dibuja una fila por elemento y los botones de página. Devuelve la página corregida.</summary>
        private static int Paged<T>(Rect area, IList<T> items, int page, float rowHeight, Action<Rect, T> row)
        {
            int perPage = Mathf.Max(1, Mathf.FloorToInt((area.height - 40) / rowHeight));
            int pages = Mathf.Max(1, Mathf.CeilToInt(items.Count / (float)perPage));
            page = Mathf.Clamp(page, 0, pages - 1);
            for (int i = 0; i < perPage && page * perPage + i < items.Count; i++)
                row(new Rect(area.x, area.y + i * rowHeight, area.width, rowHeight - 4), items[page * perPage + i]);
            if (pages > 1)
            {
                float y = area.yMax - 36;
                if (GuiKit.Button(new Rect(area.x, y, 70, 34), "◀", page > 0, 18)) page--;
                GuiKit.Label(new Rect(area.x + 76, y, 120, 34), $"{page + 1} / {pages}", 18, Palette.Parchment, TextAnchor.MiddleCenter);
                if (GuiKit.Button(new Rect(area.x + 200, y, 70, 34), "▶", page < pages - 1, 18)) page++;
            }
            return page;
        }
    }
}
