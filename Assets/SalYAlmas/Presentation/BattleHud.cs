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
            DrawFloatingTexts();
            DrawEnemyPlates();
            DrawTurnLine(new Rect(16, 12, 1180, 86));
            DrawTopRight(new Rect(1560, 12, 344, 86));
            DrawResonance(new Vector2(1290, 56));
            DrawLog(new Rect(16, 112, 520, 600));
            DrawGrainsAndBelt(new Rect(1600, 112, 304, 410));
            for (int i = 0; i < B.Witches.Count; i++) DrawWitchPanel(B.Witches[i], new Rect(16 + i * 392, 790, 380, 278));
            DrawActions(new Rect(1600, 640, 304, 428));
            DrawChainPlayback();
            DrawBanner();
            DrawToast();
            if (!_chain.IsOpen) DrawHint(new Rect(560, 112, 1020, 46));
        }

        private void DrawTurnLine(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 10, r.y + 2, 300, 22), "Línea de turnos (AV)", 16, Palette.GoldBright);
            var order = B.IsOver ? new List<TurnOrderEntry>() : B.TurnOrderPreview(8);
            float x = r.x + 10;
            for (int i = 0; i < order.Count; i++)
            {
                var u = order[i].Unit;
                var chip = new Rect(x, r.y + 28, 140, 50);
                bool active = i == 0 && u == B.ActiveUnit;
                GuiKit.Fill(chip, Palette.WithAlpha(u.Side == Side.Ally ? Palette.Abyss : Palette.Crimson, 0.85f));
                Color stripe = u is WitchUnit w ? Palette.Planet(w.Dominant) : Palette.Danger;
                GuiKit.Fill(new Rect(chip.x, chip.y, 6, chip.height), stripe);
                GuiKit.Label(new Rect(chip.x + 12, chip.y + 3, chip.width - 14, 24), u.Name, 18, Palette.Parchment, TextAnchor.MiddleLeft, true);
                GuiKit.Label(new Rect(chip.x + 12, chip.y + 26, chip.width - 14, 20), active ? "ahora" : "+" + order[i].AvFromNow.ToString("0"), 15, Palette.ParchmentShadow);
                if (active) GuiKit.Frame(chip, Palette.GoldBright, 2);
                x += 146;
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
            Color on = full ? Color.Lerp(Palette.GoldBright, Palette.Spark, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)) : Palette.Gold;
            GuiKit.Dot(c, 46, Palette.WithAlpha(Palette.Panel, 0.92f));
            GuiKit.Ring(c, 50, frac, on, Palette.WithAlpha(Palette.ParchmentShadow, 0.25f), 40, 4.5f);
            GuiKit.Label(new Rect(c.x - 50, c.y - 22, 100, 30), _d.Resonance.ToString(), 28, full ? Palette.Spark : Palette.Parchment, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(c.x - 60, c.y + 6, 120, 20), "Resonancia", 14, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
        }

        private void DrawLog(Rect r)
        {
            GuiKit.PanelBox(r);
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
            for (int i = 0; i < B.Rules.Grains.Max; i++)
            {
                var c = new Vector2(r.x + 34 + i * 56, r.y + 62);
                bool on = i < _d.Grains;
                GuiKit.Dot(c, 22, Palette.WithAlpha(Palette.Abyss, 0.9f));
                GuiKit.Dot(c, 18, on ? Palette.SaltWhite : Palette.WithAlpha(Palette.ParchmentShadow, 0.18f));
            }
            string boost = B.GrainBoostActive ? "  · Excavación: ×2" : "";
            GuiKit.Label(new Rect(r.x + 12, r.y + 92, r.width - 20, 24), $"{_d.Grains}/{B.Rules.Grains.Max}{boost}", 17, Palette.Parchment);
            GuiKit.Label(new Rect(r.x + 12, r.y + 116, r.width - 20, 24), "Sal Negra: " + B.BlackSalt, 17, Palette.Band(SalinityBand.ExtasisAbisal));

            GuiKit.Label(new Rect(r.x + 12, r.y + 150, 280, 24), "Cinturón (1 por turno)", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            bool canUse = _d.AwaitingPlayer && !B.PotionUsedThisTurn;
            for (int i = 0; i < B.Rules.Potions.BeltSlots; i++)
            {
                var slot = new Rect(r.x + 12, r.y + 180 + i * 74, r.width - 24, 68);
                if (i >= B.Belt.Count)
                {
                    GuiKit.Fill(slot, Palette.WithAlpha(Palette.Abyss, 0.5f));
                    GuiKit.Label(slot, "vacía", 16, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
                    continue;
                }
                var belt = B.Belt[i];
                var def = B.Db.Potion(belt.PotionId);
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
            GuiKit.PanelBox(r, w.IsAlive ? 0.9f : 0.5f);
            if (!InputBlocked && Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition) && w.IsAlive) _allyTarget = w.Id;

            int hp = _d.Hp.TryGetValue(w.Id, out var h) ? h : w.Hp;
            int sal = _d.Salinity.TryGetValue(w.Id, out var s) ? s : w.Salinity;
            int anima = _d.Anima.TryGetValue(w.Id, out var a) ? a : w.Anima;
            var band = SalinityBands.FromValue(sal, B.Rules.Salinity);

            GuiKit.Fill(new Rect(r.x, r.y, r.width, 6), Palette.Planet(w.Dominant));
            GuiKit.Label(new Rect(r.x + 12, r.y + 10, 240, 32), w.Name, 26, Palette.Parchment, TextAnchor.MiddleLeft, true);
            string rank = w.Def.Rarity == 5 ? "Regente" : w.Def.Rarity == 4 ? "Oficiala" : "Aprendiza";
            GuiKit.Label(new Rect(r.x + 12, r.y + 40, 240, 20), $"{rank} · grado {w.EffectiveGrade} · {w.Def.Role}", 14, Palette.ParchmentShadow);
            double p = B.Purity(w);
            GuiKit.PlanetPill(new Rect(r.x + 12, r.y + 64, 128, 24), w.Dominant, true);
            GuiKit.Label(new Rect(r.x + 144, r.y + 64, 50, 24), (p * 100).ToString("0") + "%", 15, Palette.Parchment);
            GuiKit.PlanetPill(new Rect(r.x + 190, r.y + 64, 110, 24), w.Recessive);

            // Vida
            GuiKit.Bar(new Rect(r.x + 12, r.y + 98, 268, 18), hp / (float)w.MaxHp, Color.Lerp(Palette.Danger, Palette.Planet(Planet.Venus), hp / (float)w.MaxHp), Palette.WithAlpha(Palette.Abyss, 0.9f));
            if (w.ShieldAmount > 0) GuiKit.Fill(new Rect(r.x + 12, r.y + 116, 268 * Mathf.Clamp01((float)(w.ShieldAmount / w.MaxHp)), 4), Palette.Planet(Planet.Luna));
            GuiKit.Label(new Rect(r.x + 16, r.y + 98, 260, 18), $"Vida {hp:N0}/{w.MaxHp:N0}", 13, Palette.SaltWhite);

            // Salinidad coloreada por banda, con marcas en 40, 70, 100 y 150.
            var salRect = new Rect(r.x + 12, r.y + 126, 268, 18);
            GuiKit.Bar(salRect, sal / (float)B.Rules.Salinity.Max, Palette.Band(band), Palette.WithAlpha(Palette.Abyss, 0.9f));
            foreach (int mark in new[] { B.Rules.Salinity.SalobreAt, B.Rules.Salinity.SobresaladaAt, B.Rules.Salinity.CristalizacionAt })
                GuiKit.Fill(new Rect(salRect.x + salRect.width * mark / B.Rules.Salinity.Max, salRect.y, 2, salRect.height), Palette.WithAlpha(Palette.Ink, 0.8f));
            GuiKit.Label(new Rect(r.x + 12, r.y + 146, 268, 20), $"Salinidad {sal}/150 · {SalinityBands.DisplayName(band)}", 14, Palette.Band(band));
            GuiKit.Label(new Rect(r.x + 12, r.y + 166, 268, 18), SalinityBands.EffectSummary(band), 12, Palette.ParchmentShadow);

            // Ánima: anillo de 0 a 300 con tres tramos.
            var center = new Vector2(r.xMax - 50, r.y + 132);
            GuiKit.Dot(center, 36, Palette.WithAlpha(Palette.Abyss, 0.9f));
            for (int tier = 0; tier < 3; tier++)
            {
                float frac = Mathf.Clamp01((anima - tier * 100) / 100f);
                Color c = tier == 0 ? Palette.Gold : tier == 1 ? Palette.GoldBright : Palette.Spark;
                GuiKit.Ring(center, 30 + tier * 7, frac, c, Palette.WithAlpha(Palette.ParchmentShadow, 0.15f), 24, 2.4f);
            }
            GuiKit.Label(new Rect(center.x - 30, center.y - 12, 60, 24), anima.ToString(), 20, Palette.Parchment, TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(center.x - 40, center.y + 52, 80, 16), "Ánima", 13, Palette.ParchmentShadow, TextAnchor.MiddleCenter);

            // Estados
            var tags = new List<string>();
            if (w.InEcstasy) tags.Add("<color=#D5A4F4><b>ÉXTASIS ABISAL</b></color>");
            if (w.IsDesalada) tags.Add("<b>Desalada</b>");
            foreach (var st in w.Statuses)
            {
                if (st.Kind == StatusKind.Ecstasy || st.Kind == StatusKind.Desalada || st.Name.StartsWith("Regencia")) continue;
                tags.Add(st.TurnsLeft > 0 ? $"{st.Name} ({st.TurnsLeft})" : st.Name);
            }
            GuiKit.Label(new Rect(r.x + 12, r.y + 188, r.width - 24, 36), string.Join(" · ", tags.Distinct()), 13, Palette.Parchment, TextAnchor.UpperLeft, false, true);

            // Ascensiones: se lanzan en cualquier momento e interrumpen el orden.
            bool window = !_d.IsPlaying && !_d.Auto && !_chain.IsOpen;
            for (int lvl = 1; lvl <= 3; lvl++)
            {
                var b = new Rect(r.x + 12 + (lvl - 1) * 122, r.y + 230, 116, 40);
                bool can = window && B.AscensionBlocker(w, lvl).Length == 0;
                string label = "Asc. " + (lvl == 1 ? "I" : lvl == 2 ? "II" : "III");
                if (GuiKit.Button(b, can ? "<b>" + label + "</b>" : label, can, 18))
                {
                    var move = w.Def.Ascensions[lvl - 1];
                    int target = move.Target == TargetMode.Ally ? AllyTarget : EnemyTarget;
                    _d.Submit(BattleCommand.Ascension(w.Id, lvl, target));
                }
                if (can) GuiKit.Frame(b, Palette.FiveStar, 2);
            }

            if (active) GuiKit.Frame(r, Palette.GoldBright, 4);
            else if (selected) GuiKit.Frame(r, Palette.Planet(Planet.Venus), 3);
            if (active) GuiKit.Label(new Rect(r.xMax - 130, r.y + 10, 118, 20), "EN TURNO", 15, Palette.GoldBright, TextAnchor.MiddleRight, true);
            if (!w.IsAlive) GuiKit.Label(r, "CAÍDA", 40, Palette.Danger, TextAnchor.MiddleCenter, true);
        }

        private void DrawEnemyPlates()
        {
            if (_camera == null) return;
            foreach (var e in B.Enemies)
            {
                if (!_d.Views.TryGetValue(e.Id, out var view)) continue;
                var screen = _camera.WorldToScreenPoint(view.Head + Vector3.up * 0.4f);
                if (screen.z < 0) continue;
                var p = GuiKit.ScreenToCanvas(screen);
                float w = e.IsBoss ? 380 : 260;
                var r = new Rect(p.x - w / 2, p.y - 150, w, 146);
                if (!e.IsAlive)
                {
                    GuiKit.Label(new Rect(r.x, r.yMax - 30, r.width, 30), e.Name + " · se deshizo en sal", 16, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
                    continue;
                }
                bool targeted = EnemyTarget == e.Id;
                GuiKit.PanelBox(r, 0.85f);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) _enemyTarget = e.Id;

                int hp = _d.Hp.TryGetValue(e.Id, out var h) ? h : e.Hp;
                double tough = _d.Toughness.TryGetValue(e.Id, out var t) ? t : e.Toughness;
                GuiKit.Label(new Rect(r.x + 10, r.y + 4, r.width - 20, 26), e.Name + (e.IsBoss ? "  · JEFE" : ""), 20, Palette.Parchment, TextAnchor.MiddleLeft, true);
                GuiKit.Bar(new Rect(r.x + 10, r.y + 34, r.width - 20, 16), hp / (float)e.MaxHp, Palette.Crimson, Palette.WithAlpha(Palette.Abyss, 0.9f));
                GuiKit.Label(new Rect(r.x + 14, r.y + 34, r.width - 28, 16), $"{hp:N0}/{e.MaxHp:N0}", 12, Palette.SaltWhite);
                GuiKit.Bar(new Rect(r.x + 10, r.y + 54, r.width - 20, 8), (float)(tough / e.MaxToughness), Palette.SaltWhite, Palette.WithAlpha(Palette.Abyss, 0.9f));
                GuiKit.Label(new Rect(r.x + 10, r.y + 62, 120, 16), e.Broken ? "<b>QUEBRADO</b>" : "Firmeza " + tough.ToString("0"), 12, e.Broken ? Palette.FiveStar : Palette.ParchmentShadow);

                float px = r.x + 10;
                foreach (var weak in e.Def.Weaknesses)
                {
                    GuiKit.PlanetPill(new Rect(px, r.y + 82, 100, 22), weak);
                    px += 104;
                }
                string intent;
                Color intentColor = Palette.ParchmentShadow;
                if (e.IsCharging)
                {
                    intent = "¡Carga " + e.ChargedSkill.Name + "! Quiébralo para cancelarlo";
                    intentColor = Palette.Danger;
                }
                else intent = "Siguiente: " + e.NextSkill.Name;
                var buffs = e.Statuses.Where(s => s.IsBuff).Select(s => s.Name).Distinct().ToList();
                if (buffs.Count > 0) intent += "  · " + string.Join(", ", buffs);
                GuiKit.Label(new Rect(r.x + 10, r.y + 108, r.width - 20, 34), intent, 14, intentColor, TextAnchor.UpperLeft, e.IsCharging, true);
                if (targeted) GuiKit.Frame(r, Palette.GoldBright, 3);
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
            GuiKit.PanelBox(r);
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
            GuiKit.Label(new Rect(r.x + 14, r.y + 36, r.width - 28, 20), "Objetivo: " + (target?.Name ?? "-") + " (toca su placa)", 14, Palette.ParchmentShadow);

            bool ready = _d.AwaitingPlayer && !_chain.IsOpen;
            float y = r.y + 62;
            float bw = r.width - 28;

            if (GuiKit.Button(new Rect(r.x + 14, y, bw, 52), $"<b>Básico</b>: {w.Def.Basic.Name}\n<size=13>+1 Grano, −5 Salinidad</size>", ready, 17))
                _d.Submit(BattleCommand.Basic(w.Id, EnemyTarget));
            y += 58;

            var spell = w.Def.Spell;
            int spellTarget = spell.Target == TargetMode.Ally ? AllyTarget : EnemyTarget;
            bool canSpell = ready && w.CanCastMagic && B.Grains >= B.Rules.Grains.SpellCost;
            if (GuiKit.Button(new Rect(r.x + 14, y, bw, 60), $"<b>Hechizo</b>: {spell.Name}\n<size=13>−1 Grano, +10 Salinidad. {Short(spell.Description)}</size>", canSpell, 17))
                _d.Submit(BattleCommand.Spell(w.Id, spellTarget));
            y += 66;

            string selfText = SelfCastLabel(w);
            bool canSelf = ready && w.Def.SelfCast != SelfCastForm.None && w.CanCastMagic && !B.SelfCastUsedThisTurn;
            if (GuiKit.Button(new Rect(r.x + 14, y, bw / 2 - 4, 66), selfText, canSelf, 14))
                _d.Submit(BattleCommand.SelfCast(w.Id));
            bool canVis = ready && w.CanCastMagic && !w.VislumbreUsed && (B.BlackSalt >= B.Rules.Vislumbre.BlackSaltCost || w.Has(StatusKind.FreeVislumbre));
            if (GuiKit.Button(new Rect(r.x + 14 + bw / 2 + 4, y, bw / 2 - 4, 66), $"<b>Vislumbre</b>\n<size=12>1 Sal Negra, +30 Sal.\nGrado {w.Grade}→{PurityMath.Next(w.Grade)} 2 turnos</size>", canVis, 14))
                _d.Submit(BattleCommand.Vislumbre(w.Id));
            y += 72;

            bool canChain = ready && B.ChainBlocker().Length == 0;
            var chainRect = new Rect(r.x + 14, y, bw, 70);
            if (GuiKit.Button(chainRect, canChain ? "<b>Abrir Cadena</b>\n<size=13>−100 Resonancia · elige el orden</size>" : "Abrir Cadena\n<size=13>" + B.ChainBlocker() + "</size>", canChain, 20))
                _chain.Show(EnemyTarget);
            if (canChain) GuiKit.Frame(chainRect, Color.Lerp(Palette.GoldBright, Palette.Spark, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)), 3);
        }

        private string SelfCastLabel(WitchUnit w)
        {
            var sc = B.Rules.SelfCast;
            switch (w.Def.SelfCast)
            {
                case SelfCastForm.AgujaYSello: return $"<b>{w.Def.SelfCastName}</b>\n<size=12>Aguja y Sello: +30 % ATQ 2 turnos. {sc.AgujaGrains} Granos, +25 Sal.</size>";
                case SelfCastForm.Hilos: return $"<b>{w.Def.SelfCastName}</b>\n<size=12>Hilos: actúa de nuevo. −10 % vida, +25 Sal.</size>";
                case SelfCastForm.FrenesiSalNegra: return $"<b>{w.Def.SelfCastName}</b>\n<size=12>Frenesí: +100 % daño, −50 % def. 1 Sal Negra, +40 Sal.</size>";
                default: return "Sin Autohechizo\n<size=12>(Aprendiza 3★)</size>";
            }
        }

        private static string Short(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length > 70 ? s.Substring(0, 68) + "…" : s);

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
            if (_camera == null) return;
            var ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
            if (!Physics.Raycast(ray, out var hit, 100f)) return;
            var view = hit.collider.GetComponentInParent<UnitView>();
            if (view == null) return;
            if (view.IsEnemy) _enemyTarget = view.UnitId;
            else _allyTarget = view.UnitId;
        }
    }
}
