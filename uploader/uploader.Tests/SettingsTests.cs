using uploader;
using System.IO;
using System;
using Newtonsoft.Json;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private string _settingsPath;
        private string _backupContent;
        private bool _hadSettings;

        public SettingsTests()
        {
            _settingsPath = Settings.GetSettingsFilename();
            _hadSettings = File.Exists(_settingsPath);
            if (_hadSettings)
            {
                _backupContent = File.ReadAllText(_settingsPath);
                File.Delete(_settingsPath);
            }
        }

        public void Dispose()
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

        [Fact]
        public void SaveSettings_WithDefaultLanguage_SetsLanguageToEmptyString()
        {
            // Arrange
            var settings = new Settings { Language = "Default" };

            // Act
            Settings.SaveSettings(settings);

            // Assert
            var savedSettings = Settings.LoadSettings();
            Assert.Equal("", savedSettings.Language);

            var fileContent = File.ReadAllText(_settingsPath);
            Assert.True(fileContent.Contains("\"Language\":\"\"") || fileContent.Contains("\"Language\": \"\""), "The JSON file should contain the empty string for Language");
        }
    }
}
