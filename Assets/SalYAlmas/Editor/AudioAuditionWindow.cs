using System.IO;
using System.Linq;
using SalYAlmas.Presentation;
using UnityEditor;
using UnityEngine;

namespace SalYAlmas.EditorTools
{
    /// <summary>Banco de escucha que usa la misma mezcla del juego; no modifica assets ni el perfil.</summary>
    internal sealed class AudioAuditionWindow : EditorWindow
    {
        private string[] _clips;
        private string _filter = "";
        private Vector2 _scroll;

        [MenuItem("Sal y Almas/Audio/Banco de escucha")]
        private static void Open() => GetWindow<AudioAuditionWindow>("Banco de escucha");

        private void OnEnable()
        {
            _clips = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/SalYAlmas/Audio/Resources/SfxTemporal" })
                .Select(AssetDatabase.GUIDToAssetPath).Select(Path.GetFileNameWithoutExtension).OrderBy(n => n).ToArray();
        }

        private void OnGUI()
        {
            var player = Object.FindAnyObjectByType<TempSfxPlayer>();
            EditorGUILayout.HelpBox("Entra en Play para escuchar con los niveles y prioridades del juego. Los ajustes de esta ventana duran esta sesión de Play. M: música · N: efectos.", MessageType.Info);
            if (Application.isPlaying && player != null)
            {
                player.MasterVolume = EditorGUILayout.Slider("Efectos", player.MasterVolume, 0f, 1f);
                var music = Object.FindAnyObjectByType<TempMusicPlayer>();
                if (music != null) music.MasterVolume = EditorGUILayout.Slider("Música", music.MasterVolume, 0f, 1f);
                EditorGUILayout.LabelField("Voces", player.ActiveVoices + " / " + TempSfxPlayer.VoiceLimit + (player.Muted ? " · silenciadas" : ""));
            }
            _filter = EditorGUILayout.TextField("Filtrar", _filter);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || player == null || player.Muted))
                foreach (var clip in _clips ?? new string[0])
                    if ((_filter.Length == 0 || clip.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        && GUILayout.Button("▶  " + clip, EditorStyles.miniButton)) player.PlayCue(clip);
            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate() => Repaint();
    }
}
