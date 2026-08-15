using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _tempAppData;
        private readonly string _originalAppData;

        public SettingsTests()
        {
            _originalAppData = Environment.GetEnvironmentVariable("APPDATA") ?? string.Empty;
            _tempAppData = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempAppData);
            Environment.SetEnvironmentVariable("APPDATA", _tempAppData);
        }

        public void Dispose()
        {
            // Reset the static cache to ensure test isolation
            var field = typeof(Settings).GetField("_cachedSettings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, null);
            }

            Environment.SetEnvironmentVariable("APPDATA", string.IsNullOrEmpty(_originalAppData) ? null : _originalAppData);
            if (Directory.Exists(_tempAppData))
            {
                Directory.Delete(_tempAppData, true);
            }
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefault()
        {
            var settingsFile = Settings.GetSettingsFilename();

            // Ensure it does not exist
            Assert.False(File.Exists(settingsFile));

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
            // Arrange
            var settingsFile = Settings.GetSettingsFilename();

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
    }
}
