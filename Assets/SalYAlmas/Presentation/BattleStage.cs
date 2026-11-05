using System.Collections.Generic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Escenario del combate en vista lateral, al estilo de las formaciones enfrentadas de los RPG por turnos móviles:
    /// brujas a la izquierda, enemigos a la derecha, en dos filas escalonadas, delante de un bioma pintado.
    /// Solo coloca y decora; las reglas siguen en el núcleo.
    /// </summary>
    public sealed class BattleStage
    {
        // Pies de cada puesto en el lienzo de 1920×1080. El HUD deja libre la franja de 100 a 870 y la columna
        // derecha a partir de 1590, así que las figuras viven dentro de ese hueco. // TUNING
        private static readonly Vector2[] WitchSlots =
        {
            new Vector2(800, 862), new Vector2(470, 848), new Vector2(660, 738), new Vector2(330, 728),
        };
        private static readonly Vector2[] EnemySlots =
        {
            new Vector2(1170, 858), new Vector2(1430, 826), new Vector2(1060, 742), new Vector2(1310, 726), new Vector2(1500, 704),
        };
        private static readonly Vector2 BossSlot = new Vector2(1290, 860);

        // Cámara lateral un poco elevada: la profundidad se lee sin que la fila de atrás se vuelva diminuta. // TUNING
        private static readonly Vector3 CameraPosition = new Vector3(0f, 3.4f, -12f);
        private static readonly Vector3 CameraTarget = new Vector3(0f, 1.2f, 0f);
        private const float CameraFov = 40f;
        private const float BackdropDistance = 60f;

        private readonly Camera _camera;
        private readonly List<GameObject> _hidden = new List<GameObject>();
        private GameObject _backdrop;
        private Vector3 _savedPos;
        private Quaternion _savedRot;
        private float _savedFov;
        private CameraClearFlags _savedClear;
        private bool _active;

        public string Biome { get; private set; } = "salinas";

        /// <summary>Luz ambiente de cada bioma, para que las figuras no parezcan pegadas encima del fondo. // TUNING</summary>
        public Color Tint
        {
            get
            {
                switch (Biome)
                {
                    case "raices": return new Color(1.0f, 0.90f, 0.78f);
                    case "tronco": return new Color(1.0f, 0.93f, 0.84f);
                    case "ramas": return new Color(0.96f, 0.92f, 1.0f);
                    case "copa": return new Color(0.92f, 0.88f, 1.0f);
                    default: return new Color(1.0f, 0.95f, 0.95f);
                }
            }
        }

        public BattleStage(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>Monta la cámara, el fondo y oculta el suelo de prueba de la Fase 1.</summary>
        public void Enter(string biome)
        {
            Biome = string.IsNullOrEmpty(biome) ? "salinas" : biome;
            if (_camera == null) return;
            if (!_active)
            {
                _savedPos = _camera.transform.position;
                _savedRot = _camera.transform.rotation;
                _savedFov = _camera.fieldOfView;
                _savedClear = _camera.clearFlags;
                foreach (var name in new[] { "Suelo de sal", "Círculo del aquelarre", "Círculo de las raíces" })
                {
                    var go = GameObject.Find(name);
                    if (go == null || !go.activeSelf) continue;
                    go.SetActive(false);
                    _hidden.Add(go);
                }
            }
            _active = true;
            _camera.transform.position = CameraPosition;
            _camera.transform.rotation = Quaternion.LookRotation(CameraTarget - CameraPosition, Vector3.up);
            _camera.fieldOfView = CameraFov;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            BuildBackdrop();
        }

        /// <summary>Deja la escena como estaba para el hub.</summary>
        public void Exit()
        {
            if (!_active) return;
            _active = false;
            if (_backdrop != null) Object.Destroy(_backdrop);
            _backdrop = null;
            foreach (var go in _hidden) if (go != null) go.SetActive(true);
            _hidden.Clear();
            if (_camera == null) return;
            _camera.transform.position = _savedPos;
            _camera.transform.rotation = _savedRot;
            _camera.fieldOfView = _savedFov;
            _camera.clearFlags = _savedClear;
        }

        private void BuildBackdrop()
        {
            if (_backdrop != null) Object.Destroy(_backdrop);
            var tex = Resources.Load<Texture2D>("Ui/battle/" + Biome) ?? Resources.Load<Texture2D>("Ui/battle/salinas");
            var shader = Shader.Find("Sprites/Default");
            if (tex == null || shader == null) return;
            _backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backdrop.name = "Bioma " + Biome;
            var col = _backdrop.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var t = _backdrop.transform;
            t.position = _camera.transform.position + _camera.transform.forward * BackdropDistance;
            t.rotation = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
            // Llena el encuadre sin deformarse: se recorta lo que sobra del lado largo.
            float h = 2f * BackdropDistance * Mathf.Tan(CameraFov * 0.5f * Mathf.Deg2Rad) * 1.02f;
            float aspect = Mathf.Max(_camera.aspect, 1f);
            float w = h * aspect;
            t.localScale = new Vector3(w, h, 1f);
            var mat = new Material(shader) { mainTexture = tex, renderQueue = 2000 };
            float ta = tex.width / (float)tex.height;
            if (ta > aspect)
            {
                float sx = aspect / ta;
                mat.mainTextureScale = new Vector2(sx, 1f);
                mat.mainTextureOffset = new Vector2((1f - sx) / 2f, 0f);
            }
            else
            {
                float sy = ta / aspect;
                mat.mainTextureScale = new Vector2(1f, sy);
                mat.mainTextureOffset = new Vector2(0f, (1f - sy) * 0.35f);
            }
            _backdrop.GetComponent<Renderer>().material = mat;
        }

        /// <summary>Punto del suelo bajo un puesto del lienzo. Los puestos se piensan en pantalla y se proyectan al mundo.</summary>
        public Vector3 Place(int index, int count, bool enemy, bool boss)
        {
            Vector2 slot;
            if (enemy && boss && count == 1) slot = BossSlot;
            else if (enemy) slot = Spread(EnemySlots, index, count);
            else slot = Spread(WitchSlots, index, count);
            return Ground(slot);
        }

        private static Vector2 Spread(Vector2[] slots, int index, int count)
        {
            if (index < slots.Length) return slots[index];
            // Más unidades que puestos: se reparten entre los de atrás, corridos.
            var b = slots[index % slots.Length];
            return b + new Vector2(-40f * (index / slots.Length), -30f * (index / slots.Length));
        }

        private Vector3 Ground(Vector2 canvas)
        {
            if (_camera == null) return Vector3.zero;
            var ray = _camera.ScreenPointToRay(GuiKit.CanvasToScreen(canvas));
            var plane = new Plane(Vector3.up, Vector3.zero);
            return plane.Raycast(ray, out float d) ? ray.GetPoint(d) : new Vector3(0, 0, 4f);
        }

        /// <summary>Arte de enemigo según su ID. Los que no tienen dibujo propio toman el más cercano por familia.</summary>
        public static string EnemyArt(string id)
        {
            if (string.IsNullOrEmpty(id)) return "voluta";
            if (id.Contains("cristal")) return "golem";
            if (id.Contains("corneja") || id.Contains("cuervo") || id.Contains("ave")) return "cuervo";
            if (id.Contains("larva")) return "larva";
            if (id.Contains("hueco") || id.Contains("hueca") || id.Contains("lenador") || id.Contains("podadora")) return "hueco";
            if (id.Contains("raiz") || id.Contains("guardian") || id.Contains("jardinera") || id.Contains("hoja")) return "raiz";
            return "voluta";
        }

        /// <summary>Recortes que miran hacia la izquierda y se voltean para encarar a los enemigos. // TUNING</summary>
        public static bool FacesLeft(string witchArtId) => witchArtId == "brea" || witchArtId == "selenia";
    }
}
