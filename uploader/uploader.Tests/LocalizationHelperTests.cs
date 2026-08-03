using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Newtonsoft.Json;
using uploader;

namespace uploader.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class LocalizationHelperTests
    {
        private string? _settingsPath;
        private string? _testLanguageFile;

        [TestInitialize]
        public void Setup()
        {
            // Use a temporary path for the tests so it doesn't destructively touch %APPDATA%
            _settingsPath = Path.Combine(Path.GetTempPath(), $"vtu_settings_test_{Guid.NewGuid()}.json");
            _testLanguageFile = Path.Combine(Path.GetTempPath(), $"test_lang_{Guid.NewGuid()}.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_settingsPath != null && File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }

            if (_testLanguageFile != null && File.Exists(_testLanguageFile))
            {
                File.Delete(_testLanguageFile);
            }

            // Reset Base
            LocalizationHelper.Base = null!;
        }

        [TestMethod]
        public void Update_WithNoSettings_SetsBaseToNewInstance()
        {
            LocalizationHelper.Update(_settingsPath);

            Assert.IsNotNull(LocalizationHelper.Base);
            Assert.AreEqual("Settings", LocalizationHelper.Base.SettingsForm_Title); // Verify default
        }

        [TestMethod]
        public void Update_WithEmptyLanguage_SetsBaseToNewInstance()
        {
            // Create settings with empty language
            var settings = new Settings { Language = "" };
            var serialized = JsonConvert.SerializeObject(settings);
            if (_settingsPath != null)
                File.WriteAllText(_settingsPath, serialized);

            LocalizationHelper.Update(_settingsPath);

            Assert.IsNotNull(LocalizationHelper.Base);
            Assert.AreEqual("Settings", LocalizationHelper.Base.SettingsForm_Title);
        }

        [TestMethod]
        public void Update_WithValidLanguage_LoadsLocalization()
        {
            // Create a test localization file
            var testBase = new LocalizationBase { SettingsForm_Title = "Custom Settings Title" };
            var serializedBase = JsonConvert.SerializeObject(testBase);
            if (_testLanguageFile != null)
                File.WriteAllText(_testLanguageFile, serializedBase);

            // Create settings with the test language file
            var settings = new Settings { Language = _testLanguageFile! };
            var serializedSettings = JsonConvert.SerializeObject(settings);
            if (_settingsPath != null)
                File.WriteAllText(_settingsPath, serializedSettings);

            LocalizationHelper.Update(_settingsPath);

            Assert.IsNotNull(LocalizationHelper.Base);
            Assert.AreEqual("Custom Settings Title", LocalizationHelper.Base.SettingsForm_Title);
        }
    }
}
