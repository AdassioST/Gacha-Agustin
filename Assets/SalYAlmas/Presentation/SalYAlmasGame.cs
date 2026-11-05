using System;
using System.Collections.Generic;
using System.IO;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Punto de entrada de la escena de combate. Carga y valida los JSON con el núcleo, muestra la formación,
    /// crea las figuras de prueba y delega el dibujo en el HUD y la pantalla de Cadena.
    /// La escena la arma el menú "Sal y Almas > Construir escena de combate".
    /// </summary>
    public sealed class SalYAlmasGame : MonoBehaviour
    {
        [Header("Datos (Assets/SalYAlmas/Data)")]
        public TextAsset RulesJson;
        public TextAsset WitchesJson;
        public TextAsset EnemiesJson;
        public TextAsset EncountersJson;
        public TextAsset PotionsJson;

        [Header("Datos de la Fase 2")]
        public TextAsset EconomyJson;
        public TextAsset RosterJson;
        public TextAsset ProgressionJson;
        public TextAsset WeaponsJson;
        public TextAsset BrewingJson;
        public TextAsset GachaJson;
        public TextAsset FresnoJson;
        public TextAsset FresnoEnemiesJson;

        [Header("Datos de la Fase 3")]
        public TextAsset StoryJson;
        public TextAsset ServicesJson;
        public TextAsset LocJson;

        [Header("Arte")]
        [Tooltip("Bustos y cuerpos del elenco. Los asigna el constructor de la escena.")]
        public Texture2D[] Portraits = new Texture2D[0];

        [Header("Escena")]
        public Camera BattleCamera;
        [Tooltip("Un material por planeta en el orden Sol, Luna, Marte, Mercurio, Venus, Júpiter, Saturno.")]
        public Material[] PlanetMaterials = new Material[7];
        public Material EnemyMaterial;
        public Transform[] AllyAnchors = new Transform[0];
        public Transform[] EnemyAnchors = new Transform[0];

        private enum Mode { Error, Hub, Formation, Battle }

        private Mode _mode;
        private MetaHub _hub;
        /// <summary>Si no es null, el combate lo pidió el hub (Fresno o Juicio) y al terminar se le devuelve.</summary>
        private Action<Battle> _external;
        private string _battleHint = "";
        private PortraitBank _art;
        private string _error = "";
        private GameDatabase _db;
        private FormationScreen _formation;
        private BattleDirector _director;
        private BattleHud _hud;
        private ChainScreen _chain;
        private readonly SavedPartituras _saved = new SavedPartituras();
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private BattleSetup _lastSetup;
        private List<BattleCommand> _lastCommands = new List<BattleCommand>();
        private string _lastFingerprint = "";
        private string _savedReportPath = "";
        private TempMusicPlayer _music;
        private TempSfxPlayer _sfx;
        private bool _chainWasOpen;
        private BattleStage _stage;
        private string _biome = "salinas";
        private bool _celebrated;

        public MetaHub Hub => _hub;
        public bool InHub => _mode == Mode.Hub;
        public Core.Meta.MetaGame Game => _hub?.Game;
        public PortraitBank Art => _art;
        /// <summary>Los efectos están silenciados (tecla N). La capa uGUI lo usa para sus sonidos de interfaz.</summary>
        public bool SfxMuted => _sfx != null && _sfx.Muted;
        public BattleDirector Director => _director;
        public GameDatabase Database => _db;

        private void Awake()
        {
            if (BattleCamera == null) BattleCamera = Camera.main;
            _stage = new BattleStage(BattleCamera);
            _music = gameObject.AddComponent<TempMusicPlayer>();
            _sfx = gameObject.AddComponent<TempSfxPlayer>();
            GuiKit.ButtonPressed += _sfx.OnButtonPressed;
            PullSequence.Sfx = _sfx.PlayCue;
            _saved.Load();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var src = new DataSources
                {
                    Rules = RulesJson != null ? RulesJson.text : null,
                    Witches = WitchesJson != null ? WitchesJson.text : null,
                    Enemies = EnemiesJson != null ? EnemiesJson.text : null,
                    Encounters = EncountersJson != null ? EncountersJson.text : null,
                    Potions = PotionsJson != null ? PotionsJson.text : null,
                };
                if (src.Witches == null || src.Enemies == null || src.Encounters == null || src.Potions == null)
                    throw new InvalidOperationException("Faltan los JSON en el componente SalYAlmasGame. Vuelve a usar el menú Sal y Almas > Construir escena de combate.");
                _db = DataLoader.Load(src);
                _formation = new FormationScreen(_db);
                _formation.OnStart += StartBattle;
                var meta = MetaLoader.Load(MetaSources(), _db);
                _art = new PortraitBank(LoadPortraits());
                BattleHud.Bust = id => _art.Bust(id);
                _hub = new MetaHub(meta, new FileSaveStore(Path.Combine(Application.persistentDataPath, "guardado")), _art)
                {
                    StartBattle = StartExternalBattle,
                    OpenFormation = () => _mode = Mode.Formation,
                    ApplySettings = ApplySettings,
                };
                ApplySettings();
                _mode = Mode.Hub;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                var walk = ScreenWalk.Requested();
                if (walk != null)
                {
                    var w = gameObject.AddComponent<ScreenWalk>();
                    w.Hub = _hub;
                    w.Folder = walk;
                }
#endif
            }
            catch (DataValidationException ex)
            {
                _error = ex.Message;
                _mode = Mode.Error;
                Debug.LogError(ex.Message);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                _mode = Mode.Error;
                Debug.LogException(ex);
            }
        }

        private IEnumerable<Texture2D> LoadPortraits()
        {
#if UNITY_EDITOR
            // Una escena construida antes de este cambio no trae los retratos: en el Editor se buscan en la carpeta de arte.
            if (Portraits == null || Portraits.Length == 0)
            {
                var list = new List<Texture2D>();
                foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/SalYAlmas/Art" }))
                {
                    var t = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                    if (t != null) list.Add(t);
                }
                return list;
            }
#endif
            return Portraits;
        }

        /// <summary>Aplica fps y gama (GDD 21.1). El ahorro de batería manda sobre la gama alta.</summary>
        private void ApplySettings()
        {
            var s = _hub?.Game?.Profile.Settings;
            if (s == null) return;
            Application.targetFrameRate = s.BatterySaver || !s.HighQuality ? 30 : 60;
            BattleFx.Density = s.BatterySaver ? 0.4f : s.ReduceFlashes ? 0.6f : 1f; // TUNING
            int levels = QualitySettings.names.Length;
            if (levels > 0) QualitySettings.SetQualityLevel(s.HighQuality ? levels - 1 : 0, false);
        }

        private MetaDataSources MetaSources()
        {
            if (StoryJson == null || ServicesJson == null || LocJson == null || EconomyJson == null || RosterJson == null || ProgressionJson == null || WeaponsJson == null ||
                BrewingJson == null || GachaJson == null || FresnoJson == null || FresnoEnemiesJson == null)
            {
#if UNITY_EDITOR
                // Una escena construida antes de la Fase 2 no tiene estos campos: en el Editor se leen de la carpeta de datos.
                return MetaDataSources.FromDirectory(Path.Combine(Application.dataPath, "SalYAlmas", "Data"));
#else
                throw new InvalidOperationException("Faltan los JSON de la Fase 2 en SalYAlmasGame. Vuelve a construir la escena.");
#endif
            }
            return new MetaDataSources
            {
                Economy = EconomyJson.text,
                Roster = RosterJson.text,
                Progression = ProgressionJson.text,
                Weapons = WeaponsJson.text,
                Brewing = BrewingJson.text,
                Gacha = GachaJson.text,
                Fresno = FresnoJson.text,
                FresnoEnemies = FresnoEnemiesJson.text,
                Story = StoryJson.text,
                Services = ServicesJson.text,
                Loc = LocJson.text,
            };
        }

        public void StartBattle(BattleSetup setup)
        {
            _external = null;
            _battleHint = "";
            _biome = "salinas";
            StartBattle(setup, null);
        }

        /// <summary>Combate pedido por el hub: mismo HUD y misma Cadena; al terminar, el botón Continuar devuelve el Battle.</summary>
        private void StartExternalBattle(BattleSetup setup, Action<Battle> done, string hint)
        {
            _external = done;
            _battleHint = hint ?? "";
            // En el Fresno, el bioma es la zona de la run; fuera de él, las salinas.
            var fresno = _hub?.Game?.Fresno;
            _biome = fresno != null && fresno.InRun && fresno.Zone != null ? fresno.Zone.Id : "salinas";
            StartBattle(setup, null);
        }

        private void FinishExternal()
        {
            var done = _external;
            var battle = _director.Battle;
            _external = null;
            ClearSpawned();
            _mode = Mode.Hub;
            done(battle);
        }

        /// <summary>Vuelve a jugar el último combate con sus órdenes grabadas y compara los eventos.</summary>
        private void StartReplay()
        {
            _lastCommands = new List<BattleCommand>(_director.Battle.CommandLog);
            _lastFingerprint = _director.EventFingerprint();
            StartBattle(_lastSetup.Clone(), _lastCommands);
            _director.Speed = 2f;
        }

        private void StartBattle(BattleSetup setup, List<BattleCommand> replay)
        {
            ClearSpawned();
            _lastSetup = setup.Clone();
            _savedReportPath = "";
            _director = new BattleDirector(_db, setup, _saved.Items, replay);
            if (_sfx != null)
            {
                _sfx.Bind(_director.Battle);
                _director.EventPresented += _sfx.OnEvent;
            }
            _chainWasOpen = false;
            _chain = new ChainScreen(_director, _saved);
            _stage.Enter(_biome);
            _celebrated = false;
            _hud = new BattleHud(_director, _chain, BattleCamera);
            SpawnUnits();
            _mode = Mode.Battle;
        }

        private void ClearSpawned()
        {
            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();
            _stage?.Exit();
        }

        /// <summary>
        /// Puesto de cada unidad en la vista lateral (BattleStage). Los anclajes de la escena de la Fase 1 eran para la
        /// vista de frente y ya no se usan.
        /// </summary>
        private Vector3 Anchor(int index, int count, bool enemy, bool boss) => _stage.Place(index, count, enemy, boss);

        private void SpawnUnits()
        {
            var b = _director.Battle;
            for (int i = 0; i < b.Witches.Count; i++)
            {
                var w = b.Witches[i];
                var view = CreateFigure(w.Name, Anchor(i, b.Witches.Count, false, false), Palette.Planet(w.Dominant), MaterialFor(w.Dominant), 1f, false, w.Id, _art?.Body(w.Key),
                    BattleStage.FacesLeft(PortraitBank.ArtId(w.Key)));
                // Con arte, el Ascendente ya viene en el recorte. Sin arte: una esfera del planeta secundario detrás de la bruja.
                if (_art?.Body(w.Key) != null) continue;
                var asc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                asc.name = "Ascendente";
                RemoveCollider(asc);
                asc.transform.SetParent(view.transform, false);
                var ascRenderer = asc.GetComponent<Renderer>();
                var mat = MaterialFor(w.Recessive);
                if (mat != null) ascRenderer.sharedMaterial = mat;
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", Palette.Planet(w.Recessive));
                block.SetColor("_Color", Palette.Planet(w.Recessive));
                ascRenderer.SetPropertyBlock(block);
                view.Ascendant = asc.transform;
            }
            for (int i = 0; i < b.Enemies.Count; i++)
            {
                var e = b.Enemies[i];
                var art = Resources.Load<Texture2D>("Ui/enemigos/" + BattleStage.EnemyArt(e.Def.Id));
                float scale = art != null ? (e.IsBoss ? 1.45f : 0.92f) : (e.IsBoss ? 2.1f : 1.3f); // TUNING
                Color c = Color.Lerp(Palette.Planet(e.Def.AttackPlanet), Palette.Abyss, 0.45f);
                // Los dibujos de enemigos miran a la izquierda, hacia las brujas: no se voltean.
                CreateFigure(e.Name, Anchor(i, b.Enemies.Count, true, e.IsBoss), c, EnemyMaterial, scale, true, e.Id, art);
            }
        }

        private Material MaterialFor(Planet p)
        {
            int i = (int)p;
            return PlanetMaterials != null && i < PlanetMaterials.Length ? PlanetMaterials[i] : null;
        }

        private UnitView CreateFigure(string name, Vector3 position, Color color, Material material, float scale, bool enemy, int unitId, Texture2D art = null, bool flip = false)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            var shader = art != null ? Shader.Find("Sprites/Default") : null;
            if (shader != null)
            {
                // Con arte: un recorte que mira a la cámara. Sin arte: la cápsula de prueba de la Fase 1.
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "Recorte";
                RemoveCollider(quad);
                quad.transform.SetParent(root.transform, false);
                const float height = 2.05f; // TUNING
                float width = height * art.width / art.height;
                // Volteado con escala negativa: el quad queda de espaldas y Sprites/Default no recorta caras traseras.
                quad.transform.localScale = new Vector3(flip ? -width : width, height, 1f);
                quad.transform.localPosition = new Vector3(0, height / 2f - 0.04f, 0);
                // Mira a la cámara pero se queda vertical, para que los pies no se despeguen de la sombra.
                if (BattleCamera != null)
                {
                    var fwd = BattleCamera.transform.forward;
                    fwd.y = 0;
                    quad.transform.rotation = Quaternion.LookRotation(fwd.sqrMagnitude > 0.001f ? fwd : Vector3.forward, Vector3.up);
                }
                var quadRenderer = quad.GetComponent<Renderer>();
                quadRenderer.material = new Material(shader) { mainTexture = art };
                var artView = root.AddComponent<UnitView>();
                artView.IsArt = true;
                artView.Shadow = ContactShadow(root.transform, width);
                artView.Setup(unitId, enemy, _stage.Tint, quadRenderer, null, height * scale);
                _spawned.Add(root);
                _director.Views[unitId] = artView;
                return artView;
            }
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            var renderer = body.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            var view = root.AddComponent<UnitView>();
            view.Setup(unitId, enemy, color, renderer, null, 2.1f * scale);
            _spawned.Add(root);
            _director.Views[unitId] = view;
            return view;
        }

        /// <summary>Sombra suave bajo los pies, para que la figura pese sobre el suelo del bioma.</summary>
        private static Renderer ContactShadow(Transform parent, float width)
        {
            var tex = Resources.Load<Texture2D>("Ui/enemigos/sombra");
            var shader = Shader.Find("Sprites/Default");
            if (tex == null || shader == null) return null;
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Sombra";
            RemoveCollider(q);
            q.transform.SetParent(parent, false);
            q.transform.localPosition = new Vector3(0, 0.02f, 0);
            q.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            q.transform.localScale = new Vector3(width * 0.9f, width * 0.32f, 1f);
            var r = q.GetComponent<Renderer>();
            r.material = new Material(shader) { mainTexture = tex, color = new Color(0, 0, 0, 0.75f), renderQueue = 2900 };
            return r;
        }

        private static void RemoveCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private void Update()
        {
            if (_mode == Mode.Battle) _director?.Tick(Time.unscaledDeltaTime);
            if (_mode == Mode.Battle && _director != null && _director.ResultReady && !_celebrated)
            {
                _celebrated = true;
                if (_director.Battle.Outcome == Core.Events.BattleOutcome.Victory)
                    foreach (var w in _director.Battle.Witches)
                        if (w.IsAlive && _director.Views.TryGetValue(w.Id, out var v) && v != null) BattleFx.Souls(v.transform.position);
            }
            if (_music != null) _music.SetState(CurrentMusic(), _mode == Mode.Battle && _chain != null && _chain.IsOpen);
            bool chainOpen = _mode == Mode.Battle && _chain != null && _chain.IsOpen;
            if (chainOpen != _chainWasOpen)
            {
                _chainWasOpen = chainOpen;
                if (_sfx != null) _sfx.NotifyChainScreen(chainOpen);
            }
        }

        private void OnDestroy()
        {
            if (_sfx != null) GuiKit.ButtonPressed -= _sfx.OnButtonPressed;
        }

        /// <summary>
        /// Música temporal según la pantalla. El hub y la formación son el menú general: al abrir el juego suena primero
        /// el tema de inicio y después las tres canciones del menú al azar, sin cortarse al pasar de una a otra
        /// (musica_temporal.json).
        /// </summary>
        private MusicContext CurrentMusic()
        {
            if (_mode == Mode.Hub && _hub != null && (_hub.Current == MetaHub.Screen.Termas || _hub.Current == MetaHub.Screen.Summon || _hub.Current == MetaHub.Screen.Mirror)) return MusicContext.Termas;
            if (_mode == Mode.Formation || _mode == Mode.Hub) return MusicContext.Menu;
            if (_mode != Mode.Battle || _director == null) return MusicContext.None;
            if (_director.ResultReady)
                return _director.Battle.Outcome == Core.Events.BattleOutcome.Victory ? MusicContext.Victory : MusicContext.Defeat;
            foreach (var enemy in _director.Battle.Enemies)
                if (enemy.IsBoss) return MusicContext.Boss;
            return MusicContext.Battle;
        }

        private void OnGUI()
        {
            // M silencia o activa la música y N los efectos, salvo que se esté escribiendo en un campo de texto.
            if (Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == 0)
            {
                if (Event.current.keyCode == KeyCode.M && _music != null)
                {
                    _music.ToggleMute();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.N && _sfx != null)
                {
                    _sfx.ToggleMute();
                    Event.current.Use();
                }
            }

            // Toque sobre el mundo antes de escalar el lienzo (coordenadas reales de pantalla).
            if (_mode == Mode.Battle && Event.current.type == EventType.MouseDown && _hud != null && !_hud.InputBlocked)
            {
                var mp = Event.current.mousePosition;
                _hud.HandleWorldClick(new Vector2(mp.x, Screen.height - mp.y));
            }

            GuiKit.BeginScreen();
            switch (_mode)
            {
                case Mode.Error:
                    GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.Abyss);
                    GuiKit.Label(new Rect(80, 60, 1760, 60), "Los datos no pasaron la validación", 36, Palette.Danger, TextAnchor.MiddleLeft, true);
                    GuiKit.Label(new Rect(80, 130, 1760, 900), _error, 20, Palette.Parchment, TextAnchor.UpperLeft, false, true);
                    break;
                case Mode.Hub:
                    _hub.Draw();
                    break;
                case Mode.Formation:
                    _formation.Draw();
                    if (GuiKit.Button(new Rect(GuiKit.Width - 196, GuiKit.Height - 56, 180, 46), "Volver al hub", true, 18)) _mode = Mode.Hub;
                    break;
                case Mode.Battle:
                    _hud.InputBlocked = _chain.IsOpen || _director.ResultReady;
                    _hud.Draw();
                    _chain.Draw();
                    if (_battleHint.Length > 0 && !_director.ResultReady && _director.Chains.Count == 0)
                    {
                        var hint = new Rect(GuiKit.Width / 2 - 400, 166, 980, 60);
                        Theme.Panel(hint, Palette.Hex("#EDE3CC"), Palette.Hex("#CFBE9A"), Palette.GoldBright);
                        GuiKit.Label(hint, _battleHint, 22, Palette.Ink, TextAnchor.MiddleCenter, true, true);
                    }
                    if (_director.ResultReady) DrawResult();
                    break;
            }
        }

        private void DrawResult()
        {
            var b = _director.Battle;
            var r = new Rect(460, 200, 1000, 620);
            GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.WithAlpha(Palette.Abyss, 0.45f));
            GuiKit.PanelBox(r, 0.97f);
            bool win = b.Outcome == Core.Events.BattleOutcome.Victory;
            GuiKit.Label(new Rect(r.x, r.y + 20, r.width, 60), win ? "Victoria" : "Derrota", 54, win ? Palette.FiveStar : Palette.Danger, TextAnchor.MiddleCenter, true);
            float time = _director.RealEndTime - _director.RealStartTime;
            string stats = $"Tiempo real: {BattleDirector.FormatTime(time)}   ·   Turnos: {b.TurnNumber}   ·   Spark: {b.TotalSparks}   ·   Almitas: {b.Almitas}\nSemilla: {_director.Setup.Seed}";
            GuiKit.Label(new Rect(r.x + 40, r.y + 96, r.width - 80, 60), stats, 22, Palette.Parchment, TextAnchor.UpperCenter);
            GuiKit.Label(new Rect(r.x + 40, r.y + 164, r.width - 80, 30), "Cadenas", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float y = r.y + 198;
            if (_director.Chains.Count == 0) GuiKit.Label(new Rect(r.x + 40, y, r.width - 80, 28), "No abriste ninguna Cadena.", 18, Palette.ParchmentShadow);
            foreach (var c in _director.Chains)
            {
                GuiKit.Label(new Rect(r.x + 40, y, r.width - 80, 28), $"{c.Order}  ·  {c.Sparks} Spark (la mejor daba {c.BestPossibleSparks})  ·  {c.Damage:N0} de daño", 17, Palette.Parchment);
                y += 30;
                if (y > r.y + 470) break;
            }
            if (_director.IsReplay)
            {
                bool same = _director.EventFingerprint() == _lastFingerprint;
                GuiKit.Label(new Rect(r.x + 40, r.yMax - 170, r.width - 80, 30),
                    (same ? "Repetición idéntica al combate original: " : "La repetición NO coincide: ") + _director.EventFingerprint(),
                    17, same ? Palette.Planet(Planet.Venus) : Palette.Danger);
            }
            if (!string.IsNullOrEmpty(_savedReportPath))
                GuiKit.Label(new Rect(r.x + 40, r.yMax - 130, r.width - 80, 40), "Registro guardado en: " + _savedReportPath, 14, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);

            float bx = r.x + 30, by = r.yMax - 86;
            if (_external != null)
            {
                if (GuiKit.Button(new Rect(r.x + 300, by, 400, 60), "Continuar", true, 24)) FinishExternal();
                return;
            }
            if (GuiKit.Button(new Rect(bx, by, 180, 60), "Jugar otra vez\n<size=13>misma semilla</size>", true, 17)) StartBattle(_lastSetup.Clone());
            if (GuiKit.Button(new Rect(bx + 188, by, 190, 60), "Ver repetición\n<size=13>mismas órdenes</size>", !_director.IsReplay, 17))
                StartReplay();
            if (GuiKit.Button(new Rect(bx + 386, by, 170, 60), "Nueva semilla", true, 17))
            {
                var s = _lastSetup.Clone();
                s.Seed = FormationScreen.NewSeed();
                StartBattle(s);
            }
            if (GuiKit.Button(new Rect(bx + 564, by, 160, 60), "Formación", true, 17))
            {
                ClearSpawned();
                _mode = Mode.Formation;
            }
            if (GuiKit.Button(new Rect(bx + 732, by, 208, 60), "Guardar registro", true, 17)) SaveReport();
        }

        private void SaveReport()
        {
            try
            {
                string name = $"registro_{_director.Setup.EncounterId}_{_director.Setup.Seed}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string path = Path.Combine(Application.persistentDataPath, name);
                File.WriteAllText(path, _director.ExportReport());
                _savedReportPath = path;
                Debug.Log("Registro de combate guardado en " + path);
            }
            catch (Exception ex)
            {
                _savedReportPath = "No se pudo guardar: " + ex.Message;
            }
        }
    }
}
