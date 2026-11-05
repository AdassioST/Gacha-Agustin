using System;
using System.Collections.Generic;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Save;
using SalYAlmas.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>Arte de las brujas sin deformar. Sin retrato todavía: silueta teñida con su planeta, nunca una imagen estirada.</summary>
    public static class UiWitchArt
    {
        public static bool ThirdPhase(Core.Meta.MetaGame g, string id) =>
            g.Profile.Owns(id) && g.Profile.Witches[id].Grade >= Grade.S && !g.Profile.Settings.ModestMode;

        public static Sprite Body(Core.Meta.MetaGame g, string id)
        {
            var s = ThirdPhase(g, id) ? Load(id + "_f3_cuerpo") : null;
            return s != null ? s : Load(id + "_f1_cuerpo");
        }

        public static Sprite Bust(Core.Meta.MetaGame g, string id)
        {
            var s = ThirdPhase(g, id) ? Load(id + "_f3_busto") : null;
            return s != null ? s : Load(id + "_f1_busto");
        }

        private static Sprite Load(string name) => Resources.Load<Texture2D>("Ui/brujas/" + name) != null ? UiTheme.Sp("brujas/" + name) : null;

        /// <summary>Ajusta el tamaño a una altura conservando la proporción del sprite.</summary>
        public static void FitHeight(Image i, float height, float maxWidth = 99999)
        {
            if (i.sprite == null) return;
            float aspect = i.sprite.rect.width / i.sprite.rect.height;
            float w = height * aspect;
            if (w > maxWidth) { w = maxWidth; height = w / aspect; }
            i.rectTransform.sizeDelta = new Vector2(w, height);
        }
    }

    /// <summary>
    /// El home son las Termas (propuesta META 10.1): tu favorita en la orilla, el resto del equipo a la izquierda, y los
    /// destinos al borde derecho, al alcance del pulgar. Un toque lleva a cada sistema principal.
    /// </summary>
    public sealed class UiHome : UiScreen
    {
        private RectTransform[] _layers;
        private Vector2[] _layerHome;
        private Image _hero, _shadow;
        private Material _heroMat;
        private RectTransform _heroRt;
        private string _heroId = "";
        private int _touches;
        private RectTransform _bubble;
        private TextMeshProUGUI _bubbleText, _heroName, _heroInfo, _dayText, _storySub, _fresnoSub;
        private Image _dayIcon;
        private RectTransform _party, _actions, _dest;
        private readonly List<RectTransform> _cards = new List<RectTransform>();
        private UiParticles _sparks;
        private UiBtn _talkBtn, _bathBtn;
        private GameObject _talkBadge, _bathBadge;
        private float _t, _bubbleHide, _typeT;
        private int _typeTotal;

        public override void Build()
        {
            UiBackdrop.Scene(Root, 1f, out _layers);
            _layerHome = new Vector2[_layers.Length];

            var wheel = U.Img(Root, "fx/astrolabe", new Color(1f, 0.85f, 0.6f, 0.08f), "Rueda natal");
            wheel.material = UiTheme.Mat("Additive");
            U.At(wheel.rectTransform, 0.37f, 0.62f, 0, 0, 980, 980, 0.5f, 0.5f);
            wheel.gameObject.AddComponent<UiSpin>().Speed = 1.2f;
            wheel.transform.SetSiblingIndex(1);

            var veil = U.Img(Root, "fx/grad_v", new Color(1f, 0.82f, 0.74f, 0.22f), "Bruma");
            veil.material = UiTheme.NewMat("Veil");
            veil.rectTransform.anchorMin = new Vector2(0, 0);
            veil.rectTransform.anchorMax = new Vector2(1, 0.46f);
            veil.rectTransform.offsetMin = veil.rectTransform.offsetMax = Vector2.zero;

            // Sombra de contacto y bruja. La sombra evita que parezca pegada.
            _shadow = U.Img(Root, "fx/soft_dot", new Color(0.05f, 0.01f, 0.05f, 0.62f), "Sombra");
            U.At(_shadow.rectTransform, 0.36f, 0, 0, 190, 560, 110, 0.5f, 0.5f);
            _hero = U.Img(Root, null, Color.white, "Favorita", true);
            _heroRt = _hero.rectTransform;
            // TUNING: la favorita baja y crece (antes 860 de alto desde y=165): el marco más delgado le deja sitio.
            U.At(_heroRt, 0.36f, 0, 0, 128, 640, 930, 0.5f, 0);
            _heroMat = UiTheme.NewMat("Hero");
            _hero.material = _heroMat;
            var hb = _hero.gameObject.AddComponent<UiBtn>();
            hb.PressScale = 0.985f;
            hb.Sound = null;
            hb.Click = TouchHero;

            var fx = U.Fill(U.Node(Root, "Efectos"));
            var sub = fx.gameObject.AddComponent<Canvas>();
            sub.overrideSorting = false;
            UiAmbience.Steam(fx, new Rect(-1050, -560, 2100, 170), 3.4f, new Color(1f, 0.92f, 0.86f, 0.2f), 480);
            UiAmbience.Almitas(fx, new Rect(-1000, -480, 1800, 500), 1.4f);
            _sparks = UiParticles.Make(fx, "fx/sparkle", UiTheme.Mat("Additive"), "Chispas");
            _sparks.Gravity = new Vector2(0, -60);

            BuildParty();
            BuildDestinations();
            BuildNameAndBubble();
            BuildDayChip();
        }

        private void BuildNameAndBubble()
        {
            _heroName = U.Txt(Root, "", 50, UiTheme.Spark, UiTheme.Gothic, TextAlignmentOptions.MidlineLeft);
            _heroName.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Gothic, new Color(0.08f, 0.03f, 0.08f, 1), 0.5f, -1f, 0.12f);
            U.At(_heroName.rectTransform, 0, 0, 36, 248, 620, 58, 0, 0);
            _heroInfo = U.Txt(Root, "", 23, UiTheme.Parchment, UiTheme.Bold);
            _heroInfo.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0.08f, 0.03f, 0.08f, 1), 0.4f, -0.7f, 0.07f);
            U.At(_heroInfo.rectTransform, 0, 0, 38, 214, 620, 32, 0, 0);

            _bubble = U.Paper(Root, "Diálogo");
            // Anclado a la favorita (x 0.36), no al borde: en pantallas 20:9 el ancla izquierda la ponía sobre su cara.
            U.At(_bubble, 0.36f, 1, 236, -104, 320, 186, 0, 1);
            var tail = U.Img(_bubble, "chrome/bubble_tail", null, "Cola");
            U.At(tail.rectTransform, 0, 0, 18, -20, 44, 30, 0, 0);
            _bubbleText = U.Txt(_bubble, "", 25, UiTheme.Ink, UiTheme.Body, TextAlignmentOptions.MidlineLeft, true);
            _bubbleText.enableAutoSizing = true;
            _bubbleText.fontSizeMin = 17;
            _bubbleText.fontSizeMax = 25;
            U.Fill(_bubbleText.rectTransform, 26, 20, 26, 20);
            _bubble.gameObject.SetActive(false);
        }

        private void BuildDayChip()
        {
            var chip = U.Node(Root, "Día planetario");
            U.At(chip, 0, 1, 30, -112, 320, 52, 0, 1);
            _dayIcon = U.Img(chip, null, null, "Sello");
            U.At(_dayIcon.rectTransform, 0, 0.5f, 0, 0, 52, 52, 0, 0.5f);
            _dayText = U.Txt(chip, "", 24, UiTheme.Parchment, UiTheme.Title);
            _dayText.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0.05f, 0.02f, 0.06f, 1), 0.45f, -0.8f, 0.1f);
            U.At(_dayText.rectTransform, 0, 0.5f, 62, 0, 260, 36, 0, 0.5f);
        }

        private void BuildParty()
        {
            _party = U.Node(Root, "Equipo");
            U.At(_party, 0, 1, 30, -176, 112, 480, 0, 1);
            _actions = U.Node(Root, "Acciones");
            U.At(_actions, 0, 0, 34, 132, 140, 0, 0, 0);
        }

        private RectTransform MakeDestCard(string art, string title, string subtitle, MetaHub.Screen go, float x, float y, float w, float h, out TextMeshProUGUI sub, float titleSize = 42, string overlayWitch = null)
        {
            var root = U.Img(_dest, null, new Color(0, 0, 0, 0.001f), "Destino " + title, true);
            var rt = root.rectTransform;
            U.At(rt, 1, 1, x, y, w, h, 1, 1);
            var mask = U.Node(rt, "Recorte");
            U.Fill(mask, 7, 7, 7, 7);
            mask.gameObject.AddComponent<RectMask2D>();
            var img = U.Img(mask, art, null, "Arte");
            // El arte cubre la tarjeta sin deformarse.
            float aspect = 900f / 520f;
            float cw = Mathf.Max(w, h * aspect), ch = cw / aspect;
            U.At(img.rectTransform, 0.5f, 0.5f, 0, 0, cw, ch, 0.5f, 0.5f);
            if (overlayWitch != null)
            {
                var wi = U.Img(mask, null, null, "Bruja");
                wi.sprite = UiTheme.Sp("brujas/" + overlayWitch + "_f1_busto");
                wi.preserveAspect = true;
                U.At(wi.rectTransform, 1, 0, 24, -10, h * 1.25f, h * 1.25f, 1, 0);
            }
            var fx = UiParticles.Make(mask, "fx/soft_dot", UiTheme.Mat("Additive"), "Almitas");
            fx.MaxParticles = 16;
            fx.FadeIn = 0.8f;
            fx.FadeOut = 1.2f;
            fx.Source = new UiParticles.Emitter { Rate = 1.1f, Area = new Rect(-w / 2, -h / 2, w, h * 0.7f), VelMin = new Vector2(-6, 10), VelMax = new Vector2(6, 26), LifeMin = 3, LifeMax = 6, Size0Min = 7, Size0Max = 16, SizeGrow = 0.7f, ColorA = new Color(1, 0.86f, 0.5f, 0.9f), ColorB = new Color(1, 0.95f, 0.8f, 0.7f), WobbleAmp = 9, WobbleFreq = 0.9f };
            fx.Prewarm(5);
            var shade = U.Img(mask, "chrome/shade_down", null, "Sombra");
            U.Fill(shade.rectTransform);
            var frame = U.Img(rt, h > 250 ? "chrome/frame_panel" : null, h > 250 ? Color.white : UiTheme.Gold, "Marco");
            if (h <= 250)
            {
                // TUNING: una sola regla de oro en las tarjetas secundarias, sin cuatro esquinas repetidas.
                U.At(frame.rectTransform, 0, 0, 0, 0, w, 2, 0, 0);
                var backing = U.Img(rt, null, UiTheme.Abyss, "Pie de lectura");
                U.At(backing.rectTransform, 0, 0, 7, 7, w - 14, h > 150 ? 101 : h - 14, 0, 0);
            }
            frame.fillCenter = false;
            if (h > 250) U.Fill(frame.rectTransform);
            var tt = U.Txt(rt, title, titleSize, UiTheme.Spark, UiTheme.Title);
            tt.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0.04f, 0.01f, 0.05f, 1), 0.45f, -0.9f, 0.1f);
            sub = U.Txt(rt, subtitle, Mathf.Min(26, titleSize * 0.66f), UiTheme.Parchment, UiTheme.Italic, TextAlignmentOptions.MidlineLeft, true);
            sub.enableAutoSizing = true;
            sub.fontSizeMin = 22;
            sub.fontSizeMax = Mathf.Min(26, titleSize * 0.66f);
            sub.overflowMode = TextOverflowModes.Ellipsis;
            sub.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Italic, new Color(0.04f, 0.01f, 0.05f, 1), 0.4f, -0.7f, 0.05f);
            U.At(tt.rectTransform, 0, 0, 30, h > 150 ? 72 : 22, w - 50, titleSize + 8, 0, 0);
            U.At(sub.rectTransform, 0, 0, 32, h > 150 ? 12 : 2, w - 50, 58, 0, 0);
            if (h <= 150) sub.gameObject.SetActive(false);
            var b = root.gameObject.AddComponent<UiBtn>();
            b.Background = null;
            b.PressScale = 0.97f;
            b.Click = () => App.Go(go);
            _cards.Add(rt);
            return rt;
        }

        private void BuildDestinations()
        {
            _dest = U.Fill(U.Node(Root, "Destinos"));
            // TUNING: columna de destinos de 640 a 548 de ancho y unos 15 % más baja: la favorita manda en el centro.
            const float W = 548, X = -28;
            float y = -112;
            MakeDestCard("cards/fresno", UiLoc.Get("dest.fresno"), UiLoc.Get("dest.fresno.sub"), MetaHub.Screen.Fresno, X, y, W, 262, out _fresnoSub, 48);
            y -= 274;
            MakeDestCard("cards/story", UiLoc.Get("dest.story"), UiLoc.Get("dest.story.sub"), MetaHub.Screen.Story, X - W / 2 - 6, y, W / 2 - 6, 192, out _storySub, 34);
            MakeDestCard("cards/spring", UiLoc.Get("dest.summon"), UiLoc.Get("dest.summon.sub"), MetaHub.Screen.Summon, X, y, W / 2 - 6, 192, out _, 34);
            y -= 204;
            MakeDestCard("cards/trial", UiLoc.Get("dest.trials"), "", MetaHub.Screen.Coven, X - W / 2 - 6, y, W / 2 - 6, 80, out _, 28);
            MakeDestCard("cards/story", UiLoc.Get("dest.ledger"), "", MetaHub.Screen.Ledger, X, y, W / 2 - 6, 80, out _, 28);
        }

        private List<string> Team()
        {
            var list = new List<string>();
            foreach (var id in G.Base.Resting()) if (!list.Contains(id)) list.Add(id);
            foreach (var id in G.Profile.Party) if (!list.Contains(id) && G.Profile.Owns(id)) list.Add(id);
            return list;
        }

        public override void Refresh()
        {
            var team = Team();
            string fav = G.Profile.TermasBase.Favorite;
            if (string.IsNullOrEmpty(fav) || !G.Profile.Owns(fav)) fav = team.Count > 0 ? team[0] : "";
            SetHero(_heroId.Length > 0 && G.Profile.Owns(_heroId) ? _heroId : fav, false);

            U.Clear(_party);
            float y = 0;
            int shown = 0;
            foreach (var id in team)
            {
                if (shown++ >= 4) break;
                var seal = U.Img(_party, "chrome/seal_dark", null, id, true);
                U.At(seal.rectTransform, 0, 1, 0, y, 104, 104, 0, 1);
                var bust = U.Img(seal.transform, null, null, "Busto");
                bust.sprite = UiWitchArt.Bust(G, id);
                if (bust.sprite == null) { bust.sprite = UiTheme.Sp("fx/silhouette"); bust.color = UiTheme.Planet(G.Meta.Roster[id].Primary); }
                bust.preserveAspect = true;
                bust.material = UiTheme.Mat("Disc");
                U.Fill(bust.rectTransform, 5, 5, 5, 5);
                var ringImg = U.Img(seal.transform, "chrome/seal_dark", null, "Aro");
                ringImg.color = new Color(1, 1, 1, 0);
                var r = U.Img(_party, "fx/ring", id == _heroId ? UiTheme.Spark : new Color(1, 0.86f, 0.55f, 0.75f), "Aro");
                U.At(r.rectTransform, 0, 1, -3, y + 3, 110, 110, 0, 1);
                var entry = G.Meta.Roster[id];
                var pl = U.Img(_party, "seals/" + UiTheme.PlanetId(entry.Primary), null, "Planeta");
                U.At(pl.rectTransform, 0, 1, 72, y - 72, 38, 38, 0, 1);
                var b = seal.gameObject.AddComponent<UiBtn>();
                var cap = id;
                b.Click = () => SetHero(cap, true);
                y -= 116;
            }

            RefreshActions();

            var planet = TodayPlanet();
            _dayIcon.sprite = UiTheme.Sp("seals/" + UiTheme.PlanetId(planet));
            _dayText.text = UiLoc.F("home.day", UiLoc.Get("planet." + UiTheme.PlanetId(planet)));
            _fresnoSub.text = G.Fresno.InRun ? UiLoc.Get("dest.run") : UiLoc.Get("dest.fresno.sub");
            if (_storySub != null) _storySub.text = G.Story.InMission ? UiLoc.Get("dest.continue") : UiLoc.Get("dest.story.sub");
        }

        private void RefreshActions()
        {
            U.Clear(_actions);
            _talkBtn = Action("book", UiLoc.Get("home.talk"), Talk, 0, out _talkBadge);
            _bathBtn = Action("spring", UiLoc.Get("home.bath"), Bathe, 1, out _bathBadge);
            Action("mirror", UiLoc.Get("home.mirror"), () => App.Go(MetaHub.Screen.Mirror), 2, out _);
            bool canTalk = _heroId.Length > 0 && !G.Base.Talked(_heroId);
            _talkBadge.SetActive(canTalk);
            _bathBadge.SetActive(G.Base.CanBathe);
        }

        private UiBtn Action(string icon, string label, Action click, int i, out GameObject badge)
        {
            float x = 0;
            float yy = 0;
            // Debajo del equipo, en abanico cerca del pulgar izquierdo.
            var b = U.Btn(_actions, "chrome/seal_dark", null, 0, click, null, null, "Acción " + label);
            U.At(b.Background.rectTransform, 0, 0, x + i * 0, yy - 4, 0, 0, 0, 0);
            var rt = b.Background.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(74, 74);
            rt.anchoredPosition = new Vector2(i * 90, 0);
            var ic = U.Img(rt, "iconsg/" + icon, null, "Icono");
            U.Fill(ic.rectTransform, 15, 15, 15, 15);
            var captionBacking = U.Img(rt, null, UiTheme.Abyss, "Fondo de etiqueta");
            U.At(captionBacking.rectTransform, 0.5f, 0, 0, -26, 88, 26, 0.5f, 0);
            var caption = U.Txt(rt, label, 20, UiTheme.Parchment, UiTheme.Body, TextAlignmentOptions.Center);
            U.At(caption.rectTransform, 0.5f, 0, 0, -26, 88, 26, 0.5f, 0);
            badge = UiBadge.Make(rt, "", 30);
            return b;
        }

        private static Planet TodayPlanet()
        {
            // Días planetarios clásicos. El reloj real solo se lee en presentación.
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday: return Planet.Sol;
                case DayOfWeek.Monday: return Planet.Luna;
                case DayOfWeek.Tuesday: return Planet.Marte;
                case DayOfWeek.Wednesday: return Planet.Mercurio;
                case DayOfWeek.Thursday: return Planet.Jupiter;
                case DayOfWeek.Friday: return Planet.Venus;
                default: return Planet.Saturno;
            }
        }

        private void SetHero(string id, bool animate)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (id == _heroId && _hero.sprite != null && animate) return;
            _heroId = id;
            var sp = UiWitchArt.Body(G, id);
            if (sp == null)
            {
                sp = UiTheme.Sp("fx/silhouette");
                _hero.color = Color.Lerp(UiTheme.Planet(G.Meta.Roster[id].Primary), Color.black, 0.45f);
            }
            else _hero.color = Color.white;
            _hero.sprite = sp;
            UiWitchArt.FitHeight(_hero, 930, 900);
            _shadow.rectTransform.sizeDelta = new Vector2(_heroRt.sizeDelta.x * 0.92f, 110);
            var e = G.Meta.Roster[id];
            string rarity = new string('★', e.Rarity);
            _heroName.text = e.Name;
            _heroInfo.text = $"<color=#{ColorUtility.ToHtmlStringRGB(UiTheme.Rarity(e.Rarity))}>{rarity}</color>   {UiLoc.Get("rank." + e.Rarity)}  ·  {UiLoc.F("home.trust", G.Base.TrustLevel(id))}";
            if (animate)
            {
                UiSfx.Play("confirm", 0.7f);
                var g = U.Group(_heroRt);
                float bottom = _heroRt.anchoredPosition.y;
                g.alpha = 0;
                _heroRt.anchoredPosition = new Vector2(_heroRt.anchoredPosition.x, bottom - 50);
                Tw.Run(0.5f, p =>
                {
                    if (_heroRt == null) return;
                    g.alpha = p;
                    _heroRt.anchoredPosition = new Vector2(_heroRt.anchoredPosition.x, bottom - 50 * (1 - p));
                }, Ease.OutQuint);
                Refresh();
                Say(G.Base.Touch(id, 0));
            }
        }

        private void TouchHero()
        {
            if (_heroId.Length == 0) return;
            _touches++;
            UiSfx.Play("star", 0.8f, 1f + UnityEngine.Random.Range(-0.05f, 0.08f));
            Say(G.Base.Touch(_heroId, _touches));
            var e = new UiParticles.Emitter { VelMin = new Vector2(-180, 60), VelMax = new Vector2(180, 300), LifeMin = 0.5f, LifeMax = 1.1f, Size0Min = 26, Size0Max = 62, SizeGrow = 0.2f, RotVel = 90, ColorA = new Color(1, 0.9f, 0.6f, 1), ColorB = new Color(1, 0.8f, 0.9f, 1) };
            float cx = _heroRt.anchoredPosition.x + _heroRt.anchorMin.x * Root.rect.width - Root.rect.width / 2;
            _sparks.Burst(e, 10, new Vector2(cx, 260), 150, 160);
            float flashMax = UiTheme.ReduceFlashes ? 0.08f : 0.28f;
            Tw.Run(0.4f, p => { if (_heroMat != null) _heroMat.SetColor("_Flash", new Color(1, 0.86f, 0.7f, flashMax * (1 - p))); });
            // Rebote con cuerpo: se comprime y se estira, nunca más de lo que permita leerse como vida.
            Tw.Run(0.5f, p =>
            {
                if (_heroRt == null) return;
                float s = Mathf.Sin(p * Mathf.PI * 2) * (1 - p) * 0.03f;
                _heroRt.localScale = new Vector3(1 + s, 1 - s, 1);
            }, Ease.Linear, 0, null, _heroRt);
        }

        private void Talk()
        {
            if (_heroId.Length == 0) return;
            var r = G.Base.Talk(_heroId);
            UiSfx.Play("confirm");
            if (r.Ok) { Say(r.Message); App.Hub.SaveNow(); RefreshActions(); App.Shell.Refresh(); SetHeroLine(); }
            else UiToast.Show(r.Message, true);
        }

        private void SetHeroLine()
        {
            var e = G.Meta.Roster[_heroId];
            _heroInfo.text = $"<color=#{ColorUtility.ToHtmlStringRGB(UiTheme.Rarity(e.Rarity))}>{new string('★', e.Rarity)}</color>   {UiLoc.Get("rank." + e.Rarity)}  ·  {UiLoc.F("home.trust", G.Base.TrustLevel(_heroId))}";
        }

        private void Bathe()
        {
            var r = G.Base.Bathe();
            if (r.Ok) { UiSfx.Play("drop"); UiSfx.Play("steam"); App.Hub.SaveNow(); RefreshActions(); UiToast.Show(UiLoc.Get("top.parallax.safe") + ". " + r.Message); }
            else UiToast.Show(r.Message, true);
        }

        private void Say(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            _bubble.gameObject.SetActive(true);
            _bubbleText.text = line;
            _bubbleText.ForceMeshUpdate();
            _typeTotal = _bubbleText.textInfo.characterCount;
            _bubbleText.maxVisibleCharacters = 0;
            _typeT = 0;
            _bubbleHide = 3.2f + line.Length * 0.045f;
            _bubble.localScale = Vector3.one * 0.86f;
            var rt = _bubble;
            Tw.Kill(rt);
            Tw.Run(0.3f, p => { if (rt != null) rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.86f, 1f, p); }, Ease.OutBack, 0, null, rt);
        }

        public override void OnShown()
        {
            // Entrada en orden de lectura: paisaje quieto, bruja, equipo, tarjetas.
            float bottom = _heroRt.anchoredPosition.y;
            var hg = U.Group(_heroRt);
            hg.alpha = 0;
            _heroRt.anchoredPosition = new Vector2(_heroRt.anchoredPosition.x, bottom - 70);
            Tw.Run(0.7f, p =>
            {
                if (_heroRt == null) return;
                hg.alpha = Mathf.Clamp01(p * 1.8f);
                _heroRt.anchoredPosition = new Vector2(_heroRt.anchoredPosition.x, bottom - 70 * (1 - p));
            }, Ease.OutQuint, 0.04f);
            for (int i = 0; i < _party.childCount; i++) { }
            U.Slide(_party, new Vector2(-190, 0), 0.5f, 0.12f);
            int k = 0;
            foreach (var c in _cards) U.Rise(c, 42, 0.5f, 0.1f + 0.05f * k++);
            if (_heroId.Length > 0 && G.Base != null) Tw.After(0.9f, () => { if (_bubble != null && Root.gameObject.activeInHierarchy) Say(G.Base.Touch(_heroId, _touches)); }, this);
        }

        public override void Tick(float dt)
        {
            _t += dt;
            // Parallax lento sin giroscopio; con ratón, sigue al puntero. Las capas cercanas se mueven más.
            float tx = Mathf.Sin(_t * 0.11f) * 6f;
            float ty = Mathf.Sin(_t * 0.07f) * 2.5f;
            if (!Input.touchSupported && Screen.width > 0)
            {
                tx += (Input.mousePosition.x / Screen.width - 0.5f) * -18f;
                ty += (Input.mousePosition.y / Screen.height - 0.5f) * -6f;
            }
            float m = UiTheme.MotionScale;
            float[] depth = { 0.12f, 0.2f, 0.34f, 0.62f, 1f };
            for (int i = 0; i < _layers.Length; i++) _layers[i].anchoredPosition = new Vector2(tx * depth[i] * m, ty * depth[i] * m);

            if (_bubble != null && _bubble.gameObject.activeSelf)
            {
                _typeT += dt * 46f;
                _bubbleText.maxVisibleCharacters = UiTheme.MotionScale <= 0 ? _typeTotal : Mathf.Min(_typeTotal, Mathf.FloorToInt(_typeT));
                _bubbleHide -= dt;
                if (_bubbleHide <= 0) _bubble.gameObject.SetActive(false);
            }
        }

        public override void OnHidden()
        {
            Tw.Kill(this);
            if (_bubble != null) _bubble.gameObject.SetActive(false);
        }
    }
}
