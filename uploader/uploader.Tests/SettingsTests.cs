using Xunit;
using System;
using System.IO;
using uploader;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class SettingsTests : IDisposable
    {
        private string _settingsFile;
        private string _backupFile;
        private bool _hadExistingSettings;

        public SettingsTests()
        {
            _settingsFile = Settings.GetSettingsFilename();
            _backupFile = _settingsFile + ".bak";
            _hadExistingSettings = File.Exists(_settingsFile);

            if (_hadExistingSettings)
            {
                File.Move(_settingsFile, _backupFile);
            }
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }
            Settings.ClearCache();
        }

        public void Dispose()
        {
            Settings.ClearCache();
            if (_hadExistingSettings)
            {
                if (File.Exists(_settingsFile))
                {
                    File.Delete(_settingsFile);
                }
                File.Move(_backupFile, _settingsFile);
            }
            else if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefaultSettings()
        {
            // Act
            var settings = Settings.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_CachesResult_And_SaveUpdatesCache()
        {
            // Act
            var initialSettings = Settings.LoadSettings();
            initialSettings.ApiKey = "test-key";
            Settings.SaveSettings(initialSettings);

            // Delete the file to prove we are reading from cache
            if (File.Exists(Settings.GetSettingsFilename()))
            {
                File.Delete(Settings.GetSettingsFilename());
            }

            var cachedSettings = Settings.LoadSettings();

            // Assert
            Assert.Equal("test-key", cachedSettings.ApiKey);
        }
    }
}
