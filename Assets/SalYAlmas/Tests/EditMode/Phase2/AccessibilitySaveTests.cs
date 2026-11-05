using NUnit.Framework;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Tests
{
    public class AccessibilitySaveTests
    {
        [Test]
        public void ReducirMovimientoSobreviveAlGuardado()
        {
            var profile = Phase2Kit.NewGame().Profile;
            profile.Settings.ReduceMotion = true;
            var loaded = ProfileSerializer.FromJson(ProfileSerializer.ToJson(profile));
            Assert.IsTrue(loaded.Settings.ReduceMotion);
        }

        [Test]
        public void GuardadosAnterioresSinLaOpcionConservanLasDemasPreferencias()
        {
            var profile = Phase2Kit.NewGame().Profile;
            profile.Settings.Language = "en";
            profile.Settings.LargeText = true;
            string json = ProfileSerializer.ToJson(profile);
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""ReduceMotion""\s*:\s*false\s*,?", "");
            var loaded = ProfileSerializer.FromJson(json);
            Assert.IsFalse(loaded.Settings.ReduceMotion);
            Assert.IsTrue(loaded.Settings.LargeText);
            Assert.AreEqual("en", loaded.Settings.Language);
        }
    }
}
