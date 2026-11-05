using System.IO;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Data;

namespace SalYAlmas.Tests
{
    /// <summary>Pruebas que solo corren fuera de Unity: vigilan que el núcleo siga siendo C# puro.</summary>
    public class CoreIsolationTests
    {
        private static string Root() => Directory.GetParent(TestKit.DataDir()).Parent.Parent.FullName;

        [Test]
        public void ElAsmdefDelNucleoNoReferenciaAUnity()
        {
            var asmdef = Json.Parse(File.ReadAllText(Path.Combine(Root(), "Assets", "SalYAlmas", "Core", "SalYAlmas.Core.asmdef")));
            Assert.IsTrue(asmdef.Get("noEngineReferences").AsBool, "noEngineReferences = true");
            Assert.AreEqual(0, asmdef.Get("references").Items.Count);
        }

        [Test]
        public void NingunArchivoDelNucleoUsaUnityEngine()
        {
            var files = Directory.GetFiles(Path.Combine(Root(), "Assets", "SalYAlmas", "Core"), "*.cs", SearchOption.AllDirectories);
            Assert.IsNotEmpty(files);
            foreach (var f in files)
            {
                var text = File.ReadAllText(f);
                Assert.IsFalse(text.Contains("using UnityEngine"), f);
                Assert.IsFalse(text.Contains("using UnityEditor"), f);
                Assert.IsFalse(text.Contains("System.Random"), f + ": el azar pasa por IRandomSource");
                Assert.IsFalse(text.Contains("DateTime.Now"), f + ": el núcleo no lee el reloj");
            }
        }

        [Test]
        public void LasPruebasCompartidasNoDependenDeUnity()
        {
            var files = Directory.GetFiles(Path.Combine(Root(), "Assets", "SalYAlmas", "Tests", "EditMode"), "*.cs", SearchOption.TopDirectoryOnly);
            Assert.IsTrue(files.Length >= 10);
            Assert.IsFalse(files.Any(f => File.ReadAllText(f).Contains("using UnityEngine")));
        }
    }
}
