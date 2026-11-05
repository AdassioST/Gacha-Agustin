using System;
using System.Collections.Generic;
using SalYAlmas.Core.Economy;
using SalYAlmas.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>Barra superior (cuenta y monedas) y barra de navegación. El manantial va al centro, como medallón.</summary>
    public sealed class UiShell
    {
        private sealed class Wallet { public string Res; public TextMeshProUGUI Value; public long Shown; public RectTransform Icon; public long Target; }
        private sealed class NavItem { public MetaHub.Screen Screen; public RectTransform Root; public Image Icon, Glow, Active; public TextMeshProUGUI Label; public bool Center; }

        private readonly UiApp _app;
        private readonly RectTransform _top, _nav, _topRoot, _navRoot;
        private RectTransform _topLeft, _topRight;
        // TUNING: el marco ocupa menos alto para que las brujas tengan el centro. Antes 130 y 132 px; contenido al 82 %.
        private const float TopHeight = 108f, NavHeight = 110f, ChromeScale = 0.82f, NavScale = 0.84f;
        private readonly List<Wallet> _wallets = new List<Wallet>();
        private readonly List<NavItem> _items = new List<NavItem>();
        private TextMeshProUGUI _name, _height;
        private UiBar _heightBar;

        public UiShell(UiApp app, RectTransform safe)
        {
            _app = app;
            // Las barras pintadas llegan más allá de la zona segura para que el fondo no asome en el notch.
            _topRoot = U.Node(safe, "Barra superior");
            U.At(_topRoot, 0.5f, 1, 0, 0, 0, 0, 0.5f, 1);
            _topRoot.anchorMin = new Vector2(0, 1);
            _topRoot.anchorMax = new Vector2(1, 1);
            _topRoot.sizeDelta = new Vector2(0, TopHeight);
            _top = _topRoot;
            var tbg = U.Img(_topRoot, "chrome/bar_top", null, "Fondo");
            U.Fill(tbg.rectTransform, -200, -10, -200, -60);

            _navRoot = U.Node(safe, "Barra inferior");
            _navRoot.anchorMin = new Vector2(0, 0);
            _navRoot.anchorMax = new Vector2(1, 0);
            _navRoot.pivot = new Vector2(0.5f, 0);
            _navRoot.anchoredPosition = Vector2.zero;
            _navRoot.sizeDelta = new Vector2(0, NavHeight);
            _nav = _navRoot;
            var nbg = U.Img(_navRoot, "chrome/bar_bottom", null, "Fondo");
            U.Fill(nbg.rectTransform, -200, -60, -200, 0);

            BuildTop();
            BuildNav();
        }

        /// <summary>Grupo que se encoge hacia su esquina: lo de la izquierda hacia la izquierda y las monedas hacia la derecha.</summary>
        private RectTransform CornerGroup(string name, float pivotX)
        {
            var g = U.Fill(U.Node(_top, name));
            g.pivot = new Vector2(pivotX, 1);
            g.localScale = new Vector3(ChromeScale, ChromeScale, 1);
            return g;
        }

        private void BuildTop()
        {
            _topLeft = CornerGroup("Cuenta y altura", 0);
            _topRight = CornerGroup("Monedas", 1);
            // Cuenta
            var crest = U.Btn(_topLeft, "chrome/seal_dark", null, 0, () => _app.Go(MetaHub.Screen.Settings), null, null, "Cuenta");
            U.At(crest.Background.rectTransform, 0, 1, 26, -12, 92, 92, 0, 1);
            var hat = U.Img(crest.transform, "iconsg/settings", null, "Ajustes");
            U.Fill(hat.rectTransform, 19, 19, 19, 19);
            _name = U.Txt(_topLeft, "", 30, UiTheme.Parchment, UiTheme.Title);
            _name.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0, 0, 0, 0.85f));
            U.At(_name.rectTransform, 0, 1, 132, -14, 380, 38, 0, 1);
            _height = U.Txt(_topLeft, "", 21, UiTheme.GoldBright, UiTheme.Bold);
            _height.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0, 0, 0, 0.85f));
            U.At(_height.rectTransform, 0, 1, 132, -52, 120, 28, 0, 1);
            _heightBar = UiBar.Make(_topLeft, UiTheme.Hex("#D08A4A"), "Altura");
            U.At(_heightBar.GetComponent<RectTransform>(), 0, 1, 232, -56, 250, 20, 0, 1);

            // Monedas, de mayor a menor peso.
            float x = -26;
            var shop = U.Btn(_topRight, "chrome/seal_dark", null, 0, () => _app.Go(MetaHub.Screen.Shop), null, null, "Tienda");
            U.At(shop.Background.rectTransform, 1, 1, x, -18, 62, 62, 1, 1);
            var plus = U.Img(shop.transform, "iconsg/plus", null, "Más");
            U.Fill(plus.rectTransform, 15, 15, 15, 15);
            x -= 76;
            AddWallet(ref x, Res.Perlas, "currency/perlas", 196);
            AddWallet(ref x, Res.SalesDeBano, "currency/sales_de_bano", 196);
            AddWallet(ref x, Res.Almitas, "currency/almitas", 214);
        }

        private void AddWallet(ref float x, string res, string icon, float w)
        {
            var pill = U.Img(_topRight, "chrome/btn_secondary", null, "Moneda " + res);
            U.At(pill.rectTransform, 1, 1, x, -24, w, 52, 1, 1);
            var im = U.Img(pill.transform, icon, null, "Icono");
            U.At(im.rectTransform, 0, 0.5f, -14, 0, 66, 66, 0, 0.5f);
            var t = U.Txt(pill.transform, "0", 26, UiTheme.Salt, UiTheme.Bold, TextAlignmentOptions.MidlineRight);
            U.Fill(t.rectTransform, 50, 2, 18, 2);
            _wallets.Add(new Wallet { Res = res, Value = t, Icon = im.rectTransform });
            x -= w + 22;
        }

        private void BuildNav()
        {
            var bar = U.Node(_nav, "Fila");
            bar.anchorMin = new Vector2(0.5f, 0);
            bar.anchorMax = new Vector2(0.5f, 0);
            bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(1320, 120);
            bar.anchoredPosition = new Vector2(0, 2);
            bar.localScale = new Vector3(NavScale, NavScale, 1);
            (MetaHub.Screen s, string icon, string key)[] defs =
            {
                (MetaHub.Screen.Home, "home", "nav.home"), (MetaHub.Screen.Coven, "coven", "nav.coven"), (MetaHub.Screen.Summon, "spring", "dest.summon"),
                (MetaHub.Screen.Forge, "forge", "nav.forge"), (MetaHub.Screen.Cauldron, "cauldron", "nav.cauldron"), (MetaHub.Screen.Shop, "shop", "nav.shop"),
            };
            float step = 1320f / defs.Length;
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                bool center = d.s == MetaHub.Screen.Summon;
                var it = new NavItem { Screen = d.s, Center = center };
                var hit = U.Img(bar, null, new Color(0, 0, 0, 0), d.key, true);
                it.Root = hit.rectTransform;
                U.At(it.Root, 0, 0, step * i + step / 2, 0, step - 6, 118, 0.5f, 0);
                var btn = hit.gameObject.AddComponent<UiBtn>();
                btn.Background = null;
                var s = d.s;
                btn.Click = () => _app.Go(s);

                it.Glow = U.Img(it.Root, "fx/soft_dot", new Color(1, 0.8f, 0.45f, 0), "Luz");
                it.Glow.material = UiTheme.Mat("Additive");
                if (center)
                {
                    U.At(it.Glow.rectTransform, 0.5f, 0.5f, 0, 36, 260, 260, 0.5f, 0.5f);
                    var ring = U.Img(it.Root, "fx/ring", new Color(1, 0.84f, 0.5f, 0.9f), "Anillo");
                    U.At(ring.rectTransform, 0.5f, 0.5f, 0, 36, 168, 168, 0.5f, 0.5f);
                    ring.gameObject.AddComponent<UiSpin>().Speed = 10;
                    var seal = U.Img(it.Root, "chrome/seal_dark", null, "Medallón");
                    U.At(seal.rectTransform, 0.5f, 0.5f, 0, 36, 148, 148, 0.5f, 0.5f);
                    it.Icon = U.Img(it.Root, "iconsg/" + d.icon, null, "Icono");
                    U.At(it.Icon.rectTransform, 0.5f, 0.5f, 0, 36, 100, 100, 0.5f, 0.5f);
                }
                else
                {
                    U.At(it.Glow.rectTransform, 0.5f, 0, 0, 60, 170, 170, 0.5f, 0.5f);
                    it.Icon = U.Img(it.Root, "iconsg/" + d.icon, null, "Icono");
                    U.At(it.Icon.rectTransform, 0.5f, 0, 0, 40, 76, 76, 0.5f, 0);
                }
                it.Label = U.Txt(it.Root, UiLoc.Get(d.key), 23, UiTheme.ParchmentDim, UiTheme.Bold, TextAlignmentOptions.Center);
                it.Label.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0, 0, 0, 0.9f), 0.3f, -0.6f, 0.05f);
                U.At(it.Label.rectTransform, 0.5f, 0, 0, 8, step, 30, 0.5f, 0);
                it.Active = U.Img(it.Root, null, UiTheme.GoldBright, "Pestaña activa");
                U.At(it.Active.rectTransform, 0.5f, 0, 0, 1, 132, 5, 0.5f, 0);
                it.Active.gameObject.SetActive(false);
                _items.Add(it);
            }
        }

        public bool Visible => _topRoot.gameObject.activeSelf;

        public void Show(bool on)
        {
            _topRoot.gameObject.SetActive(on);
            _navRoot.gameObject.SetActive(on);
        }

        /// <summary>Las barras llegan deslizándose por los bordes.</summary>
        public void Enter()
        {
            Tw.Kill(this);
            U.Slide(_topRoot, new Vector2(0, 150), 0.55f, 0.02f, this);
            U.Slide(_navRoot, new Vector2(0, -170), 0.55f, 0.02f, this);
        }

        public void Mark(MetaHub.Screen current)
        {
            foreach (var it in _items)
            {
                bool on = it.Screen == current;
                it.Active.gameObject.SetActive(on);
                var c = it.Glow.color;
                c.a = on ? 0.5f : 0f;
                it.Glow.color = c;
                it.Label.color = on ? UiTheme.Spark : UiTheme.ParchmentDim;
                if (!it.Center) it.Icon.rectTransform.localScale = Vector3.one * (on ? 1.12f : 1f);
            }
        }

        public void Refresh()
        {
            var g = _app.Game.Game;
            string n = g.Profile.Calculista.Name;
            _name.text = string.IsNullOrEmpty(n) ? "Calculista" : n;
            var f = g.Fresno;
            int height = f.OfrendaLevel + 1;
            _height.text = UiLoc.Get("top.level") + " " + height;
            long prev = height <= 1 ? 0 : g.Meta.Fresno.Ofrenda.Threshold(height - 1);
            long next = f.NextOfrendaThreshold;
            _heightBar.Set(next > prev ? (float)(g.Profile.Fresno.OfrendaTotal - prev) / (next - prev) : 0);
            foreach (var w in _wallets) w.Target = g.Ledger.Balance(w.Res);
        }

        /// <summary>Los saldos ruedan hasta el valor nuevo. Un cambio de moneda se tiene que notar.</summary>
        public void Tick(float dt)
        {
            foreach (var w in _wallets)
            {
                if (w.Shown == w.Target) { if (w.Value.text == "0" && w.Target != 0) w.Value.text = U.Compact(w.Target); continue; }
                long d = w.Target - w.Shown;
                long step = (long)(d * Mathf.Min(1f, dt * 8f));
                if (step == 0) step = Math.Sign(d);
                w.Shown += step;
                w.Value.text = U.Compact(w.Shown);
            }
        }
    }
}
