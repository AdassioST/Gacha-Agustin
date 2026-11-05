using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Gacha;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private string _banner = "inicio";
        private readonly List<PullResult> _lastPull = new List<PullResult>();
        private int _historyPage;
        private int _ledgerPage;
        private int _balancePage;

        private static Color VaporColor(int rarity) =>
            rarity == 5 ? Palette.FiveStar : rarity == 4 ? Palette.Hex("#A77BEA") : Palette.SaltWhite;

        private string PullText(PullResult p)
        {
            switch (p.Kind)
            {
                case "bruja": return $"{WitchName(p.WitchId)}{(p.Featured ? " · destacada" : p.LostFeatured ? " · perdió el 75/25" : "")}";
                case "polvo": return $"{p.Dust} de Polvo Astral (ya tienes a todas)";
                default:
                    string recipe = p.RecipeId != null ? "Receta: " + _meta.Weapon(_meta.Recipes[p.RecipeId].Output).Name + " · " : "";
                    return $"{recipe}{p.SaltAmount} Sal Cruda";
            }
        }

        private void DrawTermas(Rect a)
        {
            var rules = _meta.Gacha;
            var left = new Rect(a.x, a.y, 400, a.height);
            GuiKit.PanelBox(left);
            GuiKit.Label(new Rect(left.x + 12, left.y + 6, 376, 36), "Manantiales", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float y = left.y + 48;
            foreach (var b in rules.Banners)
            {
                var box = new Rect(left.x + 12, y, 376, 62);
                string sub = b.Type == BannerType.Inicio ? $"quedan {Game.Termas.StarterPullsLeft(b)} tiradas" : $"contador {Game.Termas.Pity(b).Since5}";
                if (GuiKit.Button(box, $"{b.Name}\n<size=14>{sub}</size>", true, 18)) { _banner = b.Id; _lastPull.Clear(); }
                if (_banner == b.Id) GuiKit.Frame(box, Palette.GoldBright, 3f);
                y += 68;
            }

            var banner = rules.Banner(_banner);
            var center = new Rect(left.xMax + 10, a.y, 800, a.height);
            GuiKit.PanelBox(center);
            float x = center.x + 16, width = center.width - 32;
            y = center.y + 6;
            GuiKit.Label(new Rect(x, y, width, 40), banner.Name, 30, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 42;
            GuiKit.Label(new Rect(x, y, width, 30), banner.Description, 17, Palette.Parchment);
            y += 36;

            var pity = Game.Termas.Pity(banner);
            bool boiling = Game.Termas.Boiling(banner.Id);
            GuiKit.Label(new Rect(x, y, width, 28), $"Termómetro: {pity.Since5}/{rules.HardPity}{(boiling ? "  ·  ¡hierve!" : "")}{(pity.Guaranteed ? "  ·  la siguiente 5★ es la destacada" : "")}", 19, boiling ? Palette.Danger : Palette.Parchment);
            y += 30;
            GuiKit.Bar(new Rect(x, y, width, 26), (float)Game.Termas.Thermometer(banner.Id), boiling ? Palette.Danger : Palette.Hex("#3EA48E"), Palette.Abyss);
            float softX = x + width * (rules.SoftPityStart - 1) / rules.HardPity;
            GuiKit.Fill(new Rect(softX, y - 3, 2, 32), Palette.GoldBright);
            y += 36;

            if (banner.Type == BannerType.Inicio)
            {
                GuiKit.Label(new Rect(x, y, width, 26), "Tu Regente garantizada:", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                y += 28;
                for (int i = 0; i < banner.Choices.Count; i++)
                {
                    string id = banner.Choices[i];
                    var box = new Rect(x + i * 190, y, 184, 42);
                    if (GuiKit.Button(box, WitchName(id), !Game.Profile.Gacha.StarterFiveStar, 16)) Apply(Game.Termas.SetStarterChoice(id));
                    if (Game.Profile.Gacha.StarterChoice == id) GuiKit.Frame(box, Palette.GoldBright, 3f);
                }
                y += 50;
            }
            if (banner.Type == BannerType.Limitado && Game.Profile.Owns(banner.Featured))
            {
                // HUECOS C-23: el banner avisa antes de tirar.
                GuiKit.Label(new Rect(x, y, width, 30), $"Ya tienes a {WitchName(banner.Featured)}: la 5★ saldrá del estándar.", 18, Palette.Danger);
                y += 34;
            }
            if (banner.Type == BannerType.Estandar)
            {
                string label = Game.Termas.SelectorAvailable ? "Selector disponible: elige una Regente" : $"Selector: {Game.Profile.Gacha.StandardPulls}/{rules.SelectorAt}{(Game.Profile.Gacha.SelectorUsed ? " (usado)" : "")}";
                GuiKit.Label(new Rect(x, y, width, 26), label, 18, Game.Termas.SelectorAvailable ? Palette.FiveStar : Palette.Parchment);
                y += 28;
                if (Game.Termas.SelectorAvailable)
                {
                    var five = Game.Termas.StandardFiveStars();
                    for (int i = 0; i < five.Count; i++)
                        if (GuiKit.Button(new Rect(x + i * 190, y, 184, 42), WitchName(five[i]), !Game.Profile.Owns(five[i]), 16)) Apply(Game.Termas.ClaimSelector(five[i]));
                    y += 50;
                }
            }

            string why1 = Game.Termas.CanPull(banner.Id, 1), why10 = Game.Termas.CanPull(banner.Id, 10);
            if (GuiKit.Button(new Rect(x, y, 300, 66), $"Una tirada\n<size=14>{rules.CostPerPull} Sales de Baño</size>", why1.Length == 0, 22)) Pull(banner.Id, 1);
            if (GuiKit.Button(new Rect(x + 310, y, 300, 66), $"Diez tiradas\n<size=14>{rules.CostPerPull * 10:N0} Sales de Baño</size>", why10.Length == 0, 22)) Pull(banner.Id, 10);
            y += 76;

            int best = _lastPull.Count > 0 ? _lastPull.Max(p => p.Rarity) : 0;
            var vapor = new Rect(x, y, width, 54);
            GuiKit.Fill(vapor, best > 0 ? Palette.WithAlpha(VaporColor(best), 0.85f) : Palette.WithAlpha(Palette.Abyss, 0.6f));
            GuiKit.Label(vapor, best == 0 ? "El vapor espera" : "Vapor " + _lastPull.First(p => p.Rarity == best).Vapor, 24, best > 0 ? Palette.Ink : Palette.ParchmentShadow, TextAnchor.MiddleCenter, true);
            y += 60;
            foreach (var p in _lastPull.Take(10))
            {
                GuiKit.Fill(new Rect(x, y + 3, 10, 22), VaporColor(p.Rarity));
                GuiKit.Label(new Rect(x + 18, y, width - 18, 28), $"{Stars(p.Rarity)}  {PullText(p)}", 18, p.Rarity == 5 ? Palette.FiveStar : Palette.Parchment);
                y += 29;
            }

            var right = new Rect(center.xMax + 10, a.y, a.xMax - center.xMax - 10, a.height);
            GuiKit.PanelBox(right);
            GuiKit.Label(new Rect(right.x + 12, right.y + 6, right.width - 24, 36), "Historial de tiradas", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var history = Enumerable.Reverse(Game.Profile.Gacha.History).ToList();
            _historyPage = Paged(new Rect(right.x + 12, right.y + 48, right.width - 24, right.height - 58), history, _historyPage, 30, (box, h) =>
            {
                GuiKit.Fill(new Rect(box.x, box.y + 4, 8, 18), VaporColor(h.Rarity));
                string what = h.Kind == "bruja" ? WitchName(h.Id) : h.Kind == "polvo" ? $"{h.Amount} de Polvo Astral" : "Receta y Sal Cruda";
                GuiKit.Label(new Rect(box.x + 14, box.y, box.width - 14, box.height), $"#{h.Seq} {Stars(h.Rarity)} {what} · tirada {h.Pity}", 15, h.Rarity == 5 ? Palette.FiveStar : Palette.Parchment);
            });
        }

        private void Pull(string banner, int count)
        {
            var r = Game.Termas.Pull(banner, count, out var results);
            _lastPull.Clear();
            _lastPull.AddRange(results);
            Apply(r);
            if (r.Ok && results.Count > 0) PullRevealed?.Invoke(results.Max(p => p.Rarity));
        }

        // ------------------------------------------------------------------ Libro contable

        private void DrawLedger(Rect a)
        {
            var l = Game.Ledger;
            var left = new Rect(a.x, a.y, 560, a.height);
            GuiKit.PanelBox(left);
            GuiKit.Label(new Rect(left.x + 12, left.y + 6, 536, 36), "Saldos", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var balances = _meta.Catalog.Order.Where(id => l.Balance(id) != 0).ToList();
            _balancePage = Paged(new Rect(left.x + 12, left.y + 48, 536, left.height - 58), balances, _balancePage, 28, (box, id) =>
            {
                GuiKit.Label(new Rect(box.x, box.y, 380, box.height), _meta.Catalog.NameOf(id), 16, Palette.Parchment);
                GuiKit.Label(new Rect(box.x + 380, box.y, 156, box.height), l.Balance(id).ToString("N0"), 16, Palette.GoldBright, TextAnchor.MiddleRight);
            });

            var right = new Rect(left.xMax + 10, a.y, a.xMax - left.xMax - 10, a.height);
            GuiKit.PanelBox(right);
            GuiKit.Label(new Rect(right.x + 12, right.y + 6, right.width - 24, 36), "Movimientos: cada uno con su fuente o su sumidero", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var entries = Enumerable.Reverse(l.Entries).ToList();
            _ledgerPage = Paged(new Rect(right.x + 12, right.y + 48, right.width - 24, right.height - 58), entries, _ledgerPage, 28, (box, e) =>
            {
                string sign = e.Delta > 0 ? "+" : "";
                GuiKit.Label(new Rect(box.x, box.y, 90, box.height), "#" + e.Seq, 14, Palette.ParchmentShadow);
                GuiKit.Label(new Rect(box.x + 90, box.y, 300, box.height), _meta.Catalog.NameOf(e.Resource), 15, Palette.Parchment);
                GuiKit.Label(new Rect(box.x + 390, box.y, 130, box.height), sign + e.Delta.ToString("N0"), 15, e.Delta > 0 ? Palette.Planet(Core.Magic.Planet.Venus) : Palette.Danger, TextAnchor.MiddleRight);
                GuiKit.Label(new Rect(box.x + 540, box.y, box.width - 540, box.height), _meta.Catalog.FlowName(e.Flow) + (e.Note.Length > 0 ? " · " + e.Note : ""), 14, Palette.ParchmentShadow);
            });
        }
    }
}
