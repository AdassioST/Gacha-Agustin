using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Interfaz de combate (GDD 9.1) en las orillas de la pantalla, para que el centro sea de las brujas y los enemigos:
    /// línea de turnos arriba a la izquierda, Resonancia arriba al centro, cartas del aquelarre abajo con su anillo de
    /// Ánima y su barra de Salinidad, y Granos y cinturón abajo a la derecha.
    ///
    /// Las órdenes se dan con gestos sobre la carta de la bruja (el "astrolabio de órdenes", GDD 19.1):
    /// tocar la carta en turno lanza el Básico; arrastrarla hacia arriba abre Hechizo, Autohechizo, Vislumbre,
    /// Cadena y las Ascensiones; soltar sobre un enemigo (o una aliada, si la orden la apoya) elige el objetivo en el
    /// mismo gesto. Mantener presionado deja el abanico abierto para tocar con calma. Las pociones se arrastran igual.
    /// </summary>
    public sealed class BattleHud
    {
        private readonly BattleDirector _d;
        private readonly ChainScreen _chain;
        private readonly Camera _camera;
        private int _enemyTarget = -1;
        private int _allyTarget = -1;
        private readonly HashSet<string> _hintsSeen = new HashSet<string>();
        private int _logScroll;

        /// <summary>Hay otra pantalla encima (Cadena o resultado): el HUD se dibuja pero no recibe toques.</summary>
        public bool InputBlocked;
        /// <summary>Busto de una bruja por ID, para las cartas y la línea de turnos.</summary>
        public static System.Func<string, Texture2D> Bust;
        public static System.Func<string, string> Localize;
        private static string L(string key) => Localize?.Invoke(key) ?? key;
        private static string L(string key, object a) => string.Format(L(key), a);
        // TUNING: señales de bando independientes de los colores de planeta.
        private static readonly Color AllyAccent = Palette.Hex("#9CCDBA");
        private static readonly Color EnemyAccent = Palette.Hex("#F09591");
        private static readonly Color Rail = Palette.Hex("#34303C");

        // ------------------------------------------------------------------ Medidas (lienzo de 1920×1080). TUNING

        private const float CardW = 268, CardH = 104, CardGap = 10, CardX = 20, CardBottom = 14, CardRaise = 14;
        private static float CardY => GuiKit.Height - CardH - CardBottom;
        private static readonly Rect TrayRect = new Rect(1140, 980, 760, 86);
        private const float FanRadius = 140f, PetalRadius = 46f, GhostScale = 0.62f, AscStep = 96f;
        private const float TapSlop = 16f, LongPress = 0.4f, FlickDistance = 70f;

        // ------------------------------------------------------------------ Gestos

        private enum Cmd { None, Basic, Spell, SelfCast, Vislumbre, Chain, Asc1, Asc2, Asc3 }
        private enum Grab { None, Card, Potion }

        private struct Petal
        {
            public Cmd Kind;
            public Vector2 Center;
            public float Radius;
            public string Title;
            public string Cost;
            public string Detail;
            public string Blocker;
            public Color Accent;
            /// <summary>En reposo las órdenes son fichas en fila sobre la carta; al sostener la carta, pétalos del astrolabio.</summary>
            public bool Chip;
        }

        private readonly List<Petal> _petals = new List<Petal>();
        private readonly Dictionary<int, Rect> _cards = new Dictionary<int, Rect>();
        private readonly List<Rect> _potionChips = new List<Rect>();
        private readonly List<(int id, Rect rect)> _plates = new List<(int, Rect)>();
        /// <summary>Barras de jefe dibujadas en este cuadro (la pista de arriba se acomoda a ellas).</summary>
        private int _bossBars;
        private Grab _grab;
        private int _grabWitch = -1;
        private int _grabPotion = -1;
        private int _grabPetal = -1;
        private Cmd _grabPetalKind;
        private Vector2 _grabStart, _grabPoint;
        private float _grabTime;
        private bool _grabMoved;
        private bool _pinned;
        private int _pinWitch = -1;
        private Cmd _armed;
        private int _aimEnemy = -1, _aimAlly = -1;
        private bool _fanOpen;
        private bool _gestureUsed;
        /// <summary>Solo para el recorrido de capturas: sostiene un arrastre simulado sin botón presionado.</summary>
        private bool _debugHold;

        public BattleHud(BattleDirector director, ChainScreen chain, Camera camera)
        {
            _d = director;
            _chain = chain;
            _camera = camera;
        }

        private Battle B => _d.Battle;

        public int EnemyTarget
        {
            get
            {
                var e = B.FindEnemy(_enemyTarget);
                if (e == null || !e.IsAlive) _enemyTarget = AutoPilot.PreferredTarget(B, _d.ActiveWitch)?.Id ?? -1;
                return _enemyTarget;
            }
            set => _enemyTarget = value;
        }

        public int AllyTarget
        {
            get
            {
                var w = B.FindWitch(_allyTarget);
                if (w == null || !w.IsAlive) return _d.ActiveWitch?.Id ?? (B.AliveWitches().FirstOrDefault()?.Id ?? -1);
                return _allyTarget;
            }
        }

        /// <summary>El registro empieza plegado: abierto tapa a las brujas de la fila de atrás.</summary>
        public bool LogOpen;

        // ------------------------------------------------------------------ Dibujo

        public void Draw()
        {
            bool oldEnabled = GUI.enabled;
            if (InputBlocked) GUI.enabled = false;
            LayoutCards();
            BuildPetals();
            if (InputBlocked || _d.Auto || B.IsOver) CancelGrab();
            else HandleGestures();
            _d.HoldDecision = _grab != Grab.None || _pinned;
            DrawAll();
            GUI.enabled = oldEnabled;
        }

        private void DrawAll()
        {
            DrawStageMarks();
            DrawEnemyPlates();
            DrawFloatingTexts();
            DrawTurnLine(new Vector2(20, 14));
            DrawResonance(new Vector2(960, 54));
            DrawTopRight();
            DrawLog(new Rect(20, 112, 520, 540));
            DrawCards();
            DrawTray(TrayRect);
            DrawFan();
            DrawPotionAim();
            DrawChainPlayback();
            DrawBanner();
            DrawToast();
            if (!_chain.IsOpen) DrawHint();
        }

        // ------------------------------------------------------------------ Piezas comunes

        private static void InkPanel(Rect r, Color accent, float alpha = 0.86f)
        {
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, alpha));
            var leather = Theme.Kit("chrome/tile_leather");
            if (leather != null)
            {
                var old = GUI.color;
                GUI.color = new Color(1, 1, 1, 0.08f * alpha);
                GUI.DrawTextureWithTexCoords(r, leather, new Rect(0, 0, r.width / 256f, r.height / 256f));
                GUI.color = old;
            }
            GuiKit.Frame(r, Palette.WithAlpha(Palette.Hex("#514B50"), alpha), 1);
            GuiKit.Fill(new Rect(r.x, r.y, r.width, 2), accent);
        }

        private static void SideSeal(Vector2 c, bool enemy, string number, float radius = 16)
        {
            if (enemy)
            {
                GuiKit.Diamond(c, radius + 3, EnemyAccent);
                GuiKit.Diamond(c, radius, Palette.Crimson);
            }
            else
            {
                GuiKit.Dot(c, radius + 3, AllyAccent);
                GuiKit.Dot(c, radius, Palette.Abyss);
            }
            if (number.Length > 0)
                GuiKit.Label(new Rect(c.x - radius, c.y - radius, radius * 2, radius * 2), number, Mathf.RoundToInt(radius * 0.95f), Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        private static void TargetBrackets(Rect r, Color color, float arm = 20, float thick = 4)
        {
            foreach (float x in new[] { r.x, r.xMax })
                foreach (float y in new[] { r.y, r.yMax })
                {
                    float dx = x == r.x ? arm : -arm;
                    float dy = y == r.y ? arm : -arm;
                    GuiKit.Fill(new Rect(Mathf.Min(x, x + dx), y - thick / 2, arm, thick), color);
                    GuiKit.Fill(new Rect(x - thick / 2, Mathf.Min(y, y + dy), thick, arm), color);
                }
        }

        private float Pulse(float speed = 6f) => Theme.ReduceMotion ? 0.5f : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed);

        private static string Short(string s, int max = 96) => string.IsNullOrEmpty(s) ? "" : (s.Length > max ? s.Substring(0, max - 2) + "…" : s);

        private int IndexOf(Unit u) => u.Side == Side.Enemy ? B.Enemies.ToList().FindIndex(e => e.Id == u.Id) : B.Witches.ToList().FindIndex(w => w.Id == u.Id);

        // ------------------------------------------------------------------ Escenario: sellos, objetivo y foco

        private void DrawStageMarks()
        {
            int aimedEnemy = _aimEnemy >= 0 ? _aimEnemy : -1;
            foreach (var u in B.Witches.Cast<Unit>().Concat(B.Enemies))
            {
                if (!_d.Views.TryGetValue(u.Id, out var v) || v == null) continue;
                bool enemy = u.Side == Side.Enemy;
                bool focus = !B.IsOver && (enemy ? u.Id == EnemyTarget || u.Id == aimedEnemy : u.Id == _aimAlly);
                v.SetFocus(focus);
                if (!u.IsAlive) continue;
                var screen = _camera.WorldToScreenPoint(v.transform.position);
                if (screen.z <= 0) continue;
                var feet = GuiKit.ScreenToCanvas(screen);
                SideSeal(new Vector2(feet.x, feet.y - 6), enemy, (IndexOf(u) + 1).ToString(), 13);
                bool aimed = enemy ? u.Id == aimedEnemy : u.Id == _aimAlly;
                if (enemy && EnemyTarget == u.Id || aimed)
                {
                    var head = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(v.Head));
                    // Hasta 900: un coloso llega casi a la barra de jefe y el corchete lo abraza entero. // TUNING
                    float height = Mathf.Clamp(feet.y - head.y, 80, 900);
                    float half = HalfWidth(v, height);
                    var color = aimed ? (enemy ? Palette.Spark : AllyAccent) : Palette.WithAlpha(Palette.Spark, 0.85f);
                    float arm = (aimed ? 26 : 18) * (height > 420 ? 1.5f : 1f);
                    TargetBrackets(new Rect(feet.x - half, feet.y - height, half * 2, height - 30), color, arm, aimed ? 5 : 3);
                }
            }
        }

        // ------------------------------------------------------------------ Arriba: turnos, Resonancia y controles

        private void DrawTurnLine(Vector2 at)
        {
            var order = B.IsOver ? new List<TurnOrderEntry>() : B.TurnOrderPreview(8);
            if (order.Count == 0) return;
            // Una cinta de tinta muy tenue: separa los retratos del fondo sin ser un panel.
            GuiKit.Fill(new Rect(at.x - 8, at.y - 6, 70 + (order.Count - 1) * 58 + 16, 100), Palette.WithAlpha(Palette.Abyss, 0.42f));
            float x = at.x;
            for (int i = 0; i < order.Count; i++)
            {
                var u = order[i].Unit;
                bool enemy = u.Side == Side.Enemy;
                bool first = i == 0;
                float size = first ? 64 : 50;
                var chip = new Rect(x, at.y + (first ? 0 : 7), size, size);
                GuiKit.Fill(chip, enemy ? Palette.Hex("#352027") : Palette.Hex("#15262A"));
                Texture2D face = null;
                if (u is WitchUnit w) face = Bust?.Invoke(w.Key);
                else if (_d.Views.TryGetValue(u.Id, out var v) && v != null && v.Body != null && v.Body.sharedMaterial != null)
                    face = v.Body.sharedMaterial.mainTexture as Texture2D;
                if (face != null) PortraitBank.Fill(new Rect(chip.x + 2, chip.y + 2, chip.width - 4, chip.height - 4), face, 1f);
                GuiKit.Frame(chip, first ? Palette.GoldBright : (enemy ? EnemyAccent : AllyAccent), first ? 3 : 2);
                SideSeal(new Vector2(chip.x + 9, chip.yMax - 9), enemy, "", first ? 7 : 6);
                string av = first && u == B.ActiveUnit ? L("battle.now") : "+" + order[i].AvFromNow.ToString("0");
                GuiKit.Label(new Rect(chip.x - 6, at.y + 68, size + 12, 22), av, first ? 14 : 14, first ? Palette.Spark : (enemy ? EnemyAccent : AllyAccent), TextAnchor.MiddleCenter, first);
                x += size + 8;
            }
            // El nombre de quien actúa, junto a su retrato: lo único que hace falta leer de la línea.
            var active = order[0].Unit;
            GuiKit.Label(new Rect(at.x + 2, at.y + 86, 420, 24), L("battle.turnOf", active.Name) + (B.IsExtraTurn ? " · Hilos" : ""), 17, active.Side == Side.Enemy ? EnemyAccent : Palette.Parchment, TextAnchor.MiddleLeft, true);
        }

        private Rect ResonanceRect(Vector2 c) => new Rect(c.x - 46, c.y - 46, 92, 92);

        private void DrawResonance(Vector2 c)
        {
            float frac = _d.Resonance / (float)B.Rules.Resonance.Max;
            bool canChain = _d.AwaitingPlayer && !_chain.IsOpen && B.ChainBlocker().Length == 0;
            bool full = _d.Resonance >= B.Rules.Resonance.ChainCost;
            Color on = full ? Color.Lerp(Palette.GoldBright, Palette.Spark, Pulse()) : Palette.Gold;
            GuiKit.Dot(c, 36, Palette.WithAlpha(Palette.Abyss, 0.8f));
            GuiKit.Ring(c, 40, frac, on, Palette.WithAlpha(Palette.ParchmentShadow, 0.22f), 36, 3.6f);
            GuiKit.Label(new Rect(c.x - 40, c.y - 20, 80, 28), _d.Resonance.ToString(), 24, full ? Palette.Spark : Palette.Parchment, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(c.x - 70, c.y + 4, 140, 20), canChain ? L("battle.cmd.chain") : L("battle.resonance"), 13, canChain ? Palette.Spark : Palette.ParchmentShadow, TextAnchor.MiddleCenter, canChain);
            // Con Resonancia llena, el propio medidor abre la Cadena: un toque.
            if (canChain)
            {
                GuiKit.Ring(c, 48, 1f, Palette.WithAlpha(Palette.Spark, 0.35f + 0.4f * Pulse()), Palette.Abyss, 36, 2f);
                if (GUI.Button(ResonanceRect(c), GUIContent.none, GUIStyle.none)) { GuiKit.Pressed(); _chain.Show(EnemyTarget); }
            }
        }

        private void DrawTopRight()
        {
            const float size = 54, gap = 18;
            float x = GuiKit.Width - 20 - 3 * size - 2 * gap;
            float y = 16;
            float t = Time.realtimeSinceStartup - _d.RealStartTime;
            if (_d.RealEndTime > 0) t = _d.RealEndTime - _d.RealStartTime;
            GuiKit.Label(new Rect(x - 250, y, 240, 26), L("battle.cycle", B.Cycle) + " · AV " + B.ElapsedAv.ToString("0"), 15, Palette.Parchment, TextAnchor.MiddleRight, true);
            GuiKit.Label(new Rect(x - 250, y + 24, 240, 24), BattleDirector.FormatTime(t), 14, Palette.ParchmentShadow, TextAnchor.MiddleRight);
            var logRect = new Rect(x, y, size, size);
            if (GuiKit.Button(logRect, "", true, 18)) LogOpen = !LogOpen;
            var book = Theme.Kit("iconsg/book");
            if (book != null) GUI.DrawTexture(new Rect(logRect.x + 11, logRect.y + 11, size - 22, size - 22), book, ScaleMode.ScaleToFit);
            if (LogOpen) GuiKit.Frame(logRect, Palette.GoldBright, 2);
            if (GuiKit.Button(new Rect(x + size + gap, y, size, size), _d.Speed > 1 ? "×2" : "×1", true, 18)) _d.Speed = _d.Speed > 1 ? 1f : 2f;
            var autoRect = new Rect(x + 2 * (size + gap), y, size, size);
            if (GuiKit.Button(autoRect, "Auto", true, 14))
            {
                _d.Auto = !_d.Auto;
                if (_d.Auto) { _chain.Close(); _pinned = false; }
            }
            if (_d.Auto) GuiKit.Frame(autoRect, Color.Lerp(Palette.GoldBright, Palette.Spark, Pulse(4f)), 2);
            // Rótulos bajo los iconos: nadie tiene que adivinar qué hace cada botón.
            var cap = new[] { L("battle.log"), L("battle.speed"), L("battle.auto") };
            for (int i = 0; i < 3; i++)
                GuiKit.Label(new Rect(x + i * (size + gap) - 8, y + size, size + 16, 20), cap[i], 12, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
        }

        private void DrawLog(Rect r)
        {
            if (!LogOpen) return;
            GuiKit.PanelBox(r, 0.94f);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 300, 26), "Libro Mayor · registro", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            bool hits = GUI.Toggle(new Rect(r.xMax - 150, r.y + 6, 140, 22), _d.ShowHits, " ver golpes");
            if (hits != _d.ShowHits) _d.ShowHits = hits;

            const float lineH = 22f;
            int visible = Mathf.FloorToInt((r.height - 44) / lineH);
            if (!InputBlocked && Event.current.type == EventType.ScrollWheel && r.Contains(Event.current.mousePosition))
            {
                _logScroll = Mathf.Clamp(_logScroll + (Event.current.delta.y > 0 ? -3 : 3), 0, Mathf.Max(0, _d.Log.Count - visible));
                Event.current.Use();
            }
            int start = Mathf.Max(0, _d.Log.Count - visible - _logScroll);
            float y = r.y + 36;
            var style = GuiKit.Style(16, Palette.Parchment, TextAnchor.UpperLeft);
            for (int i = start; i < _d.Log.Count && y < r.yMax - lineH; i++)
            {
                var line = _d.Log[i];
                var old = GUI.color;
                GUI.color = line.Color;
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, lineH), line.Text, style);
                GUI.color = old;
                y += lineH;
            }
        }

        // ------------------------------------------------------------------ Placas enemigas

        private void DrawEnemyPlates()
        {
            if (_camera == null) return;
            _plates.Clear();
            var alive = B.Enemies.Where(e => e.IsAlive && _d.Views.ContainsKey(e.Id)).OrderBy(e => e.Id).ToList();
            // Jefes y gigantes van a la barra de arriba (los jefes primero); el resto, en placas sobre la cabeza.
            var bosses = alive.Where(HasBossBar).OrderBy(e => e.IsBoss ? 0 : 1).ThenBy(e => e.Id).Take(BattleHudLayout.MaxBossBars).ToList();
            var bars = BattleHudLayout.BossBars(bosses.Count);
            _bossBars = bars.Count;
            for (int i = 0; i < bosses.Count; i++) DrawBossBar(bosses[i], bars[i]);

            var order = alive.Where(e => !bosses.Contains(e)).ToList();
            var heads = order.Select(e => GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(_d.Views[e.Id].Head + Vector3.up * 0.2f))).ToList();
            var resolved = BattleHudLayout.ResolveEnemyPlates(heads, BattleHudLayout.PlatesTop(bosses.Count > 0));
            for (int i = 0; i < order.Count && i < resolved.Count; i++)
            {
                var e = order[i];
                var view = _d.Views[e.Id];
                if (_camera.WorldToScreenPoint(view.Head).z < 0) continue;
                var r = resolved[i];
                _plates.Add((e.Id, r));
                bool targeted = EnemyTarget == e.Id;
                bool aimed = _aimEnemy == e.Id;
                InkPanel(r, targeted || aimed ? Palette.Spark : EnemyAccent, 0.78f);
                if (_grab == Grab.None && GUI.Button(r, GUIContent.none, GUIStyle.none)) _enemyTarget = e.Id;
                int number = IndexOf(e) + 1;
                SideSeal(new Vector2(r.x + 14, r.y + 15), true, number.ToString(), 9);
                // Si el nombre no cabe: primero el nombre corto del dato y, si tampoco, letra más chica antes que cortarlo. // TUNING
                float nameRoom = r.width - 98; // hasta donde empieza la cifra de vida, alineada a la derecha
                string plateName = e.Name;
                if (NameWidth(plateName, 17) > nameRoom && !string.IsNullOrEmpty(e.Def.ShortName)) plateName = e.Def.ShortName;
                int nameSize = NameWidth(plateName, 17) > nameRoom ? 14 : 17;
                GuiKit.Label(new Rect(r.x + 30, r.y + 2, nameRoom, 26), plateName, nameSize, Palette.Parchment, TextAnchor.MiddleLeft, true);
                int hp = _d.Hp.TryGetValue(e.Id, out var h) ? h : e.Hp;
                double tough = _d.Toughness.TryGetValue(e.Id, out var t) ? t : e.Toughness;
                GuiKit.Label(new Rect(r.xMax - 92, r.y + 2, 84, 26), hp.ToString("N0"), 15, Palette.SaltWhite, TextAnchor.MiddleRight, true);
                GuiKit.Bar(new Rect(r.x + 8, r.y + 29, r.width - 16, 8), hp / (float)e.MaxHp, EnemyAccent, Rail);
                // Firmeza en blanco y debilidades como sellos de planeta: lo que decide qué bruja golpea.
                float weakW = e.Def.Weaknesses.Count * 20f;
                GuiKit.Bar(new Rect(r.x + 8, r.y + 42, r.width - 24 - weakW, 4), (float)(tough / e.MaxToughness), e.Broken ? Palette.Danger : Palette.SaltWhite, Rail);
                WeakSeals(e, r.xMax - 8 - weakW + 10, r.y + 44);
                GuiKit.Label(new Rect(r.x + 8, r.y + 52, r.width - 16, 22), Intent(e), 14, e.IsCharging ? Palette.Danger : EnemyAccent, TextAnchor.MiddleLeft, e.IsCharging);
                var head = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(view.Head));
                GuiKit.Line(new Vector2(r.center.x, r.yMax), new Vector2(head.x, Mathf.Max(r.yMax + 4, head.y)), 1, Palette.WithAlpha(EnemyAccent, 0.4f));
                if (targeted) TargetBrackets(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), Palette.Spark, 14, 3);
            }
        }

        private static float NameWidth(string text, int size) => GuiKit.Style(size, Palette.Parchment, TextAnchor.MiddleLeft, true).CalcSize(new GUIContent(text)).x;

        /// <summary>Un jefe o un gigante: sobre su cabeza no cabe una placa, así que su vida va arriba, a lo ancho.</summary>
        private static bool HasBossBar(EnemyUnit e) => e.IsBoss || BattleStage.IsBig(e.Def.Size);

        private string Intent(EnemyUnit e)
        {
            if (e.Broken) return L("battle.broken");
            if (e.IsCharging) return L("battle.charging") + " " + (e.ChargedSkill?.Name ?? "");
            return e.NextSkill != null ? L("battle.next") + ": " + e.NextSkill.Name : "";
        }

        private static void WeakSeals(EnemyUnit e, float x, float y)
        {
            foreach (var weak in e.Def.Weaknesses)
            {
                var seal = Theme.Kit("seals/" + weak.ToString().ToLowerInvariant());
                var c = new Vector2(x, y);
                GuiKit.Dot(c, 9, Palette.Planet(weak));
                if (seal != null) GUI.DrawTexture(new Rect(c.x - 8, c.y - 8, 16, 16), seal);
                x += 20;
            }
        }

        private void DrawBossBar(EnemyUnit e, Rect r)
        {
            _plates.Add((e.Id, r));
            bool targeted = EnemyTarget == e.Id;
            bool aimed = _aimEnemy == e.Id;
            InkPanel(r, targeted || aimed ? Palette.Spark : EnemyAccent, 0.8f);
            if (_grab == Grab.None && GUI.Button(r, GUIContent.none, GUIStyle.none)) _enemyTarget = e.Id;
            SideSeal(new Vector2(r.x + 16, r.y + 15), true, (IndexOf(e) + 1).ToString(), 10);
            int hp = _d.Hp.TryGetValue(e.Id, out var h) ? h : e.Hp;
            double tough = _d.Toughness.TryGetValue(e.Id, out var t) ? t : e.Toughness;
            // Nombre a la izquierda, intención a la derecha: en una barra ancha se leen de un vistazo.
            // Con dos barras el espacio se acorta: el nombre toma su ancho y la intención, si no cabe, pierde el "Siguiente:".
            float nameW = Mathf.Min(GuiKit.Style(18, Palette.Parchment, TextAnchor.MiddleLeft, true).CalcSize(new GUIContent(e.Name)).x + 12, (r.width - 40) * 0.6f);
            GuiKit.Label(new Rect(r.x + 32, r.y + 3, nameW, 24), e.Name, 18, Palette.Parchment, TextAnchor.MiddleLeft, true);
            float intentW = r.width - 40 - nameW;
            string intent = Intent(e);
            if (!e.Broken && !e.IsCharging && e.NextSkill != null && GuiKit.Style(14, EnemyAccent, TextAnchor.MiddleRight).CalcSize(new GUIContent(intent)).x > intentW)
                intent = e.NextSkill.Name;
            GuiKit.Label(new Rect(r.x + 32 + nameW, r.y + 3, intentW, 24), intent, 14, e.IsCharging ? Palette.Danger : EnemyAccent, TextAnchor.MiddleRight, e.IsCharging);
            GuiKit.Bar(new Rect(r.x + 10, r.y + 30, r.width - 104, 10), hp / (float)e.MaxHp, EnemyAccent, Rail);
            GuiKit.Label(new Rect(r.xMax - 92, r.y + 24, 84, 22), hp.ToString("N0"), 15, Palette.SaltWhite, TextAnchor.MiddleRight, true);
            float weakW = e.Def.Weaknesses.Count * 20f;
            GuiKit.Bar(new Rect(r.x + 10, r.y + 46, r.width - 104 - weakW, 4), (float)(tough / e.MaxToughness), e.Broken ? Palette.Danger : Palette.SaltWhite, Rail);
            WeakSeals(e, r.xMax - 94 - weakW + 10, r.y + 48);
            if (targeted) TargetBrackets(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), Palette.Spark, 16, 3);
        }

        /// <summary>
        /// Media anchura en el lienzo de la silueta de una figura. Los recortes usan su ancho real (un cangrejo es más ancho
        /// que alto); el arte deja aire transparente a los lados, por eso el 0.8. Las figuras sin arte siguen la altura. // TUNING
        /// </summary>
        private static float HalfWidth(UnitView v, float canvasHeight)
        {
            if (!v.IsArt || v.Height <= 0f) return canvasHeight * 0.27f;
            return Mathf.Max(canvasHeight * 0.18f, v.Width * 0.5f * 0.8f * canvasHeight / v.Height);
        }

        private void DrawFloatingTexts()
        {
            if (_camera == null) return;
            foreach (var view in _d.Views.Values)
            {
                foreach (var f in view.Floats)
                {
                    var screen = _camera.WorldToScreenPoint(f.World);
                    if (screen.z < 0) continue;
                    var p = GuiKit.ScreenToCanvas(screen);
                    float alpha = Mathf.Clamp01(1.4f - f.Age);
                    var rect = new Rect(p.x - 150, p.y - 20, 300, 40);
                    GuiKit.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), f.Text, f.Size, Palette.WithAlpha(Color.black, alpha * 0.8f), TextAnchor.MiddleCenter, true);
                    GuiKit.Label(rect, f.Text, f.Size, Palette.WithAlpha(f.Color, alpha), TextAnchor.MiddleCenter, true);
                }
            }
        }

        // ------------------------------------------------------------------ Cartas del aquelarre

        private void LayoutCards()
        {
            _cards.Clear();
            for (int i = 0; i < B.Witches.Count; i++)
            {
                var w = B.Witches[i];
                bool active = B.ActiveUnit == w;
                float raise = active ? CardRaise : 0;
                _cards[w.Id] = new Rect(CardX + i * (CardW + CardGap), CardY - raise, CardW, CardH + raise);
            }
        }

        private void DrawCards()
        {
            foreach (var w in B.Witches)
                if (_cards.TryGetValue(w.Id, out var r)) DrawCard(w, r);
        }

        private void DrawCard(WitchUnit w, Rect r)
        {
            bool active = B.ActiveUnit == w;
            bool selected = AllyTarget == w.Id && !active && _allyTarget == w.Id;
            bool held = _grab == Grab.Card && _grabWitch == w.Id;
            bool aimed = _aimAlly == w.Id;
            InkPanel(r, active ? Palette.GoldBright : AllyAccent, held ? 0.95f : 0.84f);
            int hp = _d.Hp.TryGetValue(w.Id, out var h) ? h : w.Hp;
            int sal = _d.Salinity.TryGetValue(w.Id, out var s) ? s : w.Salinity;
            int anima = _d.Anima.TryGetValue(w.Id, out var a) ? a : w.Anima;
            var band = SalinityBands.FromValue(sal, B.Rules.Salinity);

            var bustRect = new Rect(r.x + 8, r.yMax - CardH + 10, 84, 84);
            var bust = Bust?.Invoke(w.Key);
            if (bust != null) PortraitBank.Fill(bustRect, bust);
            GuiKit.Frame(bustRect, active ? Palette.GoldBright : AllyAccent, active ? 2 : 1);
            SideSeal(new Vector2(bustRect.x + 10, bustRect.yMax - 10), false, (B.Witches.ToList().IndexOf(w) + 1).ToString(), 10);
            var planetSeal = Theme.Kit("seals/" + w.Dominant.ToString().ToLowerInvariant());
            GuiKit.Dot(new Vector2(bustRect.xMax - 10, bustRect.y + 10), 12, Palette.WithAlpha(Palette.Abyss, 0.9f));
            if (planetSeal != null) GUI.DrawTexture(new Rect(bustRect.xMax - 20, bustRect.y, 20, 20), planetSeal);

            float tx = r.x + 102, top = r.yMax - CardH;
            float bw = r.xMax - 64 - tx;
            GuiKit.Label(new Rect(tx, top + 4, bw + 10, 28), w.Name, 19, Palette.Parchment, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(tx, top + 30, bw, 24), hp.ToString("N0"), 17, Palette.SaltWhite, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(tx, top + 30, bw, 24), L("battle.grade") + " " + w.EffectiveGrade, 13, Palette.ParchmentShadow, TextAnchor.MiddleRight);
            GuiKit.Bar(new Rect(tx, top + 56, bw, 9), hp / (float)w.MaxHp, AllyAccent, Rail);
            if (w.ShieldAmount > 0) GuiKit.Fill(new Rect(tx, top + 65, bw * Mathf.Clamp01((float)(w.ShieldAmount / w.MaxHp)), 3), Palette.SaltWhite);
            var salRect = new Rect(tx, top + 74, bw, 5);
            GuiKit.Bar(salRect, sal / (float)B.Rules.Salinity.Max, Palette.Band(band), Rail);
            foreach (int mark in new[] { B.Rules.Salinity.SalobreAt, B.Rules.Salinity.SobresaladaAt, B.Rules.Salinity.CristalizacionAt })
                GuiKit.Fill(new Rect(salRect.x + salRect.width * mark / B.Rules.Salinity.Max, salRect.y, 2, salRect.height), Palette.ParchmentShadow);
            GuiKit.Label(new Rect(tx, top + 80, bw + 40, 20), L("battle.salt") + " " + sal + " · " + SalinityBands.DisplayName(band), 13, Palette.Band(band) == Palette.Band(SalinityBand.Estable) ? Palette.ParchmentShadow : Palette.Band(band));

            // Ánima: tres anillos (100/200/300, GDD 9.3). Lleno, brilla y la carta invita a arrastrar.
            var c = new Vector2(r.xMax - 32, top + 44);
            bool ascReady = AscensionsReady(w) > 0;
            if (ascReady) GuiKit.Dot(c, 31, Palette.WithAlpha(Palette.FiveStar, 0.18f + 0.22f * Pulse(4f)));
            for (int tier = 0; tier < 3; tier++)
                GuiKit.Ring(c, 20 + tier * 4.5f, (anima - tier * 100) / 100f, ascReady ? Palette.FiveStar : Palette.GoldBright, Rail, 32, 1.4f);
            GuiKit.Label(new Rect(c.x - 30, c.y - 14, 60, 28), anima.ToString(), 15, Palette.Parchment, TextAnchor.MiddleCenter, true);
            if (ascReady) GuiKit.Label(new Rect(c.x - 34, c.y + 18, 68, 18), "▲ " + L("battle.asc"), 12, Palette.FiveStar, TextAnchor.MiddleCenter, true);

            var tags = w.Statuses.Where(st => !st.Name.StartsWith("Regencia")).Select(st => st.TurnsLeft > 0 ? $"{st.Name} ({st.TurnsLeft})" : st.Name).Distinct().ToArray();
            if (tags.Length > 0)
            {
                var tag = new Rect(r.x, r.y - 24, r.width, 22);
                GuiKit.Fill(tag, Palette.WithAlpha(Palette.Abyss, 0.78f));
                GuiKit.Label(new Rect(tag.x + 6, tag.y, tag.width - 12, tag.height), string.Join(" · ", tags), 13, Palette.Parchment);
            }

            if (active)
            {
                GuiKit.Frame(r, Color.Lerp(Palette.Gold, Palette.GoldBright, _d.AwaitingPlayer ? Pulse(3f) : 1f), 3);
                if (_d.AwaitingPlayer && _grab == Grab.None && !_pinned)
                    GuiKit.Label(new Rect(r.x, r.y + 2, r.width - 8, 18), L("battle.tapHint"), 12, Palette.Spark, TextAnchor.MiddleRight, true);
            }
            else if (aimed) GuiKit.Frame(r, AllyAccent, 4);
            else if (selected) { GuiKit.Frame(r, AllyAccent, 2); GuiKit.Label(new Rect(r.x, r.y + 2, r.width - 8, 18), L("battle.allyTarget"), 12, AllyAccent, TextAnchor.MiddleRight, true); }
            if (!w.IsAlive) { GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.85f)); GuiKit.Label(r, L("battle.fallen"), 26, Palette.ParchmentShadow, TextAnchor.MiddleCenter, true); }
        }

        private int AscensionsReady(WitchUnit w)
        {
            if (!AscWindow) return 0;
            int n = 0;
            for (int lvl = 1; lvl <= 3; lvl++) if (B.AscensionBlocker(w, lvl).Length == 0) n++;
            return n;
        }

        private bool AscWindow => !_d.IsPlaying && !_d.Auto && !_chain.IsOpen && !B.IsOver;

        // ------------------------------------------------------------------ Granos y cinturón

        private void DrawTray(Rect r)
        {
            InkPanel(r, Palette.Gold, 0.8f);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 200, 22), L("battle.grains") + $" {_d.Grains}/{B.Rules.Grains.Max}" + (B.GrainBoostActive ? " ×2" : ""), 14, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < B.Rules.Grains.Max; i++)
            {
                var c = new Vector2(r.x + 26 + i * 38, r.y + 50);
                bool on = i < _d.Grains;
                GuiKit.Dot(c, 15, Palette.WithAlpha(Palette.Abyss, 0.9f));
                GuiKit.Dot(c, 12, on ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.18f));
            }
            var black = new Vector2(r.x + 230, r.y + 50);
            GuiKit.Diamond(black, 14, Palette.Band(SalinityBand.ExtasisAbisal));
            GuiKit.Label(new Rect(black.x - 14, black.y - 14, 28, 28), B.BlackSalt.ToString(), 15, Palette.SaltWhite, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(black.x - 40, r.y + 4, 80, 22), L("battle.blackSalt"), 12, Palette.ParchmentShadow, TextAnchor.MiddleCenter);

            _potionChips.Clear();
            bool canUse = _d.AwaitingPlayer && !B.PotionUsedThisTurn && !_chain.IsOpen;
            float cw = 150, gap = 8, x0 = r.x + 280;
            for (int i = 0; i < B.BeltSlots; i++)
            {
                var chip = new Rect(x0 + i * (cw + gap), r.y + 8, cw, r.height - 16);
                if (chip.xMax > r.xMax - 6) break;
                _potionChips.Add(chip);
                if (i >= B.Belt.Count)
                {
                    GuiKit.Fill(chip, Palette.WithAlpha(Palette.Abyss, 0.45f));
                    GuiKit.Label(chip, L("battle.empty"), 14, Palette.WithAlpha(Palette.ParchmentShadow, 0.6f), TextAnchor.MiddleCenter);
                    continue;
                }
                var belt = B.Belt[i];
                var def = B.PotionDef(belt.PotionId);
                bool usable = canUse && belt.Charges > 0 && def.UsableInCombat;
                bool held = _grab == Grab.Potion && _grabPotion == i;
                GuiKit.Fill(chip, Palette.WithAlpha(held ? Theme.Dusk : Palette.Panel, usable ? 0.95f : 0.5f));
                GuiKit.Frame(chip, usable ? Palette.Gold : Palette.WithAlpha(Palette.ParchmentShadow, 0.3f), usable ? 2 : 1);
                GuiKit.Label(new Rect(chip.x + 8, chip.y + 2, chip.width - 16, 38), def.Name, 14, usable ? Palette.Parchment : Palette.ParchmentShadow, TextAnchor.MiddleLeft, true, true);
                GuiKit.Label(new Rect(chip.x + 8, chip.yMax - 24, chip.width - 16, 22), "×" + belt.Charges + "  " + (def.Target == PotionTarget.Enemy ? "→ ◆" : def.Target == PotionTarget.AllAllies ? "→ ●●" : "→ ●"), 13, usable ? Palette.GoldBright : Palette.ParchmentShadow, TextAnchor.MiddleLeft);
            }
        }

        // ------------------------------------------------------------------ El astrolabio de órdenes

        /// <summary>Qué bruja muestra su abanico: la que sostienes, la que dejaste abierta o, en reposo, la que está en turno.</summary>
        private int FanWitchId
        {
            get
            {
                if (_grab == Grab.Card) return _grabWitch;
                if (_pinned) return _pinWitch;
                if (_d.AwaitingPlayer && !_chain.IsOpen && _grab == Grab.None) return _d.ActiveWitch?.Id ?? -1;
                return -1;
            }
        }

        /// <summary>Centro del abanico: arriba de la carta, corrido hacia dentro si la carta está en la orilla.</summary>
        private static Vector2 FanOrigin(Rect card)
        {
            float margin = FanRadius + PetalRadius + 12;
            return new Vector2(Mathf.Clamp(card.center.x, margin, GuiKit.Width - margin), card.y + 6);
        }

        private static Vector2 Polar(Vector2 o, float degrees, float radius)
        {
            float a = degrees * Mathf.Deg2Rad;
            return o + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * radius;
        }

        private void BuildPetals()
        {
            _petals.Clear();
            int id = FanWitchId;
            var w = B.FindWitch(id);
            if (w == null || !w.IsAlive || !_cards.TryGetValue(id, out var card)) { _fanOpen = false; return; }
            // La carta sostenida abre el astrolabio; una ficha sostenida se arrastra sola hasta el objetivo.
            _fanOpen = _pinned || _grab == Grab.Card && _grabPetal < 0 && (_grabMoved || Time.unscaledTime - _grabTime > 0.16f);
            float s = _fanOpen ? 1f : GhostScale;
            var o = FanOrigin(card);
            bool isTurn = B.ActiveUnit == w && B.Pending == DecisionKind.AllyTurn;
            bool ready = isTurn && _d.AwaitingPlayer && !_chain.IsOpen;

            if (isTurn)
            {
                var spell = w.Def.Spell;
                string spellBlock = !ready ? L("battle.wait") : !w.CanCastMagic ? L("battle.noMagic") : B.Grains < B.Rules.Grains.SpellCost ? L("battle.noGrains") : "";
                Add(Cmd.Basic, Polar(o, 168, FanRadius * s), s, L("battle.cmd.basic"), L("battle.cost.basic"), w.BasicMove.Name + ": " + w.BasicMove.Description, ready ? "" : L("battle.wait"), AllyAccent);
                if (w.Def.SelfCast != SelfCastForm.None)
                {
                    string selfBlock = !ready ? L("battle.wait") : !w.CanCastMagic ? L("battle.noMagic") : B.SelfCastUsedThisTurn ? L("battle.usedThisTurn") : "";
                    Add(Cmd.SelfCast, Polar(o, 128, FanRadius * s), s, L("battle.cmd.self"), SelfCastCost(w), (w.Def.SelfCastName ?? "") + ": " + SelfCastDetail(w), selfBlock, Palette.Planet(w.Recessive));
                }
                Add(Cmd.Spell, Polar(o, 90, FanRadius * s), s, L("battle.cmd.spell"), L("battle.cost.spell"), spell.Name + ": " + spell.Description, spellBlock, Palette.Planet(w.Dominant));
                if (!w.VislumbreUsed)
                {
                    bool canVis = w.CanCastMagic && (B.BlackSalt >= B.Rules.Vislumbre.BlackSaltCost || w.Has(StatusKind.FreeVislumbre));
                    Add(Cmd.Vislumbre, Polar(o, 52, FanRadius * s), s, L("battle.cmd.vislumbre"), L("battle.cost.vislumbre"),
                        $"Grado {w.Grade}→{PurityMath.Next(w.Grade)} por 2 turnos. +30 Sal.", !ready ? L("battle.wait") : canVis ? "" : L("battle.noBlackSalt"), Palette.Band(SalinityBand.ExtasisAbisal));
                }
                string chainBlock = B.ChainBlocker();
                if (chainBlock.Length == 0 || _d.Resonance >= B.Rules.Resonance.ChainCost / 2)
                    Add(Cmd.Chain, Polar(o, 12, FanRadius * s), s, L("battle.cmd.chain"), L("battle.cost.chain"), L("battle.chainDetail"), !ready ? L("battle.wait") : chainBlock, Palette.Spark);
            }

            // Ascensiones: más arriba, más poder. Se ven las que alcanzan y la siguiente que falta.
            if (AscWindow)
            {
                bool shownLocked = false;
                for (int lvl = 1; lvl <= 3; lvl++)
                {
                    string block = B.AscensionBlocker(w, lvl);
                    if (block.Length > 0)
                    {
                        if (shownLocked || !_fanOpen) continue;
                        shownLocked = true;
                    }
                    float dist = isTurn ? FanRadius + 92 + (lvl - 1) * AscStep : FanRadius - 12 + (lvl - 1) * AscStep;
                    var move = w.Def.Ascensions[lvl - 1];
                    var kind = lvl == 1 ? Cmd.Asc1 : lvl == 2 ? Cmd.Asc2 : Cmd.Asc3;
                    Add(kind, Polar(o, 90, dist * s), s, L("battle.asc") + " " + new[] { "I", "II", "III" }[lvl - 1], L("battle.cost.anima", B.AscensionCost(lvl)),
                        move != null ? move.Name + ": " + move.Description : "", block, Palette.FiveStar);
                }
            }
            if (!_fanOpen) LayChips(card);
        }

        /// <summary>
        /// En reposo, las órdenes de la bruja en turno son una fila de fichas sobre su carta: tocar una la lanza al
        /// objetivo marcado y arrastrarla la lanza a quien la sueltes. Se leen de izquierda a derecha como el astrolabio.
        /// </summary>
        private void LayChips(Rect card)
        {
            const float w = 100, h = 36, step = 106; // TUNING
            int n = _petals.Count;
            if (n == 0) return;
            float start = card.center.x - (n - 1) * step / 2f;
            start = Mathf.Clamp(start, 16 + w / 2, GuiKit.Width - 16 - w / 2 - (n - 1) * step);
            for (int i = 0; i < n; i++)
            {
                var p = _petals[i];
                p.Chip = true;
                p.Center = new Vector2(start + i * step, card.y - h / 2 - 8);
                p.Radius = w / 2;
                _petals[i] = p;
            }
        }

        private static Rect ChipRect(Petal p) => new Rect(p.Center.x - 50, p.Center.y - 18, 100, 36);

        private void Add(Cmd kind, Vector2 center, float scale, string title, string cost, string detail, string blocker, Color accent)
        {
            _petals.Add(new Petal { Kind = kind, Center = center, Radius = PetalRadius * scale, Title = title, Cost = cost, Detail = detail ?? "", Blocker = blocker ?? "", Accent = accent });
        }

        private string SelfCastCost(WitchUnit w)
        {
            var sc = B.Rules.SelfCast;
            switch (w.Def.SelfCast)
            {
                case SelfCastForm.AgujaYSello: return sc.AgujaGrains + " " + L("battle.grains");
                case SelfCastForm.Hilos: return "−10 % vida";
                case SelfCastForm.FrenesiSalNegra: return "1 " + L("battle.blackSalt");
                default: return "";
            }
        }

        private string SelfCastDetail(WitchUnit w)
        {
            var sc = B.Rules.SelfCast;
            switch (w.Def.SelfCast)
            {
                case SelfCastForm.AgujaYSello: return $"Aguja y Sello: +30 % ATQ 2 turnos. {sc.AgujaGrains} Granos, +25 Sal. No termina el turno.";
                case SelfCastForm.Hilos: return "Hilos: actúa de nuevo. −10 % vida, +25 Sal.";
                case SelfCastForm.FrenesiSalNegra: return "Frenesí: +100 % daño, −50 % def. 1 Sal Negra, +40 Sal. No termina el turno.";
                default: return "";
            }
        }

        private int PetalAt(Vector2 m)
        {
            int best = -1;
            float bestD = float.MaxValue;
            for (int i = 0; i < _petals.Count; i++)
            {
                if (_petals[i].Chip)
                {
                    var cr = ChipRect(_petals[i]);
                    if (new Rect(cr.x - 3, cr.y - 6, cr.width + 6, cr.height + 12).Contains(m)) return i;
                    continue;
                }
                float d = (m - _petals[i].Center).magnitude;
                float reach = _petals[i].Radius * (_fanOpen ? 1.25f : 1.15f);
                if (d < reach && d < bestD) { best = i; bestD = d; }
            }
            return best;
        }

        // ------------------------------------------------------------------ Entrada

        private void HandleGestures()
        {
            var e = Event.current;
            var m = e.mousePosition;
            if (e.type == EventType.Repaint && _grab != Grab.None && !_debugHold && !Input.GetMouseButton(0) && Input.touchCount == 0)
            {
                // Se perdió el soltar (ventana sin foco): nada se lanza por accidente.
                CancelGrab();
                return;
            }
            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                {
                    if (_pinned)
                    {
                        int hit = PetalAt(m);
                        if (hit >= 0)
                        {
                            StartGrab(Grab.Card, _pinWitch, m);
                            _grabPetal = hit;
                            _grabPetalKind = _armed = _petals[hit].Kind;
                            e.Use();
                            return;
                        }
                        // Un toque sobre una figura elige objetivo y deja el abanico abierto; en el vacío, lo cierra.
                        if (!_cards.Values.Any(r => r.Contains(m)))
                        {
                            if (UnitAtCanvas(m) == null) _pinned = false;
                            return;
                        }
                        _pinned = false;
                    }
                    int ghost = PetalAt(m);
                    if (ghost >= 0 && FanWitchId >= 0)
                    {
                        StartGrab(Grab.Card, FanWitchId, m);
                        _grabPetal = ghost;
                        _grabPetalKind = _armed = _petals[ghost].Kind;
                        e.Use();
                        return;
                    }
                    foreach (var kv in _cards)
                    {
                        var w = B.FindWitch(kv.Key);
                        if (w == null || !w.IsAlive || !kv.Value.Contains(m)) continue;
                        StartGrab(Grab.Card, w.Id, m);
                        _armed = B.ActiveUnit == w && _d.AwaitingPlayer ? Cmd.Basic : Cmd.None;
                        e.Use();
                        return;
                    }
                    for (int i = 0; i < _potionChips.Count && i < B.Belt.Count; i++)
                    {
                        if (!_potionChips[i].Contains(m)) continue;
                        StartGrab(Grab.Potion, -1, m);
                        _grabPotion = i;
                        e.Use();
                        return;
                    }
                    break;
                }
                case EventType.MouseDrag:
                    if (_grab == Grab.None) break;
                    Track(m);
                    e.Use();
                    break;
                case EventType.MouseUp:
                    if (_grab == Grab.None) break;
                    Track(m);
                    Release(m);
                    e.Use();
                    break;
            }
        }

        private void StartGrab(Grab kind, int witchId, Vector2 m)
        {
            _grab = kind;
            _grabWitch = witchId;
            _grabPotion = -1;
            _grabPetal = -1;
            _grabPetalKind = Cmd.None;
            _grabStart = _grabPoint = m;
            _grabTime = Time.unscaledTime;
            _grabMoved = false;
            _aimEnemy = _aimAlly = -1;
            _pinned = false;
        }

        private void CancelGrab()
        {
            _grab = Grab.None;
            _grabWitch = _grabPotion = _grabPetal = -1;
            _grabPetalKind = Cmd.None;
            _aimEnemy = _aimAlly = -1;
            _armed = Cmd.None;
            if (_d.Auto || B.IsOver || InputBlocked) _pinned = false;
        }

        private WitchUnit GrabbedWitch => B.FindWitch(_grabWitch);

        private void Track(Vector2 m)
        {
            _grabPoint = m;
            if ((m - _grabStart).magnitude > TapSlop) _grabMoved = true;
            int p = PetalAt(m);
            if (_grab == Grab.Card && p >= 0 && _fanOpen) _armed = _petals[p].Kind;
            // Volver a la carta propia desarma: así se cancela.
            if (_grab == Grab.Card && _grabMoved && _cards.TryGetValue(_grabWitch, out var own) && own.Contains(m)) _armed = Cmd.None;
            _aimEnemy = _aimAlly = -1;
            if (p >= 0 && _fanOpen) return;
            bool wantsAlly = _grab == Grab.Potion ? PotionWantsAlly(_grabPotion) : CommandWantsAlly(_armed, GrabbedWitch);
            bool wantsEnemy = _grab == Grab.Potion ? PotionWantsEnemy(_grabPotion) : CommandWantsEnemy(_armed, GrabbedWitch);
            var unit = UnitAtCanvas(m);
            if (unit == null) return;
            if (unit.Side == Side.Enemy && wantsEnemy) _aimEnemy = unit.Id;
            if (unit.Side == Side.Ally && wantsAlly) _aimAlly = unit.Id;
        }

        private void Release(Vector2 m)
        {
            var w = GrabbedWitch;
            float held = Time.unscaledTime - _grabTime;
            if (_grab == Grab.Potion)
            {
                int slot = _grabPotion;
                if (_aimEnemy >= 0) { _enemyTarget = _aimEnemy; UsePotion(slot); }
                else if (_aimAlly >= 0) { _allyTarget = _aimAlly; UsePotion(slot); }
                else if (!_grabMoved) UsePotion(slot);
                CancelGrab();
                return;
            }

            int petal = PetalAt(m);
            bool isTurn = w != null && B.ActiveUnit == w && _d.AwaitingPlayer;
            if (!_grabMoved && _grabPetalKind != Cmd.None) CastKind(_grabPetalKind, w);
            else if (petal >= 0 && _fanOpen) CastPetal(petal, w);
            else if (_aimEnemy >= 0 && _armed != Cmd.None) { _enemyTarget = _aimEnemy; CastKind(_armed, w); _gestureUsed = true; }
            else if (_aimAlly >= 0 && _armed != Cmd.None) { _allyTarget = _aimAlly; CastKind(_armed, w); _gestureUsed = true; }
            else if (!_grabMoved)
            {
                if (held >= LongPress) { _pinned = true; _pinWitch = w?.Id ?? -1; }
                else if (isTurn) Cast(Cmd.Basic, w);
                else if (w != null && w.IsAlive) _allyTarget = w.Id;
            }
            // Marcado en el astrolabio y soltado en el vacío: se lanza al objetivo marcado. Una ficha soltada en el vacío se cancela.
            else if (_grabPetal < 0 && _armed != Cmd.None && _armed != Cmd.Basic) { CastKind(_armed, w); _gestureUsed = true; }
            else if (isTurn && Flick(m))
            {
                // Deslizar hacia arriba sin apuntar, tan rápido que no tocó ningún pétalo: Hechizo, como en el estilo clásico.
                CastKind(Cmd.Spell, w);
                _gestureUsed = true;
            }
            bool keepPinned = _pinned && _grabPetal < 0;
            CancelGrab();
            _pinned = keepPinned;
        }

        private bool Flick(Vector2 m)
        {
            var v = m - _grabStart;
            return v.magnitude > FlickDistance && v.y < 0 && Mathf.Abs(v.x) < -v.y * 0.7f;
        }

        private void CastPetal(int index, WitchUnit w)
        {
            var p = _petals[index];
            if (p.Blocker.Length > 0) { _d.ShowToast(p.Title + ": " + p.Blocker); return; }
            Cast(p.Kind, w);
        }

        /// <summary>Lanza una orden por tipo, con el mismo aviso que su pétalo si hoy no se puede.</summary>
        private void CastKind(Cmd kind, WitchUnit w)
        {
            for (int i = 0; i < _petals.Count; i++)
                if (_petals[i].Kind == kind) { CastPetal(i, w); return; }
            Cast(kind, w);
        }

        private static MoveDef AscMove(WitchUnit w, Cmd c)
        {
            int lvl = c == Cmd.Asc1 ? 1 : c == Cmd.Asc2 ? 2 : c == Cmd.Asc3 ? 3 : 0;
            return w == null || lvl == 0 ? null : w.Def.Ascensions[lvl - 1];
        }

        private static bool CommandWantsAlly(Cmd c, WitchUnit w)
        {
            if (w == null) return false;
            if (c == Cmd.Spell) return w.Def.Spell.Target == TargetMode.Ally;
            var asc = AscMove(w, c);
            return asc != null && asc.Target == TargetMode.Ally;
        }

        private static bool CommandWantsEnemy(Cmd c, WitchUnit w)
        {
            if (w == null) return false;
            switch (c)
            {
                case Cmd.Basic: case Cmd.Chain: return true;
                case Cmd.Spell: return w.Def.Spell.Target == TargetMode.Enemy || w.Def.Spell.Target == TargetMode.AllEnemies;
                case Cmd.Asc1: case Cmd.Asc2: case Cmd.Asc3:
                    var asc = AscMove(w, c);
                    return asc != null && (asc.Target == TargetMode.Enemy || asc.Target == TargetMode.AllEnemies);
                default: return false;
            }
        }

        private bool PotionWantsAlly(int slot) => slot >= 0 && slot < B.Belt.Count && B.PotionDef(B.Belt[slot].PotionId).Target == PotionTarget.Ally;
        private bool PotionWantsEnemy(int slot) => slot >= 0 && slot < B.Belt.Count && B.PotionDef(B.Belt[slot].PotionId).Target == PotionTarget.Enemy;

        private void Cast(Cmd c, WitchUnit w)
        {
            if (w == null) return;
            GuiKit.Pressed();
            switch (c)
            {
                case Cmd.Basic:
                    _d.Submit(BattleCommand.Basic(w.Id, EnemyTarget));
                    break;
                case Cmd.Spell:
                    _d.Submit(BattleCommand.Spell(w.Id, w.Def.Spell.Target == TargetMode.Ally ? AllyTarget : EnemyTarget));
                    break;
                case Cmd.SelfCast:
                    _d.Submit(BattleCommand.SelfCast(w.Id));
                    break;
                case Cmd.Vislumbre:
                    _d.Submit(BattleCommand.Vislumbre(w.Id));
                    break;
                case Cmd.Chain:
                    if (B.ChainBlocker().Length == 0) _chain.Show(EnemyTarget);
                    else _d.ShowToast(B.ChainBlocker());
                    break;
                case Cmd.Asc1:
                case Cmd.Asc2:
                case Cmd.Asc3:
                {
                    int lvl = c == Cmd.Asc1 ? 1 : c == Cmd.Asc2 ? 2 : 3;
                    var move = w.Def.Ascensions[lvl - 1];
                    _hintsSeen.Add("asc");
                    _d.Submit(BattleCommand.Ascension(w.Id, lvl, move != null && move.Target == TargetMode.Ally ? AllyTarget : EnemyTarget));
                    break;
                }
            }
        }

        private void UsePotion(int slot)
        {
            if (slot < 0 || slot >= B.Belt.Count || B.ActiveUnit == null) return;
            if (!_d.AwaitingPlayer || B.PotionUsedThisTurn) { _d.ShowToast(L("battle.potionWait")); return; }
            var def = B.PotionDef(B.Belt[slot].PotionId);
            GuiKit.Pressed();
            _d.Submit(BattleCommand.Potion(B.ActiveUnit.Id, slot, def.Target == PotionTarget.Enemy ? EnemyTarget : AllyTarget));
        }

        // ------------------------------------------------------------------ Dibujo del abanico

        private void DrawFan()
        {
            if (_petals.Count == 0) return;
            var w = B.FindWitch(FanWitchId);
            if (w == null || !_cards.TryGetValue(w.Id, out var card)) return;
            var o = FanOrigin(card);
            float alpha = 1f;
            if (_fanOpen)
            {
                // Arco de astrolabio detrás de los pétalos.
                GuiKit.Ring(o, FanRadius, 0.5f, Palette.WithAlpha(Palette.Gold, 0.35f), Palette.WithAlpha(Palette.Gold, 0.35f), 40, 1.6f);
                GuiKit.Dot(o, 8, Palette.WithAlpha(Palette.GoldBright, 0.7f));
            }
            var pointer = _grab != Grab.None ? _grabPoint : Event.current.mousePosition;
            int hover = _grab == Grab.Card || _pinned ? PetalAt(pointer) : -1;
            for (int i = 0; i < _petals.Count; i++)
            {
                var p = _petals[i];
                bool enabled = p.Blocker.Length == 0;
                bool armed = _grab == Grab.Card && _armed == p.Kind && p.Kind != Cmd.None;
                bool hot = i == hover || armed;
                if (p.Chip)
                {
                    DrawChip(p, enabled, hot);
                    continue;
                }
                float r = p.Radius * (hot ? 1.12f : 1f);
                if (_fanOpen) GuiKit.Line(o, p.Center, 1, Palette.WithAlpha(Palette.Gold, 0.25f));
                GuiKit.Dot(p.Center, r + 3, Palette.WithAlpha(enabled ? p.Accent : Palette.ParchmentShadow, alpha * (hot ? 1f : 0.75f)));
                GuiKit.Dot(p.Center, r, Palette.WithAlpha(hot ? Theme.Dusk : Palette.Abyss, alpha * 0.94f));
                if (p.Kind == Cmd.Asc1 || p.Kind == Cmd.Asc2 || p.Kind == Cmd.Asc3 || p.Kind == Cmd.Spell)
                {
                    var planet = p.Kind == Cmd.Spell ? w.Dominant : w.Recessive;
                    var seal = Theme.Kit("seals/" + planet.ToString().ToLowerInvariant());
                    if (seal != null && _fanOpen)
                    {
                        var old = GUI.color;
                        GUI.color = new Color(1, 1, 1, enabled ? 0.28f : 0.12f);
                        GUI.DrawTexture(new Rect(p.Center.x - r * 0.8f, p.Center.y - r * 0.8f, r * 1.6f, r * 1.6f), seal);
                        GUI.color = old;
                    }
                }
                var titleColor = Palette.WithAlpha(enabled ? Palette.Parchment : Palette.ParchmentShadow, alpha * (enabled ? 1f : 0.55f));
                GuiKit.Label(new Rect(p.Center.x - r - 12, p.Center.y - 20, r * 2 + 24, 24), p.Title, p.Title.Length > 9 ? 13 : 15, titleColor, TextAnchor.MiddleCenter, true);
                GuiKit.Label(new Rect(p.Center.x - r - 8, p.Center.y + 2, r * 2 + 16, 20), p.Cost, 12, Palette.WithAlpha(enabled ? Palette.GoldBright : Palette.ParchmentShadow, enabled ? 1f : 0.6f), TextAnchor.MiddleCenter);
            }

            // Ficha de la orden bajo el dedo: nombre, efecto y por qué no se puede, si no se puede.
            if (_fanOpen && hover >= 0) DrawPetalDetail(_petals[hover], o);
            else if (_grab == Grab.Card && _armed != Cmd.None && _grabMoved) DrawAimLabel(w);
            if (_grab == Grab.Card && _grabMoved && (_aimEnemy >= 0 || _aimAlly >= 0))
                GuiKit.Line(_grabPetal >= 0 && !_fanOpen ? _grabStart : o, _grabPoint, 3, Palette.WithAlpha(_aimEnemy >= 0 ? Palette.Spark : AllyAccent, 0.85f));
        }

        /// <summary>Ficha de orden en reposo: tinta, filete del color de la orden y su costo.</summary>
        private void DrawChip(Petal p, bool enabled, bool hot)
        {
            var r = ChipRect(p);
            GuiKit.Fill(r, Palette.WithAlpha(hot ? Theme.Dusk : Palette.Abyss, enabled ? 0.9f : 0.6f));
            GuiKit.Frame(r, Palette.WithAlpha(enabled ? p.Accent : Palette.ParchmentShadow, enabled ? (hot ? 1f : 0.7f) : 0.3f), hot ? 2 : 1);
            GuiKit.Fill(new Rect(r.x, r.y, 4, r.height), Palette.WithAlpha(p.Accent, enabled ? 1f : 0.35f));
            var ink = enabled ? Palette.Parchment : Palette.WithAlpha(Palette.ParchmentShadow, 0.55f);
            GuiKit.Label(new Rect(r.x + 6, r.y + 1, r.width - 8, 20), p.Title, p.Title.Length > 9 ? 12 : 14, ink, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(r.x + 6, r.y + 18, r.width - 8, 16), p.Cost, 11, Palette.WithAlpha(enabled ? Palette.GoldBright : Palette.ParchmentShadow, enabled ? 0.95f : 0.5f), TextAnchor.MiddleCenter);
        }

        /// <summary>Poción en la mano: línea hasta el dedo y a quién se le dará.</summary>
        private void DrawPotionAim()
        {
            if (_grab != Grab.Potion || !_grabMoved || _grabPotion < 0 || _grabPotion >= _potionChips.Count || _grabPotion >= B.Belt.Count) return;
            var chip = _potionChips[_grabPotion];
            bool hit = _aimEnemy >= 0 || _aimAlly >= 0;
            GuiKit.Line(chip.center, _grabPoint, 3, Palette.WithAlpha(hit ? Palette.Spark : Palette.Gold, 0.8f));
            var def = B.PotionDef(B.Belt[_grabPotion].PotionId);
            string target = _aimEnemy >= 0 ? B.FindEnemy(_aimEnemy)?.Name : _aimAlly >= 0 ? B.FindWitch(_aimAlly)?.Name : null;
            string text = target != null ? def.Name + " → " + target : def.Name + " · " + L(def.Target == PotionTarget.Enemy ? "battle.dropEnemy" : def.Target == PotionTarget.Ally ? "battle.dropAlly" : "battle.releaseHint");
            var size = GuiKit.Style(17, Palette.Spark, TextAnchor.MiddleCenter, true).CalcSize(new GUIContent(text));
            var r = new Rect(Mathf.Clamp(_grabPoint.x - size.x / 2 - 12, 10, GuiKit.Width - size.x - 34), _grabPoint.y - 70, size.x + 24, 32);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.85f));
            GuiKit.Label(r, text, 17, Palette.Spark, TextAnchor.MiddleCenter, true);
        }

        private void DrawPetalDetail(Petal p, Vector2 origin)
        {
            const float w = 380, h = 120;
            float x = origin.x + FanRadius + 70;
            if (x + w > GuiKit.Width - 20) x = origin.x - FanRadius - 70 - w;
            var r = new Rect(Mathf.Clamp(x, 20, GuiKit.Width - w - 20), origin.y - FanRadius - h - 10, w, h);
            InkPanel(r, p.Accent, 0.94f);
            GuiKit.Label(new Rect(r.x + 12, r.y + 6, w - 24, 26), p.Title + " · " + p.Cost, 17, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x + 12, r.y + 34, w - 24, h - 40), p.Blocker.Length > 0 ? "<color=#F09591>" + p.Blocker + "</color>\n" + Short(p.Detail, 120) : Short(p.Detail, 150), 14, Palette.Parchment, TextAnchor.UpperLeft, false, true);
        }

        private void DrawAimLabel(WitchUnit w)
        {
            string order = _armed == Cmd.Basic ? L("battle.cmd.basic") : _petals.FirstOrDefault(p => p.Kind == _armed).Title ?? "";
            string target = _aimEnemy >= 0 ? B.FindEnemy(_aimEnemy)?.Name : _aimAlly >= 0 ? B.FindWitch(_aimAlly)?.Name : null;
            string text = target != null ? order + " → " + target : _cards.TryGetValue(w.Id, out var own) && own.Contains(_grabPoint) ? L("battle.cancelHint") : order + " · " + L("battle.releaseHint");
            var size = GuiKit.Style(18, Palette.Spark, TextAnchor.MiddleCenter, true).CalcSize(new GUIContent(text));
            var r = new Rect(_grabPoint.x - size.x / 2 - 12, _grabPoint.y - 74, size.x + 24, 34);
            r.x = Mathf.Clamp(r.x, 10, GuiKit.Width - r.width - 10);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.85f));
            GuiKit.Frame(r, Palette.WithAlpha(Palette.Gold, 0.8f), 1);
            GuiKit.Label(r, text, 18, Palette.Spark, TextAnchor.MiddleCenter, true);
        }

        // ------------------------------------------------------------------ Avisos

        private void DrawChainPlayback()
        {
            if (_d.PlayingChain == null) return;
            var r = new Rect(460, 118, 1000, 140);
            GuiKit.PanelBox(r, 0.86f);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 400, 22), "Partitura", 16, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            PartituraDrawer.Draw(new Rect(r.x + 12, r.y + 26, r.width - 24, r.height - 34), _d.PlayingChain,
                id => B.FindWitch(id)?.Dominant ?? Planet.Saturno, true, _d.PlayheadFrame, _d.PlayedNotes, B.Rules.Chain.SparkWindowFrames);
        }

        private void DrawBanner()
        {
            if (_d.BannerTime <= 0 || string.IsNullOrEmpty(_d.Banner)) return;
            float a = Mathf.Clamp01(_d.BannerTime * 2f);
            var r = new Rect(460, 300, 1000, 72);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.55f * a));
            GuiKit.Label(new Rect(r.x + 3, r.y + 3, r.width, r.height), _d.Banner, 40, Palette.WithAlpha(Color.black, a), TextAnchor.MiddleCenter, true);
            GuiKit.Label(r, _d.Banner, 40, Palette.WithAlpha(_d.BannerColor, a), TextAnchor.MiddleCenter, true);
        }

        private void DrawToast()
        {
            if (_d.ToastTime <= 0 || string.IsNullOrEmpty(_d.Toast)) return;
            var r = new Rect(560, 900 - 70, 800, 44);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Crimson, 0.92f));
            GuiKit.Label(r, _d.Toast, 18, Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        /// <summary>Una sola línea, arriba al centro, solo mientras aplica. Las primeras explican los gestos.</summary>
        private void DrawHint()
        {
            string hint = CurrentHint();
            if (string.IsNullOrEmpty(hint)) return;
            var size = GuiKit.Style(16, Palette.Parchment, TextAnchor.MiddleCenter).CalcSize(new GUIContent(hint));
            float w = Mathf.Min(size.x + 40, 1060);
            // Con barra de jefe, la pista baja a la orilla de la barra en vez de taparla.
            float y = _bossBars > 0 ? BattleHudLayout.PlatesTop(true) - 2 : 112;
            var r = new Rect(960 - w / 2, y, w, 34);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.66f));
            GuiKit.Fill(new Rect(r.x, r.yMax - 2, r.width, 2), Palette.WithAlpha(Palette.Gold, 0.6f));
            GuiKit.Label(new Rect(r.x + 14, r.y, r.width - 28, r.height), hint, 16, Palette.Parchment, TextAnchor.MiddleCenter);
        }

        private string CurrentHint()
        {
            if (B.IsOver || _d.Auto || _grab != Grab.None) return "";
            var w = _d.ActiveWitch;
            var boss = B.Enemies.FirstOrDefault(e => e.IsAlive && e.IsCharging);
            if (boss != null)
                return $"{boss.Name} carga {boss.ChargedSkill.Name}. Quiébralo (barra blanca de Firmeza, usa sus planetas débiles) o cierra una Cadena con la Zarigüeya.";
            if (_pinned) return L("battle.pinnedHint");
            if (B.Pending == DecisionKind.AllyTurn && B.ChainBlocker().Length == 0)
                return L("battle.chainHint");
            if (w != null && B.Band(w) >= SalinityBand.Sobresalada)
                return $"{w.Name} está {SalinityBands.DisplayName(B.Band(w))}. A 150 su Ascendente toma el control. Básico o Destilado de Rocío bajan la Salinidad.";
            if (B.Witches.Any(x => AscensionsReady(x) > 0) && !_hintsSeen.Contains("asc"))
                return L("battle.ascHint");
            if (!_gestureUsed && _d.AwaitingPlayer && w != null && B.TurnNumber <= 6)
                return L("battle.gestureHint", w.Name);
            return "";
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        /// <summary>
        /// Recorrido de capturas: "pin" deja el abanico abierto, "hold:Spell:1" sostiene la carta en turno con esa orden
        /// armada sobre el enemigo vivo de ese índice, "release" suelta y devuelve la orden que llegó al núcleo.
        /// Usa el mismo camino que un dedo (Track y Release); solo se salta el evento del sistema operativo.
        /// </summary>
        public string DebugAction(string action)
        {
            var parts = action.Split(':');
            switch (parts[0])
            {
                case "pin":
                {
                    var w = _d.ActiveWitch;
                    if (w == null || !_d.AwaitingPlayer) return "sin turno";
                    _pinned = true;
                    _pinWitch = w.Id;
                    return "abanico abierto de " + w.Name;
                }
                case "hold":
                    return DebugHold((Cmd)System.Enum.Parse(typeof(Cmd), parts[1]), int.Parse(parts[2]));
                case "release":
                {
                    if (_grab == Grab.None) return "nada en la mano";
                    int before = B.CommandLog.Count;
                    _debugHold = false;
                    Release(_grabPoint);
                    return B.CommandLog.Count > before ? B.CommandLog[B.CommandLog.Count - 1].ToString() : "sin orden";
                }
                default:
                    _pinned = false;
                    _debugHold = false;
                    CancelGrab();
                    return "cerrado";
            }
        }

        private string DebugHold(Cmd kind, int enemyIndex)
        {
            var w = _d.ActiveWitch;
            if (w == null || !_d.AwaitingPlayer || !_cards.TryGetValue(w.Id, out var card)) return "sin turno";
            StartGrab(Grab.Card, w.Id, card.center);
            _armed = Cmd.Basic;
            _grabMoved = true;
            _debugHold = true;
            BuildPetals();
            int pi = _petals.FindIndex(p => p.Kind == kind);
            if (pi >= 0) Track(_petals[pi].Center);
            var enemy = B.Enemies.Where(e => e.IsAlive).ElementAtOrDefault(enemyIndex);
            if (enemy == null || !_d.Views.TryGetValue(enemy.Id, out var v) || v == null) return "sin enemigo";
            var point = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(v.transform.position + Vector3.up * v.Height * 0.45f));
            Track(point);
            return $"{w.Name}: {kind} armado como {_armed}, apunta a {B.FindEnemy(_aimEnemy)?.Name ?? "nadie"}";
        }
