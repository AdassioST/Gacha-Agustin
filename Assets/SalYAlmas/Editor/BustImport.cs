using UnityEditor;
using UnityEngine;

namespace SalYAlmas.EditorTools
{
    /// <summary>
    /// Importación de los bustos de 160 px nativos: sprite con filtro Point, sin compresión y sin
    /// mipmaps, para que los píxeles lleguen intactos a la pantalla. Los copia a esta carpeta
    /// <c>python scripts/recortes.py unity</c>.
    /// </summary>
    internal sealed class BustImport : AssetPostprocessor
    {
        internal const string Folder = "Assets/SalYAlmas/Art/Bustos/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder, System.StringComparison.Ordinal)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
        }
    }
}
