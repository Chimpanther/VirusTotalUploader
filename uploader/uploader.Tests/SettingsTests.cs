using System;
using System.IO;
using uploader;
using Newtonsoft.Json;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string? originalAppData;
        private readonly string? originalXdgConfigHome;
        private readonly string testAppDataPath;
        private readonly string testSettingsFile;

        public SettingsTests()
        {
            testAppDataPath = Path.Combine(Path.GetTempPath(), "uploader_test_appdata_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(testAppDataPath);

            // Set Environment to our test folder temporarily
            originalAppData = Environment.GetEnvironmentVariable("APPDATA");
            Environment.SetEnvironmentVariable("APPDATA", testAppDataPath);

            originalXdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testAppDataPath);

            testSettingsFile = Path.Combine(testAppDataPath, "vtu_settings.json");
        }

        public void Dispose()
        {
            // Restore original environment
            Environment.SetEnvironmentVariable("APPDATA", originalAppData);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", originalXdgConfigHome);

            if (Directory.Exists(testAppDataPath))
            {
                Directory.Delete(testAppDataPath, true);
            }
        }

        [Fact]
        public void LoadSettings_WhenFileDoesNotExist_ReturnsDefaultSettings()
        {
            var file = Settings.GetSettingsFilename();
            if (File.Exists(file))
                File.Delete(file);

            var settings = Settings.LoadSettings();

            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_WhenFileExists_LoadsSettingsCorrectly()
        {
            var testSettings = new Settings
            {
                ApiKey = "test_api_key_12345",
                Language = "English",
                DirectUpload = true
            };

            var file = Settings.GetSettingsFilename();
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, JsonConvert.SerializeObject(testSettings));

            var loadedSettings = Settings.LoadSettings();

            Assert.NotNull(loadedSettings);
            Assert.Equal("test_api_key_12345", loadedSettings.ApiKey);
            Assert.Equal("English", loadedSettings.Language);
            Assert.True(loadedSettings.DirectUpload);
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
