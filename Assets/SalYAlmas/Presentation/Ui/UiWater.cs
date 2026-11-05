using UnityEngine;

namespace SalYAlmas.Ui
{
    /// <summary>
    /// Agua de verdad para la tirada: una ecuación de onda corre en la GPU sobre un campo de altura, las gotas la perturban y
    /// el shader de la escena deriva de ahí normales, reflejo, Fresnel y brillo. Cada cubo de sal deja una onda que choca con
    /// las de los demás, y al emerger la bruja el agua reacciona a su paso. Corre a 60 pasos por segundo fijos.
    /// </summary>
    public sealed class UiWater : System.IDisposable
    {
        private const int Width = 384, Height = 256; // TUNING
        private const float StepTime = 1f / 60f;

        private RenderTexture _a, _b;
        private readonly Material _sim, _drop;
        private float _accum;

        public RenderTexture Current => _a;

        public static bool Supported => SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf);

        public UiWater()
        {
            _sim = new Material(Shader.Find("SalYAlmas/UI/WaterSim")) { hideFlags = HideFlags.HideAndDontSave };
            _drop = new Material(Shader.Find("SalYAlmas/UI/WaterDrop")) { hideFlags = HideFlags.HideAndDontSave };
            _a = Make();
            _b = Make();
        }

        private static RenderTexture Make()
        {
            var fmt = Supported ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGB32;
            var rt = new RenderTexture(Width, Height, 0, fmt) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            rt.Create();
            var old = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(0.5f, 0.5f, 0, 0));
            RenderTexture.active = old;
            return rt;
        }

        /// <summary>Perturba el agua. uv en el plano (0..1), radio en fracción del plano, fuerza negativa = hundir.</summary>
        public void Drop(Vector2 uv, float radius, float strength)
        {
            _drop.SetVector("_Center", new Vector4(uv.x, uv.y, radius, strength));
            Graphics.Blit(_a, _b, _drop);
            Swap();
        }

        public void Tick(float dt, float damping = 0.992f)
        {
            _accum += Mathf.Min(dt, 0.1f);
            _sim.SetVector("_Texel", new Vector4(1f / Width, 1f / Height, 0, 0));
            _sim.SetFloat("_Damp", damping);
            int steps = 0;
            while (_accum >= StepTime && steps < 4)
            {
                Graphics.Blit(_a, _b, _sim);
                Swap();
                _accum -= StepTime;
                steps++;
            }
        }

        private void Swap() { var t = _a; _a = _b; _b = t; }

        public void Dispose()
        {
            if (_a != null) { _a.Release(); Object.Destroy(_a); }
            if (_b != null) { _b.Release(); Object.Destroy(_b); }
            Object.Destroy(_sim);
            Object.Destroy(_drop);
        }
    }
}
