using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private readonly List<string> _team = new List<string>();
        private readonly List<string> _belt = new List<string>();
        private int _ecoChoice = -1;
        private readonly HashSet<string> _curses = new HashSet<string>();
        private readonly HashSet<string> _ecoGrafts = new HashSet<string>();
        private int _ecoTrained = -1;
        private int _logScroll;

        private FresnoService F => Game.Fresno;

        private void DrawFresno(Rect a)
        {
            if (F.InRun) DrawRun(a);
            else DrawFresnoHome(a);
        }

        // ------------------------------------------------------------------ Antes de la run

        private void DrawFresnoHome(Rect a)
        {
            var left = new Rect(a.x, a.y, 640, a.height);
            GuiKit.PanelBox(left);
            GuiKit.Label(new Rect(left.x + 16, left.y + 8, 600, 40), "Elige de 1 a 4 brujas", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float y = left.y + 54;
            foreach (var id in _meta.RosterOrder)
            {
                if (!Game.Profile.Owns(id) || !_meta.HasKit(id)) continue;
                var w = Game.Profile.Witches[id];
                var r = new Rect(left.x + 16, y, 608, 54);
                bool on = _team.Contains(id);
                if (GuiKit.Button(r, $"{(on ? "● " : "")}{WitchName(id)}  {Stars(_meta.Roster[id].Rarity)}  ·  nivel {w.Level}  ·  grado {w.Grade}", true, 20))
                {
                    if (on) _team.Remove(id);
                    else if (_team.Count < 4) _team.Add(id);
                }
                y += 58;
            }

            y += 8;
            GuiKit.Label(new Rect(left.x + 16, y, 600, 34), "Eco para empezar", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 38;
            var ecos = Game.Profile.Fresno.Eco;
            if (GuiKit.Button(new Rect(left.x + 16, y, 120, 50), (_ecoChoice < 0 ? "● " : "") + "Sin Eco", true, 18)) _ecoChoice = -1;
            for (int i = 0; i < 3; i++)
            {
                var eco = i < ecos.Count ? ecos[i] : null;
                bool unlocked = F.EcoSlotUnlocked(i);
                string label = !unlocked ? $"Ranura {i + 1}\n<size=13>bloqueada</size>" : eco == null ? $"Ranura {i + 1}\n<size=13>vacía</size>" : $"{(_ecoChoice == i ? "● " : "")}{eco.Name}\n<size=13>{eco.Grafts.Count} Injertos</size>";
                if (GuiKit.Button(new Rect(left.x + 142 + i * 162, y, 156, 50), label, eco != null, 16)) _ecoChoice = i;
            }
            y += 60;

            GuiKit.Label(new Rect(left.x + 16, y, 600, 34), $"Salmueras para el cinturón ({_belt.Count}/{F.BeltSlots})", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 38;
            var potions = Game.Profile.Potions.Keys.OrderBy(k => k, StringComparer.Ordinal).Take(6).ToList();
            if (potions.Count == 0) GuiKit.Label(new Rect(left.x + 16, y, 600, 30), "No tienes Salmueras: prepáralas en el Caldero.", 17, Palette.ParchmentShadow);
            for (int i = 0; i < potions.Count; i++)
            {
                string p = potions[i];
                bool on = _belt.Contains(p);
                var r = new Rect(left.x + 16 + (i % 2) * 306, y + (i / 2) * 44, 300, 40);
                if (GuiKit.Button(r, $"{(on ? "● " : "")}{Cauldron.NameOf(_meta, p)} ×{Game.Profile.Potions[p]}", true, 15))
                {
                    if (on) _belt.Remove(p);
                    else if (_belt.Count < F.BeltSlots) _belt.Add(p);
                }
            }

            string why = F.CanStart(_team);
            if (GuiKit.Button(new Rect(left.x + 16, left.yMax - 74, 608, 62), "Escalar el Fresno", why.Length == 0, 28))
            {
                var r = F.StartRun(_team, _ecoChoice, null, _belt.Count > 0 ? _belt : null);
                _belt.Clear();
                _curses.Clear();
                _ecoChoice = -1;
                Apply(r);
            }

            DrawRootsAndOffering(new Rect(left.xMax + 12, a.y, 620, a.height));
            DrawRunHistory(new Rect(left.xMax + 644, a.y, a.xMax - left.xMax - 644, a.height));
        }

        private void DrawRootsAndOffering(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 40), "Raíces (se pagan con Savia)", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float y = r.y + 54;
            foreach (var root in _meta.Fresno.Roots)
            {
                int level = F.RootLevel(root.Id);
                string cost = level < root.Max ? $"{root.Costs[level]:N0}" : "tope";
                if (GuiKit.Button(new Rect(r.x + 16, y, r.width - 32, 62), $"{root.Name}  {level}/{root.Max}  ·  {cost}\n<size=14>{root.Description}</size>", level < root.Max, 18))
                    Apply(F.BuyRoot(root.Id));
                y += 66;
            }
            y += 8;
            GuiKit.Label(new Rect(r.x + 16, y, r.width - 32, 34), $"Ofrenda: nivel {F.OfrendaLevel}  ·  siguiente con {F.NextOfrendaThreshold:N0}", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 38;
            long[] amounts = { 5000, 25000, 100000 };
            for (int i = 0; i < amounts.Length; i++)
                if (GuiKit.Button(new Rect(r.x + 16 + i * 196, y, 190, 48), $"Entregar {amounts[i]:N0}", Game.Ledger.Balance(Res.Almitas) >= amounts[i], 17))
                    Apply(F.Offer(amounts[i]));
            y += 56;
            GuiKit.Label(new Rect(r.x + 16, y, r.width - 32, 56), "Bonos: " + F.OfrendaBonus(), 16, Palette.Parchment, TextAnchor.UpperLeft, false, true);
        }

        private void DrawRunHistory(Rect r)
        {
            GuiKit.PanelBox(r);
            var fp = Game.Profile.Fresno;
            GuiKit.Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 40), "Tus runs", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float y = r.y + 54;
            GuiKit.Label(new Rect(r.x + 16, y, r.width - 32, 30), $"Tope semanal: quedan {F.WeeklyRemaining:N0} almitas", 18, Palette.Parchment);
            y += 34;
            string mancha = fp.Mancha != null
                ? $"Mancha de Sal: {fp.Mancha.Almitas:N0} almitas en {fp.Mancha.Zone + 1}-{fp.Mancha.Floor + 1}-{fp.Mancha.Lane + 1}. Tienes un intento."
                : "Sin Mancha de Sal pendiente.";
            GuiKit.Label(new Rect(r.x + 16, y, r.width - 32, 56), mancha, 18, fp.Mancha != null ? Palette.GoldBright : Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
            y += 60;
            for (int i = fp.History.Count - 1; i >= 0 && y < r.yMax - 90; i--)
            {
                var h = fp.History[i];
                string outcome = h.Outcome == nameof(RunOutcome.Victory) ? "Victoria" : h.Outcome == nameof(RunOutcome.Abandoned) ? "Abandono" : "Caída";
                string text = $"Run {h.Run}: {outcome} en {h.ReachedNode}\n<size=14>Guardó {h.AlmitasBanked:N0} · Mancha {h.ManchaLeft:N0} · Recuperó {h.ManchaRecovered:N0} · Savia {h.SaviaEarned:N0}{(h.EcoUsed.Length > 0 ? " · Eco " + h.EcoUsed : "")}</size>";
                GuiKit.Label(new Rect(r.x + 16, y, r.width - 32, 74), text, 18, Palette.Parchment, TextAnchor.UpperLeft, false, true);
                y += 78;
            }
        }

        // ------------------------------------------------------------------ Durante la run

        private void DrawRun(Rect a)
        {
            var mapRect = new Rect(a.x, a.y, 1290, 470);
            DrawMap(mapRect);
            DrawDecision(new Rect(a.x, mapRect.yMax + 10, 1290, a.yMax - mapRect.yMax - 10));
            DrawRunState(new Rect(mapRect.xMax + 10, a.y, a.xMax - mapRect.xMax - 10, a.height));
        }

        private static Color NodeColor(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combate: return Palette.Hex("#7A3A32");
                case NodeType.Elite: return Palette.Crimson;
                case NodeType.Evento: return Palette.Hex("#40608C");
                case NodeType.Santuario: return Palette.Hex("#3E8E7E");
                case NodeType.Caldero: return Palette.Hex("#6E5A9C");
                case NodeType.Mercader: return Palette.Hex("#8C7A32");
                case NodeType.Nido: return Palette.Hex("#A8743C");
                case NodeType.Mancha: return Palette.SaltWhite;
                default: return Palette.Hex("#3A1420");
            }
        }

        private void DrawMap(Rect r)
        {
            GuiKit.PanelBox(r);
            var run = F.Run;
            var zone = F.Zone;
            GuiKit.Label(new Rect(r.x + 16, r.y + 6, r.width - 32, 40), $"{zone.Name}  ·  zona {run.Zone + 1} de {_meta.Fresno.Zones.Count}  ·  {zone.Theme}", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var map = F.Map;
            var available = F.AvailableNodes();
            const float W = 112, H = 62;
            Vector2 Pos(MapNode n) => new Vector2(r.x + 14 + n.Floor * 127 + W / 2, r.y + 130 + n.Lane * 120);

            foreach (var floor in map.Zones[run.Zone])
                foreach (var n in floor)
                    foreach (var lane in n.Next)
                    {
                        var to = map.Node(n.Zone, n.Floor + 1, lane);
                        if (to != null) GuiKit.Line(Pos(n), Pos(to), 2.5f, Palette.WithAlpha(Palette.ParchmentShadow, 0.35f));
                    }

            int? clicked = null;
            foreach (var floor in map.Zones[run.Zone])
            {
                foreach (var n in floor)
                {
                    var c = Pos(n);
                    var box = new Rect(c.x - W / 2, c.y - H / 2, W, H);
                    bool isCurrent = run.Floor == n.Floor && run.Lane == n.Lane;
                    bool isAvailable = available.Contains(n);
                    bool isPast = n.Floor < run.Floor || (isCurrent && run.Phase == RunPhase.ChooseNode);
                    GuiKit.Fill(box, Palette.WithAlpha(NodeColor(n.Type), isPast && !isCurrent ? 0.3f : 0.95f));
                    Color text = n.Type == NodeType.Mancha ? Palette.Ink : Palette.Parchment;
                    GuiKit.Label(box, FresnoService.NodeName(n.Type).Replace(" de Sal", "").Replace(" de Almas", "").Replace(" LitRPG", ""), 16, text, TextAnchor.MiddleCenter, isAvailable);
                    if (isCurrent) GuiKit.Frame(box, Palette.FiveStar, 4f);
                    else if (isAvailable) GuiKit.Frame(box, Palette.Spark, 2f);
                    if (n.HasMancha && !run.ManchaResolved) GuiKit.Diamond(new Vector2(box.xMax - 8, box.y + 8), 10, Palette.FiveStar);
                    if (isAvailable && GUI.Button(box, GUIContent.none, GUIStyle.none)) clicked = n.Lane;
                }
            }
            if (clicked.HasValue) { Apply(F.EnterNode(clicked.Value)); _note = ""; }
        }

        private void DrawDecision(Rect r)
        {
            GuiKit.PanelBox(r);
            var run = F.Run;
            var inner = new Rect(r.x + 16, r.y + 8, r.width - 32, r.height - 80);
            switch (run.Phase)
            {
                case RunPhase.Curses: DrawCurses(inner); break;
                case RunPhase.ChooseNode:
                    GuiKit.Label(inner, "Elige el siguiente nodo en el mapa.", 26, Palette.Parchment);
                    break;
                case RunPhase.Battle: DrawBattleNode(inner); break;
                case RunPhase.Choice: DrawChoice(inner); break;
                case RunPhase.EcoSave: DrawEcoSave(inner); break;
            }

            if (run.Phase == RunPhase.EcoSave) return;
            float y = r.yMax - 62;
            if (GuiKit.Button(new Rect(r.x + 16, y, 240, 52), "Piloto: un paso", true, 18)) AutoStep(1);
            if (GuiKit.Button(new Rect(r.x + 264, y, 280, 52), "Piloto: hasta el final", true, 18)) AutoStep(2000);
            if (GuiKit.Button(new Rect(r.xMax - 216, y, 200, 52), "Abandonar\n<size=13>cuenta como caer</size>", true, 17)) Apply(F.Abandon());
        }

        private void AutoStep(int steps)
        {
            try
            {
                var climber = new AutoClimber(Game);
                for (int i = 0; i < steps && F.InRun && F.Run.Phase != RunPhase.EcoSave; i++) climber.StepOnce();
                Save();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Note(ex.Message, false);
            }
        }

        private void DrawCurses(Rect r)
        {
            var run = F.Run;
            GuiKit.Label(new Rect(r.x, r.y, r.width, 34), $"Maldiciones de {F.Zone.Name}: hasta {_meta.Fresno.MaxCursesPerZone}, cada una ×{_meta.Fresno.CurseAlmitasMultiplier} almitas", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var list = _meta.Fresno.Curses;
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                bool on = _curses.Contains(c.Id);
                var box = new Rect(r.x + (i % 2) * 630, r.y + 40 + (i / 2) * 56, 622, 52);
                if (GuiKit.Button(box, $"{(on ? "● " : "")}{c.Name}\n<size=13>{c.Mods}</size>", on || _curses.Count < _meta.Fresno.MaxCursesPerZone, 17))
                {
                    if (on) _curses.Remove(c.Id);
                    else _curses.Add(c.Id);
                }
            }
            double mult = Math.Pow(_meta.Fresno.CurseAlmitasMultiplier, _curses.Count);
            if (GuiKit.Button(new Rect(r.xMax - 360, r.yMax - 60, 360, 56), $"Entrar a la zona  ·  almitas ×{mult:0.##}", true, 20))
            {
                Apply(F.ChooseCurses(_curses.OrderBy(x => x, StringComparer.Ordinal).ToList()));
                _curses.Clear();
            }
        }

        private void DrawBattleNode(Rect r)
        {
            var node = F.CurrentNode;
            GuiKit.Label(new Rect(r.x, r.y, r.width, 40), $"{FresnoService.NodeName(node.Type)} en el nodo {node.Id}", 28, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x, r.y + 46, r.width, 60), "Lo que queda de vida y Salinidad pasa de un combate al siguiente.", 20, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            if (GuiKit.Button(new Rect(r.x, r.y + 110, 340, 72), "Pelear", true, 30))
                StartBattle?.Invoke(F.PrepareBattle(), b => { Apply(F.ResolveBattle(b)); });
        }

        private void DrawChoice(Rect r)
        {
            var run = F.Run;
            switch (run.Pending.Kind)
            {
                case ChoiceKind.Graft:
                    GuiKit.Label(new Rect(r.x, r.y, r.width, 34), "Elige un Injerto", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                    OptionsGrid(r, run.Pending.Options, id =>
                    {
                        var g = _meta.Fresno.Graft(id);
                        return $"{g.Name}  ·  {_meta.Fresno.Rama(g.Rama).Name}  ({F.GraftsOf(g.Rama)}/{_meta.Fresno.FruitAt})\n<size=14>{g.Description}</size>";
                    }, id => Apply(F.ChooseGraft(id)));
                    if (GuiKit.Button(new Rect(r.xMax - 220, r.yMax - 50, 220, 46), "Ninguno", true, 18)) Apply(F.ChooseGraft(null));
                    break;
                case ChoiceKind.Mutation:
                {
                    var m = _meta.Fresno.Mutation(run.Pending.Options[0]);
                    GuiKit.Label(new Rect(r.x, r.y, r.width, 34), "Mutación del Árbol: bendición y maldición a la vez", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                    GuiKit.Label(new Rect(r.x, r.y + 44, r.width, 80), $"{m.Name}\n<size=18>{m.Description}</size>", 26, Palette.Parchment, TextAnchor.UpperLeft, false, true);
                    if (GuiKit.Button(new Rect(r.x, r.y + 130, 260, 60), "Aceptar", true, 24)) Apply(F.ChooseMutation(true));
                    if (GuiKit.Button(new Rect(r.x + 270, r.y + 130, 260, 60), "Rechazar", true, 24)) Apply(F.ChooseMutation(false));
                    break;
                }
                case ChoiceKind.Event: DrawEvent(r); break;
                case ChoiceKind.Santuario: DrawSantuario(r); break;
                case ChoiceKind.Mercader:
                    GuiKit.Label(new Rect(r.x, r.y, r.width, 34), $"Mercader de Almas: llevas {run.Loose:N0} almitas sueltas", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                    for (int i = 0; i < run.Pending.Options.Count; i++)
                    {
                        var box = new Rect(r.x, r.y + 44 + i * 52, 900, 48);
                        if (GuiKit.Button(box, $"{F.MerchantLabel(run.Pending.Options[i])}  ·  {run.Pending.Prices[i]:N0}", run.Loose >= run.Pending.Prices[i], 18))
                        {
                            Apply(F.MerchantBuy(i));
                            break;
                        }
                    }
                    if (GuiKit.Button(new Rect(r.xMax - 220, r.yMax - 50, 220, 46), "Irte", true, 18)) Apply(F.LeaveChoice());
                    break;
                case ChoiceKind.Caldero:
                    GuiKit.Label(new Rect(r.x, r.y, r.width, 34), "Caldero: prepara con los ingredientes de la run", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                    OptionsGrid(r, run.Pending.Options, id => id == "turbia" ? "Mezclar sin receta\n<size=14>Salmuera Turbia con un efecto al azar</size>" : Cauldron.NameOf(_meta, id), id => Apply(F.CalderoBrew(id)));
                    if (GuiKit.Button(new Rect(r.xMax - 220, r.yMax - 50, 220, 46), "Irte", true, 18)) Apply(F.LeaveChoice());
                    break;
            }
        }

        private static void OptionsGrid(Rect r, IList<string> options, Func<string, string> label, Action<string> pick)
        {
            string chosen = null;
            for (int i = 0; i < options.Count; i++)
            {
                var box = new Rect(r.x + (i % 2) * 630, r.y + 44 + (i / 2) * 68, 622, 62);
                if (GuiKit.Button(box, label(options[i]), true, 18)) chosen = options[i];
            }
            if (chosen != null) pick(chosen);
        }

        private void DrawEvent(Rect r)
        {
            var ev = _meta.Fresno.Event(F.Run.Pending.EventId);
            GuiKit.Label(new Rect(r.x, r.y, r.width, 34), ev.Name, 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x, r.y + 36, r.width, 60), ev.Text, 18, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            for (int i = 0; i < ev.Options.Count; i++)
            {
                var opt = ev.Options[i];
                string blocker = F.OptionBlocker(opt);
                string label = opt.Text + (blocker.Length > 0 ? $"\n<size=13>{blocker}</size>" : "");
                if (GuiKit.Button(new Rect(r.x, r.y + 100 + i * 58, r.width, 54), label, blocker.Length == 0, 18))
                {
                    Apply(F.ChooseEventOption(i));
                    break;
                }
            }
        }

        private void DrawSantuario(Rect r)
        {
            var rules = _meta.Fresno;
            GuiKit.Label(new Rect(r.x, r.y, r.width, 34), "Santuario de Sal: tus almitas quedaron guardadas", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            if (GuiKit.Button(new Rect(r.x, r.y + 44, 400, 60), $"Descansar\n<size=14>{rules.RestSalinity} de Salinidad y +{rules.RestHeal:P0} de vida</size>", true, 20)) { Apply(F.SantuarioRest()); return; }
            GuiKit.Label(new Rect(r.x, r.y + 112, r.width, 30), $"O entrenar un movimiento de Cadena (+{rules.TrainBonus:P0} hasta el final de la run):", 18, Palette.Parchment);
            var run = F.Run;
            for (int i = 0; i < run.Team.Count; i++)
            {
                string id = run.Team[i];
                var def = Game.Combat.Witch(id);
                foreach (ChainRole role in new[] { ChainRole.Apertura, ChainRole.Enlace, ChainRole.Cierre })
                {
                    var box = new Rect(r.x + (int)role * 310, r.y + 146 + i * 44, 304, 40);
                    if (GuiKit.Button(box, $"{def.ShortName}: {def.MoveFor(role).Name}", true, 15)) { Apply(F.SantuarioTrain(id, role)); return; }
                }
            }
        }

        private void DrawEcoSave(Rect r)
        {
            var run = F.Run;
            var rules = _meta.Fresno;
            string title = run.Outcome == RunOutcome.Victory ? "¡Llegaste a la Copa!" : "La run terminó";
            GuiKit.Label(new Rect(r.x, r.y, r.width, 34), $"{title} Guarda un Eco: {rules.EcoGrafts} Injertos y 1 movimiento entrenado", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < run.Grafts.Count && i < 16; i++)
            {
                string id = run.Grafts[i];
                bool on = _ecoGrafts.Contains(id);
                var box = new Rect(r.x + (i % 4) * 316, r.y + 40 + (i / 4) * 40, 310, 36);
                if (GuiKit.Button(box, (on ? "● " : "") + rules.Graft(id).Name, on || _ecoGrafts.Count < rules.EcoGrafts, 16))
                {
                    if (on) _ecoGrafts.Remove(id);
                    else _ecoGrafts.Add(id);
                }
            }
            float y = r.y + 210;
            for (int i = 0; i < run.Trained.Count && i < 4; i++)
            {
                var t = run.Trained[i];
                if (GuiKit.Button(new Rect(r.x + i * 316, y, 310, 36), $"{(_ecoTrained == i ? "● " : "")}{Game.Combat.Witch(t.Witch).ShortName}: {t.Role}", true, 16)) _ecoTrained = _ecoTrained == i ? -1 : i;
            }
            y += 44;
            for (int s = 0; s < rules.EcoSlots; s++)
            {
                if (GuiKit.Button(new Rect(r.x + s * 240, y, 232, 54), $"Guardar en la ranura {s + 1}", F.EcoSlotUnlocked(s), 18))
                {
                    var trained = _ecoTrained >= 0 && _ecoTrained < run.Trained.Count ? run.Trained[_ecoTrained] : null;
                    var result = F.SaveEco(s, _ecoGrafts.OrderBy(x => run.Grafts.IndexOf(x)).ToList(), trained);
                    Apply(result);
                    if (result.Ok) { _ecoGrafts.Clear(); _ecoTrained = -1; }
                    return;
                }
            }
            if (GuiKit.Button(new Rect(r.x + 3 * 240, y, 232, 54), "No guardar", true, 18))
            {
                Apply(F.SkipEco());
                _ecoGrafts.Clear();
                _ecoTrained = -1;
            }
        }

        private void DrawRunState(Rect r)
        {
            GuiKit.PanelBox(r);
            var run = F.Run;
            float x = r.x + 14, w = r.width - 28, y = r.y + 8;
            GuiKit.Label(new Rect(x, y, w, 30), $"Run {run.RunNumber}  ·  nodo {run.NodeId}  ·  semilla {run.Seed}", 16, Palette.ParchmentShadow);
            y += 32;
            GuiKit.Label(new Rect(x, y, w, 34), $"Sueltas {run.Loose:N0}", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 36;
            GuiKit.Label(new Rect(x, y, w, 28), $"Guardadas {run.Banked:N0}  ·  almitas ×{F.AlmitasMultiplier():0.##}  ·  tope {F.WeeklyRemaining:N0}", 16, Palette.Parchment);
            y += 32;
            foreach (var id in run.Team)
            {
                GuiKit.Label(new Rect(x, y, 130, 24), Game.Combat.Witch(id).ShortName, 16, Palette.Parchment);
                GuiKit.Bar(new Rect(x + 130, y + 4, w - 270, 16), (float)run.Hp[id], Palette.Planet(Planet.Venus), Palette.Panel);
                GuiKit.Label(new Rect(x + w - 134, y, 134, 24), $"{run.Hp[id]:P0} · sal {run.Salinity[id]}", 15, Palette.Parchment, TextAnchor.MiddleRight);
                y += 26;
            }
            y += 6;
            var fruits = _meta.Fresno.Ramas.Where(b => F.FruitActive(b.Planet)).Select(b => b.Fruit.Name).ToList();
            string summary = $"<b>Injertos ({run.Grafts.Count}):</b> {string.Join(", ", run.Grafts.Select(g => _meta.Fresno.Graft(g).Name))}\n"
                + (fruits.Count > 0 ? $"<b>Frutos:</b> {string.Join(", ", fruits)}\n" : "")
                + (run.Mutations.Count > 0 ? $"<b>Mutaciones:</b> {string.Join(", ", run.Mutations.Select(m => _meta.Fresno.Mutation(m).Name))}\n" : "")
                + (run.Trained.Count > 0 ? $"<b>Entrenados:</b> {string.Join(", ", run.Trained.Select(t => Game.Combat.Witch(t.Witch).ShortName + " " + t.Role))}\n" : "")
                + (run.Belt.Count > 0 ? $"<b>Cinturón:</b> {string.Join(", ", run.Belt.Select(b => Cauldron.NameOf(_meta, b.PotionId) + " ×" + b.Charges))}\n" : "")
                + (run.Mancha != null && !run.ManchaResolved ? $"<b>Mancha:</b> {run.Mancha.Almitas:N0} en {run.Mancha.Zone + 1}-{run.Mancha.Floor + 1}-{run.Mancha.Lane + 1}\n" : "")
                + (run.EcoName.Length > 0 ? $"<b>Eco:</b> {run.EcoName}" : "");
            GuiKit.Label(new Rect(x, y, w, 230), summary, 15, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            y += 236;
            GuiKit.Label(new Rect(x, y, w, 26), "Registro", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 28;
            int lines = Mathf.Max(1, Mathf.FloorToInt((r.yMax - y - 8) / 40));
            int start = Mathf.Max(0, run.Log.Count - lines);
            for (int i = start; i < run.Log.Count; i++)
            {
                GuiKit.Label(new Rect(x, y, w, 38), run.Log[i], 14, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
                y += 40;
            }
        }
    }
}
