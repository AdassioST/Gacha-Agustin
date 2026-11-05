using System.Collections.Generic;
using SalYAlmas.Core.Data;
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
        // Pies de cada puesto en el lienzo de 1920×1080. Las dos formaciones se acercan al centro y dejan suelo visible
        // entre los pies y las cartas: el aquelarre ocupa de x≈350 a 800 y los enemigos de x≈1040 a 1560. // TUNING
        private static readonly Vector2[] WitchSlots =
        {
            new Vector2(800, 788), new Vector2(505, 780), new Vector2(655, 702), new Vector2(360, 696),
        };
        private static readonly Vector2[] EnemySlots =
        {
            new Vector2(1150, 788), new Vector2(1440, 780), new Vector2(1040, 702), new Vector2(1300, 696), new Vector2(1560, 690),
        };
        private static readonly Vector2 BossSlot = new Vector2(1340, 788);
        // Con un gigante en escena, los demás enemigos le quedan a los lados y un poco al frente. // TUNING
        private static readonly Vector2[] EscortSlots =
        {
            new Vector2(1100, 812), new Vector2(1700, 806), new Vector2(990, 730), new Vector2(1800, 726),
        };
        /// <summary>Franja de los gigantes: de x=1020 a 1890, con los pies entre las dos filas. // TUNING</summary>
        private const float BigLeft = 1020f, BigRight = 1890f, BigFeet = 762f;
        /// <summary>Lo más alto que puede llegar una cabeza: debajo de los turnos y de la barra de jefe. // TUNING</summary>
        public const float HeadLimit = 186f;

        // Cámara lateral un poco elevada: la profundidad se lee sin que la fila de atrás se vuelva diminuta.
        // FOV 36 de base; con gigantes se abre para que quepan y las brujas se vean pequeñas a su lado. // TUNING
        private static readonly Vector3 CameraPosition = new Vector3(0f, 3.4f, -12f);
        private static readonly Vector3 CameraTarget = new Vector3(0f, 1.2f, 0f);
        private const float CameraFov = 36f;

        /// <summary>
        /// Altura de cada tamaño respecto a una bruja. Una gota de obsidiana le llega a la cintura; un cangrejo con un pueblo
        /// en el caparazón la triplica. // TUNING
        /// </summary>
        public static float SizeFactor(EnemySize size)
        {
            switch (size)
            {
                case EnemySize.Small: return 0.62f;
                case EnemySize.Large: return 1.35f;
                case EnemySize.Giant: return 2.2f;
                case EnemySize.Colossal: return 3.1f;
                default: return 0.95f;
            }
        }

        /// <summary>Apertura de la cámara según lo más grande del encuentro. // TUNING</summary>
        public static float FovFor(EnemySize biggest) => biggest >= EnemySize.Colossal ? 44f : biggest >= EnemySize.Giant ? 40f : CameraFov;

        public static bool IsBig(EnemySize size) => size >= EnemySize.Giant;
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

        private float _fov = CameraFov;

        /// <summary>Monta la cámara, el fondo y oculta el suelo de prueba de la Fase 1. <paramref name="fov"/> abre el encuadre para los gigantes.</summary>
        public void Enter(string biome, float fov = CameraFov)
        {
            _fov = fov;
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
            _camera.fieldOfView = _fov;
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
            if (tex == null) return;
            _backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backdrop.name = "Bioma " + Biome;
            var col = _backdrop.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var t = _backdrop.transform;
            t.position = _camera.transform.position + _camera.transform.forward * BackdropDistance;
            t.rotation = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
            // Llena el encuadre sin deformarse: se recorta lo que sobra del lado largo.
            float h = 2f * BackdropDistance * Mathf.Tan(_fov * 0.5f * Mathf.Deg2Rad) * 1.02f;
            float aspect = Mathf.Max(_camera.aspect, 1f);
            float w = h * aspect;
            t.localScale = new Vector3(w, h, 1f);
            // Más claro que antes (0.68): las figuras ya no se separan por contraste sino por luz compartida. // TUNING
            var mat = BattleLighting.BackdropMaterial(tex, new Color(0.84f, 0.84f, 0.88f), 2000);
            if (mat == null) return;
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

        /// <summary>El fondo del bioma, para que la luz del combate lea sus colores.</summary>
        public Renderer BackdropRenderer => _backdrop != null ? _backdrop.GetComponent<Renderer>() : null;

        /// <summary>Punto del suelo bajo un puesto del lienzo. Los puestos se piensan en pantalla y se proyectan al mundo.</summary>
        public Vector3 Place(int index, int count, bool enemy, bool boss, bool besideGiant = false)
        {
            Vector2 slot;
            if (enemy && besideGiant) slot = Spread(EscortSlots, index, count);
            else if (enemy && boss && count == 1) slot = BossSlot;
            else if (enemy) slot = Spread(EnemySlots, index, count);
            else slot = Spread(WitchSlots, index, count);
            return Ground(slot);
        }

        /// <summary>
        /// Puesto de un gigante: la franja derecha se reparte entre los gigantes del encuentro. Devuelve también el ancho
        /// en pantalla que le toca, para que dos colosos no se tapen entre sí.
        /// </summary>
        public Vector3 PlaceBig(int index, int count, out float widthBudget)
        {
            count = Mathf.Max(1, count);
            widthBudget = (BigRight - BigLeft) / count;
            float x = BigLeft + widthBudget * (index + 0.5f);
            // Escalonados: el segundo un poco más atrás, para que se lea la profundidad.
            float y = BigFeet - (index % 2) * 22f;
            return Ground(new Vector2(x, y));
        }

        /// <summary>Píxeles del lienzo por unidad de altura del mundo en un punto del suelo.</summary>
        public float CanvasPerUnit(Vector3 feet)
        {
            if (_camera == null) return 150f;
            var a = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(feet));
            var b = GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(feet + Vector3.up));
            return Mathf.Max(1f, a.y - b.y);
        }

        /// <summary>Altura de lienzo de los pies (para encajar la cabeza bajo HeadLimit).</summary>
        public float CanvasY(Vector3 world) => _camera == null ? 0f : GuiKit.ScreenToCanvas(_camera.WorldToScreenPoint(world)).y;

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
