using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SalYAlmas.EditorTools
{
    /// <summary>
    /// Compilación por línea de comandos para revisar la interfaz con capturas. Uso:
    /// Unity -batchmode -quit -projectPath . -executeMethod SalYAlmas.EditorTools.UiBuild.Player -buildTarget Win64
    /// Deja el juego en Build/Revision/SalYAlmas.exe.
    /// </summary>
    public static class UiBuild
    {
        private const string TmpPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        /// <summary>TextMeshPro necesita sus shaders y ajustes dentro de Assets. Solo se importan una vez.</summary>
        [MenuItem("Sal y Almas/Interfaz/Importar recursos de TextMeshPro", priority = 40)]
        public static void EnsureTmp()
        {
            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro")) return;
            AssetDatabase.ImportPackage(Path.GetFullPath(TmpPackage), false);
            AssetDatabase.Refresh();
            Debug.Log("Recursos de TextMeshPro importados.");
        }

        public static void Player()
        {
            EnsureTmp();
            AssetDatabase.Refresh();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) scenes = new[] { BattleSceneBuilder.ScenePath };
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.GetFullPath("Build/Revision/SalYAlmas.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Compilación de revisión: " + report.summary.result + " en " + report.summary.totalTime + ", errores " + report.summary.totalErrors);
            if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        /// <summary>
        /// Compilación de producción, sin la opción Development: comprueba que compilan las ramas que solo existen fuera
        /// del desarrollo (sin perfil de prueba, sin tienda de pruebas, sin capturas). Uso:
        /// Unity -batchmode -quit -projectPath . -executeMethod SalYAlmas.EditorTools.UiBuild.Release -buildTarget Win64
        /// (o -buildTarget Android). Deja el juego en Build/Produccion/.
        /// </summary>
        public static void Release()
        {
            EnsureTmp();
            AssetDatabase.Refresh();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) scenes = new[] { BattleSceneBuilder.ScenePath };
            var target = EditorUserBuildSettings.activeBuildTarget;
            string file = target == BuildTarget.Android ? "SalYAlmas.apk" : "SalYAlmas.exe";
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.GetFullPath(Path.Combine("Build/Produccion", target.ToString(), file)),
                target = target,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Compilación de producción (" + target + "): " + report.summary.result + " en " + report.summary.totalTime + ", errores " + report.summary.totalErrors);
            if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        /// <summary>Reconstruye la escena con el constructor oficial y compila.</summary>
        public static void RebuildAndPlayer()
        {
            EnsureTmp();
            BattleSceneBuilder.BuildAndSave();
            Player();
        }
    }

    /// <summary>
    /// Importación del kit de interfaz: sin pixelado (bilineal con mipmaps), sin compresión visible en lo fino, y con
    /// repetición en las superficies que se tilean para que nunca se estiren. Los bustos de 160 px quedan fuera del juego.
    /// </summary>
    internal sealed class UiArtImport : AssetPostprocessor
    {
        private const string Folder = "Assets/SalYAlmas/Art/Ui/Resources/Ui/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder, StringComparison.Ordinal)) return;
            var imp = (TextureImporter)assetImporter;
            string rel = assetPath.Substring(Folder.Length);
            bool tile = rel.Contains("/tile_") || rel.EndsWith("noise.png", StringComparison.Ordinal);
            bool linear = rel.EndsWith("noise.png", StringComparison.Ordinal);
            imp.textureType = TextureImporterType.Default;
            imp.alphaIsTransparency = !linear;
            imp.npotScale = TextureImporterNPOTScale.None;
            imp.mipmapEnabled = true;
            imp.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
            imp.streamingMipmaps = false;
            imp.filterMode = FilterMode.Trilinear;
            imp.anisoLevel = 2;
            imp.wrapMode = tile ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            imp.sRGBTexture = !linear;
            imp.isReadable = false;
            imp.maxTextureSize = rel.StartsWith("bg/", StringComparison.Ordinal) ? 4096 : 2048;
            imp.textureCompression = rel.StartsWith("brujas/", StringComparison.Ordinal) || rel.StartsWith("logo/", StringComparison.Ordinal)
                ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Compressed;
            imp.crunchedCompression = false;
        }
    }
}
