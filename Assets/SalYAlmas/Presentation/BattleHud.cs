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
    /// Interfaz de combate (GDD 9.1): línea de AV arriba a la izquierda, retratos con anillo de Ánima y barra de
    /// Salinidad abajo, Granos a la derecha y Resonancia al centro. Más el registro LitRPG, el cinturón de
    /// pociones, las acciones y las pistas para la prueba con jugadores.
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
        /// <summary>Busto de una bruja por ID, para las placas del HUD.</summary>
        public static System.Func<string, Texture2D> Bust;
        public static System.Func<string, string> Localize;
        private static string L(string key) => Localize?.Invoke(key) ?? key;
        // TUNING: señales de bando independientes de los colores de planeta.
        private static readonly Color AllyAccent = Palette.Hex("#9CCDBA");
        private static readonly Color EnemyAccent = Palette.Hex("#F09591");

        private static void InkPanel(Rect r, Color accent)
        {
            GuiKit.Fill(r, Palette.Abyss);
            var leather = Theme.Kit("chrome/tile_leather");
            if (leather != null)
            {
                var old = GUI.color;
                GUI.color = new Color(1, 1, 1, 0.08f);
                GUI.DrawTextureWithTexCoords(r, leather, new Rect(0, 0, r.width / 256f, r.height / 256f));
                GUI.color = old;
            }
            GuiKit.Frame(r, Palette.Hex("#514B50"), 1);
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
            GuiKit.Label(new Rect(c.x - radius, c.y - radius, radius * 2, radius * 2), number, 18, Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        private static void TargetBrackets(Rect r)
        {
            const float arm = 20; // TUNING
            foreach (float x in new[] { r.x, r.xMax })
                foreach (float y in new[] { r.y, r.yMax })
                {
                    float dx = x == r.x ? arm : -arm;
                    float dy = y == r.y ? arm : -arm;
                    // Rectángulos alineados al lienzo: conservan su anclaje con franjas laterales.
                    GuiKit.Fill(new Rect(Mathf.Min(x, x + dx), y - 2, arm, 4), Palette.Spark);
                    GuiKit.Fill(new Rect(x - 2, Mathf.Min(y, y + dy), 4, arm), Palette.Spark);
                }
        }

        private void DrawSides()
        {
            foreach (bool enemy in new[] { false, true })
            {
                var r = new Rect(enemy ? 1080 : 32, 174, enemy ? 488 : 420, 44);
                InkPanel(r, enemy ? EnemyAccent : AllyAccent);
                SideSeal(new Vector2(r.x + 24, r.center.y), enemy, "");
                GuiKit.Label(new Rect(r.x + 52, r.y, r.width - 64, r.height), L(enemy ? "battle.enemies" : "battle.allies"), 23, enemy ? EnemyAccent : AllyAccent, TextAnchor.MiddleLeft, true);
            }
            foreach (var u in B.Witches.Cast<Unit>().Concat(B.Enemies))
            {
                if (!u.IsAlive || !_d.Views.TryGetValue(u.Id, out var v) || v == null) continue;
                var screen = _camera.WorldToScreenPoint(v.transform.position);
                if (screen.z <= 0) continue;
                var feet = GuiKit.ScreenToCanvas(screen);
                bool enemy = u.Side == Side.Enemy;
                int index = enemy ? B.Enemies.ToList().FindIndex(e => e.Id == u.Id) : B.Witches.ToList().FindIndex(w => w.Id == u.Id);
                SideSeal(new Vector2(feet.x, feet.y - 8), enemy, (index + 1).ToString(), 19);
                if (enemy && EnemyTarget == u.Id)
                {
                    var head = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(v.Head));
                    float height = Mathf.Clamp(feet.y - head.y, 80, 340);
                    TargetBrackets(new Rect(feet.x - height * 0.27f, feet.y - height, height * 0.54f, height - 36));
                }
            }
        }

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

        // ------------------------------------------------------------------ Dibujo

        public void Draw()
        {
            bool oldEnabled = GUI.enabled;
            if (InputBlocked) GUI.enabled = false;
            DrawAll();
            GUI.enabled = oldEnabled;
        }

        private void DrawAll()
        {
            DrawSides();
            DrawEnemyPlates();
            DrawFloatingTexts();
            DrawTurnLine(new Rect(16, 12, 1180, 86));
            DrawTopRight(new Rect(1560, 12, 344, 86));
            DrawResonance(new Vector2(1290, 56));
            DrawLog(new Rect(16, 112, 520, 560));
            DrawGrainsAndBelt(new Rect(1600, 112, 304, 312));
            for (int i = 0; i < B.Witches.Count; i++)
                DrawWitchPanel(B.Witches[i], new Rect(16 + i * 392, 840, 380, 228));
            DrawActions(new Rect(1600, 436, 304, 632));
            DrawChainPlayback();
            DrawBanner();
            DrawToast();
            if (!_chain.IsOpen) DrawHint(new Rect(560, 112, 1020, 46));
        }

        private void DrawTurnLine(Rect r)
        {
            InkPanel(r, Palette.Gold);
            GuiKit.Label(new Rect(r.x + 12, r.y + 1, 600, 24), L("battle.order"), 19, Palette.ParchmentShadow);
            var order = B.IsOver ? new List<TurnOrderEntry>() : B.TurnOrderPreview(8);
            for (int i = 0; i < order.Count; i++)
            {
                var u = order[i].Unit;
                bool enemy = u.Side == Side.Enemy;
                var chip = new Rect(r.x + 10 + i * 146, r.y + 28, 140, 50);
                bool active = i == 0 && u == B.ActiveUnit;
                GuiKit.Fill(chip, enemy ? Palette.Hex("#352027") : Palette.Hex("#15262A"));
                SideSeal(new Vector2(chip.x + 17, chip.y + 17), enemy, "", 7);
                GuiKit.Label(new Rect(chip.x + 32, chip.y, chip.width - 34, 28), u.Name, 18, Palette.Parchment, TextAnchor.MiddleLeft, true);
                GuiKit.Label(new Rect(chip.x + 8, chip.y + 27, chip.width - 12, 22), active ? L("battle.now") : "+" + order[i].AvFromNow.ToString("0"), 17, enemy ? EnemyAccent : AllyAccent);
                if (active) GuiKit.Frame(chip, Palette.GoldBright, 2);
            }
        }

        private void DrawTopRight(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 160, 30), "Ciclo " + B.Cycle, 22, Palette.Parchment, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x + 12, r.y + 34, 170, 24), "AV " + B.ElapsedAv.ToString("0"), 16, Palette.ParchmentShadow);
            float t = Time.realtimeSinceStartup - _d.RealStartTime;
            if (_d.RealEndTime > 0) t = _d.RealEndTime - _d.RealStartTime;
            GuiKit.Label(new Rect(r.x + 12, r.y + 56, 170, 24), "Tiempo " + BattleDirector.FormatTime(t), 16, Palette.ParchmentShadow);
            if (GuiKit.Button(new Rect(r.x + 180, r.y + 8, 72, 34), _d.Speed > 1 ? "x2" : "x1", true, 20)) _d.Speed = _d.Speed > 1 ? 1f : 2f;
            if (GuiKit.Button(new Rect(r.x + 258, r.y + 8, 78, 34), _d.Auto ? "<b>Auto</b>" : "Auto", true, 18))
            {
                _d.Auto = !_d.Auto;
                if (_d.Auto) _chain.Close();
            }
            if (_d.Auto) GuiKit.Frame(new Rect(r.x + 258, r.y + 8, 78, 34), Palette.GoldBright, 2);
            GuiKit.Label(new Rect(r.x + 180, r.y + 46, 160, 34), "Partituras: " + _d.Pilot.Partituras.Count, 15, Palette.ParchmentShadow);
        }

        private void DrawResonance(Vector2 c)
        {
            float frac = _d.Resonance / (float)B.Rules.Resonance.Max;
            bool full = _d.Resonance >= B.Rules.Resonance.ChainCost;
            Color on = full ? Color.Lerp(Palette.GoldBright, Palette.Spark, Theme.ReduceMotion ? 0.5f : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)) : Palette.Gold;
            GuiKit.Dot(c, 46, Palette.WithAlpha(Palette.Panel, 0.92f));
            GuiKit.Ring(c, 50, frac, on, Palette.WithAlpha(Palette.ParchmentShadow, 0.25f), 40, 4.5f);
            GuiKit.Label(new Rect(c.x - 50, c.y - 22, 100, 30), _d.Resonance.ToString(), 28, full ? Palette.Spark : Palette.Parchment, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(c.x - 60, c.y + 6, 120, 20), "Resonancia", 14, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
        }

        /// <summary>El registro empieza plegado: abierto tapa a las brujas de la fila de atrás.</summary>
        public bool LogOpen;

        private void DrawLog(Rect full)
        {
            var tab = new Rect(full.x, full.y, 210, 40);
            if (GuiKit.Button(tab, LogOpen ? "Registro ▾" : "Registro ▸", true, 17)) LogOpen = !LogOpen;
            if (!LogOpen) return;
            var r = new Rect(full.x, full.y + 46, full.width, full.height - 46);
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

        private void DrawGrainsAndBelt(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 200, 26), "Granos de Sal", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            string boost = B.GrainBoostActive ? " ×2" : "";
            GuiKit.Label(new Rect(r.xMax - 112, r.y + 4, 100, 26), $"{_d.Grains}/{B.Rules.Grains.Max}{boost}", 17, Palette.Parchment, TextAnchor.MiddleRight, true);
            float step = Mathf.Min(44f, (r.width - 40) / Mathf.Max(1, B.Rules.Grains.Max - 1));
            for (int i = 0; i < B.Rules.Grains.Max; i++)
            {
                var c = new Vector2(r.x + 26 + i * step, r.y + 50);
                bool on = i < _d.Grains;
                GuiKit.Dot(c, 16, Palette.WithAlpha(Palette.Abyss, 0.9f));
                GuiKit.Dot(c, 13, on ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.18f));
            }
            GuiKit.Label(new Rect(r.x + 12, r.y + 72, r.width - 20, 22), "Sal Negra: " + B.BlackSalt, 16, Palette.Band(SalinityBand.ExtasisAbisal));

            GuiKit.Label(new Rect(r.x + 12, r.y + 98, 280, 24), "Cinturón (1 por turno)", 17, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            bool canUse = _d.AwaitingPlayer && !B.PotionUsedThisTurn;
            float slotH = Mathf.Min(58f, (r.height - 132) / Mathf.Max(1, B.BeltSlots) - 4);
            for (int i = 0; i < B.BeltSlots; i++)
            {
                var slot = new Rect(r.x + 12, r.y + 126 + i * (slotH + 4), r.width - 24, slotH);
                if (i >= B.Belt.Count)
                {
                    GuiKit.Fill(slot, Palette.WithAlpha(Palette.Abyss, 0.5f));
                    GuiKit.Label(slot, "vacía", 16, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
                    continue;
                }
                var belt = B.Belt[i];
                var def = B.PotionDef(belt.PotionId);
                string target = def.Target == PotionTarget.Enemy ? "→ objetivo" : def.Target == PotionTarget.AllAllies ? "→ todas" : "→ " + (B.FindWitch(AllyTarget)?.Name ?? "");
                string text = $"<b>{def.Name}</b> ×{belt.Charges}\n<size=13>{def.Description} {target}</size>";
                if (GuiKit.Button(slot, text, canUse && belt.Charges > 0 && def.UsableInCombat, 15))
                {
                    int t = def.Target == PotionTarget.Enemy ? EnemyTarget : AllyTarget;
                    _d.Submit(BattleCommand.Potion(B.ActiveUnit.Id, i, t));
                }
            }
        }

        private void DrawWitchPanel(WitchUnit w, Rect r)
        {
            bool active = B.ActiveUnit == w;
            bool selected = AllyTarget == w.Id && !active;
            InkPanel(r, AllyAccent);
            if (!InputBlocked && Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition) && w.IsAlive) _allyTarget = w.Id;
            int hp = _d.Hp.TryGetValue(w.Id, out var h) ? h : w.Hp;
            int sal = _d.Salinity.TryGetValue(w.Id, out var s) ? s : w.Salinity;
            int anima = _d.Anima.TryGetValue(w.Id, out var a) ? a : w.Anima;
            var band = SalinityBands.FromValue(sal, B.Rules.Salinity);
            var bustRect = new Rect(r.x + 12, r.y + 12, 64, 64);
            var bust = Bust?.Invoke(w.Key);
            if (bust != null) PortraitBank.Fill(bustRect, bust);
            GuiKit.Frame(bustRect, AllyAccent, 1);
            SideSeal(new Vector2(r.x + 24, r.y + 65), false, (B.Witches.ToList().IndexOf(w) + 1).ToString(), 12);
            GuiKit.Label(new Rect(r.x + 90, r.y + 14, 278, 34), w.Name, 26, Palette.Parchment, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x + 90, r.y + 46, 214, 28), w.Dominant + " · " + w.EffectiveGrade + " · " + (B.Purity(w) * 100).ToString("0") + "%", 19, Palette.ParchmentShadow);
            float bw = r.width - 104;
            GuiKit.Label(new Rect(r.x + 12, r.y + 78, bw, 28), $"{L("battle.hp")} {hp:N0}/{w.MaxHp:N0}", 22, Palette.SaltWhite, TextAnchor.MiddleLeft, true);
            GuiKit.Bar(new Rect(r.x + 12, r.y + 108, bw, 10), hp / (float)w.MaxHp, AllyAccent, Palette.Hex("#34303C"));
            if (w.ShieldAmount > 0) GuiKit.Fill(new Rect(r.x + 12, r.y + 118, bw * Mathf.Clamp01((float)(w.ShieldAmount / w.MaxHp)), 3), Palette.SaltWhite);
            GuiKit.Label(new Rect(r.x + 12, r.y + 122, r.width - 24, 28), $"{L("battle.salt")} {sal}/{B.Rules.Salinity.Max} · {SalinityBands.DisplayName(band)}", 20, Palette.Parchment);
            var salRect = new Rect(r.x + 12, r.y + 150, r.width - 24, 7);
            GuiKit.Bar(salRect, sal / (float)B.Rules.Salinity.Max, Palette.Band(band), Palette.Hex("#34303C"));
            foreach (int mark in new[] { B.Rules.Salinity.SalobreAt, B.Rules.Salinity.SobresaladaAt, B.Rules.Salinity.CristalizacionAt })
                GuiKit.Fill(new Rect(salRect.x + salRect.width * mark / B.Rules.Salinity.Max, salRect.y, 2, salRect.height), Palette.ParchmentShadow);
            var c = new Vector2(r.xMax - 45, r.y + 98);
            // Los tres círculos conservan los umbrales de 100/200/300 Ánima del GDD.
            for (int tier = 0; tier < 3; tier++)
                GuiKit.Ring(c, 24 + tier * 5, (anima - tier * 100) / 100f, Palette.GoldBright, Palette.Hex("#34303C"), 36, 1.5f);
            GuiKit.Label(new Rect(c.x - 34, c.y - 15, 68, 30), anima.ToString(), 24, Palette.Parchment, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(c.x - 40, r.y + 38, 80, 24), L("battle.anima"), 18, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
            bool window = !_d.IsPlaying && !_d.Auto && !_chain.IsOpen;
            float ab = (r.width - 36) / 3;
            for (int lvl = 1; lvl <= 3; lvl++)
            {
                var b = new Rect(r.x + 12 + (lvl - 1) * (ab + 6), r.y + 168, ab, 48);
                bool can = window && B.AscensionBlocker(w, lvl).Length == 0;
                if (GuiKit.Button(b, L("battle.asc") + " " + new[] { "I", "II", "III" }[lvl - 1], can, 20))
                {
                    var move = w.Def.Ascensions[lvl - 1];
                    _d.Submit(BattleCommand.Ascension(w.Id, lvl, move.Target == TargetMode.Ally ? AllyTarget : EnemyTarget));
                }
                if (can) GuiKit.Frame(b, Palette.GoldBright, 2);
            }
            var tags = w.Statuses.Where(st => !st.Name.StartsWith("Regencia")).Select(st => st.TurnsLeft > 0 ? $"{st.Name} ({st.TurnsLeft})" : st.Name).Distinct().ToArray();
            if (tags.Length > 0)
            {
                var tag = new Rect(r.x, r.y - 30, r.width, 28);
                GuiKit.Fill(tag, Palette.Abyss);
                GuiKit.Label(new Rect(tag.x + 8, tag.y, tag.width - 16, tag.height), string.Join(" · ", tags), 18, Palette.Parchment);
            }
            if (active) { GuiKit.Frame(r, Palette.GoldBright, 3); GuiKit.Label(new Rect(r.xMax - 116, r.y + 2, 106, 24), L("battle.now"), 17, Palette.Spark, TextAnchor.MiddleRight, true); }
            else if (selected) { GuiKit.Frame(r, AllyAccent, 3); GuiKit.Label(new Rect(r.xMax - 132, r.y + 2, 122, 24), L("battle.allyTarget"), 16, AllyAccent, TextAnchor.MiddleRight, true); }
            if (!w.IsAlive) { GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.85f)); GuiKit.Label(r, L("battle.fallen"), 32, Palette.ParchmentShadow, TextAnchor.MiddleCenter, true); }
        }

        /// <summary>Texto claro con sombra de tinta, legible sobre cualquier relleno de barra.</summary>
        private static void Shadowed(Rect r, string text, int size)
        {
            // Una cinta de tinta detrás del texto: sobre el amarillo de Salobre o el verde de la vida, sin ella no se lee.
            float w = GuiKit.Style(size, Palette.SaltWhite, TextAnchor.MiddleLeft, true).CalcSize(new GUIContent(text)).x;
            GuiKit.Fill(new Rect(r.x - 4, r.y + 1, Mathf.Min(r.width, w + 8), r.height - 2), Palette.WithAlpha(Palette.Abyss, 0.55f));
            GuiKit.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, size, Palette.WithAlpha(Palette.Ink, 0.95f), TextAnchor.MiddleLeft, true);
            GuiKit.Label(r, text, size, Palette.SaltWhite, TextAnchor.MiddleLeft, true);
        }

        private readonly List<Rect> _plates = new List<Rect>();

        /// <summary>
        /// Placas sobre la cabeza de cada enemigo, angostas, sin salir de la franja del escenario y sin encimarse:
        /// si dos chocan, la de atrás sube.
        /// </summary>
        private void DrawEnemyPlates()
        {
            if (_camera == null) return;
            _plates.Clear();
            var order = B.Enemies.Where(e => e.IsAlive && _d.Views.ContainsKey(e.Id)).OrderBy(e => e.Id).ToList();
            var heads = order.Select(e => GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(_d.Views[e.Id].Head + Vector3.up * 0.25f))).ToList();
            var resolved = BattleHudLayout.ResolveEnemyPlates(heads);
            foreach (var e in order)
            {
                var view = _d.Views[e.Id];
                var screen = _camera.WorldToScreenPoint(view.Head + Vector3.up * 0.25f);
                if (screen.z < 0) continue;
                var p = GuiKit.ScreenToCanvas(screen);
                var r = resolved[order.IndexOf(e)];
                _plates.Add(r);
                bool targeted = EnemyTarget == e.Id;
                InkPanel(r, EnemyAccent);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) _enemyTarget = e.Id;
                int number = B.Enemies.ToList().IndexOf(e) + 1;
                SideSeal(new Vector2(r.x + 18, r.y + 17), true, number.ToString(), 10);
                string role = L(e.IsBoss ? "battle.boss" : "battle.enemy") + " " + number;
                GuiKit.Label(new Rect(r.x + 38, r.y + 2, 120, 26), role, 17, EnemyAccent, TextAnchor.MiddleLeft, true);
                GuiKit.Label(new Rect(r.x + 144, r.y + 2, r.width - 152, 26), targeted ? L("battle.target") : e.IsCharging ? L("battle.charging") : "", 16, targeted ? Palette.Spark : EnemyAccent, TextAnchor.MiddleRight, true);
                GuiKit.Label(new Rect(r.x + 12, r.y + 27, r.width - 24, 30), e.Name, 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
                int hp = _d.Hp.TryGetValue(e.Id, out var h) ? h : e.Hp;
                double tough = _d.Toughness.TryGetValue(e.Id, out var t) ? t : e.Toughness;
                GuiKit.Label(new Rect(r.x + 12, r.y + 56, r.width - 24, 25), $"{L("battle.hp")} {hp:N0}/{e.MaxHp:N0}", 20, Palette.SaltWhite);
                GuiKit.Bar(new Rect(r.x + 12, r.y + 84, r.width - 24, 10), hp / (float)e.MaxHp, EnemyAccent, Palette.Hex("#34303C"));
                GuiKit.Label(new Rect(r.x + 12, r.y + 97, 106, 24), L(e.Broken ? "battle.broken" : "battle.toughness"), 17, Palette.ParchmentShadow);
                GuiKit.Bar(new Rect(r.x + 120, r.y + 107, r.width - 132, 6), (float)(tough / e.MaxToughness), Palette.SaltWhite, Palette.Hex("#34303C"));
                float pw = Mathf.Min(76, (r.width - 24 - (e.Def.Weaknesses.Count - 1) * 4) / Mathf.Max(1, e.Def.Weaknesses.Count));
                float x = r.x + 12;
                foreach (var weak in e.Def.Weaknesses) { GuiKit.PlanetPill(new Rect(x, r.y + 124, pw, 24), weak); x += pw + 4; }
                // Vincula placa y actor cuando la placa se desplaza para evitar otra.
                var head = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(view.Head));
                GuiKit.Line(new Vector2(r.center.x, r.yMax), new Vector2(head.x, Mathf.Max(r.yMax + 4, head.y)), 1, Palette.WithAlpha(EnemyAccent, 0.55f));
                if (targeted) TargetBrackets(new Rect(r.x - 4, r.y - 4, r.width + 8, r.height + 8));
            }
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

        private void DrawActions(Rect r)
        {
            InkPanel(r, Palette.Gold);
            var w = _d.ActiveWitch;
            if (B.IsOver) return;
            if (w == null || B.Pending != DecisionKind.AllyTurn)
            {
                string who = B.ActiveUnit != null ? B.ActiveUnit.Name : "";
                string text = B.Pending == DecisionKind.EnemyTurn ? "Turno de " + who + ".\nPuedes lanzar una Ascensión antes de que actúe."
                    : B.Pending == DecisionKind.AutoTurn ? "El Ascendente de " + who + " tiene el control." : "";
                GuiKit.Label(new Rect(r.x + 14, r.y + 14, r.width - 28, 120), text, 18, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
                return;
            }

            GuiKit.Label(new Rect(r.x + 14, r.y + 8, r.width - 28, 30), "Turno de " + w.Name + (B.IsExtraTurn ? " (Hilos)" : ""), 21, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var target = B.FindEnemy(EnemyTarget);
            GuiKit.Label(new Rect(r.x + 14, r.y + 42, r.width - 28, 28), L("battle.attackTarget") + " " + (target?.Name ?? "-"), 20, Palette.ParchmentShadow);

            bool ready = _d.AwaitingPlayer && !_chain.IsOpen;
            if (target != null)
                GuiKit.Label(new Rect(r.x + 14, r.y + 75, r.width - 28, 55), (target.IsCharging ? L("battle.charging") + ": " : L("battle.next") + ": ") + target.NextSkill.Name, 19, EnemyAccent, TextAnchor.UpperLeft, false, true);
            float y = r.y + 138;
            float bw = r.width - 28;

            if (GuiKit.Button(new Rect(r.x + 14, y, bw, 68), $"<b>Básico</b>: {w.Def.Basic.Name}\n<size=18>+1 Grano, −5 Salinidad</size>", ready, 17))
                _d.Submit(BattleCommand.Basic(w.Id, EnemyTarget));
            y += 76;

            var spell = w.Def.Spell;
            int spellTarget = spell.Target == TargetMode.Ally ? AllyTarget : EnemyTarget;
            bool canSpell = ready && w.CanCastMagic && B.Grains >= B.Rules.Grains.SpellCost;
            if (GuiKit.Button(new Rect(r.x + 14, y, bw, 142), $"<b>Hechizo</b>: {spell.Name}\n<size=18>−1 Grano, +10 Salinidad. {Short(spell.Description)}</size>", canSpell, 17))
                _d.Submit(BattleCommand.Spell(w.Id, spellTarget));
            y += 150;

            string selfText = SelfCastLabel(w);
            bool canSelf = ready && w.Def.SelfCast != SelfCastForm.None && w.CanCastMagic && !B.SelfCastUsedThisTurn;
            if (GuiKit.Button(new Rect(r.x + 14, y, bw / 2 - 4, 156), selfText, canSelf, 14))
                _d.Submit(BattleCommand.SelfCast(w.Id));
            bool canVis = ready && w.CanCastMagic && !w.VislumbreUsed && (B.BlackSalt >= B.Rules.Vislumbre.BlackSaltCost || w.Has(StatusKind.FreeVislumbre));
            if (GuiKit.Button(new Rect(r.x + 14 + bw / 2 + 4, y, bw / 2 - 4, 156), $"<b>Vislumbre</b>\n<size=17>1 Sal Negra, +30 Sal.\nGrado {w.Grade}→{PurityMath.Next(w.Grade)} 2 turnos</size>", canVis, 14))
                _d.Submit(BattleCommand.Vislumbre(w.Id));
            y += 164;

            bool canChain = ready && B.ChainBlocker().Length == 0;
            var chainRect = new Rect(r.x + 14, y, bw, Mathf.Min(70f, r.yMax - 10 - y));
            if (GuiKit.Button(chainRect, canChain ? "<b>Abrir Cadena</b>\n<size=18>−100 Resonancia · elige el orden</size>" : "Abrir Cadena\n<size=18>" + B.ChainBlocker() + "</size>", canChain, 20))
                _chain.Show(EnemyTarget);
            if (canChain) GuiKit.Frame(chainRect, Color.Lerp(Palette.GoldBright, Palette.Spark, Theme.ReduceMotion ? 0.5f : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)), 3);
        }

        private string SelfCastLabel(WitchUnit w)
        {
            var sc = B.Rules.SelfCast;
            switch (w.Def.SelfCast)
            {
                case SelfCastForm.AgujaYSello: return $"<b>{w.Def.SelfCastName}</b>\n<size=17>Aguja y Sello: +30 % ATQ 2 turnos. {sc.AgujaGrains} Granos, +25 Sal.</size>";
                case SelfCastForm.Hilos: return $"<b>{w.Def.SelfCastName}</b>\n<size=17>Hilos: actúa de nuevo. −10 % vida, +25 Sal.</size>";
                case SelfCastForm.FrenesiSalNegra: return $"<b>{w.Def.SelfCastName}</b>\n<size=17>Frenesí: +100 % daño, −50 % def. 1 Sal Negra, +40 Sal.</size>";
                default: return "Sin Autohechizo\n<size=17>(Aprendiza 3★)</size>";
            }
        }

        private static string Short(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length > 64 ? s.Substring(0, 62) + "…" : s);

        private void DrawChainPlayback()
        {
            if (_d.PlayingChain == null) return;
            var r = new Rect(560, 620, 1020, 160);
            GuiKit.PanelBox(r, 0.9f);
            GuiKit.Label(new Rect(r.x + 12, r.y + 4, 400, 22), "Partitura", 16, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            PartituraDrawer.Draw(new Rect(r.x + 12, r.y + 26, r.width - 24, r.height - 34), _d.PlayingChain,
                id => B.FindWitch(id)?.Dominant ?? Planet.Saturno, true, _d.PlayheadFrame, _d.PlayedNotes, B.Rules.Chain.SparkWindowFrames);
        }

        private void DrawBanner()
        {
            if (_d.BannerTime <= 0 || string.IsNullOrEmpty(_d.Banner)) return;
            float a = Mathf.Clamp01(_d.BannerTime * 2f);
            var r = new Rect(460, 340, 1000, 80);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.65f * a));
            GuiKit.Label(new Rect(r.x + 3, r.y + 3, r.width, r.height), _d.Banner, 46, Palette.WithAlpha(Color.black, a), TextAnchor.MiddleCenter, true);
            GuiKit.Label(r, _d.Banner, 46, Palette.WithAlpha(_d.BannerColor, a), TextAnchor.MiddleCenter, true);
        }

        private void DrawToast()
        {
            if (_d.ToastTime <= 0 || string.IsNullOrEmpty(_d.Toast)) return;
            var r = new Rect(560, 560, 1020, 50);
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Crimson, 0.9f));
            GuiKit.Label(r, _d.Toast, 20, Palette.SaltWhite, TextAnchor.MiddleCenter, true);
        }

        /// <summary>Pistas cortas para alguien que juega por primera vez. Cada una aparece mientras aplica.</summary>
        private void DrawHint(Rect r)
        {
            string hint = CurrentHint();
            if (string.IsNullOrEmpty(hint)) return;
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Parchment, 0.92f));
            GuiKit.Frame(r, Palette.Gold, 2);
            GuiKit.Label(new Rect(r.x + 14, r.y, r.width - 28, r.height), hint, 18, Palette.Ink, TextAnchor.MiddleLeft, false, true);
        }

        private string CurrentHint()
        {
            if (B.IsOver || _d.Auto) return "";
            var w = _d.ActiveWitch;
            var boss = B.Enemies.FirstOrDefault(e => e.IsAlive && e.IsCharging);
            if (boss != null)
                return $"{boss.Name} carga {boss.ChargedSkill.Name}. Quiébralo (barra blanca de Firmeza, usa sus planetas débiles) o cierra una Cadena con la Zarigüeya.";
            if (B.Pending == DecisionKind.AllyTurn && B.ChainBlocker().Length == 0)
                return "¡Resonancia llena! Abre una Cadena: el orden decide los Spark (notas doradas) y los Aspectos entre vecinas.";
            if (w != null && B.Band(w) >= SalinityBand.Sobresalada)
                return $"{w.Name} está {SalinityBands.DisplayName(B.Band(w))}. A 150 su Ascendente toma el control. Básico o Destilado de Rocío bajan la Salinidad.";
            if (B.Witches.Any(x => B.AscensionBlocker(x, 1).Length == 0) && !_hintsSeen.Contains("asc"))
                return "Un anillo de Ánima llegó a 100: la Ascensión se lanza cuando quieras, incluso antes del turno enemigo.";
            if (B.TurnNumber <= 3)
                return "El básico da Granos de Sal; el Hechizo gasta uno y es más fuerte. Los planetas débiles del enemigo bajan su Firmeza.";
            return "";
        }

        /// <summary>Toque sobre una cápsula enemiga: la elige como objetivo.</summary>
        public void HandleWorldClick(Vector2 screenPos)
        {
            if (_camera == null || InputBlocked) return;
            var point = GuiKit.ScreenToCanvas(new Vector3(screenPos.x, screenPos.y, 0));
            if (point.x >= 1590 || point.y >= 800 || point.y < 230 || _plates.Any(r => r.Contains(point)) || LogOpen && point.x < 550) return;
            UnitView view = null;
            var ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
            if (Physics.Raycast(ray, out var hit, 100f)) view = hit.collider.GetComponentInParent<UnitView>();
            // Los recortes no llevan collider: se elige la figura viva cuya silueta (pies a cabeza) contiene el toque.
            if (view == null)
            {
                float best = float.MaxValue;
                foreach (var v in _d.Views.Values)
                {
                    if (v == null || v.IsDead) continue;
                    var feet = _camera.WorldToScreenPoint(v.transform.position);
                    var head = _camera.WorldToScreenPoint(v.Head);
                    if (feet.z < 0) continue;
                    float halfW = (head.y - feet.y) * 0.3f;
                    if (screenPos.y < feet.y || screenPos.y > head.y || Mathf.Abs(screenPos.x - feet.x) > halfW) continue;
                    if (feet.z < best) { best = feet.z; view = v; }
                }
            }
            if (view == null) return;
            if (view.IsEnemy) _enemyTarget = view.UnitId;
            else _allyTarget = view.UnitId;
        }
    }
}
