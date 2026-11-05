using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SalYAlmas.Core.Magic;
using SalYAlmas.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SalYAlmas.EditorTools
{
    /// <summary>
    /// Arma la escena de combate completa desde código: render con URP, materiales por planeta, cámara, luces,
    /// suelo y el objeto raíz con los JSON asignados. Menú: Sal y Almas &gt; Construir escena de combate.
    /// </summary>
    public static class BattleSceneBuilder
    {
        public const string Root = "Assets/SalYAlmas";
        public const string DataDir = Root + "/Data";
        public const string ScenesDir = Root + "/Scenes";
        public const string MaterialsDir = Root + "/Presentation/Materials";
        public const string RenderingDir = Root + "/Presentation/Rendering";
        public const string ScenePath = ScenesDir + "/Combate.unity";

        [MenuItem("Sal y Almas/Construir escena de combate", priority = 1)]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var path = BuildAndSave();
            EditorUtility.DisplayDialog("Sal y Almas",
                "Escena lista en " + path + ".\n\nPulsa Play para empezar. Elige el equipo y el encuentro (el Guardián es el jefe de la prueba).",
                "Entendido");
        }

        [MenuItem("Sal y Almas/Jugar combate", priority = 2)]
        public static void Play()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) BuildAndSave();
            else EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Sal y Almas/Borrar Partituras guardadas", priority = 20)]
        public static void ClearSavedPartituras()
        {
            PlayerPrefs.DeleteKey("SalYAlmas.Fase1.Partituras");
            PlayerPrefs.Save();
            Debug.Log("Partituras guardadas borradas.");
        }

        /// <summary>Construye la escena, la guarda y la agrega a Build Settings. Devuelve su ruta.</summary>
        public static string BuildAndSave()
        {
            EnsureFolder(ScenesDir);
            EnsureFolder(MaterialsDir);
            EnsureFolder(RenderingDir);
            EnsureUniversalRenderPipeline();
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            var materials = CreatePlanetMaterials();
            var enemy = CreateMaterial("Enemigo", Palette.Hex("#3A2A36"));
            var ground = CreateMaterial("Suelo de sal", Palette.Hex("#1E1B22"));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Populate(scene, materials, enemy, ground);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Sal y Almas: escena de combate construida en " + ScenePath);
            return ScenePath;
        }

        /// <summary>Crea todos los objetos de la escena dentro de la escena indicada. No guarda nada.</summary>
        public static SalYAlmasGame Populate(Scene scene, Material[] planetMaterials, Material enemyMaterial, Material groundMaterial)
        {
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Palette.Hex("#2B2638");
                RenderSettings.fog = false;

                var camGo = new GameObject("Cámara de combate");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Palette.Abyss;
                cam.fieldOfView = 50f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 200f;
                // Encuadre calculado para el lienzo de 1920×1080: las brujas quedan sobre sus retratos y la placa
                // del jefe bajo la línea de turnos.
                camGo.transform.position = new Vector3(0f, 7f, -14f);
                camGo.transform.LookAt(new Vector3(0f, 1.5f, 1.5f));

                var sun = new GameObject("Luz principal").AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = Palette.Hex("#FFE2B8");
                sun.intensity = 1.15f;
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

                var rim = new GameObject("Luz de contorno").AddComponent<Light>();
                rim.type = LightType.Directional;
                rim.color = Palette.Hex("#B9A6F0");
                rim.intensity = 0.45f;
                rim.shadows = LightShadows.None;
                rim.transform.rotation = Quaternion.Euler(20f, 160f, 0f);

                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Suelo de sal";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(3.2f, 1f, 2.2f);
                if (groundMaterial != null) floor.GetComponent<Renderer>().sharedMaterial = groundMaterial;
                UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());

                // Círculo de sal bajo el aquelarre y bajo los enemigos: solo decoración de prueba.
                Ring("Círculo del aquelarre", new Vector3(0, 0.02f, -2.2f), 6.2f, groundMaterial);
                Ring("Círculo de las raíces", new Vector3(0, 0.02f, 3.6f), 9f, groundMaterial);

                var root = new GameObject("Sal y Almas");
                var game = root.AddComponent<SalYAlmasGame>();
                game.BattleCamera = cam;
                game.RulesJson = LoadJson("rules.json");
                game.WitchesJson = LoadJson("witches.json");
                game.EnemiesJson = LoadJson("enemies.json");
                game.EncountersJson = LoadJson("encounters.json");
                game.PotionsJson = LoadJson("potions.json");
                game.EconomyJson = LoadJson("economy.json");
                game.RosterJson = LoadJson("roster.json");
                game.ProgressionJson = LoadJson("progression.json");
                game.WeaponsJson = LoadJson("weapons.json");
                game.BrewingJson = LoadJson("brewing.json");
                game.GachaJson = LoadJson("gacha.json");
                game.FresnoJson = LoadJson("fresno.json");
                game.FresnoEnemiesJson = LoadJson("fresno_enemies.json");
                game.PlanetMaterials = planetMaterials ?? new Material[7];
                game.EnemyMaterial = enemyMaterial;
                return game;
            }
            finally
            {
                if (previous.IsValid() && previous != scene && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static void Ring(string name, Vector3 position, float diameter, Material material)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = name;
            ring.transform.position = position;
            ring.transform.localScale = new Vector3(diameter, 0.01f, diameter * 0.45f);
            UnityEngine.Object.DestroyImmediate(ring.GetComponent<Collider>());
            var renderer = ring.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Palette.Hex("#3A3442"));
            block.SetColor("_Color", Palette.Hex("#3A3442"));
            renderer.SetPropertyBlock(block);
        }

        private static TextAsset LoadJson(string file)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(DataDir + "/" + file);
            if (asset == null) Debug.LogError("No encontré " + DataDir + "/" + file);
            return asset;
        }

        // ------------------------------------------------------------------ Materiales

        public static Material[] CreatePlanetMaterials()
        {
            var mats = new Material[7];
            foreach (var p in Planets.All)
                mats[(int)p] = CreateMaterial("Planeta " + Planets.DisplayName(p), Palette.Planet(p));
            return mats;
        }

        private static Shader LitShader()
        {
            Shader shader = null;
            if (GraphicsSettings.defaultRenderPipeline != null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return shader;
        }

        public static Material CreateMaterial(string name, Color color)
        {
            string path = MaterialsDir + "/" + name + ".mat";
            var shader = LitShader();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (shader != null && mat.shader != shader) mat.shader = shader;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.3f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.3f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ------------------------------------------------------------------ URP

        /// <summary>
        /// Si el proyecto no tiene un Render Pipeline asignado, crea uno de URP y lo activa. Usa reflexión para no
        /// romper la compilación del Editor si el paquete de URP cambia de nombre o falta.
        /// </summary>
        public static void EnsureUniversalRenderPipeline()
        {
            if (GraphicsSettings.defaultRenderPipeline != null) return;
            try
            {
                var assetType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
                var dataType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
                if (assetType == null || dataType == null)
                {
                    Debug.LogWarning("Sal y Almas: no encontré el paquete de URP (com.unity.render-pipelines.universal). La escena usará el pipeline integrado.");
                    return;
                }

                RenderPipelineAsset asset = null;
                var existing = AssetDatabase.FindAssets("t:" + assetType.Name);
                if (existing.Length > 0) asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(existing[0]));

                if (asset == null)
                {
                    var data = ScriptableObject.CreateInstance(dataType);
                    AssetDatabase.CreateAsset(data, RenderingDir + "/SalYAlmas_Renderer.asset");
                    var create = assetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m => m.Name == "Create" && m.GetParameters().Length == 1);
                    if (create == null) throw new MissingMethodException(assetType.Name, "Create");
                    asset = (RenderPipelineAsset)create.Invoke(null, new object[] { data });
                    AssetDatabase.CreateAsset(asset, RenderingDir + "/SalYAlmas_URP.asset");
                }

                GraphicsSettings.defaultRenderPipeline = asset;
                if (QualitySettings.renderPipeline != null) QualitySettings.renderPipeline = asset;
                AssetDatabase.SaveAssets();
                Debug.Log("Sal y Almas: URP activado con " + AssetDatabase.GetAssetPath(asset));
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Sal y Almas: no pude activar URP automáticamente (" + ex.Message + "). Asígnalo en Project Settings > Graphics.");
            }
        }

        // ------------------------------------------------------------------ Utilidades

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
