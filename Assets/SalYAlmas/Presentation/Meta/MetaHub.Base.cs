using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Save;
using SalYAlmas.Core.World;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        private int _touches;
        private string _baseLine = "";
        private string _baseWitch = "";

        // ------------------------------------------------------------------ Termas sobre la Tortuga

        private void DrawBase(Rect a)
        {
            var map = new Rect(a.x, a.y, 1320, a.height);
            Theme.GoldPanel(map, 0.92f);
            Theme.Title(new Rect(map.x + 20, map.y + 6, 800, 46), "Las Termas sobre la Tortuga", 32, Palette.GoldBright, TextAnchor.MiddleLeft);
            GuiKit.Label(new Rect(map.x + 720, map.y + 6, map.width - 740, 46), "Aquí nada te persigue.", 20, Palette.ParchmentShadow, TextAnchor.MiddleRight);

            // El caparazón y el manantial.
            var c = new Vector2(map.center.x, map.y + 420);
            GuiKit.Dot(c, 340, Palette.Hex("#3A3020"));
            GuiKit.Dot(c, 322, Palette.Hex("#4E4028"));
            Color water = Palette.Hex("#3E7E8E");
            var spring = Game.Store.Equipped("manantial");
            if (spring.Length > 0 && ColorUtility.TryParseHtmlString(_meta.World.Shop.Cosmetic(spring)?.Color ?? "", out var sc)) water = sc;
            GuiKit.Dot(new Vector2(c.x, c.y + 20), 160, water);
            GuiKit.Ring(new Vector2(c.x, c.y + 20), 165, 1f, Palette.SaltWhite, Color.clear, 60, 2f);
            for (int k = 0; k < 6; k++)
            {
                float t = (Time.unscaledTime * 0.2f + k / 6f) % 1f;
                GuiKit.Dot(new Vector2(c.x - 80 + k * 32, c.y - 60 - t * 120), 9 - t * 6, Palette.WithAlpha(Palette.SaltWhite, 0.35f * (1 - t)));
            }

            // Cada sistema tiene un lugar en el caparazón (propuesta de meta-juego, 10.1).
            Place(new Vector2(c.x, c.y + 20), 150, 90, "Manantial", () => Go(Screen.Summon));
            Place(new Vector2(c.x - 430, c.y - 180), 170, 66, "Yunque Astral", () => GoRoot(Screen.Forge));
            Place(new Vector2(c.x - 430, c.y - 90), 170, 66, "Caldero", () => GoRoot(Screen.Cauldron));
            Place(new Vector2(c.x - 430, c.y), 170, 66, "Esqueje del Fresno", () => Go(Screen.Fresno));
            Place(new Vector2(c.x + 430, c.y - 180), 170, 66, "Espejo", () => Go(Screen.Mirror));
            Place(new Vector2(c.x + 430, c.y - 90), 170, 66, "Puesto", () => GoRoot(Screen.Shop));
            Place(new Vector2(c.x + 430, c.y), 170, 66, "Proa", () => Go(Screen.Story));
            Place(new Vector2(c.x + 430, c.y + 90), 170, 66, "Libro Mayor", () => Go(Screen.Ledger));

            // Reposo: hasta cuatro brujas alrededor del agua; rotan cada día.
            var resting = Game.Base.Resting();
            Vector2[] spots = { new Vector2(c.x - 210, c.y + 150), new Vector2(c.x + 210, c.y + 150), new Vector2(c.x - 220, c.y - 150), new Vector2(c.x + 220, c.y - 150) };
            for (int i = 0; i < resting.Count && i < spots.Length; i++)
            {
                string id = resting[i];
                var p = spots[i];
                float bob = Mathf.Sin(Time.unscaledTime * 1.2f + i) * 3f;
                var box = new Rect(p.x - 74, p.y - 74 + bob, 148, 148);
                GuiKit.Dot(new Vector2(p.x, p.y + bob), 78, Palette.WithAlpha(Palette.Abyss, 0.55f));
                var bust = BustFor(id);
                if (bust != null) GUI.DrawTexture(box, bust, ScaleMode.ScaleToFit, true);
                if (!Game.Base.Talked(id)) Badge(new Vector2(p.x + 60, p.y - 60 + bob), 1);
                if (_baseWitch == id) GuiKit.Ring(new Vector2(p.x, p.y + bob), 82, 1f, Palette.FiveStar, Color.clear, 40, 2.4f);
                if (GUI.Button(box, GUIContent.none, GUIStyle.none)) { _baseWitch = id; _baseLine = ""; }
            }
            if (resting.Count == 0)
                GuiKit.Label(new Rect(map.x, c.y + 200, map.width, 60), "La Tortuga está vacía. Termina el prólogo.", 24, Palette.Parchment, TextAnchor.MiddleCenter);

            var panel = new Rect(map.xMax + 10, a.y, a.width - map.width - 10, a.height);
            Theme.GoldPanel(panel, 0.95f);
            float x = panel.x + 18, w = panel.width - 36, y = panel.y + 12;

            var story = Game.Story;
            Theme.Title(new Rect(x, y, w, 40), L("base.bath"), 28, Palette.GoldBright, TextAnchor.MiddleLeft);
            y += 42;
            GuiKit.Label(new Rect(x, y, w, 50), $"El agua hace olvidar. {L("top.parallax")}: {Game.Profile.Calculista.Paralaje} ({story.Band}).", 18, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            y += 54;
            if (GuiKit.Button(new Rect(x, y, w, 62), Game.Base.CanBathe ? $"{L("base.bath")}  ·  {_meta.World.Termas.BathParalaje} de Paralaje" : "Vuelve mañana", Game.Base.CanBathe, 22))
            {
                var r = Game.Base.Bathe();
                _baseLine = r.Ok ? "Por un rato, no recuerdas qué era lo que mirabas." : r.Message;
                if (r.Ok) PullSequence.Sfx?.Invoke("sfx_termas_bano");
                Save();
            }
            y += 80;

            if (_baseWitch.Length == 0 && resting.Count > 0) _baseWitch = resting[0];
            if (_baseWitch.Length > 0 && Game.Profile.Owns(_baseWitch))
            {
                var w0 = _baseWitch;
                Theme.Title(new Rect(x, y, w, 40), WitchName(w0), 26, Palette.Parchment, TextAnchor.MiddleLeft);
                y += 42;
                int lvl = Game.Base.TrustLevel(w0);
                int xp = Game.Base.TrustOf(w0) % _meta.World.Termas.TrustPerLevel;
                GuiKit.Label(new Rect(x, y, w, 26), $"Confianza {lvl}", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                y += 28;
                GuiKit.Bar(new Rect(x, y, w, 14), xp / (float)_meta.World.Termas.TrustPerLevel, Palette.Hex("#D9709E"), Palette.Abyss);
                y += 26;
                if (GuiKit.Button(new Rect(x, y, w / 2 - 6, 58), L("base.talk"), true, 22))
                {
                    var r = Game.Base.Talk(w0);
                    _baseLine = r.Message;
                    Save();
                }
                if (GuiKit.Button(new Rect(x + w / 2 + 6, y, w / 2 - 6, 58), L("base.touch"), true, 22)) { _baseLine = Game.Base.Touch(w0, ++_touches); PullSequence.Sfx?.Invoke("sfx_termas_tocar"); }
                y += 66;
                if (GuiKit.Button(new Rect(x, y, w, 46), "Hacerla tu favorita", Game.Profile.TermasBase.Favorite != w0, 18))
                {
                    Apply(Game.Base.SetFavorite(w0));
                    Game.Profile.Party.Remove(w0);
                    if (_meta.HasKit(w0)) Game.Profile.Party.Insert(0, w0);
                    while (Game.Profile.Party.Count > 4) Game.Profile.Party.RemoveAt(4);
                    Save();
                }
                y += 56;
            }
            if (_baseLine.Length > 0)
            {
                var bubble = new Rect(x, y, w, Mathf.Min(200, panel.yMax - y - 12));
                Theme.Panel(bubble, Palette.Hex("#EDE3CC"), Palette.Hex("#CFBE9A"), Palette.Gold);
                GuiKit.Label(new Rect(bubble.x + 14, bubble.y + 10, bubble.width - 28, bubble.height - 20), _baseLine, 19, Palette.Ink, TextAnchor.UpperLeft, false, true);
            }
        }

        private void Place(Vector2 c, float w, float h, string name, System.Action go)
        {
            var r = new Rect(c.x - w / 2, c.y - h / 2, w, h);
            if (GuiKit.Button(r, name, true, 18)) go();
        }

        // ------------------------------------------------------------------ Espejo: vestidor

        private string _mirrorWitch = "";
        private int _mirrorPage;

        private void DrawMirror(Rect a)
        {
            var body = Header(a, L("base.mirror"), "Atuendos, color de sal, manantiales y marcos. Ningún cosmético da estadísticas.");
            var left = new Rect(body.x, body.y, 520, body.height);
            Theme.GoldPanel(left, 0.92f);
            var owned = _meta.RosterOrder.Where(id => Game.Profile.Owns(id)).ToList();
            if (!owned.Contains(_mirrorWitch)) _mirrorWitch = owned.FirstOrDefault() ?? "";
            if (_mirrorWitch.Length > 0)
            {
                var body2 = _art.Body(_mirrorWitch);
                var view = new Rect(left.x + 20, left.y + 14, left.width - 40, left.height - 100);
                var outfit = _meta.World.Shop.Cosmetic(Game.Store.Equipped("atuendo:" + _mirrorWitch));
                Color tint = Color.white;
                if (outfit != null && ColorUtility.TryParseHtmlString(outfit.Color, out var oc)) tint = Color.Lerp(Color.white, oc, 0.4f);
                if (body2 != null) PortraitBank.Fit(view, body2, tint);
                else PortraitBank.Fit(view, BustFor(_mirrorWitch), tint);
                GuiKit.Label(new Rect(left.x + 20, left.yMax - 82, left.width - 40, 34), WitchName(_mirrorWitch), 24, Palette.Parchment, TextAnchor.MiddleCenter, true);
                int idx = owned.IndexOf(_mirrorWitch);
                if (GuiKit.Button(new Rect(left.x + 20, left.yMax - 50, 100, 40), "◀", true, 18)) _mirrorWitch = owned[(idx + owned.Count - 1) % owned.Count];
                if (GuiKit.Button(new Rect(left.xMax - 120, left.yMax - 50, 100, 40), "▶", true, 18)) _mirrorWitch = owned[(idx + 1) % owned.Count];
            }

            var right = new Rect(left.xMax + 10, body.y, body.width - left.width - 10, body.height);
            Theme.GoldPanel(right, 0.92f);
            var list = _meta.World.Shop.Cosmetics.Where(c => Game.Store.OwnsCosmetic(c.Id) && (c.Witch.Length == 0 || c.Witch == _mirrorWitch)).ToList();
            if (list.Count == 0)
            {
                GuiKit.Label(new Rect(right.x, right.y + 40, right.width, 60), "No tienes cosméticos para esta vista. Búscalos en la Tienda.", 24, Palette.Parchment, TextAnchor.MiddleCenter);
                if (GuiKit.Button(new Rect(right.center.x - 160, right.y + 110, 320, 70), L("nav.shop"), true, 26)) GoRoot(Screen.Shop);
                return;
            }
            _mirrorPage = Paged(new Rect(right.x + 14, right.y + 14, right.width - 28, right.height - 24), list, _mirrorPage, 76, (box, c) =>
            {
                bool on = Game.Store.Equipped(StoreService.SlotOf(c)) == c.Id;
                if (GuiKit.Button(box, $"{(on ? "● " : "")}{c.Name}\n<size=15>{c.Description}</size>", true, 21))
                {
                    if (on) { Game.Store.Unequip(c); Save(); }
                    else Apply(Game.Store.Equip(c.Id));
                }
            });
        }

        // ------------------------------------------------------------------ Tienda

        private float _deleteArmedUntil;
        private int _shopTab;
        private int _shopPage;

        private void DrawShop(Rect a)
        {
            var body = Header(a, L("nav.shop"), L(DevTools ? "shop.headerDev" : "shop.header"));
            string[] tabs = { "Perlas", "Cosméticos", "Grimorio y Diezmo" };
            for (int i = 0; i < 3; i++)
            {
                var b = new Rect(body.x + i * 300, body.y, 290, 56);
                if (GuiKit.Button(b, tabs[i], true, 23)) { _shopTab = i; _shopPage = 0; }
                if (_shopTab == i) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            var content = new Rect(body.x, body.y + 64, body.width, body.height - 64);
            Theme.GoldPanel(content, 0.92f);
            var inner = new Rect(content.x + 16, content.y + 14, content.width - 32, content.height - 28);
            switch (_shopTab)
            {
                case 0: ShopPerlas(inner); break;
                case 1: ShopCosmetics(inner); break;
                default: ShopPass(inner); break;
            }
        }

        private void ShopPerlas(Rect r)
        {
            var products = _meta.World.Shop.Products.Where(p => p.Kind == ProductKind.Perlas).ToList();
            float w = r.width / products.Count;
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                var card = new Rect(r.x + i * w, r.y, w - 12, 300);
                Theme.Panel(card, Palette.Hex("#3E5A7A"), Palette.Hex("#141A2C"), Palette.GoldBright);
                GuiKit.Diamond(new Vector2(card.center.x, card.y + 80), 46, Palette.Hex("#BFD8F0"));
                Theme.Title(new Rect(card.x, card.y + 134, card.width, 40), p.Name, 24, Palette.Parchment);
                GuiKit.Label(new Rect(card.x, card.y + 176, card.width, 30), $"{p.Perlas:N0}{(p.Bonus > 0 ? $" + {p.Bonus:N0}" : "")} Perlas", 21, Palette.GoldBright, TextAnchor.MiddleCenter, true);
                string why = Game.Store.CanBuy(p);
                if (GuiKit.Button(new Rect(card.x + 20, card.yMax - 78, card.width - 40, 62), Game.Store.Platform.Available ? $"{p.PriceUsd:0.00} USD" : L("shop.soon"), why.Length == 0, 24)) Apply(Game.Store.Buy(p.Id));
            }
            float y = r.y + 316;
            GuiKit.Label(new Rect(r.x, y, r.width, 34), "Convertir Perlas en Sales de Baño (1 a 1)", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 40;
            long[] amounts = { 160, 1600 };
            for (int i = 0; i < amounts.Length; i++)
                if (GuiKit.Button(new Rect(r.x + i * 240, y, 230, 56), $"{amounts[i]:N0} Perlas", Game.Ledger.Balance(Res.Perlas) >= amounts[i], 22)) Apply(Game.Store.ConvertPerlas(amounts[i]));
            y += 72;
            var st = Game.Profile.Store;
            GuiKit.Label(new Rect(r.x, y, r.width, 34), $"{L("set.limit")}: {(st.SpendLimitUsd > 0 ? st.SpendLimitUsd.ToString("0.00") + " USD" : "sin límite")}  ·  este mes: {Game.Store.SpentThisMonth():0.00} USD", 22, Palette.Parchment);
            y += 42;
            double[] limits = { 0, 10, 30, 100 };
            for (int i = 0; i < limits.Length; i++)
                if (GuiKit.Button(new Rect(r.x + i * 200, y, 190, 52), limits[i] == 0 ? "Sin límite" : limits[i].ToString("0") + " USD", true, 20)) Apply(Game.Store.SetSpendLimit(limits[i]));
            if (DevTools && st.Purchases.Count > 0)
            {
                var last = st.Purchases[st.Purchases.Count - 1];
                if (GuiKit.Button(new Rect(r.xMax - 400, y, 400, 52), $"Reembolso de prueba: {last.Product}", !last.Refunded, 18)) Apply(Game.Store.Refund(last.Receipt));
            }
        }

        private void ShopCosmetics(Rect r)
        {
            var list = _meta.World.Shop.Cosmetics.Where(c => c.Perlas > 0 || c.Polvo > 0).ToList();
            _shopPage = Paged(r, list, _shopPage, 78, (box, c) =>
            {
                bool own = Game.Store.OwnsCosmetic(c.Id);
                Theme.Panel(box, Palette.Hex("#3A2C40"), Palette.Hex("#1A1420"), own ? Palette.FiveStar : Theme.GoldDark);
                if (ColorUtility.TryParseHtmlString(c.Color, out var col)) GuiKit.Dot(new Vector2(box.x + 38, box.center.y), 24, col);
                GuiKit.Label(new Rect(box.x + 74, box.y + 4, box.width - 560, 34), c.Name + (c.Witch.Length > 0 ? "  ·  " + WitchName(c.Witch) : ""), 23, Palette.Parchment, TextAnchor.MiddleLeft, true);
                GuiKit.Label(new Rect(box.x + 74, box.y + 38, box.width - 560, 30), c.Description, 17, Palette.ParchmentShadow);
                if (own) { GuiKit.Label(new Rect(box.xMax - 470, box.y, 450, box.height), "En tu Espejo", 22, Palette.FiveStar, TextAnchor.MiddleRight, true); return; }
                if (c.Polvo > 0 && GuiKit.Button(new Rect(box.xMax - 460, box.y + 10, 220, box.height - 20), $"{c.Polvo:N0} Polvo", Game.Ledger.Balance(Res.PolvoAstral) >= c.Polvo, 19)) Apply(Game.Store.BuyCosmetic(c.Id, true));
                if (c.Perlas > 0 && GuiKit.Button(new Rect(box.xMax - 230, box.y + 10, 220, box.height - 20), $"{c.Perlas:N0} Perlas", Game.Ledger.Balance(Res.Perlas) >= c.Perlas, 19)) Apply(Game.Store.BuyCosmetic(c.Id));
            });
        }

        private void ShopPass(Rect r)
        {
            var shop = _meta.World.Shop;
            var diezmo = shop.Products.Find(p => p.Kind == ProductKind.Diezmo);
            var grimorio = shop.Products.Find(p => p.Kind == ProductKind.Grimorio);
            var st = Game.Profile.Store;
            var top = new Rect(r.x, r.y, r.width / 2 - 8, 170);
            Theme.Panel(top, Palette.Hex("#4A3A20"), Palette.Hex("#1A1420"), Palette.GoldBright);
            Theme.Title(new Rect(top.x + 20, top.y + 8, top.width - 40, 40), diezmo.Name, 28, Palette.GoldBright, TextAnchor.MiddleLeft);
            GuiKit.Label(new Rect(top.x + 20, top.y + 50, top.width - 40, 54), $"{diezmo.Perlas} Perlas al comprar y {diezmo.DailySales} Sales de Baño diarias por {diezmo.Days} días." + (Game.Store.DiezmoActive ? $" Te quedan {st.DiezmoUntilDay - Game.Profile.Day} días." : ""), 18, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            if (GuiKit.Button(new Rect(top.x + 20, top.yMax - 62, 240, 52), Game.Store.Platform.Available ? $"{diezmo.PriceUsd:0.00} USD" : L("shop.soon"), Game.Store.CanBuy(diezmo).Length == 0, 21)) Apply(Game.Store.Buy(diezmo.Id));
            if (GuiKit.Button(new Rect(top.x + 270, top.yMax - 62, 240, 52), "Cobrar hoy", Game.Store.DiezmoActive && st.DiezmoClaimedDay != Game.Profile.Day, 21)) Apply(Game.Store.ClaimDiezmo());

            var pass = new Rect(r.x + r.width / 2 + 8, r.y, r.width / 2 - 8, 170);
            Theme.Panel(pass, Palette.Hex("#4A2C48"), Palette.Hex("#1A1420"), Palette.GoldBright);
            Theme.Title(new Rect(pass.x + 20, pass.y + 8, pass.width - 40, 40), $"Grimorio  ·  nivel {Game.Store.GrimorioLevel}", 28, Palette.GoldBright, TextAnchor.MiddleLeft);
            GuiKit.Label(new Rect(pass.x + 20, pass.y + 50, pass.width - 40, 54), $"Carril gratis para todas. El premium solo trae Sales de Baño y cosméticos. Experiencia: {st.GrimorioXp:N0}.", 18, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            if (GuiKit.Button(new Rect(pass.x + 20, pass.yMax - 62, 300, 52), st.GrimorioPremium ? "Premium activo" : $"Premium  ·  {(Game.Store.Platform.Available ? grimorio.PriceUsd.ToString("0.00") + " USD" : L("shop.soon"))}", !st.GrimorioPremium && Game.Store.Platform.Available, 21)) Apply(Game.Store.Buy(grimorio.Id));

            float y = r.y + 186, w = r.width / shop.Pass.Count;
            for (int i = 0; i < shop.Pass.Count; i++)
            {
                var t = shop.Pass[i];
                var card = new Rect(r.x + i * w, y, w - 10, r.yMax - y);
                bool reached = Game.Store.GrimorioLevel >= t.Level;
                Theme.Panel(card, reached ? Palette.Hex("#5A4A28") : Palette.Hex("#2C2234"), Palette.Hex("#17121E"), reached ? Palette.GoldBright : Theme.GoldDark);
                Theme.Title(new Rect(card.x, card.y + 8, card.width, 40), "Nivel " + t.Level, 26, reached ? Palette.FiveStar : Palette.ParchmentShadow);
                string free = string.Join(", ", t.Free.Where(kv => kv.Value > 0).Select(kv => $"{kv.Value} {_meta.Catalog.NameOf(kv.Key)}"));
                string prem = string.Join(", ", t.Premium.Select(kv => $"{kv.Value} {_meta.Catalog.NameOf(kv.Key)}"));
                if (t.PremiumCosmetic.Length > 0) prem += ", " + shop.Cosmetic(t.PremiumCosmetic).Name;
                GuiKit.Label(new Rect(card.x + 14, card.y + 54, card.width - 28, 150), $"<b>Gratis:</b> {(free.Length > 0 ? free : "—")}\n\n<b>Premium:</b> {prem}", 17, Palette.Parchment, TextAnchor.UpperLeft, false, true);
                bool canClaim = reached && (!st.PassFreeClaimed.Contains(t.Level) || (st.GrimorioPremium && !st.PassPremiumClaimed.Contains(t.Level)));
                if (GuiKit.Button(new Rect(card.x + 14, card.yMax - 66, card.width - 28, 54), "Cobrar", canClaim, 20)) Apply(Game.Store.ClaimTier(t.Level));
            }
        }

        // ------------------------------------------------------------------ Ajustes

        private void DrawSettings(Rect a)
        {
            var body = Header(a, L("menu.settings"));
            Theme.GoldPanel(body, 0.92f);
            var s = Game.Profile.Settings;
            float x = body.x + 30, y = body.y + 22;
            GuiKit.Label(new Rect(x, y, 260, 56), L("set.language"), 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            string[] langs = { "es", "en", "ja" }, names = { "Español", "English", "日本語" };
            for (int i = 0; i < 3; i++)
            {
                var b = new Rect(x + 260 + i * 190, y, 180, 56);
                if (GuiKit.Button(b, names[i], true, 22)) { s.Language = langs[i]; Save(); }
                if (s.Language == langs[i]) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            y += 70;
            GuiKit.Label(new Rect(x, y, 260, 56), L("set.country"), 24, Palette.Parchment, TextAnchor.MiddleLeft, true);
            string[] countries = { "MX", "US", "BE", "JP" };
            for (int i = 0; i < countries.Length; i++)
            {
                var b = new Rect(x + 260 + i * 140, y, 130, 56);
                if (GuiKit.Button(b, countries[i], true, 22)) { s.Country = countries[i]; Save(); }
                if (s.Country == countries[i]) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            GuiKit.Label(new Rect(x + 840, y, 900, 56), Game.GachaService.PaidAllowed ? "" : "En este país las tiradas solo usan Sales de Baño ganadas jugando.", 19, Palette.GoldBright);
            y += 80;

            Toggle(ref y, x, L("set.modest"), ref s.ModestMode);
            Toggle(ref y, x, L("set.large"), ref s.LargeText);
            Toggle(ref y, x, L("set.colorsafe"), ref s.ColorSafe);
            Toggle(ref y, x, L("set.skip"), ref s.SkipCinematics);
            Toggle(ref y, x, L("set.flash"), ref s.ReduceFlashes);
            Toggle(ref y, x, L("set.motion"), ref s.ReduceMotion);
            Toggle(ref y, x, L("set.battery"), ref s.BatterySaver);
            Toggle(ref y, x, L("set.quality"), ref s.HighQuality);

            var side = new Rect(body.x + 880, body.y + 170, body.width - 910, 420);
            Theme.Panel(side, Palette.Hex("#2C2234"), Palette.Hex("#17121E"), Theme.GoldDark);
            GuiKit.Label(new Rect(side.x + 20, side.y + 14, side.width - 40, 36), L(DevTools ? "set.accountDev" : "set.account"), 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            // Borrar la cuenta pide un segundo toque: no se puede deshacer.
            bool armed = Time.unscaledTime < _deleteArmedUntil;
            if (GuiKit.Button(new Rect(side.x + 20, side.y + 60, side.width - 40, 62), L(armed ? "set.deleteConfirm" : "set.delete"), true, 20))
            {
                if (armed) { _deleteArmedUntil = 0; _store.Delete(Slot); NewPlayer(); }
                else { _deleteArmedUntil = Time.unscaledTime + 4f; Note(L("set.deleteWarn"), false); } // TUNING: 4 s para confirmar
            }
            if (!DevTools) return;
            if (GuiKit.Button(new Rect(side.x + 20, side.y + 132, side.width - 40, 62), "Cargar perfil de prueba", true, 20)) NewTestProfile();
            if (GuiKit.Button(new Rect(side.x + 20, side.y + 204, side.width - 40, 62), "Combate libre (Fase 1)", !F.InRun, 20)) OpenFormation?.Invoke();
            var an = Game.Analytics as LocalAnalytics;
            GuiKit.Label(new Rect(side.x + 20, side.y + 280, side.width - 40, 120), $"Analítica local: {an?.Events.Count ?? 0} eventos en esta sesión.\nServidor: backend falso en memoria (sin Unity Gaming Services todavía).", 17, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
        }

        private void Toggle(ref float y, float x, string label, ref bool value)
        {
            if (GuiKit.Button(new Rect(x, y, 800, 54), (value ? "● " : "○ ") + label, true, 22))
            {
                value = !value;
                Save();
                ApplyAccessibility();
            }
            y += 60;
        }

        // ------------------------------------------------------------------ Probabilidades (GDD 16.6)

        private int _oddsPage;

        private void DrawOdds(Rect a)
        {
            var body = Header(a, L("menu.odds"), "Tasas, contador e historial de un año");
            var banners = _meta.Gacha.Banners;
            var left = new Rect(body.x, body.y, 1000, body.height);
            Theme.GoldPanel(left, 0.92f);
            for (int i = 0; i < banners.Count; i++)
            {
                var b = new Rect(left.x + 14 + i * 244, left.y + 12, 236, 62);
                if (GuiKit.Button(b, banners[i].Name, true, 16)) _banner = banners[i].Id;
                if (_banner == banners[i].Id) GuiKit.Frame(b, Palette.FiveStar, 3f);
            }
            float y = left.y + 90;
            foreach (var row in Game.GachaService.OddsPage(_banner))
            {
                // La fila crece con el texto: la Garantía ocupa dos o tres líneas y antes se cortaba.
                float vw = left.width - 350;
                float h = Mathf.Max(40f, GuiKit.Style(20, Palette.Parchment, TextAnchor.UpperLeft, false, true).CalcHeight(new GUIContent(row.Value), vw) + 8);
                GuiKit.Label(new Rect(left.x + 24, y, 300, 40), row.Key, 20, Palette.GoldBright, TextAnchor.MiddleLeft, true);
                GuiKit.Label(new Rect(left.x + 330, y + (h > 40 ? 8 : 0), vw, h), row.Value, 20, Palette.Parchment, h > 40 ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, false, true);
                y += h + 6;
            }
            var right = new Rect(left.xMax + 10, body.y, body.width - left.width - 10, body.height);
            Theme.GoldPanel(right, 0.92f);
            GuiKit.Label(new Rect(right.x + 14, right.y + 8, right.width - 28, 36), "Historial de un año", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            _oddsPage = Paged(new Rect(right.x + 14, right.y + 50, right.width - 28, right.height - 60), Game.GachaService.YearHistory(), _oddsPage, 30, (box, h) =>
            {
                GuiKit.Fill(new Rect(box.x, box.y + 4, 8, 18), VaporColor(h.Rarity));
                string what = h.Kind == "bruja" ? WitchName(h.Id) : h.Kind == "polvo" ? $"{h.Amount} Polvo Astral" : "Receta y Sal Cruda";
                GuiKit.Label(new Rect(box.x + 14, box.y, box.width - 14, box.height), $"Día {h.Day} · {Stars(h.Rarity)} {what} · tirada {h.Pity}", 15, h.Rarity == 5 ? Palette.FiveStar : Palette.Parchment);
            });
        }
    }
}
