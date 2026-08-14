using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uploader;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [TestClass]
    public class SettingsTests
    {
        private string? settingsFile;
        private string? backupFile;

        [TestInitialize]
        public void Setup()
        {
            settingsFile = Settings.GetSettingsFilename();
            backupFile = settingsFile + ".bak";

            if (File.Exists(settingsFile))
            {
                File.Copy(settingsFile, backupFile, true);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (backupFile != null && settingsFile != null)
            {
                if (File.Exists(backupFile))
                {
                    File.Copy(backupFile, settingsFile, true);
                    File.Delete(backupFile);
                }
                else if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
        }

        [TestMethod]
        public void SaveSettings_ShouldSaveFile_AndClearDefaultLanguage()
        {
            var settings = new Settings
            {
                ApiKey = "1234567890123456789012345678901234567890123456789012345678901234",
                Language = "English (Default)",
                DirectUpload = true
            };

            Settings.SaveSettings(settings);

            Assert.IsTrue(File.Exists(settingsFile));
            Assert.AreEqual("", settings.Language);

            var loadedSettings = Settings.LoadSettings();
            Assert.IsNotNull(loadedSettings);
            Assert.AreEqual("1234567890123456789012345678901234567890123456789012345678901234", loadedSettings.ApiKey);
            Assert.AreEqual("", loadedSettings.Language);
            Assert.IsTrue(loadedSettings.DirectUpload);

            Assert.IsNotNull(LocalizationHelper.Base);
        }

        [TestMethod]
        public void SaveSettings_ShouldUpdateLocalizationHelper()
        {
            var settings = new Settings
            {
                ApiKey = "test_key",
                Language = "",
                DirectUpload = false
            };

            // Set to null first to ensure it gets updated
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            LocalizationHelper.Base = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Settings.SaveSettings(settings);

            Assert.IsNotNull(LocalizationHelper.Base);
        }

        [TestMethod]
        public void SaveSettings_OverwritesExistingFile()
        {
            // Create initial file
            var initialSettings = new Settings
            {
                ApiKey = "initial_key",
                Language = "",
                DirectUpload = false
            };
            Settings.SaveSettings(initialSettings);

            // Create updated settings
            var updatedSettings = new Settings
            {
                ApiKey = "updated_key",
                Language = "",
                DirectUpload = true
            };

            Settings.SaveSettings(updatedSettings);

            var loadedSettings = Settings.LoadSettings();
            Assert.IsNotNull(loadedSettings);
            Assert.AreEqual("updated_key", loadedSettings.ApiKey);
            Assert.IsTrue(loadedSettings.DirectUpload);
        }
    }
}
