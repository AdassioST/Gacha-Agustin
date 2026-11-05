using System.Collections.Generic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Luz del combate. Lee el fondo que de verdad se ve (su color de cielo y de suelo, dónde está lo más brillante) y
    /// con eso ilumina a brujas y enemigos: luz ambiente, contraluz, envoltura de luz en el borde, niebla por fila,
    /// sombras proyectadas y luces dinámicas en golpes, hechizos y Ascensiones.
    /// Es la razón por la que los recortes dejan de verse pegados encima del bioma: comparten su luz.
    /// Solo presenta; no lee ni cambia reglas.
    /// </summary>
    public sealed class BattleLighting : MonoBehaviour
    {
        public static BattleLighting Current { get; private set; }

        /// <summary>Luces dinámicas activas. Ahorro de batería y gama baja las apagan; la luz fija se queda.</summary>
        public static bool DynamicLights = true;
        /// <summary>Reducir destellos baja la intensidad de las luces de golpe.</summary>
        public static float FlashScale = 1f;

        private const int MaxLights = 4;
        private const int EnvWidth = 64, EnvHeight = 36;   // TUNING: fondo difuminado para la envoltura de luz
        private const int ProbeWidth = 16, ProbeHeight = 9;

        private struct Pulse
        {
            public Vector3 World;
            public Transform Follow;
            public Vector3 Offset;
            public Color Color;
            public float Intensity;
            public float Radius;
            public float Life;
            public float Age;
        }

        private readonly List<Pulse> _pulses = new List<Pulse>();
        private readonly Vector4[] _pos = new Vector4[MaxLights];
        private readonly Vector4[] _col = new Vector4[MaxLights];
        private readonly List<Renderer> _shadows = new List<Renderer>();
        private Camera _camera;
        private RenderTexture _env;
        private GameObject _haze;
        private Material _hazeMat;
        private Transform _focus;

        /// <summary>Hacia dónde está la luz clave, en pantalla (x a la derecha, y arriba).</summary>
        public Vector2 KeyDir { get; private set; } = new Vector2(0.4f, 0.9f);
        public Color KeyColor { get; private set; } = Color.white;
        public float SceneLuma { get; private set; } = 0.3f;
        /// <summary>Dirección de las sombras sobre el suelo: lejos de la luz clave.</summary>
        public Vector3 ShadowDir { get; private set; } = new Vector3(-0.35f, 0f, -1f).normalized;
        public Color ShadowColor { get; private set; } = new Color(0.04f, 0.02f, 0.07f, 0.5f);

        // TUNING: aire del bioma sobre las figuras. Al frente casi nada, al fondo un poco más; negros apenas elevados.
        private const float FogFront = 0.03f, FogBack = 0.11f, BlackLift = 0.015f;

        private static readonly int EnvTexId = Shader.PropertyToID("_FigEnvTex");
        private static readonly int EnvTintId = Shader.PropertyToID("_FigEnvTint");
        private static readonly int AmbientTopId = Shader.PropertyToID("_FigAmbientTop");
        private static readonly int AmbientBottomId = Shader.PropertyToID("_FigAmbientBottom");
        private static readonly int KeyColorId = Shader.PropertyToID("_FigKeyColor");
        private static readonly int KeyDirId = Shader.PropertyToID("_FigKeyDir");
        private static readonly int GradeId = Shader.PropertyToID("_FigGrade");
        private static readonly int FogId = Shader.PropertyToID("_FigFog");
        private static readonly int LightPosId = Shader.PropertyToID("_FigLightPos");
        private static readonly int LightColId = Shader.PropertyToID("_FigLightCol");
        private static readonly int MotionId = Shader.PropertyToID("_FigMotion");

        public static Shader FigureShader => Shader.Find("SalYAlmas/Battle/Figure");
        public static Shader ShadowShader => Shader.Find("SalYAlmas/Battle/Shadow");
        public static Shader BackdropShader => Shader.Find("SalYAlmas/Battle/Backdrop");

        /// <summary>Material de fondo con brillos domados, charco de luz y luces dinámicas.</summary>
        public static Material BackdropMaterial(Texture tex, Color tint, int queue)
        {
            var shader = BackdropShader ?? Shader.Find("Sprites/Default");
            if (shader == null) return null;
            return new Material(shader) { mainTexture = tex, color = tint, renderQueue = queue };
        }

        /// <summary>
        /// Monta la luz para un combate. <paramref name="backdrop"/> es el fondo que queda al frente: de él salen los
        /// colores. Sin fondo, la luz queda neutra.
        /// </summary>
        public static BattleLighting Begin(Camera camera, Renderer backdrop)
        {
            End();
            var go = new GameObject("Luz del combate");
            var lighting = go.AddComponent<BattleLighting>();
            lighting._camera = camera;
            Current = lighting;
            lighting.Analyze(backdrop);
            lighting.BuildHaze();
            return lighting;
        }

        public static void End()
        {
            if (Current != null) Destroy(Current.gameObject);
            Current = null;
        }

        /// <summary>Destello de luz en un punto del mundo: golpe, hechizo, Quiebre, Ascensión.</summary>
        public static void Flash(Vector3 world, Color color, float intensity = 1f, float radius = 0.2f, float life = 0.35f, Transform follow = null)
        {
            var l = Current;
            if (l == null || !DynamicLights) return;
            if (l._pulses.Count > 16) l._pulses.RemoveAt(0);
            l._pulses.Add(new Pulse { World = world, Follow = follow, Offset = follow != null ? world - follow.position : Vector3.zero, Color = color, Intensity = intensity * FlashScale, Radius = radius, Life = Mathf.Max(0.05f, life), Age = 0f });
        }

        /// <summary>Foco suave sobre la unidad en turno: el ojo va a quien actúa.</summary>
        public static void SetFocus(Transform unit)
        {
            if (Current != null) Current._focus = unit;
        }

        /// <summary>La sombra proyectada se registra para apagarla con la figura.</summary>
        public void TrackShadow(Renderer r)
        {
            if (r != null) _shadows.Add(r);
        }

        // ------------------------------------------------------------------ Lectura del fondo

        private void Analyze(Renderer backdrop)
        {
            var tex = backdrop != null && backdrop.sharedMaterial != null ? backdrop.sharedMaterial.mainTexture : null;
            Color tint = backdrop != null && backdrop.sharedMaterial != null ? backdrop.sharedMaterial.color : Color.white;
            _env = new RenderTexture(EnvWidth, EnvHeight, 0, RenderTextureFormat.ARGB32) { name = "Luz del bioma", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            _env.Create();
            var probe = new Color[ProbeWidth * ProbeHeight];
            if (tex != null && _camera != null)
            {
                VisibleUv(backdrop, out var scale, out var offset);
                // Al lienzo chico en coordenadas de pantalla: los mipmaps del fondo hacen el promedio.
                var mid = RenderTexture.GetTemporary(256, 144, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(tex, mid, scale, offset);
                var half = RenderTexture.GetTemporary(128, 72, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(mid, half);
                Graphics.Blit(half, _env);
                var small = RenderTexture.GetTemporary(32, 18, 0, RenderTextureFormat.ARGB32);
                var tiny = RenderTexture.GetTemporary(ProbeWidth, ProbeHeight, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(_env, small);
                Graphics.Blit(small, tiny);
                var read = new Texture2D(ProbeWidth, ProbeHeight, TextureFormat.RGBA32, false, false);
                var old = RenderTexture.active;
                RenderTexture.active = tiny;
                read.ReadPixels(new Rect(0, 0, ProbeWidth, ProbeHeight), 0, 0, false);
                RenderTexture.active = old;
                probe = read.GetPixels();
                Destroy(read);
                RenderTexture.ReleaseTemporary(mid);
                RenderTexture.ReleaseTemporary(half);
                RenderTexture.ReleaseTemporary(small);
                RenderTexture.ReleaseTemporary(tiny);
                for (int i = 0; i < probe.Length; i++) probe[i] *= tint;
            }
            else
            {
                for (int i = 0; i < probe.Length; i++) probe[i] = new Color(0.32f, 0.3f, 0.34f);
                var old = RenderTexture.active;
                RenderTexture.active = _env;
                GL.Clear(false, true, new Color(0.32f, 0.3f, 0.34f));
                RenderTexture.active = old;
            }
            ApplyProbe(probe, tint);
        }

        /// <summary>Qué parte de la textura del fondo cubre la pantalla: proyecta las esquinas de la vista sobre el plano del fondo.</summary>
        private void VisibleUv(Renderer backdrop, out Vector2 scale, out Vector2 offset)
        {
            var t = backdrop.transform;
            var plane = new Plane(-t.forward, t.position);
            Vector2 Uv(Vector2 viewport)
            {
                var ray = _camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
                if (!plane.Raycast(ray, out float d)) return viewport;
                var local = t.InverseTransformPoint(ray.GetPoint(d));
                return new Vector2(local.x + 0.5f, local.y + 0.5f);
            }
            var a = Uv(Vector2.zero);
            var b = Uv(Vector2.one);
            var mat = backdrop.sharedMaterial;
            Vector2 ts = mat.mainTextureScale, to = mat.mainTextureOffset;
            scale = new Vector2((b.x - a.x) * ts.x, (b.y - a.y) * ts.y);
            offset = new Vector2(to.x + a.x * ts.x, to.y + a.y * ts.y);
        }

        private void ApplyProbe(Color[] probe, Color tint)
        {
            Color sky = Color.black, ground = Color.black, all = Color.black, key = Color.black;
            float skyN = 0, groundN = 0, keyW = 0, sat = 0;
            Vector2 centroid = Vector2.zero;
            for (int y = 0; y < ProbeHeight; y++)
                for (int x = 0; x < ProbeWidth; x++)
                {
                    var c = probe[y * ProbeWidth + x];
                    float l = Luma(c);
                    all += c;
                    Color.RGBToHSV(c, out _, out float s, out _);
                    sat += s;
                    if (y >= ProbeHeight - 3) { sky += c; skyN++; }
                    if (y <= 2) { ground += c; groundN++; }
                    float w = l * l * l * l + 1e-5f;
                    key += c * w;
                    keyW += w;
                    centroid += new Vector2((x + 0.5f) / ProbeWidth, (y + 0.5f) / ProbeHeight) * w;
                }
            int n = probe.Length;
            all /= n;
            sat /= n;
            sky = skyN > 0 ? sky / skyN : all;
            ground = groundN > 0 ? ground / groundN : all;
            key /= keyW;
            centroid /= keyW;
            SceneLuma = Luma(all);

            // La luz clave viene de lo más brillante del fondo, vista desde el centro del escenario. // TUNING
            var dir = centroid - new Vector2(0.5f, 0.42f);
            if (dir.sqrMagnitude < 0.0004f) dir = new Vector2(0.3f, 1f);
            dir.y = Mathf.Max(dir.y, 0.25f); // casi nunca ilumina desde abajo
            KeyDir = dir.normalized;
            KeyColor = Normalize(key);
            // La sombra cae lejos de la luz: hacia la cámara y del lado contrario.
            ShadowDir = new Vector3(-KeyDir.x * 0.8f, 0f, -1f).normalized;
            float shade = Mathf.Lerp(0.62f, 0.38f, Mathf.Clamp01(SceneLuma * 1.6f)); // TUNING
            ShadowColor = new Color(ground.r * 0.12f, ground.g * 0.1f, ground.b * 0.14f, shade);

            // TUNING: los valores de integración. Escenas oscuras bajan la exposición de las figuras, pero sin
            // dejarlas más oscuras que su fondo cercano: la lectura del personaje manda.
            float exposure = Mathf.Clamp(0.74f + SceneLuma * 0.7f, 0.78f, 1.02f);
            var top = Color.Lerp(Color.white, Normalize(sky), 0.3f) * exposure;
            var bottom = Color.Lerp(Color.white, Normalize(ground), 0.36f) * (exposure * 0.88f);
            float saturation = Mathf.Clamp(0.84f + sat * 0.35f, 0.84f, 1f);
            float aspect = _camera != null ? _camera.aspect : 16f / 9f;

            Shader.SetGlobalTexture(EnvTexId, _env);
            Shader.SetGlobalVector(EnvTintId, tint);
            Shader.SetGlobalVector(AmbientTopId, top);
            Shader.SetGlobalVector(AmbientBottomId, bottom);
            Shader.SetGlobalVector(KeyColorId, new Vector4(KeyColor.r, KeyColor.g, KeyColor.b, Mathf.Lerp(0.55f, 0.85f, Luma(key))));
            Shader.SetGlobalVector(KeyDirId, new Vector4(KeyDir.x, KeyDir.y, 0.62f, DynamicLights ? 1f : 0f));
            Shader.SetGlobalVector(GradeId, new Vector4(saturation, 0.95f, 1f, 0.3f));
            Shader.SetGlobalVector(FogId, new Vector4(FogFront, FogBack, BlackLift, aspect));
            ClearLights();
        }

        private static float Luma(Color c) => c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

        private static Color Normalize(Color c)
        {
            float m = Mathf.Max(c.r, Mathf.Max(c.g, Mathf.Max(c.b, 0.04f)));
            return new Color(c.r / m, c.g / m, c.b / m, 1f);
        }

        // ------------------------------------------------------------------ Neblina y viñeta

        private void BuildHaze()
        {
            var shader = Shader.Find("SalYAlmas/Battle/Haze");
            if (shader == null || _camera == null) return;
            _haze = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _haze.name = "Aire del combate";
            var col = _haze.GetComponent<Collider>();
            if (col != null) Destroy(col);
            _haze.transform.SetParent(transform, false);
            _hazeMat = new Material(shader) { renderQueue = 3050 };
            _hazeMat.SetFloat("_MistA", Mathf.Lerp(0.3f, 0.18f, Mathf.Clamp01(SceneLuma * 2f))); // TUNING
            _haze.GetComponent<Renderer>().sharedMaterial = _hazeMat;
            PlaceHaze();
        }

        private void PlaceHaze()
        {
            if (_haze == null || _camera == null) return;
            const float distance = 1.2f;
            var t = _camera.transform;
            float h = 2f * distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.05f;
            _haze.transform.position = t.position + t.forward * distance;
            _haze.transform.rotation = Quaternion.LookRotation(t.forward, t.up);
            _haze.transform.localScale = new Vector3(h * _camera.aspect, h, 1f);
        }

        /// <summary>La neblina se apoya en la línea de los pies. La pone el juego al colocar las figuras.</summary>
        public void SetFeetLine(IEnumerable<Vector3> feet)
        {
            if (_hazeMat == null || _camera == null) return;
            float sum = 0, min = 1, max = 0;
            int count = 0;
            foreach (var f in feet)
            {
                float y = _camera.WorldToViewportPoint(f).y;
                sum += y;
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
                count++;
            }
            if (count == 0) return;
            _hazeMat.SetFloat("_MistY", sum / count - 0.01f);
            _hazeMat.SetFloat("_MistH", Mathf.Clamp((max - min) * 0.6f + 0.035f, 0.04f, 0.11f)); // TUNING
        }

        // ------------------------------------------------------------------ Luces dinámicas

        private void ClearLights()
        {
            for (int i = 0; i < MaxLights; i++) { _pos[i] = Vector4.zero; _col[i] = Vector4.zero; }
            Shader.SetGlobalVectorArray(LightPosId, _pos);
            Shader.SetGlobalVectorArray(LightColId, _col);
        }

        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            Shader.SetGlobalFloat(MotionId, Theme.ReduceMotion ? 0f : 1f);
            if (_camera != null) Shader.SetGlobalVector(FogId, new Vector4(FogFront, FogBack, BlackLift, _camera.aspect));
            PlaceHaze();
            var key = Shader.GetGlobalVector(KeyDirId);
            key.w = DynamicLights ? 1f : 0f;
            Shader.SetGlobalVector(KeyDirId, key);

            for (int i = _pulses.Count - 1; i >= 0; i--)
            {
                var p = _pulses[i];
                p.Age += dt;
                if (p.Age >= p.Life) _pulses.RemoveAt(i);
                else _pulses[i] = p;
            }
            for (int i = 0; i < MaxLights; i++) { _pos[i] = Vector4.zero; _col[i] = Vector4.zero; }
            if (_camera == null || !DynamicLights)
            {
                Shader.SetGlobalVectorArray(LightPosId, _pos);
                Shader.SetGlobalVectorArray(LightColId, _col);
                return;
            }

            int slot = 0;
            // El foco del turno siempre ocupa una luz, tenue y cálida. // TUNING
            if (_focus != null)
            {
                var head = _focus.position + Vector3.up * 1.1f;
                var vp = _camera.WorldToViewportPoint(head);
                float breathe = Theme.ReduceMotion ? 1f : 0.9f + 0.1f * Mathf.Sin(Time.unscaledTime * 2.2f);
                _pos[slot] = new Vector4(vp.x, vp.y, 0.26f, 0.32f * breathe);
                _col[slot] = new Vector4(1f, 0.86f, 0.6f, 1f);
                slot++;
            }
            // Las más intensas en este instante.
            _pulses.Sort((a, b) => Strength(b).CompareTo(Strength(a)));
            foreach (var p in _pulses)
            {
                if (slot >= MaxLights) break;
                var world = p.Follow != null ? p.Follow.position + p.Offset : p.World;
                var vp = _camera.WorldToViewportPoint(world);
                if (vp.z <= 0) continue;
                _pos[slot] = new Vector4(vp.x, vp.y, p.Radius, Strength(p));
                _col[slot] = p.Color;
                slot++;
            }
            Shader.SetGlobalVectorArray(LightPosId, _pos);
            Shader.SetGlobalVectorArray(LightColId, _col);
        }

        /// <summary>Sube rápido y se apaga suave.</summary>
        private static float Strength(Pulse p)
        {
            float t = p.Age / p.Life;
            float rise = Mathf.Clamp01(t / 0.12f);
            return p.Intensity * rise * (1f - t) * (1f - t);
        }

        private void OnDestroy()
        {
            ClearLights();
            Shader.SetGlobalVector(KeyDirId, new Vector4(0.4f, 0.9f, 0.62f, 0f));
            if (_env != null) { _env.Release(); Destroy(_env); }
            if (_hazeMat != null) Destroy(_hazeMat);
            if (Current == this) Current = null;
        }
    }
}
