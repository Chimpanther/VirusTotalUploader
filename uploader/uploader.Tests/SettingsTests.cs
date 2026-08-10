using System;
using System.IO;
using uploader;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsFilePath;
        private readonly string _backupFilePath;
        private readonly bool _hadExistingSettings;

        public SettingsTests()
        {
            _settingsFilePath = Settings.GetSettingsFilename();
            _backupFilePath = _settingsFilePath + ".bak";
            _hadExistingSettings = File.Exists(_settingsFilePath);

            // Backup existing settings file if it exists
            if (_hadExistingSettings)
            {
                if (File.Exists(_backupFilePath))
                {
                    File.Delete(_backupFilePath);
                }
                File.Move(_settingsFilePath, _backupFilePath);
            }
        }

        public void Dispose()
        {
            // Restore settings file to original state
            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }

            if (_hadExistingSettings)
            {
                if (File.Exists(_backupFilePath))
                {
                    File.Move(_backupFilePath, _settingsFilePath);
                }
            }
        }

        [Fact]
        public void LoadSettings_FileDoesNotExist_ReturnsDefaultSettings()
        {
            // Arrange
            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }

            // Act
            var settings = Settings.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_FileExists_ReturnsDeserializedSettings()
        {
            // Arrange
            var json = @"{
                ""ApiKey"": ""test-api-key"",
                ""Language"": ""en-US"",
                ""DirectUpload"": true
            }";

            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }

            // Make sure the directory exists
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_settingsFilePath, json);

            // Act
            var settings = Settings.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("test-api-key", settings.ApiKey);
            Assert.Equal("en-US", settings.Language);
            Assert.True(settings.DirectUpload);
        }
    }
}
