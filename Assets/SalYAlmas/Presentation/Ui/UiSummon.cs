using System.Collections.Generic;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// El Manantial: elegir banner y ver sin letra chica qué puede salir, con termómetro y probabilidades a la vista
    /// (GDD 16.2). La bruja destacada está en el agua, y el agua responde cuando la tocas.
    /// </summary>
    public sealed class UiSummon : UiScreen
    {
        private string _banner = "";
        private RectTransform _tabs, _info, _picks;
        private Image _hero;
        private Material _heroMat;
        private TextMeshProUGUI _bannerName, _desc, _heroName, _heroSub, _thermoText, _warn;
        private UiBar _thermo;
        private RectTransform _softMark;
        private UiBtn _one, _ten;
        private UiWater _water;
        private Material _scene;
        private RawImage _raw;
        private float _idle, _t;
        private const float Horizon = 0.56f;

        public override void Build()
        {
            _water = new UiWater();
            _scene = UiTheme.NewMat("WaterScene");
            _scene.SetTexture("_Sky", UiTheme.Tex("bg/home_sky"));
            _scene.SetTexture("_Far", UiTheme.Tex("bg/home_far"));
            _scene.SetTexture("_NoiseTex", UiTheme.Tex("fx/noise"));
            _scene.SetFloat("_HorY", Horizon);
            _scene.SetFloat("_WaterY", 0.2f);
            _scene.SetFloat("_Dim", 0.18f);
            var rawRt = U.Fill(U.Node(Root, "Agua"));
            _raw = rawRt.gameObject.AddComponent<RawImage>();
            _raw.material = _scene;
            _raw.texture = UiTheme.Tex("fx/grad_v");
            _raw.raycastTarget = true;
            var touch = rawRt.gameObject.AddComponent<UiBtn>();
            touch.PressScale = 1;
            touch.Sound = null;
            touch.Click = TouchWater;

            var wheel = U.Img(Root, "fx/astrolabe", new Color(1f, 0.85f, 0.6f, 0.2f), "Rueda");
            wheel.material = UiTheme.Mat("Additive");
            U.At(wheel.rectTransform, 0.5f, 0.66f, 0, 0, 900, 900, 0.5f, 0.5f);
            wheel.gameObject.AddComponent<UiSpin>().Speed = 2.4f;

            _hero = U.Img(Root, null, Color.white, "Destacada");
            _heroMat = UiTheme.NewMat("HeroWater");
            _hero.material = _heroMat;

            UiAmbience.Steam(Root, new Rect(-1050, -560, 2100, 180), 3.2f, new Color(1, 0.9f, 0.82f, 0.17f), 460);
            UiAmbience.Almitas(Root, new Rect(-900, -460, 1800, 500), 1.8f);

            _tabs = U.Node(Root, "Manantiales");
            U.At(_tabs, 0, 1, 26, -118, 370, 720, 0, 1);
            _tabs.localScale = new Vector3(0.86f, 0.86f, 1); // TUNING: lista de manantiales más chica

            _heroName = U.Txt(Root, "", 76, UiTheme.Spark, UiTheme.Gothic, TextAlignmentOptions.Center);
            _heroName.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Gothic, new Color(0.05f, 0.02f, 0.06f, 1), 0.5f, -1, 0.14f);
            U.At(_heroName.rectTransform, 0.42f, 0, 0, 172, 760, 92, 0.5f, 0);
            _heroSub = U.Txt(Root, "", 27, UiTheme.Parchment, UiTheme.Bold, TextAlignmentOptions.Center);
            _heroSub.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0.05f, 0.02f, 0.06f, 1), 0.4f, -0.7f, 0.08f);
            U.At(_heroSub.rectTransform, 0.42f, 0, 0, 136, 760, 36, 0.5f, 0);

            BuildInfo();
        }

        private void BuildInfo()
        {
            _info = U.Panel(Root, "Ficha del banner");
            U.At(_info, 1, 1, -26, -116, 580, 766, 1, 1);
            _info.localScale = new Vector3(0.88f, 0.88f, 1); // TUNING: ficha del banner más chica
            _bannerName = U.Txt(_info, "", 42, UiTheme.Spark, UiTheme.Title, TextAlignmentOptions.MidlineLeft);
            _bannerName.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0, 0, 0, 1), 0.35f, -0.8f, 0.08f);
            _bannerName.enableAutoSizing = true;
            _bannerName.fontSizeMin = 28;
            _bannerName.fontSizeMax = 42;
            U.At(_bannerName.rectTransform, 0, 1, 36, -28, 508, 56, 0, 1);
            var div = U.Img(_info, "chrome/divider", null, "Filete");
            U.At(div.rectTransform, 0.5f, 1, 0, -86, 500, 22, 0.5f, 1);
            _desc = U.Txt(_info, "", 26, UiTheme.Parchment, UiTheme.Body, TextAlignmentOptions.TopLeft, true);
            U.At(_desc.rectTransform, 0, 1, 36, -112, 508, 96, 0, 1);

            var tl = U.Txt(_info, UiLoc.Get("summon.thermometer"), 24, UiTheme.GoldBright, UiTheme.Bold);
            U.At(tl.rectTransform, 0, 1, 36, -212, 240, 30, 0, 1);
            _thermoText = U.Txt(_info, "", 24, UiTheme.Parchment, UiTheme.Body, TextAlignmentOptions.MidlineRight);
            U.At(_thermoText.rectTransform, 1, 1, -36, -212, 300, 30, 1, 1);
            _thermo = UiBar.Make(_info, UiTheme.Hex("#D06A4A"), "Termómetro");
            U.At(_thermo.GetComponent<RectTransform>(), 0, 1, 36, -246, 508, 30, 0, 1);
            var mark = U.Img(_thermo.transform, null, UiTheme.GoldBright, "Inicio de hervor");
            _softMark = mark.rectTransform;
            _warn = U.Txt(_info, "", 23, UiTheme.Spark, UiTheme.Italic, TextAlignmentOptions.TopLeft, true);
            U.At(_warn.rectTransform, 0, 1, 36, -284, 508, 62, 0, 1);

            _picks = U.Node(_info, "Selector");
            U.At(_picks, 0, 1, 36, -350, 508, 210, 0, 1);

            var odds = U.Btn(_info, "chrome/btn_secondary", UiLoc.Get("summon.odds"), 22, () => App.Go(MetaHub.Screen.Odds), null, null, "Probabilidades");
            U.At(odds.Background.rectTransform, 0, 0, 36, 118, 508, 50, 0, 0);

            _one = U.Btn(_info, "chrome/btn_secondary", UiLoc.Get("pull.one"), 28, () => Pull(1), null, null, "Una tirada");
            U.At(_one.Background.rectTransform, 0, 0, 26, 24, 262, 88, 0, 0);
            _one.Sound = "confirm";
            _ten = U.Btn(_info, "chrome/btn_primary", UiLoc.Get("pull.ten"), 30, () => Pull(10), null, null, "Diez tiradas");
            U.At(_ten.Background.rectTransform, 1, 0, -26, 24, 262, 88, 1, 0);
            _ten.Sound = "confirm";
            foreach (var b in new[] { _one, _ten })
            {
                U.Fill(b.Label.rectTransform, 8, 28, 8, 4);
                var cost = U.Txt(b.transform, "", 20, UiTheme.ParchmentDim, UiTheme.Body, TextAlignmentOptions.Center, false, "Costo");
                U.Fill(cost.rectTransform, 8, 6, 8, 54);
            }
        }

        private void Pull(int n) => App.StartPull(_banner, n);

        private void TouchWater()
        {
            var uv = Input.mousePosition;
            float x = uv.x / Mathf.Max(1, Screen.width), y = uv.y / Mathf.Max(1, Screen.height);
            if (y > Horizon) return;
            float d = Mathf.Clamp01((Horizon - y) / Horizon);
            _water.Drop(new Vector2(0.5f + (x - 0.5f) / (1.04f + d * 0.16f), 0.03f + 0.94f * Mathf.Pow(d, 0.72f)), 0.022f * (0.35f + 0.75f * d), -0.5f);
            UiSfx.Play("drop", 0.5f, Random.Range(0.9f, 1.2f));
        }

        public override void Refresh()
        {
            var rules = G.Meta.Gacha;
            if (_banner.Length == 0 || rules.Banner(_banner) == null) _banner = rules.Banners[0].Id;

            U.Clear(_tabs);
            float y = 0;
            foreach (var b in rules.Banners)
            {
                string art = b.Featured;
                bool on = b.Id == _banner;
                var btn = U.Btn(_tabs, on ? "chrome/btn_primary" : "chrome/btn_secondary", null, 0, null, null, null, b.Id);
                var rt = btn.Background.rectTransform;
                U.At(rt, 0, 1, on ? 18 : 0, y, 350, 132, 0, 1);
                var capture = b.Id;
                btn.Click = () => { _banner = capture; UiSfx.Play("page", 0.6f); Refresh(); };
                string witch = !string.IsNullOrEmpty(b.Featured) ? b.Featured : b.Type == BannerType.Inicio && b.Choices.Count > 0 ? b.Choices[0] : "solenne";
                var bust = U.Img(rt, null, null, "Busto");
                bust.sprite = UiWitchArt.Bust(G, witch);
                bust.preserveAspect = true;
                U.At(bust.rectTransform, 0, 0.5f, 8, 0, 118, 118, 0, 0.5f);
                var name = U.Txt(rt, b.Name, 25, on ? UiTheme.Salt : UiTheme.Parchment, UiTheme.Title, TextAlignmentOptions.MidlineLeft, true);
                name.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0, 0, 0, 0.9f), 0.3f, -0.6f, 0.05f);
                U.At(name.rectTransform, 0, 1, 128, -12, 208, 66, 0, 1);
                string sub = b.Type == BannerType.Inicio ? UiLoc.F("summon.left", G.Termas.StarterPullsLeft(b)) : UiLoc.F("summon.count", G.Termas.Pity(b).Since5, rules.HardPity);
                var s = U.Txt(rt, sub, 20, UiTheme.ParchmentDim, UiTheme.Body);
                U.At(s.rectTransform, 0, 0, 128, 14, 208, 28, 0, 0);
                y -= 146;
            }

            var banner = rules.Banner(_banner);
            _bannerName.text = banner.Name;
            _desc.text = banner.Description;

            var pity = G.Termas.Pity(banner);
            _thermo.Set((float)G.Termas.Thermometer(banner.Id));
            bool boil = G.Termas.Boiling(banner.Id);
            _thermo.Fill.color = boil ? UiTheme.Danger : UiTheme.Hex("#D06A4A");
            _thermoText.text = UiLoc.F("summon.count", pity.Since5, rules.HardPity);
            _softMark.anchorMin = _softMark.anchorMax = new Vector2((rules.SoftPityStart - 1f) / rules.HardPity, 0.5f);
            _softMark.sizeDelta = new Vector2(3, 26);

            string warn = boil ? UiLoc.Get("summon.boiling") : "";
            if (pity.Guaranteed) warn += (warn.Length > 0 ? "  ·  " : "") + UiLoc.Get("summon.guaranteed");
            if (banner.Type == BannerType.Limitado && G.Profile.Owns(banner.Featured)) warn += (warn.Length > 0 ? "  ·  " : "") + UiLoc.Get("summon.owned");
            _warn.text = warn;

            RefreshPicks(banner);
            string heroId = HeroOf(banner);
            SetHero(heroId);

            string why1 = G.GachaService.CanPull(banner.Id, 1), why10 = G.GachaService.CanPull(banner.Id, 10);
            _one.Interactable = why1.Length == 0;
            _one.DisabledReason = why1;
            _ten.Interactable = why10.Length == 0;
            _ten.DisabledReason = why10;
            _one.transform.Find("Costo").GetComponent<TextMeshProUGUI>().text = UiLoc.F("summon.cost", rules.CostPerPull.ToString("N0"));
            _ten.transform.Find("Costo").GetComponent<TextMeshProUGUI>().text = UiLoc.F("summon.cost", (rules.CostPerPull * 10).ToString("N0"));
        }

        private string HeroOf(BannerDef b)
        {
            if (!string.IsNullOrEmpty(b.Featured)) return b.Featured;
            if (b.Type == BannerType.Inicio && !string.IsNullOrEmpty(G.Profile.Gacha.StarterChoice)) return G.Profile.Gacha.StarterChoice;
            return "solenne";
        }

        private void RefreshPicks(BannerDef b)
        {
            U.Clear(_picks);
            var ids = new List<string>();
            string title = "";
            bool isSelector = false;
            if (b.Type == BannerType.Inicio) { ids.AddRange(b.Choices); title = UiLoc.Get("summon.pick"); }
            else if (b.Type == BannerType.Estandar)
            {
                isSelector = G.Termas.SelectorAvailable;
                title = isSelector ? UiLoc.Get("summon.selector.ready") : UiLoc.F("summon.selector", G.Profile.Gacha.StandardPulls, G.Meta.Gacha.SelectorAt);
                if (isSelector) ids.AddRange(G.Termas.StandardFiveStars());
            }
            if (title.Length == 0) return;
            var t = U.Txt(_picks, title, 24, UiTheme.GoldBright, UiTheme.Bold);
            U.At(t.rectTransform, 0, 1, 0, 0, 508, 30, 0, 1);
            for (int i = 0; i < ids.Count && i < 5; i++)
            {
                string id = ids[i];
                bool chosen = b.Type == BannerType.Inicio && G.Profile.Gacha.StarterChoice == id;
                var seal = U.Img(_picks, "chrome/seal_dark", null, id, true);
                float size = 118;
                U.At(seal.rectTransform, 0, 1, i * (size + 10), -40, size, size, 0, 1);
                var bu = U.Img(seal.transform, null, null, "Busto");
                bu.sprite = UiWitchArt.Bust(G, id);
                bu.preserveAspect = true;
                bu.material = UiTheme.Mat("Disc");
                U.Fill(bu.rectTransform, 4, 4, 4, 4);
                var ring = U.Img(_picks, "fx/ring", chosen ? UiTheme.Five : new Color(1, 0.85f, 0.55f, 0.55f), "Aro");
                U.At(ring.rectTransform, 0, 1, i * (size + 10) - 4, -36, size + 8, size + 8, 0, 1);
                var btn = seal.gameObject.AddComponent<UiBtn>();
                btn.Click = () =>
                {
                    var r = isSelector ? G.Termas.ClaimSelector(id) : G.Termas.SetStarterChoice(id);
                    UiSfx.Play(r.Ok ? "confirm" : "deny");
                    if (!r.Ok) UiToast.Show(r.Message, true);
                    else App.Hub.SaveNow();
                    App.ForceRefresh();
                };
                btn.Interactable = !(b.Type == BannerType.Inicio && G.Profile.Gacha.StarterFiveStar) && !(isSelector && G.Profile.Owns(id));
            }
        }

        private void SetHero(string id)
        {
            var sp = UiWitchArt.Body(G, id);
            if (sp == null) return;
            _hero.sprite = sp;
            float h = 770;
            float aspect = sp.rect.width / sp.rect.height;
            var rt = _hero.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.42f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(h * aspect, h);
            _scene.SetTexture("_Hero", sp.texture);
            var e = G.Meta.Roster[id];
            _heroName.text = e.Name;
            _heroSub.text = $"<color=#{ColorUtility.ToHtmlStringRGB(UiTheme.Rarity(e.Rarity))}>{new string('★', e.Rarity)}</color>   {UiLoc.Get("rank." + e.Rarity)}  ·  {e.Role}";
            _heroMat.SetColor("_Glow", new Color(1, 0.85f, 0.55f, 0.7f));
            _heroMat.SetFloat("_Wet", 0.8f);
        }

        public override void OnShown()
        {
            U.Slide(_tabs, new Vector2(-420, 0), 0.5f, 0.05f);
            U.Slide(_info, new Vector2(640, 0), 0.5f, 0.05f);
            U.FadeIn(U.Group(_hero.rectTransform), 0.5f, 0.1f);
        }

        public override void Tick(float dt)
        {
            _t += dt;
            _water.Tick(dt);
            _scene.SetTexture("_Sim", _water.Current);
            _idle -= dt;
            if (_idle <= 0)
            {
                _idle = Random.Range(0.5f, 1.8f);
                _water.Drop(new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.8f)), 0.012f, Random.Range(-0.1f, -0.03f));
            }

            // La destacada de pie en el agua, con la línea de flotación ondulando sobre sus botas.
            float H = Root.rect.height, W = Root.rect.width;
            var rt = _hero.rectTransform;
            float bob = Mathf.Sin(_t * 1.1f) * 3f * UiTheme.MotionScale;
            float waterY = 0.2f;
            float clip = 0.035f;
            float heroH = rt.sizeDelta.y;
            rt.anchoredPosition = new Vector2(0, waterY * H - clip * heroH + bob);
            float bottom = rt.anchoredPosition.y / H, top = bottom + heroH / H;
            float halfW = rt.sizeDelta.x / W * 0.5f;
            _heroMat.SetFloat("_ClipUv", (waterY - bottom) / (top - bottom));
            _scene.SetVector("_HeroRect", new Vector4(0.42f - halfW, bottom, 0.42f + halfW, top));
        }
    }
}
