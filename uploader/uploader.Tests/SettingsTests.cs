using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    // Important: We disable parallelization for this class because it manipulates
    // global state (the settings file in ApplicationData).
    // Other tests running concurrently could interfere with or be interfered by this.
    [Collection("SettingsTests")]
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsFile;
        private readonly string _backupFile;

        public SettingsTests()
        {
            _settingsFile = Settings.GetSettingsFilename();
            _backupFile = _settingsFile + ".bak";

            // Backup existing settings file if it exists
            if (File.Exists(_settingsFile))
            {
                File.Copy(_settingsFile, _backupFile, true);
            }
        }

        public void Dispose()
        {
            // Restore backup if it was created
            if (File.Exists(_backupFile))
            {
                File.Copy(_backupFile, _settingsFile, true);
                File.Delete(_backupFile);
            }
            else if (File.Exists(_settingsFile))
            {
                // If there was no backup, it means the file didn't exist before the test.
                // We should clean up any file created during the test.
                File.Delete(_settingsFile);
            }
        }

        [Fact]
        public void LoadSettings_FileDoesNotExist_ReturnsDefaultSettings()
        {
            // Arrange
            // Ensure the file does not exist
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
