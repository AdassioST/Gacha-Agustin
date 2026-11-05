using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// Partículas dentro del lienzo, sin cámara extra ni compute (la Unity VFX Graph exige compute, REFERENCIAS 8.4). Dibuja
    /// quads en una sola malla, así que vapor y almitas cuestan un draw call cada capa. Debe vivir en un Canvas anidado para
    /// no reconstruir el resto de la interfaz cada frame.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiParticles : MaskableGraphic
    {
        public struct P
        {
            public Vector2 Pos, Vel;
            public float Age, Life, Size0, Size1, Rot, RotVel, WobbleAmp, WobbleFreq, Phase;
            public Color Color;
        }

        public sealed class Emitter
        {
            public float Rate;
            public Rect Area;
            public Vector2 VelMin, VelMax;
            public float LifeMin = 1, LifeMax = 2, Size0Min = 10, Size0Max = 20, SizeGrow = 1, RotVel;
            public Color ColorA = Color.white, ColorB = Color.white;
            public float WobbleAmp, WobbleFreq = 1;
            public Vector2 Gravity;
            public float FadeIn = 0.2f, FadeOut = 0.5f;
        }

        public Texture Tex;
        public int MaxParticles = 160;
        public Emitter Source;
        public Vector2 Gravity;
        public float FadeIn = 0.2f, FadeOut = 0.5f;
        /// <summary>Densidad global: gama baja y ahorro de batería reducen partículas, nunca el sentido de la escena.</summary>
        public static float Density = 1f;

        private readonly List<P> _ps = new List<P>();
        private float _spawn;

        public override Texture mainTexture => Tex != null ? Tex : s_WhiteTexture;

        public static UiParticles Make(Transform parent, string tex, Material mat, string name = "Partículas")
        {
            var rt = U.Node(parent, name);
            U.Fill(rt);
            var p = rt.gameObject.AddComponent<UiParticles>();
            p.Tex = UiTheme.Tex(tex);
            p.material = mat;
            p.raycastTarget = false;
            return p;
        }

        /// <summary>Precalienta para que la pantalla no empiece vacía de vapor.</summary>
        public void Prewarm(float seconds)
        {
            for (float t = 0; t < seconds; t += 0.1f) Step(0.1f);
        }

        public void Burst(Emitter e, int count, Vector2 at, float spreadX = 0, float spreadY = 0)
        {
            for (int i = 0; i < count; i++) Spawn(e, at + new Vector2(Random.Range(-spreadX, spreadX), Random.Range(-spreadY, spreadY)));
        }

        private void Spawn(Emitter e, Vector2 pos)
        {
            if (_ps.Count >= MaxParticles) return;
            float s = Random.Range(e.Size0Min, e.Size0Max);
            _ps.Add(new P
            {
                Pos = pos,
                Vel = new Vector2(Random.Range(e.VelMin.x, e.VelMax.x), Random.Range(e.VelMin.y, e.VelMax.y)),
                Life = Random.Range(e.LifeMin, e.LifeMax),
                Size0 = s,
                Size1 = s * e.SizeGrow,
                Rot = Random.Range(0, 360f),
                RotVel = Random.Range(-e.RotVel, e.RotVel),
                WobbleAmp = e.WobbleAmp,
                WobbleFreq = e.WobbleFreq,
                Phase = Random.Range(0, 6.28f),
                Color = Color.Lerp(e.ColorA, e.ColorB, Random.value),
            });
        }

        private void Step(float dt)
        {
            if (Source != null && Source.Rate > 0)
            {
                _spawn += Source.Rate * Density * dt;
                while (_spawn >= 1)
                {
                    _spawn -= 1;
                    var a = Source.Area;
                    Spawn(Source, new Vector2(Random.Range(a.xMin, a.xMax), Random.Range(a.yMin, a.yMax)));
                }
            }
            for (int i = _ps.Count - 1; i >= 0; i--)
            {
                var p = _ps[i];
                p.Age += dt;
                if (p.Age >= p.Life) { _ps.RemoveAt(i); continue; }
                p.Vel += Gravity * dt;
                p.Pos += p.Vel * dt;
                p.Rot += p.RotVel * dt;
                _ps[i] = p;
            }
        }

        private void Update()
        {
            if (UiTheme.MotionScale <= 0) return;
            Step(Mathf.Min(Time.unscaledDeltaTime, 0.05f));
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int v = 0;
            float now = Time.unscaledTime;
            foreach (var p in _ps)
            {
                float t = p.Age / p.Life;
                float a = Mathf.Min(1, p.Age / Mathf.Max(0.01f, FadeIn)) * Mathf.Min(1, (p.Life - p.Age) / Mathf.Max(0.01f, FadeOut));
                var c = p.Color;
                c.a *= a * color.a;
                float s = Mathf.Lerp(p.Size0, p.Size1, t) * 0.5f;
                float rad = p.Rot * Mathf.Deg2Rad;
                float cs = Mathf.Cos(rad) * s, sn = Mathf.Sin(rad) * s;
                var pos = p.Pos;
                if (p.WobbleAmp != 0) pos.x += Mathf.Sin(now * p.WobbleFreq + p.Phase) * p.WobbleAmp;
                vh.AddVert(new Vector3(pos.x + (-cs + sn), pos.y + (-sn - cs)), c, new Vector4(0, 0));
                vh.AddVert(new Vector3(pos.x + (-cs - sn), pos.y + (-sn + cs)), c, new Vector4(0, 1));
                vh.AddVert(new Vector3(pos.x + (cs - sn), pos.y + (sn + cs)), c, new Vector4(1, 1));
                vh.AddVert(new Vector3(pos.x + (cs + sn), pos.y + (sn - cs)), c, new Vector4(1, 0));
                vh.AddTriangle(v, v + 1, v + 2);
                vh.AddTriangle(v, v + 2, v + 3);
                v += 4;
            }
        }
    }

    /// <summary>Capas de ambiente listas para usar. Vapor = alfa normal; almitas y chispas = aditivo.</summary>
    public static class UiAmbience
    {
        public static UiParticles Steam(Transform parent, Rect area, float rate = 2.4f, Color? tint = null, float size = 420)
        {
            var p = UiParticles.Make(parent, "fx/steam_0", UiTheme.Mat("Soft"), "Vapor");
            p.MaxParticles = 34;
            p.FadeIn = 1.8f;
            p.FadeOut = 2.8f;
            var c = tint ?? new Color(1f, 0.92f, 0.86f, 0.17f);
            p.Source = new UiParticles.Emitter
            {
                Rate = rate,
                Area = area,
                VelMin = new Vector2(-16, 32),
                VelMax = new Vector2(24, 66),
                LifeMin = 6,
                LifeMax = 10,
                Size0Min = size * 0.6f,
                Size0Max = size,
                SizeGrow = 1.8f,
                RotVel = 6,
                ColorA = c,
                ColorB = new Color(c.r, c.g * 0.92f, c.b * 0.9f, c.a * 0.7f),
                WobbleAmp = 18,
                WobbleFreq = 0.5f,
            };
            p.Prewarm(9);
            return p;
        }

        public static UiParticles Almitas(Transform parent, Rect area, float rate = 1.2f)
        {
            var p = UiParticles.Make(parent, "fx/soft_dot", UiTheme.Mat("Additive"), "Almitas");
            p.MaxParticles = 60;
            p.FadeIn = 0.9f;
            p.FadeOut = 1.6f;
            p.Source = new UiParticles.Emitter
            {
                Rate = rate,
                Area = area,
                VelMin = new Vector2(-8, 16),
                VelMax = new Vector2(10, 46),
                LifeMin = 6,
                LifeMax = 12,
                Size0Min = 9,
                Size0Max = 26,
                SizeGrow = 0.6f,
                ColorA = new Color(1f, 0.87f, 0.55f, 0.9f),
                ColorB = new Color(1f, 0.96f, 0.82f, 0.75f),
                WobbleAmp = 24,
                WobbleFreq = 0.8f,
            };
            p.Prewarm(11);
            return p;
        }
    }
}
