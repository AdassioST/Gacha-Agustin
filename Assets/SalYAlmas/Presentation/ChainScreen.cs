using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Pantalla de Cadena: elegir de 3 a 4 brujas, reordenarlas, ver la Partitura con los Spark previstos en
    /// dorado y los Aspectos entre vecinas, y confirmar. El jugador puede reordenar las veces que quiera.
    /// </summary>
    public sealed class ChainScreen
    {
        private readonly BattleDirector _d;
        private readonly SavedPartituras _saved;
        private readonly List<int> _order = new List<int>();
        private bool _guest;
        private int _targetId;
        private ChainPreview _preview;
        private string _previewKey = "";
        private int _bestSparks;

        public bool IsOpen { get; private set; }

        public ChainScreen(BattleDirector director, SavedPartituras saved)
        {
            _d = director;
            _saved = saved;
        }

        private Battle B => _d.Battle;

        public void Show(int targetId)
        {
            IsOpen = true;
            _targetId = targetId;
            _order.Clear();
            foreach (var w in B.ChainEligibleWitches().Take(B.Rules.Chain.MaxWitches)) _order.Add(w.Id);
            _guest = B.CanUseGuestInChain;
            _bestSparks = AutoPilot.BestChain(B)?.preview.SparkCount ?? 0;
            _previewKey = "";
        }

        public void Close() => IsOpen = false;

        private Planet PlanetOf(int unitId)
        {
            var w = B.FindWitch(unitId);
            return w != null ? w.Dominant : Planet.Saturno;
        }

        private void RefreshPreview()
        {
            string key = string.Join(",", _order) + "|" + _guest;
            if (key == _previewKey && _preview != null) return;
            _previewKey = key;
            _preview = B.PreviewChain(_order, _guest);
        }

        public void Draw()
        {
            if (!IsOpen) return;
            if (B.Pending != DecisionKind.AllyTurn) { Close(); return; }
            RefreshPreview();

            GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.WithAlpha(Palette.Abyss, 0.82f));
            var panel = new Rect(40, 50, 1840, 980);
            GuiKit.PanelBox(panel, 0.96f);
            GuiKit.Label(new Rect(70, 62, 1000, 44), "Cadena · la Calculista ordena en silencio", 34, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(70, 104, 1780, 30),
                "Cada eslabón empieza 24 frames después del anterior. Hay <b>Spark</b> cuando golpes de brujas distintas caen a 2 frames o menos: +50 % a ese golpe. Reordena hasta que te guste.",
                19, Palette.ParchmentShadow);

            DrawWitchPicker(new Rect(70, 150, 450, 470));
            DrawOrder(new Rect(70, 630, 450, 380));

            var staffArea = new Rect(560, 160, 1290, 470);
            GuiKit.Fill(staffArea, Palette.WithAlpha(Palette.Abyss, 0.6f));
            if (_preview != null && _preview.IsValid)
                PartituraDrawer.Draw(new Rect(staffArea.x + 10, staffArea.y + 20, staffArea.width - 20, staffArea.height - 40), _preview, PlanetOf, false, -1, null, B.Rules.Chain.SparkWindowFrames);
            else
                GuiKit.Label(staffArea, _preview?.Error ?? "", 26, Palette.ParchmentShadow, TextAnchor.MiddleCenter);

            DrawSummary(new Rect(560, 645, 1290, 250));
            DrawButtons(new Rect(560, 905, 1290, 110));
        }

        private void DrawWitchPicker(Rect r)
        {
            GuiKit.Label(new Rect(r.x, r.y, r.width, 30), "Brujas (elige de 3 a 4)", 22, Palette.Parchment, TextAnchor.MiddleLeft, true);
            float y = r.y + 36;
            foreach (var w in B.Witches)
            {
                int pos = _order.IndexOf(w.Id);
                string reason = !w.IsAlive ? "cayó" : w.InEcstasy ? "en Éxtasis" : w.IsDesalada ? "Desalada" : "";
                var row = new Rect(r.x, y, r.width, 58);
                bool eligible = reason.Length == 0;
                string label = (pos >= 0 ? (pos + 1) + ". " : "    ") + w.Name + "  ·  " + Planets.DisplayName(w.Dominant) + (reason.Length > 0 ? "  (" + reason + ")" : "");
                if (GuiKit.Button(row, label, eligible, 22))
                {
                    if (pos >= 0) _order.RemoveAt(pos);
                    else if (_order.Count < B.Rules.Chain.MaxWitches) _order.Add(w.Id);
                }
                GuiKit.Fill(new Rect(row.x, row.y, 8, row.height), Palette.Planet(w.Dominant));
                if (pos >= 0) GuiKit.Frame(row, Palette.GoldBright, 2);
                y += 64;
            }

            y += 8;
            if (B.Guest != null)
            {
                bool can = B.CanUseGuestInChain;
                string text = can
                    ? (_guest ? "Invitada: " + B.Guest.Name + " cierra  [Sí]" : "Invitada: " + B.Guest.Name + " cierra  [No]")
                    : "Invitada: " + B.Guest.Name + " ya cerró una Cadena";
                var row = new Rect(r.x, y, r.width, 58);
                if (GuiKit.Button(row, text, can, 21)) _guest = !_guest;
                GuiKit.Fill(new Rect(row.x, row.y, 8, row.height), Palette.Planet(B.Guest.Dominant));
                if (_guest && can) GuiKit.Frame(row, Palette.GoldBright, 2);
                GuiKit.Label(new Rect(r.x, y + 60, r.width, 24), "Una vez por combate. Cierre: " + B.Guest.Def.Closing.Name, 16, Palette.ParchmentShadow);
            }
        }

        private void DrawOrder(Rect r)
        {
            GuiKit.Label(new Rect(r.x, r.y, r.width, 30), "Orden", 22, Palette.Parchment, TextAnchor.MiddleLeft, true);
            float y = r.y + 36;
            int count = _order.Count + (_guest && B.CanUseGuestInChain ? 1 : 0);
            for (int i = 0; i < _order.Count; i++)
            {
                var w = B.FindWitch(_order[i]);
                var role = ChainPlanner.RoleFor(i, count);
                string roleName = role == ChainRole.Apertura ? "Apertura" : role == ChainRole.Enlace ? "Enlace" : "Cierre";
                var row = new Rect(r.x, y, r.width, 50);
                GuiKit.Fill(row, Palette.WithAlpha(Palette.Abyss, 0.7f));
                GuiKit.Fill(new Rect(row.x, row.y, 6, row.height), Palette.Planet(w.Dominant));
                GuiKit.Label(new Rect(row.x + 14, row.y, 110, row.height), roleName, 18, Palette.GoldBright);
                GuiKit.Label(new Rect(row.x + 120, row.y, 180, row.height), w.Name, 21, Palette.Parchment, TextAnchor.MiddleLeft, true);
                if (GuiKit.Button(new Rect(row.xMax - 150, row.y + 5, 44, 40), "▲", i > 0, 20)) Swap(i, i - 1);
                if (GuiKit.Button(new Rect(row.xMax - 102, row.y + 5, 44, 40), "▼", i < _order.Count - 1, 20)) Swap(i, i + 1);
                if (GuiKit.Button(new Rect(row.xMax - 54, row.y + 5, 44, 40), "×", true, 22)) { _order.RemoveAt(i); break; }
                y += 56;
            }
            if (_guest && B.CanUseGuestInChain)
            {
                var row = new Rect(r.x, y, r.width, 50);
                GuiKit.Fill(row, Palette.WithAlpha(Palette.Abyss, 0.7f));
                GuiKit.Fill(new Rect(row.x, row.y, 6, row.height), Palette.Planet(B.Guest.Dominant));
                GuiKit.Label(new Rect(row.x + 14, row.y, 110, row.height), "Cierre", 18, Palette.GoldBright);
                GuiKit.Label(new Rect(row.x + 120, row.y, 300, row.height), B.Guest.Name + " (Invitada)", 21, Palette.Parchment, TextAnchor.MiddleLeft, true);
            }
        }

        private void Swap(int a, int b)
        {
            int t = _order[a];
            _order[a] = _order[b];
            _order[b] = t;
        }

        private void DrawSummary(Rect r)
        {
            GuiKit.Fill(r, Palette.WithAlpha(Palette.Abyss, 0.6f));
            if (_preview == null || !_preview.IsValid) return;
            string sparks = $"<b>Spark previstos: {_preview.SparkCount}</b>   (cada uno: +50 % a ese golpe, +2 de Ánima, +1 almita; la Resonancia por Spark tiene tope de +20)";
            GuiKit.Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 30), sparks, 21, Palette.Spark);
            string best = _preview.SparkCount >= _bestSparks
                ? "Es la Partitura con más Spark posible con estas brujas."
                : $"La mejor Partitura con estas brujas da {_bestSparks} Spark. Prueba otros órdenes.";
            GuiKit.Label(new Rect(r.x + 16, r.y + 40, r.width - 32, 26), best, 18, _preview.SparkCount >= _bestSparks ? Palette.GoldBright : Palette.ParchmentShadow);

            float y = r.y + 76;
            foreach (var a in _preview.Aspects)
            {
                var from = _preview.Links[a.FromLink];
                var to = _preview.Links[a.ToLink];
                string reaction = a.Reaction != HybridReaction.None ? " → " + AspectRules.DisplayName(a.Reaction) : "";
                string line = $"{from.Name} ({Planets.DisplayName(a.FromPlanet)}) y {to.Name} ({Planets.DisplayName(a.ToPlanet)}): <b>{AspectRules.DisplayName(a.Aspect)}{reaction}</b> · {AspectRules.EffectSummary(a.Aspect, a.Reaction)}";
                GuiKit.Fill(new Rect(r.x + 16, y + 6, 6, 22), a.Aspect == Aspect.Oposicion ? Palette.Spark : Palette.Gold);
                GuiKit.Label(new Rect(r.x + 30, y, r.width - 46, 34), line, 19, Palette.Parchment);
                y += 34;
            }
        }

        private void DrawButtons(Rect r)
        {
            bool valid = _preview != null && _preview.IsValid && B.ChainBlocker().Length == 0;
            if (GuiKit.Button(new Rect(r.x, r.y, 360, 70), "<b>Confirmar Cadena</b>", valid, 26))
            {
                var res = _d.Submit(BattleCommand.Chain(B.ActiveUnit.Id, _order, _guest && B.CanUseGuestInChain, _targetId));
                if (res.Ok) Close();
            }
            if (GuiKit.Button(new Rect(r.x + 380, r.y, 280, 70), "Guardar Partitura", _preview != null && _preview.IsValid, 22))
            {
                var keys = _order.Select(id => B.FindWitch(id).Key).ToList();
                string name = string.Join(" → ", _order.Select(id => B.FindWitch(id).Name)) + (_guest ? " + Inv." : "");
                _saved.Add(new SavedPartitura { Name = name, WitchIds = keys, WithGuest = _guest });
                _d.Pilot.Partituras.Clear();
                _d.Pilot.Partituras.AddRange(_saved.Items);
                _d.ShowToast("Partitura guardada: el modo automático la usará primero.");
            }
            if (GuiKit.Button(new Rect(r.x + 680, r.y, 200, 70), "Cancelar", true, 22)) Close();

            // Partituras guardadas.
            float x = r.x + 900;
            GuiKit.Label(new Rect(x, r.y - 4, 390, 22), "Partituras guardadas", 16, Palette.ParchmentShadow);
            float y = r.y + 18;
            for (int i = 0; i < _saved.Items.Count && i < 3; i++)
            {
                var p = _saved.Items[i];
                if (GuiKit.Button(new Rect(x, y, 340, 26), p.Name, true, 14)) Load(p);
                if (GuiKit.Button(new Rect(x + 346, y, 40, 26), "×", true, 14)) { _saved.Remove(i); break; }
                y += 29;
            }
        }

        private void Load(SavedPartitura p)
        {
            var ids = new List<int>();
            foreach (var key in p.WitchIds)
            {
                var w = B.Witches.FirstOrDefault(x => x.Key == key);
                if (w == null) { _d.ShowToast("Esa Partitura usa a una bruja que no está en este equipo."); return; }
                ids.Add(w.Id);
            }
            _order.Clear();
            _order.AddRange(ids);
            _guest = p.WithGuest && B.CanUseGuestInChain;
        }
    }
}
