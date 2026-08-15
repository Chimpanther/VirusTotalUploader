using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private string? _backup;
        private readonly string _settingsFile;

        public SettingsTests()
        {
            _settingsFile = Settings.GetSettingsFilename();

            // Backup existing if any
            if (File.Exists(_settingsFile))
            {
                _backup = File.ReadAllText(_settingsFile);
                File.Delete(_settingsFile);
            }
        }

        public void Dispose()
        {
            // Reset the static cache to ensure test isolation
            var field = typeof(Settings).GetField("_cachedSettings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, null);
            }

            // Restore backup
            if (_backup != null)
            {
                File.WriteAllText(_settingsFile, _backup);
            }
            else if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefault()
        {
            // Ensure it does not exist
            Assert.False(File.Exists(_settingsFile));

            // Act
            var settings = Settings.LoadSettings();

            // Assert default properties
            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefaultSettings()
        {
            // Double check that it's gone
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            // Act
            var settings = Settings.LoadSettings();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }
    }
}
