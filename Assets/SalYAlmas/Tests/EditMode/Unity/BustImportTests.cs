using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SalYAlmas.Tests.UnityOnly
{
    /// <summary>Los bustos de Assets/SalYAlmas/Art/Bustos deben conservar sus píxeles al importarse.</summary>
    public sealed class BustImportTests
    {
        private const string Folder = "Assets/SalYAlmas/Art/Bustos";
        private const int NativeSize = 160;

        [Test]
        public void EveryBust_IsAnUncompressedPointSpriteAtNativeSize()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Folder });
            Assert.That(guids, Is.Not.Empty, "No hay bustos: corre python scripts/recortes.py unity");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point), path);
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), path);
                Assert.That(importer.mipmapEnabled, Is.False, path);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite, Is.Not.Null, path);
                Assert.That(sprite.rect.width, Is.EqualTo(NativeSize), path);
                Assert.That(sprite.rect.height, Is.EqualTo(NativeSize), path);
            }
        }
    }
}
