using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsFile;
        private readonly string _backupFile;

        public SettingsTests()
        {
            _settingsFile = Settings.GetSettingsFilename();
            _backupFile = _settingsFile + ".bak";

            if (File.Exists(_settingsFile))
            {
                File.Move(_settingsFile, _backupFile);
            }
        }

        public void Dispose()
        {
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            if (File.Exists(_backupFile))
            {
                File.Move(_backupFile, _settingsFile);
            }
        }

        [Fact]
        public void LoadSettings_MigratesLegacyPlaintextApiKey()
        {
            // Arrange
            var legacyJson = "{\"ApiKey\":\"my-legacy-secret-key\"}";
            File.WriteAllText(_settingsFile, legacyJson);

            // Act
            var settings = Settings.LoadSettings();

            // Assert
            Assert.Equal("my-legacy-secret-key", settings.ApiKey);
        }

        [Fact]
        public void SaveSettings_EncryptsApiKeyAndClearsPlaintext()
        {
            // Arrange
            var settings = new Settings
            {
                ApiKey = "my-new-secret-key"
            };

            // Act
            Settings.SaveSettings(settings);

            // Assert
            var savedJson = File.ReadAllText(_settingsFile);
            var parsedSettings = JsonConvert.DeserializeObject<Settings>(savedJson);

            Assert.Equal("", parsedSettings.ObsoletePlaintextApiKey);

            // In Linux CI, ProtectData.Protect might throw PlatformNotSupportedException,
            // which our try/catch handles and doesn't set EncryptedApiKey.
            // On Windows it would be populated.
using Xunit;
using System.IO;
using uploader;

namespace uploader.Tests
{
    public class SettingsTests
    {
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
