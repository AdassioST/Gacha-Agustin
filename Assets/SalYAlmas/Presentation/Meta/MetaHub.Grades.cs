using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Progress;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private string _witch = "";
        private int _witchPage;
        private readonly List<string> _allies = new List<string>();
        private Via _trialVia = Via.Mayor;
        private TrialResult _trialResult;
        private string _trialTitle = "";

        private void DrawGrades(Rect a)
        {
            var owned = _meta.RosterOrder.Where(id => Game.Profile.Owns(id)).ToList();
            if (!owned.Contains(_witch)) _witch = owned.FirstOrDefault() ?? "";

            var list = new Rect(a.x, a.y, 400, a.height);
            GuiKit.PanelBox(list);
            _witchPage = Paged(new Rect(list.x + 10, list.y + 10, list.width - 20, list.height - 20), owned, _witchPage, 56, (r, id) =>
            {
                var w = Game.Profile.Witches[id];
                string kit = _meta.HasKit(id) ? "" : "  (sin kit)";
                if (GuiKit.Button(r, $"{WitchName(id)}{kit}\n<size=13>{Stars(_meta.Roster[id].Rarity)} · nivel {w.Level} · grado {w.Grade}</size>", true, 17)) { _witch = id; _trialResult = null; _allies.Clear(); }
                if (id == _witch) GuiKit.Frame(r, Palette.GoldBright, 3f);
            });
            if (_witch.Length == 0) return;

            DrawWitchSheet(new Rect(list.xMax + 10, a.y, 720, a.height));
            DrawTrialPanel(new Rect(list.xMax + 740, a.y, a.xMax - list.xMax - 740, a.height));
        }

        private void DrawWitchSheet(Rect r)
        {
            GuiKit.PanelBox(r);
            var entry = _meta.Roster[_witch];
            var w = Game.Profile.Witches[_witch];
            float x = r.x + 16, width = r.width - 32, y = r.y + 8;
            GuiKit.Label(new Rect(x, y, width, 40), entry.Name, 30, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 42;
            GuiKit.Label(new Rect(x, y, width, 28), $"{Stars(entry.Rarity)}  ·  {entry.Species}  ·  {entry.Role}", 18, Palette.Parchment);
            y += 32;
            GuiKit.PlanetPill(new Rect(x, y, 170, 28), entry.Primary);
            GuiKit.PlanetPill(new Rect(x + 178, y, 170, 28), entry.Secondary);
            GuiKit.Label(new Rect(x + 360, y, width - 360, 28), w.ViaChosen ? $"Vía {w.Via}" : "Sin Vía (se elige en B)", 18, Palette.Parchment);
            y += 38;

            DrawPhases(new Rect(x, y, width, 200), w.Grade);
            y += 208;

            GuiKit.Label(new Rect(x, y, width, 30), $"Nivel {w.Level} de {_meta.Progression.Level.Max}", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 32;
            int[] steps = { 1, 10 };
            for (int i = 0; i < steps.Length; i++)
                if (GuiKit.Button(new Rect(x + i * 210, y, 204, 46), $"+{steps[i]}  ·  {Game.Progress.LevelUpCost(_witch, steps[i]):N0}", w.Level < _meta.Progression.Level.Max, 17))
                    Apply(Game.Progress.LevelUp(_witch, steps[i]));
            if (GuiKit.Button(new Rect(x + 420, y, 204, 46), "Todo lo que alcance", w.Level < _meta.Progression.Level.Max, 17)) Apply(Game.Progress.LevelUpMax(_witch));
            y += 54;

            int d = w.Dignity;
            var dignity = _meta.Progression.Dignity;
            string dLabel = d < dignity.Max ? $"Dignidad: {dignity.Names[d]} → {dignity.Names[d + 1]}  ·  {dignity.Costs[d + 1]:N0}" : $"Dignidad: {dignity.Names[d]}";
            if (GuiKit.Button(new Rect(x, y, width, 44), dLabel, d < dignity.Max, 18)) Apply(Game.Progress.DignityUp(_witch));
            y += 50;

            foreach (ChainRole role in new[] { ChainRole.Apertura, ChainRole.Enlace, ChainRole.Cierre })
            {
                int level = w.ChainTalents[(int)role];
                string label = level < _meta.Progression.ChainTalents.Max
                    ? $"{role} {level}  ·  {Game.Progress.ChainTalentCost(_witch, role).Describe(_meta.Catalog)}"
                    : $"{role} {level} (máximo)";
                if (GuiKit.Button(new Rect(x, y, width, 38), label, level < _meta.Progression.ChainTalents.Max, 15)) Apply(Game.Progress.ChainTalentUp(_witch, role));
                y += 42;
            }

            if (w.ViaChosen)
            {
                var other = w.Via == Via.Mayor ? Via.Menor : Via.Mayor;
                long cost = Game.Progress.ViaChangeCost(_witch);
                if (GuiKit.Button(new Rect(x, y, width, 40), $"Cambiar a la Vía {other}  ·  {(cost == 0 ? "gratis" : cost.ToString("N0"))}", true, 17)) Apply(Game.Progress.ChangeVia(_witch, other));
                y += 46;
            }

            var weapon = w.WeaponUid != 0 ? Game.Profile.Weapon(w.WeaponUid) : null;
            GuiKit.Label(new Rect(x, y, width, 30), "Arma: " + (weapon != null ? Game.Forge.DisplayName(weapon) : "ninguna (equípala en el Yunque)"), 18, Palette.Parchment);
        }

        /// <summary>Vista previa de las tres fases visuales (GDD 12.5) con siluetas de prueba, sin arte.</summary>
        private void DrawPhases(Rect r, Grade grade)
        {
            string[] names = { "Turbia", "Destilada", "Quintaesencia" };
            string[] grades = { "D y C", "B y A", "S y SS" };
            Color[] colors = { Palette.Hex("#6B6358"), Palette.Hex("#6E5A9C"), Palette.Hex("#D5A44E") };
            int current = grade <= Grade.C ? 0 : grade <= Grade.A ? 1 : 2;
            float w = (r.width - 16) / 3f;
            for (int i = 0; i < 3; i++)
            {
                var box = new Rect(r.x + i * (w + 8), r.y, w, r.height);
                bool unlocked = i <= current;
                GuiKit.Fill(box, Palette.WithAlpha(Palette.Abyss, 0.7f));
                Color c = unlocked ? colors[i] : Palette.WithAlpha(colors[i], 0.2f);
                var center = new Vector2(box.center.x, box.y + 62);
                GuiKit.Dot(center, 26, c);
                GuiKit.Fill(new Rect(center.x - 24, center.y + 22, 48, 74), c);
                GuiKit.Label(new Rect(box.x, box.yMax - 54, box.width, 26), names[i], 18, unlocked ? Palette.Parchment : Palette.ParchmentShadow, TextAnchor.MiddleCenter, i == current);
                GuiKit.Label(new Rect(box.x, box.yMax - 28, box.width, 24), unlocked ? grades[i] : grades[i] + " · bloqueada", 14, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
                if (i == current) GuiKit.Frame(box, Palette.GoldBright, 2f);
            }
        }

        private void DrawTrialPanel(Rect r)
        {
            GuiKit.PanelBox(r);
            var w = Game.Profile.Witches[_witch];
            float x = r.x + 16, width = r.width - 32, y = r.y + 8;
            var req = Game.Progress.Requirement(_witch);
            if (req.IsMax)
            {
                GuiKit.Label(new Rect(x, y, width, 40), "Grado SS: Apoteosis", 28, Palette.FiveStar, TextAnchor.MiddleLeft, true);
                DrawMemories(new Rect(x, y + 50, width, r.yMax - y - 60), w);
                return;
            }
            GuiKit.Label(new Rect(x, y, width, 40), $"Juicio de Pureza: {w.Grade} → {req.Step.Grade}", 28, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 44;
            GuiKit.Label(new Rect(x, y, width, 54), req.Step.Unlock, 17, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            y += 58;
            GuiKit.Label(new Rect(x, y, width, 28), $"Nivel mínimo {req.Step.MinLevel}" + (req.LevelOk ? "" : $" (tiene {w.Level})"), 18, req.LevelOk ? Palette.Parchment : Palette.Danger);
            y += 30;
            GuiKit.Label(new Rect(x, y, width, 52), "Se cobra solo si ganas: " + req.Cost.Describe(_meta.Catalog), 17, req.CanAfford ? Palette.Parchment : Palette.Danger, TextAnchor.UpperLeft, false, true);
            y += 56;
            GuiKit.Label(new Rect(x, y, width, 26), "Condición LitRPG", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 28;
            if (req.Conditions.Count == 0)
            {
                GuiKit.Label(new Rect(x, y, width, 26), "Ninguna: solo gana el combate.", 17, Palette.Parchment);
                y += 28;
            }
            string shortName = _meta.HasKit(_witch) ? Game.Combat.Witch(_witch).ShortName : entryName();
            foreach (var c in req.Conditions)
            {
                GuiKit.Label(new Rect(x, y, width, 26), "· " + c.TextFor(shortName), 17, Palette.Parchment);
                y += 28;
            }
            y += 6;
            if (req.Step.Grade == Grade.B && !w.ViaChosen)
            {
                GuiKit.Label(new Rect(x, y, 130, 40), "Elige Vía:", 18, Palette.Parchment);
                foreach (var via in new[] { Via.Mayor, Via.Menor })
                    if (GuiKit.Button(new Rect(x + 130 + (int)via * 160, y, 152, 40), (_trialVia == via ? "● " : "") + "Vía " + via, true, 16)) _trialVia = via;
                y += 46;
            }

            GuiKit.Label(new Rect(x, y, width, 26), $"Aliadas ({_allies.Count}/{_meta.Progression.Trial.MaxAllies})", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 28;
            int k = 0;
            foreach (var id in _meta.RosterOrder)
            {
                if (id == _witch || !Game.Profile.Owns(id) || !_meta.HasKit(id)) continue;
                bool on = _allies.Contains(id);
                var box = new Rect(x + (k % 2) * (width / 2), y + (k / 2) * 44, width / 2 - 6, 40);
                if (GuiKit.Button(box, $"{(on ? "● " : "")}{Game.Combat.Witch(id).ShortName} {Stars(_meta.Roster[id].Rarity)}", on || _allies.Count < _meta.Progression.Trial.MaxAllies, 16))
                {
                    if (on) _allies.Remove(id);
                    else _allies.Add(id);
                }
                k++;
            }
            y += ((k + 1) / 2) * 44 + 6;

            bool can = _meta.HasKit(_witch) && req.LevelOk && req.CanAfford;
            if (GuiKit.Button(new Rect(x, y, width, 62), _meta.HasKit(_witch) ? "Entrar al Juicio\n<size=14>reintentos ilimitados y sin costo</size>" : "Su kit llega en la Fase 3", can, 22))
            {
                var seed = (ulong)(System.DateTime.Now.Ticks % 1000000007L);
                var ok = Game.Trials.Prepare(_witch, _allies, seed, out var plan, _trialVia);
                Note(ok.Message, ok.Ok);
                if (ok.Ok)
                {
                    _trialResult = null;
                    _trialTitle = plan.Title;
                    StartBattle?.Invoke(plan.Setup, b =>
                    {
                        _trialResult = Game.Trials.Resolve(plan, b);
                        Note(_trialResult.GradeUp.Message, _trialResult.GradeUp.Ok);
                        Save();
                    });
                }
            }
            y += 70;

            if (_trialResult != null)
            {
                string text = $"<b>{_trialTitle}: {(_trialResult.Passed ? "superado" : _trialResult.Victory ? "ganaste, pero faltó la condición" : "perdiste")}</b>\n";
                foreach (var c in _trialResult.Conditions) text += $"{(c.Passed ? "✓" : "✗")} {c.Text} ({c.Detail})\n";
                if (_trialResult.Memory.Length > 0) text += "<i>" + _trialResult.Memory + "</i>";
                GuiKit.Label(new Rect(x, y, width, r.yMax - y - 8), text, 16, _trialResult.Passed ? Palette.FiveStar : Palette.Parchment, TextAnchor.UpperLeft, false, true);
            }
            else DrawMemories(new Rect(x, y, width, r.yMax - y - 8), w);

            string entryName() => _meta.Roster[_witch].Name;
        }

        private static void DrawMemories(Rect r, Core.Save.WitchProgress w)
        {
            if (w.Memories.Count == 0) return;
            GuiKit.Label(r, "<b>Recuerdos</b>\n" + string.Join("\n", w.Memories), 15, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
        }
    }
}
