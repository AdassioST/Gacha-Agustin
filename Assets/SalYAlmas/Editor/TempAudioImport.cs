using UnityEditor;
using UnityEngine;

namespace SalYAlmas.EditorTools
{
    /// <summary>
    /// Importación del audio temporal del prototipo. La música va en streaming para no ocupar memoria;
    /// los efectos cortos se descomprimen al cargar. La Fase 3C pasa todo a FMOD.
    /// </summary>
    internal sealed class TempAudioImport : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            bool music = assetPath.Contains("/Resources/MusicaTemporal/");
            bool sfx = assetPath.Contains("/Resources/SfxTemporal/");
            if (!music && !sfx) return;

            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            if (music)
            {
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.7f;
            }
            else
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
            }
            importer.defaultSampleSettings = settings;
        }
    }
}
