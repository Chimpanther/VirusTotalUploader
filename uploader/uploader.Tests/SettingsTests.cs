using System;
using System.IO;
using uploader;
using Newtonsoft.Json;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsPath;
        private readonly string? _backupContent;
        private readonly bool _hadSettings;

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

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefaultSettings()
        {
            // Arrange
            var settingsFile = Settings.GetSettingsFilename();
            var backupFile = settingsFile + ".bak";
            bool hadExistingSettings = File.Exists(settingsFile);

            try
            {
                if (hadExistingSettings)
                {
                    File.Move(settingsFile, backupFile);
                }

                // Double check that it's gone
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }

                // Act
                var settings = Settings.LoadSettings();

                // Assert
                Assert.NotNull(settings);
                Assert.Equal("", settings.ApiKey);
                Assert.Equal("", settings.Language);
                Assert.False(settings.DirectUpload);
            }
            finally
            {
                // Restore
                if (hadExistingSettings)
                {
                    if (File.Exists(settingsFile))
                    {
                        File.Delete(settingsFile);
                    }
                    File.Move(backupFile, settingsFile);
                }
            }
        }
    }
}
