using Microsoft.VisualStudio.TestTools.UnitTesting;
using uploader;
using System.IO;
using System;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [TestClass]
    public class SettingsTests
    {
        private string _settingsPath;
        private string _backupContent;
        private bool _hadSettings;

        [TestInitialize]
        public void Setup()
        {
            _settingsPath = Settings.GetSettingsFilename();
            _hadSettings = File.Exists(_settingsPath);
            if (_hadSettings)
            {
                _backupContent = File.ReadAllText(_settingsPath);
                File.Delete(_settingsPath);
            }
        }

        [TestCleanup]
        public void Teardown()
        {
            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }
            if (_hadSettings && _backupContent != null)
            {
                File.WriteAllText(_settingsPath, _backupContent);
            }
        }

        [TestMethod]
        public void SaveSettings_WithDefaultLanguage_SetsLanguageToEmptyString()
        {
            // Arrange
            var settings = new Settings { Language = "Default" };

            // Act
            Settings.SaveSettings(settings);

            // Assert
            var savedSettings = Settings.LoadSettings();
            Assert.AreEqual("", savedSettings.Language, "Language should be empty string when 'Default' is passed");

            var fileContent = File.ReadAllText(_settingsPath);
            Assert.IsTrue(fileContent.Contains("\"Language\":\"\"") || fileContent.Contains("\"Language\": \"\""), "The JSON file should contain the empty string for Language");
        }
    }
}
