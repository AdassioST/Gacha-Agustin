using System.Collections.Generic;
using SalYAlmas.Core.Save;
using SalYAlmas.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// El Aquelarre: la colección como un folio de retratos. Rareza con marco y estrellas, planeta con sello y color (el color
    /// nunca va solo). Se desliza de lado, sin paginar. Al tocar una bruja se abre su hoja sobre el mismo folio.
    /// </summary>
    public sealed class UiCoven : UiScreen
    {
        private RectTransform _content, _sheet, _filters;
        private ScrollRect _scroll;
        private TextMeshProUGUI _count;
        private int _filter;
        private readonly List<RectTransform> _cards = new List<RectTransform>();

        public override void Build()
        {
            var bg = U.Img(Root, "bg/abyss", null, "Fondo");
            UiBackdrop.Cover(bg.rectTransform);
            var wheel = U.Img(Root, "fx/astrolabe", new Color(1f, 0.82f, 0.55f, 0.1f), "Rueda");
            wheel.material = UiTheme.Mat("Additive");
            U.At(wheel.rectTransform, 0.5f, 0.5f, 0, 0, 1400, 1400, 0.5f, 0.5f);
            wheel.gameObject.AddComponent<UiSpin>().Speed = 0.8f;
            UiAmbience.Almitas(Root, new Rect(-1000, -540, 2000, 1000), 1.6f);

            var title = U.Txt(Root, UiLoc.Get("coven.title"), 60, UiTheme.Spark, UiTheme.Gothic);
            title.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Gothic, new Color(0, 0, 0, 1), 0.5f, -1, 0.12f);
            // TUNING: título y filtros más bajos de alto para que el folio de retratos tome la pantalla.
            U.At(title.rectTransform, 0, 1, 52, -100, 620, 70, 0, 1);
            _count = U.Txt(Root, "", 23, UiTheme.ParchmentDim, UiTheme.Italic);
            U.At(_count.rectTransform, 0, 1, 380, -126, 420, 32, 0, 1);

            _filters = U.Node(Root, "Filtros");
            U.At(_filters, 1, 1, -36, -110, 680, 66, 1, 1);
            _filters.localScale = new Vector3(0.84f, 0.84f, 1);

            var view = U.Node(Root, "Vista");
            U.Fill(view, 0, 108, 0, 178);
            view.gameObject.AddComponent<RectMask2D>();
            var hitImg = view.gameObject.AddComponent<Image>();
            hitImg.color = new Color(0, 0, 0, 0);
            _content = U.Node(view, "Folio");
            _content.anchorMin = new Vector2(0, 0);
            _content.anchorMax = new Vector2(0, 1);
            _content.pivot = new Vector2(0, 0.5f);
            _content.anchoredPosition = Vector2.zero;
            _scroll = view.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = view;
            _scroll.content = _content;
            _scroll.horizontal = true;
            _scroll.vertical = false;
            _scroll.movementType = ScrollRect.MovementType.Elastic;
            _scroll.decelerationRate = 0.12f;
            _scroll.scrollSensitivity = 60;
        }

        public override void Refresh()
        {
            U.Clear(_filters);
            string[] labels = { UiLoc.Get("coven.all"), "5★", "4★", "3★" };
            for (int i = 0; i < labels.Length; i++)
            {
                int cap = i;
                var b = U.Btn(_filters, i == _filter ? "chrome/btn_primary" : "chrome/btn_secondary", labels[i], 27, () => { _filter = cap; UiSfx.Play("tick"); Refresh(); Stagger(); }, null, null, "Filtro " + labels[i]);
                U.At(b.Background.rectTransform, 0, 0.5f, i * 170, 0, 160, 62, 0, 0.5f);
            }

            int total = 0, owned = 0;
            foreach (var id in G.Meta.RosterOrder) { total++; if (G.Profile.Owns(id)) owned++; }
            _count.text = UiLoc.F("coven.count", owned, total);

            U.Clear(_content);
            _cards.Clear();
            // TUNING: los retratos crecen un 12 % con escala, sin rehacer el diseño interno de la carta (268×344).
            const float cw = 268, ch = 344, gap = 14, margin = 48, k = 1.12f;
            int col = 0, row = 0;
            foreach (var id in G.Meta.RosterOrder)
            {
                var e = G.Meta.Roster[id];
                if (_filter == 1 && e.Rarity != 5 || _filter == 2 && e.Rarity != 4 || _filter == 3 && e.Rarity != 3) continue;
                var card = BuildCard(id, e.Rarity);
                U.At(card, 0, 1, margin + col * (cw * k + gap), -row * (ch * k + gap) - 6, cw, ch, 0, 1);
                card.localScale = new Vector3(k, k, 1);
                _cards.Add(card);
                row++;
                if (row >= 2) { row = 0; col++; }
            }
            int cols = col + (row > 0 ? 1 : 0);
            _content.sizeDelta = new Vector2(margin * 2 + cols * (cw * k + gap), 0);
            _scroll.horizontalNormalizedPosition = 0;
        }

        private RectTransform BuildCard(string id, int rarity)
        {
            var e = G.Meta.Roster[id];
            bool owned = G.Profile.Owns(id);
            var color = UiTheme.Planet(e.Primary);
            var root = U.Node(_content, id);
            var hit = U.Img(root, null, new Color(0, 0, 0, 0.001f), "Toque", true);
            U.Fill(hit.rectTransform);

            var surf = U.Tile(root, "chrome/tile_leather", null, "Cuero");
            U.Fill(surf.rectTransform, 5, 5, 5, 5);
            // Luz del planeta tras el retrato: el mismo folio, con el color de cada bruja.
            var glow = U.Img(root, "fx/soft_dot", new Color(color.r, color.g, color.b, owned ? 0.52f : 0.14f), "Luz");
            glow.material = UiTheme.Mat("Additive");
            U.At(glow.rectTransform, 0.5f, 0.62f, 0, 0, 420, 420, 0.5f, 0.5f);

            var bust = U.Img(root, null, null, "Busto");
            bust.preserveAspect = true;
            var sp = UiWitchArt.Bust(G, id);
            if (sp != null) bust.sprite = sp;
            else
            {
                bust.sprite = UiTheme.Sp("fx/silhouette");
                bust.color = Color.Lerp(color, Color.black, 0.5f);
            }
            U.At(bust.rectTransform, 0.5f, 1, 0, -6, 254, 254, 0.5f, 1);
            if (!owned)
            {
                bust.material = UiTheme.NewMat("Fx");
                bust.material.SetFloat("_Gray", 0.85f);
                bust.material.SetFloat("_Dark", 0.55f);
            }

            var shade = U.Img(root, "chrome/shade_down", null, "Sombra");
            U.Fill(shade.rectTransform, 5, 5, 5, 5);

            var stars = U.Txt(root, new string('★', rarity), 26, UiTheme.Rarity(rarity), UiTheme.Bold, TextAlignmentOptions.Left);
            stars.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0, 0, 0, 1), 0.3f, -0.6f, 0.08f);
            U.At(stars.rectTransform, 0, 0, 18, 72, 200, 30, 0, 0);

            string shortName = e.Name;
            int cut = shortName.IndexOf(" (", System.StringComparison.Ordinal);
            if (cut > 0) shortName = shortName.Substring(0, cut);
            var name = U.Txt(root, shortName, 28, owned ? UiTheme.Salt : UiTheme.ParchmentDim, UiTheme.Title);
            name.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0, 0, 0, 1), 0.3f, -0.6f, 0.06f);
            name.enableAutoSizing = true;
            name.fontSizeMin = 18;
            name.fontSizeMax = 28;
            U.At(name.rectTransform, 0, 0, 18, 38, 232, 34, 0, 0);

            string line = owned ? UiLoc.F("coven.level", G.Profile.Witches[id].Level) + "  ·  " + UiLoc.F("coven.grade", G.Profile.Witches[id].Grade) : UiLoc.Get("coven.locked");
            var info = U.Txt(root, line, 21, UiTheme.ParchmentDim, UiTheme.Body);
            U.At(info.rectTransform, 0, 0, 18, 12, 232, 26, 0, 0);

            var pl = U.Img(root, "seals/" + UiTheme.PlanetId(e.Primary), null, "Planeta");
            U.At(pl.rectTransform, 1, 1, -8, -8, 56, 56, 1, 1);

            var frame = U.Img(root, "chrome/frame_r" + Mathf.Clamp(rarity, 3, 5), owned ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f), "Marco");
            frame.fillCenter = false;
            U.Fill(frame.rectTransform);
            if (rarity >= 5 && owned)
            {
                frame.material = UiTheme.NewMat("Shimmer");
                frame.material.SetFloat("_Pause", 2.8f + (id.Length % 3));
            }
            if (!owned)
            {
                var lk = U.Img(root, "iconsg/lock", new Color(1, 1, 1, 0.75f), "Candado");
                U.At(lk.rectTransform, 0.5f, 0.62f, 0, 0, 66, 66, 0.5f, 0.5f);
            }

            var b = root.gameObject.AddComponent<UiBtn>();
            b.PressScale = 0.965f;
            b.Click = () => OpenSheet(id);
            return root;
        }

        private void Stagger()
        {
            int i = 0;
            foreach (var c in _cards) U.Rise(c, 26, 0.36f, 0.018f * i++);
        }

        public override void OnShown() => Stagger();

        public override void OnHidden()
        {
            if (_sheet != null) { Object.Destroy(_sheet.gameObject); _sheet = null; }
        }

        private void OpenSheet(string id)
        {
            if (_sheet != null) Object.Destroy(_sheet.gameObject);
            var e = G.Meta.Roster[id];
            bool owned = G.Profile.Owns(id);
            _sheet = U.Fill(U.Node(Root, "Hoja"));
            var veil = U.Img(_sheet, null, new Color(0.03f, 0.02f, 0.05f, 0.76f), "Velo", true);
            U.Fill(veil.rectTransform);
            var vb = veil.gameObject.AddComponent<UiBtn>();
            vb.PressScale = 1;
            vb.Sound = "back";
            vb.Click = CloseSheet;
            var color = UiTheme.Planet(e.Primary);
            var glow = U.Img(_sheet, "fx/soft_dot", new Color(color.r, color.g, color.b, 0.5f), "Luz");
            glow.material = UiTheme.Mat("Additive");
            U.At(glow.rectTransform, 0.3f, 0.5f, 0, 0, 1100, 1100, 0.5f, 0.5f);
            var ring = U.Img(_sheet, "fx/astrolabe", new Color(color.r, color.g, color.b, 0.5f), "Carta natal");
            ring.material = UiTheme.Mat("Additive");
            U.At(ring.rectTransform, 0.3f, 0.52f, 0, 0, 800, 800, 0.5f, 0.5f);
            ring.gameObject.AddComponent<UiSpin>().Speed = 4;

            var body = U.Img(_sheet, null, null, "Cuerpo");
            var sp = UiWitchArt.Body(G, id);
            if (sp != null) body.sprite = sp;
            else { body.sprite = UiTheme.Sp("fx/silhouette"); body.color = Color.Lerp(color, Color.black, 0.4f); }
            body.material = UiTheme.NewMat("Hero");
            U.At(body.rectTransform, 0.3f, 0, 0, 90, 620, 900, 0.5f, 0);
            UiWitchArt.FitHeight(body, 900, 860);
            if (!owned) { body.material = UiTheme.NewMat("Fx"); body.material.SetFloat("_Gray", 0.85f); body.material.SetFloat("_Dark", 0.45f); }
            U.Rise(body.rectTransform, 60, 0.55f);

            var paper = U.Paper(_sheet, "Hoja de datos");
            U.At(paper, 1, 0.5f, -60, 0, 720, 800, 1, 0.5f);
            U.Slide(paper, new Vector2(240, 0), 0.5f);
            var g = U.Group(paper);
            U.FadeIn(g, 0.3f);

            var nm = U.Txt(paper, e.Name, 62, UiTheme.Ink, UiTheme.Gothic);
            nm.enableAutoSizing = true;
            nm.fontSizeMin = 38;
            nm.fontSizeMax = 62;
            U.At(nm.rectTransform, 0, 1, 56, -46, 610, 78, 0, 1);
            var st = U.Txt(paper, new string('★', e.Rarity) + "   " + UiLoc.Get("rank." + e.Rarity), 30, UiTheme.Crimson, UiTheme.Bold);
            U.At(st.rectTransform, 0, 1, 58, -124, 600, 38, 0, 1);
            var div = U.Img(paper, "chrome/divider", null, "Filete");
            U.At(div.rectTransform, 0.5f, 1, 0, -170, 600, 24, 0.5f, 1);

            float y = -208;
            Row(paper, UiLoc.Get("coven.species"), e.Species, ref y);
            Row(paper, UiLoc.Get("coven.role"), e.Role, ref y);
            PlanetsRow(paper, e, ref y);
            if (owned)
            {
                var w = G.Profile.Witches[id];
                Row(paper, UiLoc.F("coven.level", w.Level), UiLoc.F("coven.grade", w.Grade), ref y);
                Row(paper, UiLoc.F("home.trust", G.Base.TrustLevel(id)), "", ref y);
                bool fav = G.Profile.TermasBase.Favorite == id;
                var favB = U.Btn(paper, fav ? "chrome/btn_secondary" : "chrome/btn_parchment", fav ? UiLoc.Get("home.favorite") : UiLoc.Get("home.setfav"), 27, () =>
                {
                    G.Base.SetFavorite(id);
                    App.Hub.SaveNow();
                    UiSfx.Play("confirm");
                    UiToast.Show(UiLoc.Get("home.favorite") + ": " + e.Name);
                    OpenSheet(id);
                }, null, fav ? UiTheme.Spark : UiTheme.Ink, "Favorita");
                U.At(favB.Background.rectTransform, 0, 0, 50, 56, 300, 84, 0, 0);
                var open = U.Btn(paper, "chrome/btn_primary", UiLoc.Get("coven.open"), 27, () => { var cap = id; App.OpenWitch(cap); }, null, null, "Ficha");
                U.At(open.Background.rectTransform, 1, 0, -50, 56, 300, 84, 1, 0);
            }
            var close = U.Btn(_sheet, "chrome/seal_dark", null, 0, CloseSheet, null, null, "Cerrar");
            U.At(close.Background.rectTransform, 1, 1, -30, -24, 78, 78, 1, 1);
            close.Sound = "back";
            var x = U.Img(close.transform, "iconsg/back", null, "Volver");
            U.Fill(x.rectTransform, 15, 15, 15, 15);
        }

        private static void Row(RectTransform parent, string left, string right, ref float y)
        {
            var a = U.Txt(parent, left, 30, UiTheme.Ink, UiTheme.Bold);
            U.At(a.rectTransform, 0, 1, 58, y, 300, 40, 0, 1);
            var b = U.Txt(parent, right, 30, UiTheme.Ink, UiTheme.Body, TextAlignmentOptions.MidlineRight);
            U.At(b.rectTransform, 1, 1, -58, y, 360, 40, 1, 1);
            y -= 54;
        }

        private static void PlanetsRow(RectTransform parent, Core.Progress.RosterEntry e, ref float y)
        {
            var a = U.Txt(parent, UiLoc.Get("coven.planets"), 30, UiTheme.Ink, UiTheme.Bold);
            U.At(a.rectTransform, 0, 1, 58, y - 8, 300, 40, 0, 1);
            var p1 = U.Img(parent, "seals/" + UiTheme.PlanetId(e.Primary), null, "Primario");
            U.At(p1.rectTransform, 1, 1, -58, y, 64, 64, 1, 1);
            var p2 = U.Img(parent, "seals/" + UiTheme.PlanetId(e.Secondary), null, "Secundario");
            U.At(p2.rectTransform, 1, 1, -130, y - 10, 44, 44, 1, 1);
            y -= 72;
        }

        private void CloseSheet()
        {
            if (_sheet == null) return;
            var s = _sheet;
            _sheet = null;
            var g = U.Group(s);
            Tw.Run(0.2f, p => { if (g != null) g.alpha = 1 - p; }, Ease.Linear, 0, () => { if (s != null) Object.Destroy(s.gameObject); });
        }
    }
}
