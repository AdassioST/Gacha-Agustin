using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Gacha;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// La tirada (GDD 12.3 y 19.2): la sal cae al Manantial, el agua responde, el vapor anuncia la rareza con su color y la
    /// bruja emerge. Las reglas ya se resolvieron antes de empezar: esto solo lo cuenta. El color del vapor nunca miente.
    /// Se puede saltar siempre. Lo físico es físico: las ondas salen de una simulación, no de un sprite.
    /// </summary>
    public sealed class UiPullSequence
    {
        private enum Phase { Falling, Churn, Emerge, Hold, Grid, Done }

        private const float Horizon = 0.56f, WaterLine = 0.2f;
        private const float DropTimeSpacing = 0.14f; // TUNING

        private UiApp _app;
        private RectTransform _root, _world, _ui;
        private List<PullResult> _results;
        private string _banner;
        private int _count;
        private Action _done;

        private UiWater _water;
        private Material _scene;
        private RawImage _sceneImg;
        private UiParticles _steam, _steamLow, _splash, _sparks, _mist, _motes;
        private Image _rays, _rays2, _flash, _pillar, _glowHero, _wheel, _vignette;
        private Image _hero;
        private Material _heroMat;
        private TextMeshProUGUI _name, _sub, _line, _hint;
        private RectTransform _nameRoot, _stars, _grid, _skip;
        private Phase _phase;
        private float _t;
        private float _dropClock, _boilClock;
        private readonly List<Cube> _cubes = new List<Cube>();
        private int _best;
        private Color _vapor;
        private readonly Queue<PullResult> _reveals = new Queue<PullResult>();
        private float _shake, _shakeSeed;
        private float _underA, _underTarget, _dim, _dimTarget, _heroClip = 1f, _heroClipTarget = 1f;
        private float _heroH;
        private float _advanceLock;
        private Color _underColor = Color.white, _liveUnder = Color.white;
        private bool _emerging;
        private bool _bestRevealed;
        private int _revealVersion;

        private sealed class Cube
        {
            public RectTransform Rt;
            public Vector2 From, To;
            public float Delay, Time, Spin;
            public bool Landed;
        }

        private float W => _root.rect.width;
        private float H => _root.rect.height;

        public void Begin(UiApp app, RectTransform root, List<PullResult> results, string banner, int count, Action done)
        {
            _app = app;
            _root = root;
            _results = results;
            _banner = banner;
            _count = count;
            _done = done;
            _best = results.Count > 0 ? results.Max(r => r.Rarity) : 3;
            _vapor = _best == 5 ? UiTheme.Five : _best == 4 ? UiTheme.Four : UiTheme.Three;
            _underColor = _liveUnder = _vapor;
            foreach (var r in results.Where(r => r.Kind == "bruja" && r.Rarity >= 4).OrderBy(r => r.Rarity)) _reveals.Enqueue(r);

            BuildWorld();
            BuildUi();

            if (app.Game.Game.Profile.Settings.SkipCinematics) { GoGrid(); return; }
            StartFalling();
        }

        // -------------------------------------------------------------------------- Escena

        private void BuildWorld()
        {
            Canvas.ForceUpdateCanvases();
            _world = U.Fill(U.Node(_root, "Mundo"));
            var bg = U.Img(_world, null, Color.black, "Negro");
            U.Fill(bg.rectTransform, -60, -60, -60, -60);

            _water = new UiWater();
            _scene = UiTheme.NewMat("WaterScene");
            _scene.SetTexture("_Sky", UiTheme.Tex("bg/home_sky"));
            _scene.SetTexture("_Far", UiTheme.Tex("bg/home_far"));
            _scene.SetTexture("_NoiseTex", UiTheme.Tex("fx/noise"));
            _scene.SetFloat("_HorY", Horizon);
            _scene.SetFloat("_WaterY", WaterLine);
            var rawGo = U.Node(_world, "Agua");
            U.Fill(rawGo, -40, -40, -40, -40);
            _sceneImg = rawGo.gameObject.AddComponent<RawImage>();
            _sceneImg.material = _scene;
            _sceneImg.raycastTarget = false;

            _wheel = U.Img(_world, "fx/astrolabe", new Color(_vapor.r, _vapor.g, _vapor.b, 0), "Rueda natal");
            _wheel.material = UiTheme.Mat("Additive");
            U.At(_wheel.rectTransform, 0.38f, 0.68f, 0, 0, 980, 980, 0.5f, 0.5f);
            _wheel.gameObject.AddComponent<UiSpin>().Speed = 7;

            _pillar = U.Img(_world, "fx/soft_dot", new Color(_vapor.r, _vapor.g, _vapor.b, 0), "Columna");
            _pillar.material = UiTheme.Mat("Additive");
            U.At(_pillar.rectTransform, 0.38f, WaterLine, 0, 0, 520, 2000, 0.5f, 0);

            _rays = U.Img(_world, "fx/rays", new Color(_vapor.r, _vapor.g, _vapor.b, 0), "Rayos");
            _rays.material = UiTheme.Mat("Additive");
            U.At(_rays.rectTransform, 0.38f, WaterLine + 0.34f, 0, 0, 2400, 2400, 0.5f, 0.5f);
            _rays.gameObject.AddComponent<UiSpin>().Speed = 5;
            _rays2 = U.Img(_world, "fx/rays", new Color(1, 1, 1, 0), "Rayos 2");
            _rays2.material = UiTheme.Mat("Additive");
            U.At(_rays2.rectTransform, 0.38f, WaterLine + 0.34f, 0, 0, 1800, 1800, 0.5f, 0.5f);
            _rays2.gameObject.AddComponent<UiSpin>().Speed = -8;

            _steamLow = UiParticles.Make(_world, "fx/steam_1", UiTheme.Mat("Soft"), "Vapor bajo");
            _steamLow.MaxParticles = 26;
            _steamLow.FadeIn = 1.4f;
            _steamLow.FadeOut = 2.2f;
            _steamLow.Source = SteamEmitter(new Color(1, 0.92f, 0.86f, 0.14f), 2.4f);
            _steamLow.Prewarm(6);

            _glowHero = U.Img(_world, "fx/soft_dot", new Color(_vapor.r, _vapor.g, _vapor.b, 0), "Aura");
            _glowHero.material = UiTheme.Mat("Additive");
            U.At(_glowHero.rectTransform, 0.38f, WaterLine + 0.36f, 0, 0, 1200, 1500, 0.5f, 0.5f);

            _hero = U.Img(_world, null, Color.white, "Bruja");
            _heroMat = UiTheme.NewMat("HeroWater");
            _hero.material = _heroMat;
            _hero.gameObject.SetActive(false);

            _splash = UiParticles.Make(_world, "fx/soft_dot", UiTheme.Mat("Soft"), "Salpicadura");
            _splash.MaxParticles = 320;
            _splash.Gravity = new Vector2(0, -1500);
            _splash.FadeIn = 0.01f;
            _splash.FadeOut = 0.25f;

            _mist = UiParticles.Make(_world, "fx/steam_2", UiTheme.Mat("Soft"), "Bruma");
            _mist.MaxParticles = 40;
            _mist.FadeIn = 0.3f;
            _mist.FadeOut = 1.6f;

            _steam = UiParticles.Make(_world, "fx/steam_0", UiTheme.Mat("Soft"), "Vapor de rareza");
            _steam.MaxParticles = 60;
            _steam.FadeIn = 0.9f;
            _steam.FadeOut = 1.6f;

            _sparks = UiParticles.Make(_world, "fx/sparkle", UiTheme.Mat("Additive"), "Chispas");
            _sparks.MaxParticles = 180;
            _sparks.Gravity = new Vector2(0, -130);
            _sparks.FadeIn = 0.04f;
            _sparks.FadeOut = 0.6f;

            _motes = UiParticles.Make(_world, "fx/soft_dot", UiTheme.Mat("Additive"), "Almitas");
            _motes.MaxParticles = 70;
            _motes.FadeIn = 0.5f;
            _motes.FadeOut = 1.1f;

            _vignette = U.Img(_world, "fx/vignette", new Color(0, 0, 0, 0.55f), "Viñeta");
            U.Fill(_vignette.rectTransform, -60, -60, -60, -60);

            _flash = U.Img(_root, null, new Color(1, 1, 1, 0), "Destello");
            _flash.material = UiTheme.Mat("Additive");
            U.Fill(_flash.rectTransform);
        }

        private UiParticles.Emitter SteamEmitter(Color c, float rate) => new UiParticles.Emitter
        {
            Rate = rate,
            Area = new Rect(-W * 0.55f, -H * 0.5f, W * 1.1f, H * 0.26f),
            VelMin = new Vector2(-18, 40),
            VelMax = new Vector2(26, 110),
            LifeMin = 4.5f,
            LifeMax = 8,
            Size0Min = 360,
            Size0Max = 620,
            SizeGrow = 2f,
            RotVel = 9,
            ColorA = c,
            ColorB = new Color(c.r, c.g, c.b, c.a * 0.65f),
            WobbleAmp = 24,
            WobbleFreq = 0.5f,
        };

        private void BuildUi()
        {
            _ui = U.Fill(U.Node(_root, "Texto de tirada"));
            _nameRoot = U.Node(_ui, "Nombre");
            U.At(_nameRoot, 0.5f, 0, 520, 190, 760, 280, 0.5f, 0);
            _name = U.Txt(_nameRoot, "", 96, UiTheme.Spark, UiTheme.Gothic, TextAlignmentOptions.Center);
            _name.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Gothic, new Color(0.06f, 0.02f, 0.08f, 1), 0.5f, -1f, 0.14f);
            U.At(_name.rectTransform, 0.5f, 1, 0, -66, 760, 110, 0.5f, 1);
            _stars = U.Node(_nameRoot, "Estrellas");
            U.At(_stars, 0.5f, 1, 0, 0, 500, 60, 0.5f, 1);
            _sub = U.Txt(_nameRoot, "", 28, UiTheme.Parchment, UiTheme.Bold, TextAlignmentOptions.Center);
            _sub.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0.06f, 0.02f, 0.08f, 1), 0.4f, -0.7f, 0.08f);
            U.At(_sub.rectTransform, 0.5f, 1, 0, -178, 760, 38, 0.5f, 1);
            _line = U.Txt(_nameRoot, "", 28, UiTheme.Parchment, UiTheme.Italic, TextAlignmentOptions.Center, true);
            _line.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Italic, new Color(0.06f, 0.02f, 0.08f, 1), 0.4f, -0.7f, 0.06f);
            U.At(_line.rectTransform, 0.5f, 1, 0, -218, 760, 70, 0.5f, 1);
            _nameRoot.gameObject.SetActive(false);

            _hint = U.Txt(_ui, UiLoc.Get("pull.next"), 26, new Color(1, 0.95f, 0.85f, 0), UiTheme.Italic, TextAlignmentOptions.Center);
            _hint.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Italic, new Color(0, 0, 0, 1), 0.4f, -0.6f, 0.05f);
            U.At(_hint.rectTransform, 0.5f, 0, 0, 34, 700, 40, 0.5f, 0);

            var hit = U.Img(_ui, null, new Color(0, 0, 0, 0), "Toque", true);
            U.Fill(hit.rectTransform);
            var hb = hit.gameObject.AddComponent<UiBtn>();
            hb.PressScale = 1f;
            hb.Sound = null;
            hb.Click = Advance;

            var skip = U.Btn(_ui, "chrome/btn_secondary", UiLoc.Get("pull.skip"), 26, GoGrid, null, null, "Saltar");
            _skip = skip.Background.rectTransform;
            U.At(_skip, 1, 1, -30, -26, 170, 62, 1, 1);
            skip.Sound = "back";
        }

        // -------------------------------------------------------------------------- Fases

        private Vector2 PlaneOf(Vector2 uv)
        {
            float d = Mathf.Clamp01((Horizon - uv.y) / Horizon);
            float sx = 0.5f + (uv.x - 0.5f) / (1.04f + d * 0.16f);
            return new Vector2(sx, 0.03f + 0.94f * Mathf.Pow(d, 0.72f));
        }

        private void DropAt(Vector2 screenUv, float radius, float strength)
        {
            // Lejos, la misma gota ocupa menos: la perspectiva también va en el tamaño.
            float d = Mathf.Clamp01((Horizon - screenUv.y) / Horizon);
            _water.Drop(PlaneOf(screenUv), radius * (0.35f + 0.75f * d), strength);
        }

        private void StartFalling()
        {
            _phase = Phase.Falling;
            _t = 0;
            _dimTarget = 0.32f;
            UiSfx.Play("page", 0.5f);
            int n = Mathf.Clamp(_results.Count, 1, 10);
            for (int i = 0; i < n; i++)
            {
                var c = U.Img(_world, "fx/crystal_" + (i % 3), null, "Cubo de sal");
                c.rectTransform.anchorMin = c.rectTransform.anchorMax = Vector2.zero;
                c.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                float tx = Mathf.Lerp(0.26f, 0.74f, n == 1 ? 0.5f : (i + UnityEngine.Random.Range(-0.3f, 0.3f)) / (n - 1f));
                float ty = UnityEngine.Random.Range(0.14f, 0.36f);
                var cube = new Cube
                {
                    Rt = c.rectTransform,
                    From = new Vector2(tx + UnityEngine.Random.Range(-0.05f, 0.05f), 1.15f),
                    To = new Vector2(tx, ty),
                    Delay = 0.1f + i * DropTimeSpacing,
                    Spin = UnityEngine.Random.Range(-160f, 160f),
                };
                c.gameObject.SetActive(false);
                _cubes.Add(cube);
                c.transform.SetSiblingIndex(_sceneImg.transform.GetSiblingIndex() + 1);
            }
        }

        private void LandCube(Cube c)
        {
            c.Landed = true;
            c.Rt.gameObject.SetActive(false);
            DropAt(c.To, 0.024f, -0.6f);
            UiSfx.Play("drop", 0.8f, UnityEngine.Random.Range(0.86f, 1.18f));
            var at = new Vector2((c.To.x - 0.5f) * W, (c.To.y - 0.5f) * H);
            Splash(at, 26, 0.75f);
            AddShake(0.12f);
            var m = new UiParticles.Emitter { VelMin = new Vector2(-40, 30), VelMax = new Vector2(40, 90), LifeMin = 0.8f, LifeMax = 1.4f, Size0Min = 80, Size0Max = 150, SizeGrow = 2.2f, RotVel = 40, ColorA = new Color(1, 0.94f, 0.9f, 0.34f), ColorB = new Color(1, 0.9f, 0.85f, 0.2f) };
            _mist.Burst(m, 3, at, 18, 6);
        }

        /// <summary>Gotas con gravedad. Blancas pero con la luz de la escena; cada una es un quad, sin compute.</summary>
        private void Splash(Vector2 at, int count, float power)
        {
            var e = new UiParticles.Emitter
            {
                VelMin = new Vector2(-340 * power, 260 * power),
                VelMax = new Vector2(340 * power, 880 * power),
                LifeMin = 0.45f,
                LifeMax = 1.05f,
                Size0Min = 5,
                Size0Max = 15 + 12 * power,
                SizeGrow = 0.55f,
                ColorA = new Color(1, 0.96f, 0.92f, 0.95f),
                ColorB = new Color(1, 0.8f, 0.75f, 0.8f),
            };
            _splash.Burst(e, count, at, 22 * power, 4);
        }

        private void StartChurn()
        {
            _phase = Phase.Churn;
            _t = 0;
            _dimTarget = _best >= 5 ? 0.66f : _best == 4 ? 0.52f : 0.3f;
            _underTarget = _best >= 5 ? 1f : _best == 4 ? 0.7f : 0.3f;
            _scene.SetColor("_Under", new Color(_vapor.r, _vapor.g, _vapor.b, 0));
            // El vapor lleva el color de la rareza mezclado con blanco: es vapor iluminado, no pintura.
            var c = Color.Lerp(_vapor, Color.white, 0.42f);
            c.a = _best >= 5 ? 0.2f : 0.16f;
            _steam.Source = SteamEmitter(c, _best >= 5 ? 8f : 5.5f);
            UiSfx.Play("boil", _best >= 4 ? 0.9f : 0.5f);
            UiSfx.Play("steam");
            if (_best >= 5) Tw.After(0.55f, () => { if (_phase == Phase.Churn) UiSfx.Play("riser"); }, this);
            if (_best >= 4)
            {
                float wa = _best >= 5 ? 0.62f : 0.38f;
                Tw.Run(1.1f, p => { if (_wheel != null) _wheel.color = new Color(_vapor.r, _vapor.g, _vapor.b, wa * p); }, Ease.OutCubic);
                _motes.Source = new UiParticles.Emitter { Rate = _best >= 5 ? 26 : 12, Area = new Rect(-W * 0.3f, -H * 0.4f, W * 0.6f, H * 0.14f), VelMin = new Vector2(-30, 110), VelMax = new Vector2(30, 340), LifeMin = 1.2f, LifeMax = 2.4f, Size0Min = 12, Size0Max = 34, SizeGrow = 0.3f, ColorA = _vapor, ColorB = Color.Lerp(_vapor, Color.white, 0.6f), WobbleAmp = 30, WobbleFreq = 1.6f };
            }
        }

        private float ChurnDuration => _best >= 5 ? 1.9f : _best == 4 ? 1.35f : 0.95f;

        private void AfterChurn()
        {
            _steam.Source.Rate *= 0.3f;
            if (_motes.Source != null) _motes.Source.Rate *= 0.3f;
            if (_reveals.Count > 0) NextReveal();
            else GoGrid();
        }

        private void NextReveal()
        {
            int version = ++_revealVersion;
            UiSfx.Stop("boil", "steam", "riser", "reveal4", "reveal5", "star", "stamp");
            var r = _reveals.Dequeue();
            if (r.Rarity == _best) _bestRevealed = true;
            _phase = Phase.Emerge;
            _t = 0;
            _emerging = true;
            _advanceLock = 0.5f;
            var g = _app.Game.Game;
            var color = UiTheme.Rarity(r.Rarity);
            var sp = UiWitchArt.Body(g, r.WitchId) ?? UiTheme.Sp("fx/silhouette");
            _hero.sprite = sp;
            _hero.color = UiWitchArt.Body(g, r.WitchId) == null ? Color.Lerp(UiTheme.Planet(g.Meta.Roster[r.WitchId].Primary), Color.black, 0.4f) : Color.white;
            _heroH = H * 0.86f;
            float aspect = sp.rect.width / sp.rect.height;
            _hero.gameObject.SetActive(true);
            var hrt = _hero.rectTransform;
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.38f, 0);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.sizeDelta = new Vector2(_heroH * aspect, _heroH);
            _heroClip = 1f;
            _heroClipTarget = 0.07f;
            _underColor = color;
            _underTarget = r.Rarity >= 5 ? 1f : 0.7f;
            _scene.SetTexture("_Hero", sp.texture);
            _heroMat.SetColor("_Glow", new Color(color.r, color.g, color.b, 1));
            _heroMat.SetFloat("_Wet", 1);
            _nameRoot.gameObject.SetActive(false);
            _hint.color = new Color(_hint.color.r, _hint.color.g, _hint.color.b, 0);

            // El estallido: lo que el jugador esperaba. Con "Reducir destellos" el pico baja y no parpadea.
            bool five = r.Rarity >= 5;
            float flash = UiTheme.ReduceFlashes ? 0.12f : five ? 0.9f : 0.5f;
            Tw.Run(five ? 0.8f : 0.5f, p => { if (_flash != null) _flash.color = new Color(color.r, Mathf.Lerp(color.g, 1, 0.5f), Mathf.Lerp(color.b, 1, 0.5f), flash * (1 - p)); }, Ease.OutCubic);
            Tw.Run(0.6f, p =>
            {
                if (_rays == null) return;
                _rays.color = new Color(color.r, color.g, color.b, p * (five ? 0.6f : 0.38f));
                _rays2.color = new Color(1, 0.98f, 0.9f, p * (five ? 0.26f : 0.13f));
                _pillar.color = new Color(color.r, color.g, color.b, p * (five ? 0.32f : 0.18f));
                _glowHero.color = new Color(color.r, color.g, color.b, p * (five ? 0.42f : 0.26f));
            }, Ease.OutCubic);
            AddShake(five ? 0.9f : 0.45f);
            UiSfx.Play(five ? "reveal5" : "reveal4");
            Handheld();

            // La columna de agua que levanta al salir.
            var feet = new Vector2((0.38f - 0.5f) * W, (WaterLine - 0.5f) * H);
            Splash(feet, five ? 150 : 90, five ? 1.5f : 1.1f);
            DropAt(new Vector2(0.38f, WaterLine), 0.1f, 0.6f);
            var m = new UiParticles.Emitter { VelMin = new Vector2(-180, 40), VelMax = new Vector2(180, 240), LifeMin = 1.0f, LifeMax = 2.2f, Size0Min = 160, Size0Max = 320, SizeGrow = 2.2f, RotVel = 26, ColorA = new Color(1, 0.96f, 0.92f, 0.34f), ColorB = new Color(color.r, color.g, color.b, 0.3f) };
            _mist.Burst(m, 12, feet, 170, 12);
            var s = new UiParticles.Emitter { VelMin = new Vector2(-500, -80), VelMax = new Vector2(500, 760), LifeMin = 0.8f, LifeMax = 2f, Size0Min = 26, Size0Max = 82, SizeGrow = 0.1f, RotVel = 140, ColorA = color, ColorB = Color.Lerp(color, Color.white, 0.7f) };
            _sparks.Burst(s, five ? 90 : 45, feet + new Vector2(0, 240), 150, 200);

            // Texto: nombre, sello de estrellas y frase. Llega cuando la bruja ya está en pie.
            var entry = g.Meta.Roster[r.WitchId];
            _name.text = entry.Name;
            _name.color = color == UiTheme.Five ? UiTheme.Spark : Color.Lerp(color, Color.white, 0.5f);
            _sub.text = UiLoc.Get("rank." + entry.Rarity) + "  ·  " + entry.Species + "  ·  " + entry.Role;
            _line.text = g.Base.PullLineOf(r.WitchId);
            U.Clear(_stars);
            Tw.After(0.85f, () => { if (version == _revealVersion) ShowName(r.Rarity, color); }, this);
        }

        private void ShowName(int rarity, Color color)
        {
            if (_phase != Phase.Emerge && _phase != Phase.Hold) return;
            int version = _revealVersion;
            _nameRoot.gameObject.SetActive(true);
            U.Rise(_nameRoot, 26, 0.5f);
            UiSfx.Play("stamp", 0.8f);
            for (int i = 0; i < rarity; i++)
            {
                int k = i;
                float x = (i - (rarity - 1) / 2f) * 70;
                var st = U.Txt(_stars, "★", 62, color == UiTheme.Five ? UiTheme.Five : color, UiTheme.Bold, TextAlignmentOptions.Center);
                st.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0.08f, 0.03f, 0.08f, 1), 0.3f, -0.7f, 0.12f);
                var rt = st.rectTransform;
                U.At(rt, 0.5f, 0.5f, x, 0, 70, 66, 0.5f, 0.5f);
                rt.localScale = Vector3.zero;
                Tw.Run(0.34f, p => { if (rt != null) rt.localScale = Vector3.one * Mathf.LerpUnclamped(2.2f, 1f, p) * Mathf.Clamp01(p * 4); }, Ease.OutBack, 0.08f + 0.09f * k, () =>
                {
                    if (version != _revealVersion || (_phase != Phase.Emerge && _phase != Phase.Hold)) return;
                    UiSfx.Play("star", 0.7f, 0.9f + 0.09f * k);
                    if (_sparks == null || rt == null) return;
                    var world = (Vector2)_world.InverseTransformPoint(rt.position);
                    var e = new UiParticles.Emitter { VelMin = new Vector2(-180, -60), VelMax = new Vector2(180, 240), LifeMin = 0.4f, LifeMax = 0.8f, Size0Min = 18, Size0Max = 44, SizeGrow = 0.2f, RotVel = 180, ColorA = color, ColorB = Color.white };
                    _sparks.Burst(e, 8, world);
                }, this);
            }
            Tw.Run(0.5f, p => { if (_hint != null) _hint.color = new Color(_hint.color.r, _hint.color.g, _hint.color.b, p * 0.85f); }, Ease.Linear, 1.1f);
        }

        private void Advance()
        {
            if (_advanceLock > 0) return;
            switch (_phase)
            {
                case Phase.Falling:
                case Phase.Churn:
                    // Tocar acelera, no rompe: los cubos ya cayeron y seguimos.
                    foreach (var c in _cubes.Where(c => !c.Landed)) { c.Delay = 0; c.Time = 99; }
                    if (_phase == Phase.Churn) _t = 99;
                    break;
                case Phase.Emerge:
                case Phase.Hold:
                    if (_reveals.Count > 0) { FadeHero(NextReveal); }
                    else FadeHero(GoGrid);
                    break;
            }
        }

        private void FadeHero(Action next)
        {
            int version = ++_revealVersion;
            UiSfx.Stop("star", "stamp");
            _advanceLock = 0.35f;
            UiSfx.Play("back", 0.5f);
            var g = U.Group(_nameRoot);
            Tw.Run(0.22f, p => { if (g != null) g.alpha = 1 - p; }, Ease.Linear, 0, () => { if (g != null) g.alpha = 1; });
            _heroClipTarget = 1.05f; // se sumerge otra vez
            DropAt(new Vector2(0.38f, WaterLine), 0.09f, -0.26f);
            Tw.Run(0.3f, p =>
            {
                if (_rays == null) return;
                var c = _rays.color; c.a *= 1 - p; _rays.color = c;
                var d = _rays2.color; d.a *= 1 - p; _rays2.color = d;
                var e = _pillar.color; e.a *= 1 - p; _pillar.color = e;
                var f = _glowHero.color; f.a *= 1 - p; _glowHero.color = f;
            }, Ease.Linear, 0, () => { if (version == _revealVersion && _phase != Phase.Grid && _phase != Phase.Done) next(); }, this);
        }

        private void GoGrid()
        {
            if (_phase == Phase.Grid || _phase == Phase.Done) return;
            ++_revealVersion;
            Tw.Kill(this);
            _phase = Phase.Grid;
            UiSfx.Stop("drop", "boil", "steam", "riser", "star", "stamp");
            if (!_bestRevealed)
            {
                UiSfx.Stop("reveal4", "reveal5");
                UiSfx.Play("reveal" + Mathf.Clamp(_best, 3, 5));
                _bestRevealed = true;
            }
            _t = 0;
            _reveals.Clear();
            _emerging = false;
            if (_nameRoot != null) _nameRoot.gameObject.SetActive(false);
            if (_hint != null) _hint.gameObject.SetActive(false);
            if (_skip != null) _skip.gameObject.SetActive(false);
            if (_hero != null) _hero.gameObject.SetActive(false);
            _scene.SetVector("_HeroRect", Vector4.zero);
            foreach (var c in _cubes) if (c.Rt != null) c.Rt.gameObject.SetActive(false);
            foreach (var i in new[] { _rays, _rays2, _pillar, _glowHero }) if (i != null) { var co = i.color; co.a = 0; i.color = co; }
            _dimTarget = 0.72f;
            _underTarget = _best >= 4 ? 0.34f : 0.14f;
            if (_steam.Source != null) _steam.Source.Rate = 1.2f;
            if (_motes.Source != null) _motes.Source.Rate = 1.8f;
            if (_wheel != null) _wheel.color = new Color(_vapor.r, _vapor.g, _vapor.b, 0.14f);
            BuildGrid();
        }

        private string ItemIcon(PullResult r)
        {
            if (r.Kind == "polvo") return "currency/polvo_astral";
            return "currency/sales_de_bano";
        }

        private void BuildGrid()
        {
            // La barrera de toque sigue, pero ya no avanza nada.
            _grid = U.Fill(U.Node(_ui, "Resultados"));
            var title = U.Txt(_grid, UiLoc.Get("summon.results"), 62, UiTheme.Spark, UiTheme.Gothic, TextAlignmentOptions.Center);
            title.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Gothic, new Color(0.05f, 0.02f, 0.06f, 1), 0.5f, -1, 0.12f);
            U.At(title.rectTransform, 0.5f, 1, 0, -40, 900, 76, 0.5f, 1);
            var div = U.Img(_grid, "chrome/divider", null, "Filete");
            U.At(div.rectTransform, 0.5f, 1, 0, -118, 640, 28, 0.5f, 1);

            int n = _results.Count;
            const float cw = 256, ch = 336, gapX = 22, gapY = 22;
            int cols = n <= 1 ? 1 : 5;
            int rows = Mathf.CeilToInt(n / (float)cols);
            float totalW = cols * cw + (cols - 1) * gapX;
            float totalH = rows * ch + (rows - 1) * gapY;
            var g = _app.Game.Game;
            float delay = 0.1f;
            int bestDelayRarity = 0;
            for (int i = 0; i < n; i++)
            {
                var r = _results[i];
                int col = i % cols, row = i / cols;
                float x = -totalW / 2 + cw / 2 + col * (cw + gapX);
                float y = totalH / 2 - ch / 2 - row * (ch + gapY) - 16;
                var card = U.Node(_grid, "Carta " + i);
                U.At(card, 0.5f, 0.5f, x, y, cw, ch, 0.5f, 0.5f);
                BuildResultCard(card, r, g);
                int rarity = r.Rarity;
                float d = delay;
                card.localScale = new Vector3(0, 1, 1);
                Tw.Run(0.32f, p => { if (card != null) card.localScale = new Vector3(Mathf.Clamp01(p), Mathf.LerpUnclamped(0.88f, 1f, p), 1); }, Ease.OutBack, d, () => { });
                Tw.After(d, () =>
                {
                    if (_phase != Phase.Grid) return;
                    UiSfx.Play(rarity >= 4 ? "star" : "tick", rarity >= 5 ? 0.45f : rarity == 4 ? 0.35f : 0.25f, rarity >= 4 ? 1f : 1.1f);
                    if (rarity >= 4 && card != null && _sparks != null)
                    {
                        var col_ = UiTheme.Rarity(rarity);
                        var e = new UiParticles.Emitter { VelMin = new Vector2(-260, -60), VelMax = new Vector2(260, 330), LifeMin = 0.5f, LifeMax = 1.1f, Size0Min = 22, Size0Max = 54, SizeGrow = 0.2f, RotVel = 150, ColorA = col_, ColorB = Color.white };
                        _sparks.Burst(e, rarity >= 5 ? 26 : 12, (Vector2)_world.InverseTransformPoint(card.position), 60, 90);
                    }
                }, this);
                delay += rarity >= 5 ? 0.17f : 0.075f;
                bestDelayRarity = Mathf.Max(bestDelayRarity, rarity);
            }

            var doneB = U.Btn(_grid, "chrome/btn_secondary", UiLoc.Get("pull.done"), 28, Close, null, null, "Cerrar");
            U.At(doneB.Background.rectTransform, 0.5f, 0, -200, 26, 360, 76, 0.5f, 0);
            doneB.Sound = "back";
            var againText = UiLoc.Get("pull.again");
            var again = U.Btn(_grid, "chrome/btn_primary", againText, 30, Again, null, null, "Otra vez");
            U.At(again.Background.rectTransform, 0.5f, 0, 200, 26, 360, 76, 0.5f, 0);
            again.Sound = "confirm";
            string why = _app.Game.Game.GachaService.CanPull(_banner, _count);
            if (why.Length > 0) { again.Interactable = false; again.DisabledReason = why; }
            foreach (var b in new[] { doneB, again }) U.Rise((RectTransform)b.transform, 30, 0.5f, delay + 0.1f);
        }

        private void BuildResultCard(RectTransform card, PullResult r, Core.Meta.MetaGame g)
        {
            var color = UiTheme.Rarity(r.Rarity);
            var surf = U.Tile(card, "chrome/tile_leather", null, "Cuero");
            U.Fill(surf.rectTransform, 6, 6, 6, 6);
            var glow = U.Img(card, "fx/soft_dot", new Color(color.r, color.g, color.b, r.Rarity >= 5 ? 0.62f : r.Rarity == 4 ? 0.46f : 0.14f), "Luz");
            glow.material = UiTheme.Mat("Additive");
            U.At(glow.rectTransform, 0.5f, 0.56f, 0, 0, 340, 340, 0.5f, 0.5f);

            string title;
            string sub = "";
            if (r.Kind == "bruja")
            {
                var bust = U.Img(card, null, null, "Busto");
                bust.sprite = UiWitchArt.Bust(g, r.WitchId);
                if (bust.sprite == null) { bust.sprite = UiTheme.Sp("fx/silhouette"); bust.color = Color.Lerp(UiTheme.Planet(g.Meta.Roster[r.WitchId].Primary), Color.black, 0.35f); }
                bust.preserveAspect = true;
                U.At(bust.rectTransform, 0.5f, 1, 0, -8, 240, 240, 0.5f, 1);
                var e = g.Meta.Roster[r.WitchId];
                title = e.Name;
                sub = UiLoc.Get("rank." + e.Rarity);
                var pl = U.Img(card, "seals/" + UiTheme.PlanetId(e.Primary), null, "Planeta");
                U.At(pl.rectTransform, 1, 1, -10, -10, 58, 58, 1, 1);
                var isNew = U.Img(card, "chrome/seal_wax", null, "Nueva");
                U.At(isNew.rectTransform, 0, 1, 8, -8, 78, 78, 0, 1);
                var nt = U.Txt(isNew.transform, UiLoc.Get("pull.new"), 19, UiTheme.Salt, UiTheme.Bold, TextAlignmentOptions.Center);
                U.Fill(nt.rectTransform);
            }
            else
            {
                string icon = r.Kind == "polvo" ? "currency/polvo_astral" : r.SaltPlanet.HasValue ? "seals/" + UiTheme.PlanetId(r.SaltPlanet.Value) : "fx/crystal_0";
                var im = U.Img(card, icon, null, "Objeto");
                im.preserveAspect = true;
                U.At(im.rectTransform, 0.5f, 0.6f, 0, 0, 150, 150, 0.5f, 0.5f);
                if (r.Kind == "polvo") { title = UiLoc.Get("pull.dust"); sub = "×" + r.Dust.ToString("N0"); }
                else if (r.RecipeId != null) { title = UiLoc.Get("pull.recipe"); sub = g.Meta.Weapon(g.Meta.Recipes[r.RecipeId].Output).Name; }
                else { title = UiLoc.Get("pull.salt"); sub = "×" + r.SaltAmount.ToString("N0"); }
            }
            var sh = U.Img(card, "chrome/shade_down", null, "Sombra");
            U.Fill(sh.rectTransform, 6, 6, 6, 6);
            var stars = U.Txt(card, new string('★', r.Rarity), 30, color, UiTheme.Bold, TextAlignmentOptions.Center);
            stars.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Bold, new Color(0, 0, 0, 1), 0.3f, -0.6f, 0.08f);
            U.At(stars.rectTransform, 0.5f, 0, 0, 80, cwInner, 34, 0.5f, 0);
            var t = U.Txt(card, title, 27, UiTheme.Salt, UiTheme.Title, TextAlignmentOptions.Center);
            t.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Title, new Color(0, 0, 0, 1), 0.3f, -0.6f, 0.08f);
            t.enableAutoSizing = true;
            t.fontSizeMin = 17;
            t.fontSizeMax = 27;
            U.At(t.rectTransform, 0.5f, 0, 0, 42, cwInner - 20, 36, 0.5f, 0);
            var s = U.Txt(card, sub, 21, UiTheme.ParchmentDim, UiTheme.Body, TextAlignmentOptions.Center);
            s.fontSharedMaterial = UiTheme.Shadowed(UiTheme.Body, new Color(0, 0, 0, 1), 0.3f, -0.5f, 0.04f);
            s.enableAutoSizing = true;
            s.fontSizeMin = 15;
            s.fontSizeMax = 21;
            U.At(s.rectTransform, 0.5f, 0, 0, 14, cwInner - 20, 28, 0.5f, 0);
            var frame = U.Img(card, "chrome/frame_r" + Mathf.Clamp(r.Rarity, 3, 5), null, "Marco");
            frame.fillCenter = false;
            U.Fill(frame.rectTransform);
            if (r.Rarity >= 5)
            {
                frame.material = UiTheme.NewMat("Shimmer");
                frame.material.SetFloat("_Speed", 0.5f);
            }
        }

        private const float cwInner = 240;

        private void Again()
        {
            var r = _app.Game.Game.GachaService.Pull(_banner, _count, out var results);
            if (!r.Ok) { UiToast.Show(r.Message, true); return; }
            _app.Hub.SaveNow();
            _app.ForceRefresh();
            var app = _app;
            var done = _done;
            var banner = _banner;
            int count = _count;
            var root = _root;
            Dispose();
            U.Clear(root);
            var next = new UiPullSequence();
            app.Pull.Seq = next;
            Tw.After(0.02f, () => { if (app != null && root != null) next.Begin(app, root, results, banner, count, done); }, next);
        }

        private void Close()
        {
            if (_phase == Phase.Done) return;
            CancelAudio();
            var g = U.Group(_root);
            Tw.Run(0.32f, p => { if (g != null) g.alpha = 1 - p; }, Ease.OutCubic, 0, () =>
            {
                Dispose();
                _done?.Invoke();
            }, this);
        }

        public void Dispose()
        {
            CancelAudio();
            _water?.Dispose();
            _water = null;
        }

        private void CancelAudio()
        {
            _phase = Phase.Done;
            ++_revealVersion;
            Tw.Kill(this);
            UiSfx.Stop("drop", "boil", "steam", "riser", "reveal3", "reveal4", "reveal5", "star", "stamp");
        }

        private void AddShake(float amount)
        {
            _shake = Mathf.Max(_shake, amount * UiTheme.MotionScale * (UiTheme.ReduceFlashes ? 0.3f : 1f));
            _shakeSeed = UnityEngine.Random.value * 100;
        }

        private static void Handheld()
        {
#if UNITY_ANDROID || UNITY_IOS
            UnityEngine.Handheld.Vibrate();
#endif
        }

        // -------------------------------------------------------------------------- Paso a paso

        public void Tick(float dt)
        {
            if (_water == null) return;
            _t += dt;
            if (_advanceLock > 0) _advanceLock -= dt;
            _water.Tick(dt);
            _sceneImg.texture = UiTheme.Tex("fx/grad_v");
            _scene.SetTexture("_Sim", _water.Current);

            _dim = Mathf.Lerp(_dim, _dimTarget, 1 - Mathf.Exp(-dt * 2.6f));
            _underA = Mathf.Lerp(_underA, _underTarget, 1 - Mathf.Exp(-dt * 2.4f));
            _scene.SetFloat("_Dim", _dim);
            _liveUnder = Color.Lerp(_liveUnder, _underColor, 1 - Mathf.Exp(-dt * 5f));
            _scene.SetColor("_Under", new Color(_liveUnder.r, _liveUnder.g, _liveUnder.b, _underA));

            // Sacudida con decaimiento: perlin, nunca azar frame a frame.
            if (_shake > 0.001f)
            {
                _shake *= Mathf.Exp(-dt * 5f);
                float s = _shake * 26f;
                _world.anchoredPosition = new Vector2((Mathf.PerlinNoise(_shakeSeed, Time.unscaledTime * 22f) - 0.5f) * s, (Mathf.PerlinNoise(_shakeSeed + 7, Time.unscaledTime * 22f) - 0.5f) * s);
            }
            else _world.anchoredPosition = Vector2.zero;

            switch (_phase)
            {
                case Phase.Falling:
                    TickCubes(dt);
                    if (_cubes.All(c => c.Landed) && _t > 0.2f) { if (_boilClock == 0) _boilClock = 0.0001f; }
                    if (_boilClock > 0)
                    {
                        _boilClock += dt;
                        if (_boilClock > 0.45f) StartChurn();
                    }
                    break;
                case Phase.Churn:
                    // El agua hierve: gotas chicas al azar, cada vez más hacia el centro.
                    _dropClock += dt;
                    float rate = Mathf.Lerp(0.09f, 0.03f, Mathf.Clamp01(_t / ChurnDuration));
                    while (_dropClock > rate)
                    {
                        _dropClock -= rate;
                        var uv = new Vector2(UnityEngine.Random.Range(0.3f, 0.7f), UnityEngine.Random.Range(0.1f, 0.42f));
                        DropAt(uv, 0.014f, UnityEngine.Random.Range(-0.28f, _best >= 5 ? -0.1f : -0.06f));
                        if (UnityEngine.Random.value < 0.35f) Splash(new Vector2((uv.x - 0.5f) * W, (uv.y - 0.5f) * H), 3, 0.32f);
                    }
                    // El mundo se acerca: anticipación.
                    float k = Mathf.Clamp01(_t / ChurnDuration);
                    _world.localScale = Vector3.one * (1f + Ease.InCubic(k) * (_best >= 5 ? 0.07f : 0.035f));
                    if (_t >= ChurnDuration) { _world.localScale = Vector3.one; AfterChurn(); }
                    break;
                case Phase.Emerge:
                case Phase.Hold:
                    TickHero(dt);
                    break;
            }

            if (_emerging && _hero != null) UpdateHeroBindings();
        }

        private void TickCubes(float dt)
        {
            foreach (var c in _cubes)
            {
                if (c.Landed) continue;
                if (c.Delay > 0) { c.Delay -= dt; continue; }
                c.Rt.gameObject.SetActive(true);
                c.Time += dt;
                float t = Mathf.Clamp01(c.Time / 0.52f);
                // Caída libre: la altura crece con el cuadrado del tiempo; la perspectiva hace crecer el cubo.
                float y = Mathf.Lerp(c.From.y, c.To.y, t * t);
                float x = Mathf.Lerp(c.From.x, c.To.x, t);
                c.Rt.anchoredPosition = new Vector2(x * W, y * H);
                float size = Mathf.Lerp(0.42f, 1f, t) * (64 + (1 - c.To.y * 2) * 54);
                c.Rt.sizeDelta = new Vector2(size, size);
                c.Rt.localRotation = Quaternion.Euler(0, 0, c.Spin * c.Time);
                if (t >= 1) LandCube(c);
            }
        }

        private void TickHero(float dt)
        {
            float speed = _heroClip > _heroClipTarget ? 3.4f : 7f;
            _heroClip = Mathf.Lerp(_heroClip, _heroClipTarget, 1 - Mathf.Exp(-dt * speed));
            float rising = Mathf.Abs(_heroClip - _heroClipTarget);
            if (rising > 0.02f)
            {
                _dropClock += dt;
                while (_dropClock > 0.05f)
                {
                    _dropClock -= 0.05f;
                    DropAt(new Vector2(0.38f + UnityEngine.Random.Range(-0.1f, 0.1f), WaterLine + UnityEngine.Random.Range(-0.04f, 0.02f)), 0.03f, UnityEngine.Random.Range(-0.2f, 0.2f));
                    float wid = _hero.rectTransform.sizeDelta.x * 0.4f;
                    Splash(new Vector2((0.38f - 0.5f) * W + UnityEngine.Random.Range(-wid, wid), (WaterLine - 0.5f) * H), 7, 0.55f);
                }
            }
            else if (_phase == Phase.Emerge) { _phase = Phase.Hold; _underTarget *= 0.55f; }

            // Ya de pie, escurre: gotas que caen de la ropa y olas chicas en la cintura.
            if (_phase == Phase.Hold)
            {
                _boilClock += dt;
                if (_boilClock > 0.12f)
                {
                    _boilClock = 0;
                    float wid = _hero.rectTransform.sizeDelta.x * 0.36f;
                    var from = new Vector2((0.38f - 0.5f) * W + UnityEngine.Random.Range(-wid, wid), (WaterLine - 0.5f) * H + UnityEngine.Random.Range(40f, 340f));
                    var drip = new UiParticles.Emitter { VelMin = new Vector2(-6, -80), VelMax = new Vector2(6, -20), LifeMin = 0.4f, LifeMax = 0.8f, Size0Min = 4, Size0Max = 9, SizeGrow = 0.7f, ColorA = new Color(1, 0.97f, 0.94f, 0.9f), ColorB = new Color(1, 0.86f, 0.8f, 0.7f) };
                    _splash.Burst(drip, 1, from);
                    if (UnityEngine.Random.value < 0.4f) DropAt(new Vector2(0.38f + UnityEngine.Random.Range(-0.09f, 0.09f), WaterLine - 0.01f), 0.014f, UnityEngine.Random.Range(-0.1f, -0.03f));
                }
            }
            _heroMat.SetFloat("_Wet", Mathf.Clamp01(1.6f - _t * 0.12f));
            float bob = Mathf.Sin(Time.unscaledTime * 1.2f) * 3.5f * UiTheme.MotionScale;
            var rt = _hero.rectTransform;
            rt.anchoredPosition = new Vector2(0, WaterLine * H - _heroClip * _heroH + bob);
        }

        private void UpdateHeroBindings()
        {
            var rt = _hero.rectTransform;
            float bottom = rt.anchoredPosition.y / H;
            float top = bottom + _heroH / H;
            float cx = rt.anchorMin.x;
            float halfW = rt.sizeDelta.x / W * 0.5f;
            _heroMat.SetFloat("_ClipUv", (WaterLine - bottom) / (top - bottom));
            _scene.SetVector("_HeroRect", new Vector4(cx - halfW, bottom, cx + halfW, top));
            _scene.SetVector("_UnderPos", new Vector4(cx, WaterLine, 0.42f, 0));
        }
    }
}
