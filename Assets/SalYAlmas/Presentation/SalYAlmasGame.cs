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

        public BattleDirector Director => _director;
        public GameDatabase Database => _db;

        private void Awake()
        {
            if (BattleCamera == null) BattleCamera = Camera.main;
            _music = gameObject.AddComponent<TempMusicPlayer>();
            _sfx = gameObject.AddComponent<TempSfxPlayer>();
            _sfx.MusicDuckRequested += _music.DuckFor;
            GuiKit.ButtonPressed += _sfx.OnButtonPressed;
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
                _hub = new MetaHub(meta, new FileSaveStore(Path.Combine(Application.persistentDataPath, "guardado")))
                {
                    StartBattle = StartExternalBattle,
                    OpenFormation = () => _mode = Mode.Formation,
                    SoundRequested = (id, parts) => _sfx.PlayCue(id, parts),
                    PullRevealed = rarity => _sfx.NotifyPull(rarity),
                };
                _mode = Mode.Hub;
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

        private MetaDataSources MetaSources()
        {
            if (EconomyJson == null || RosterJson == null || ProgressionJson == null || WeaponsJson == null ||
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
            };
        }

        public void StartBattle(BattleSetup setup)
        {
            _external = null;
            StartBattle(setup, null);
        }

        /// <summary>Combate pedido por el hub: mismo HUD y misma Cadena; al terminar, el botón Continuar devuelve el Battle.</summary>
        private void StartExternalBattle(BattleSetup setup, Action<Battle> done)
        {
            _external = done;
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
            _hud = new BattleHud(_director, _chain, BattleCamera);
            SpawnUnits();
            _mode = Mode.Battle;
        }

        private void ClearSpawned()
        {
            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();
        }

        private Vector3 Anchor(Transform[] anchors, int index, int count, bool enemy)
        {
            if (anchors != null && index < anchors.Length && anchors[index] != null) return anchors[index].position;
            float spacing = enemy ? 4.6f : 2.6f;
            float x = (index - (count - 1) / 2f) * spacing;
            return new Vector3(x, 0f, enemy ? 3.6f : -2.2f);
        }

        private void SpawnUnits()
        {
            var b = _director.Battle;
            for (int i = 0; i < b.Witches.Count; i++)
            {
                var w = b.Witches[i];
                var view = CreateFigure(w.Name, Anchor(AllyAnchors, i, b.Witches.Count, false), Palette.Planet(w.Dominant), MaterialFor(w.Dominant), 1f, false, w.Id);
                // El Ascendente: una esfera del planeta secundario que flota detrás de la bruja.
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
                float scale = e.IsBoss ? 2.1f : 1.3f;
                Color c = Color.Lerp(Palette.Planet(e.Def.AttackPlanet), Palette.Abyss, 0.45f);
                CreateFigure(e.Name, Anchor(EnemyAnchors, i, b.Enemies.Count, true), c, EnemyMaterial, scale, true, e.Id);
            }
        }

        private Material MaterialFor(Planet p)
        {
            int i = (int)p;
            return PlanetMaterials != null && i < PlanetMaterials.Length ? PlanetMaterials[i] : null;
        }

        private UnitView CreateFigure(string name, Vector3 position, Color color, Material material, float scale, bool enemy, int unitId)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
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

        private static void RemoveCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private void Update()
        {
            if (_mode == Mode.Battle) _director?.Tick(Time.unscaledDeltaTime);
            if (_music != null) _music.SetState(CurrentMusic(), _mode == Mode.Battle && _chain != null && _chain.IsOpen);
            if (_sfx != null)
            {
                _sfx.SetAmbience(_mode == Mode.Hub && _hub != null ? _hub.AmbienceId : null);
                if (_mode != Mode.Battle) _sfx.LeaveBattle();
            }
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
            if (_sfx != null && _music != null) _sfx.MusicDuckRequested -= _music.DuckFor;
        }

        /// <summary>
        /// Música temporal según la pantalla. El hub y la formación son el menú general: al abrir el juego suena primero
        /// el tema de inicio y después las tres canciones del menú al azar, sin cortarse al pasar de una a otra
        /// (musica_temporal.json).
        /// </summary>
        private MusicContext CurrentMusic()
        {
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
                    if (_director.ResultReady) DrawResult();
                    break;
            }
        }

        private void DrawResult()
        {
            var b = _director.Battle;
            var r = new Rect(460, 200, 1000, 620);
            GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.WithAlpha(Palette.Abyss, 0.6f));
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
