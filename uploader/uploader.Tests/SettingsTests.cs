using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using uploader;

namespace uploader.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void SaveAndLoadSettings_ApiKeyIsHandledCorrectly()
        {
            var file = Settings.GetSettingsFilename();
            var backupFile = file + ".bak";

            if (File.Exists(file))
                File.Move(file, backupFile);

            try
            {
                var originalSettings = new Settings
                {
                    ApiKey = "super_secret_api_key_123",
                    Language = "",
                    DirectUpload = true
                };

                Settings.SaveSettings(originalSettings);

                var loadedSettings = Settings.LoadSettings();

                Assert.AreEqual(originalSettings.ApiKey, loadedSettings.ApiKey, "ApiKey should match after load.");
                Assert.AreEqual(originalSettings.Language, loadedSettings.Language, "Language should match after load.");
                Assert.AreEqual(originalSettings.DirectUpload, loadedSettings.DirectUpload, "DirectUpload should match after load.");
            }
            finally
            {
                if (File.Exists(file))
                    File.Delete(file);
                if (File.Exists(backupFile))
                    File.Move(backupFile, file);
            }
        }
    }
}