#endif

        // ------------------------------------------------------------------ Toques en el escenario

        /// <summary>Lo que hay bajo un punto del lienzo: placa enemiga, carta aliada o la silueta de una figura.</summary>
        private Unit UnitAtCanvas(Vector2 p)
        {
            foreach (var (id, rect) in _plates)
                if (rect.Contains(p)) return B.FindEnemy(id);
            foreach (var kv in _cards)
                if (kv.Value.Contains(p)) return B.FindWitch(kv.Key);
            var view = FigureAt(GuiKit.CanvasToScreen(p));
            if (view == null) return null;
            return view.IsEnemy ? (Unit)B.FindEnemy(view.UnitId) : B.FindWitch(view.UnitId);
        }

        /// <summary>Los recortes no llevan collider: gana la figura viva cuya silueta (pies a cabeza) contiene el punto.</summary>
        private UnitView FigureAt(Vector3 screenPos)
        {
            if (_camera == null) return null;
            UnitView found = null;
            var ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
            if (Physics.Raycast(ray, out var hit, 100f)) found = hit.collider.GetComponentInParent<UnitView>();
            if (found != null && !found.IsDead) return found;
            float best = float.MaxValue;
            foreach (var v in _d.Views.Values)
            {
                if (v == null || v.IsDead) continue;
                var feet = _camera.WorldToScreenPoint(v.transform.position);
                var head = _camera.WorldToScreenPoint(v.Head);
                if (feet.z < 0) continue;
                // Un poco más ancha que el corchete: el dedo es menos preciso que la vista. // TUNING
                float halfW = HalfWidth(v, head.y - feet.y) * 1.18f;
                if (screenPos.y < feet.y - 20 || screenPos.y > head.y || Mathf.Abs(screenPos.x - feet.x) > halfW) continue;
                // Gana la más cercana a la cámara; ante un gigante detrás, la escolta del frente sigue siendo tocable.
                if (feet.z < best) { best = feet.z; found = v; }
            }
            return found;
        }

        /// <summary>El punto cae sobre el HUD: las cartas, la bandeja, el abanico abierto, los controles o el registro.</summary>
        private bool OverHud(Vector2 p)
        {
            if (_cards.Values.Any(r => new Rect(r.x, r.y - 26, r.width, r.height + 26).Contains(p))) return true;
            if (TrayRect.Contains(p) || p.y < 108 && (p.x < 560 || p.x > 1440)) return true;
            if (ResonanceRect(new Vector2(960, 54)).Contains(p)) return true;
            if (LogOpen && new Rect(20, 112, 520, 540).Contains(p)) return true;
            if (_plates.Any(t => t.rect.Contains(p))) return true;
            return (_fanOpen || _grab != Grab.None) && PetalAt(p) >= 0;
        }

        /// <summary>Toque sobre el escenario (coordenadas reales de pantalla): elige objetivo enemigo o aliada de apoyo.</summary>
        public void HandleWorldClick(Vector2 screenPos)
        {
            if (_camera == null || InputBlocked || _grab != Grab.None) return;
            var point = GuiKit.ScreenToCanvas(new Vector3(screenPos.x, screenPos.y, 0));
            if (OverHud(point) || PetalAt(point) >= 0) return;
            var view = FigureAt(screenPos);
            if (view == null) return;
            if (view.IsEnemy) _enemyTarget = view.UnitId;
            else _allyTarget = view.UnitId;
        }
    }
}
