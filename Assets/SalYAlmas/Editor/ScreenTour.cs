using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SalYAlmas.EditorTools
{
    /// <summary>Compila un jugador de Windows de desarrollo para revisar las pantallas fuera del Editor. Menú y modo batch.</summary>
    public static class ScreenTour
    {
        [MenuItem("Sal y Almas/Compilar jugador de Windows", priority = 30)]
        public static void BuildPlayer()
        {
            var scene = BattleSceneBuilder.BuildAndSave();
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scene },
                locationPathName = "Builds/Windows/SalYAlmas.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            });
            Debug.Log("Compilación: " + report.summary.result + " · " + report.summary.totalErrors + " errores");
            if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
