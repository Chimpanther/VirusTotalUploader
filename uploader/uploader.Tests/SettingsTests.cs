using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string settingsFile;
        private readonly string backupFile;

        public SettingsTests()
        {
            settingsFile = Settings.GetSettingsFilename();
            backupFile = settingsFile + ".bak";

            if (File.Exists(settingsFile))
            {
                File.Copy(settingsFile, backupFile, true);
            }
        }

        public void Dispose()
        {
            if (File.Exists(backupFile))
            {
                File.Copy(backupFile, settingsFile, true);
                File.Delete(backupFile);
            }
            else if (File.Exists(settingsFile))
            {
                File.Delete(settingsFile);
            }
        }

        [Fact]
        public void SaveSettings_ShouldSaveFile_AndClearDefaultLanguage()
        {
            var settings = new Settings
            {
                ApiKey = "1234567890123456789012345678901234567890123456789012345678901234",
                Language = "English (Default)",
                DirectUpload = true
            };

            Settings.SaveSettings(settings);

            Assert.True(File.Exists(settingsFile), "Settings file should be created");
            Assert.Equal("", settings.Language, "Language containing 'Default' should be cleared in the object");

            var loadedSettings = Settings.LoadSettings();
            Assert.NotNull(loadedSettings);
            Assert.Equal("1234567890123456789012345678901234567890123456789012345678901234", loadedSettings.ApiKey);
            Assert.Equal("", loadedSettings.Language);
            Assert.True(loadedSettings.DirectUpload);

            Assert.NotNull(LocalizationHelper.Base);
        }

        [Fact]
        public void SaveSettings_ShouldUpdateLocalizationHelper()
        {
            var settings = new Settings
            {
                ApiKey = "test_key",
                Language = "",
                DirectUpload = false
            };

            // Set to null first to ensure it gets updated
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            LocalizationHelper.Base = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Settings.SaveSettings(settings);

            Assert.NotNull(LocalizationHelper.Base);
        }

        [Fact]
        public void SaveSettings_OverwritesExistingFile()
        {
            // Create initial file
            var initialSettings = new Settings
            {
                ApiKey = "initial_key",
                Language = "",
                DirectUpload = false
            };
            Settings.SaveSettings(initialSettings);

            // Create updated settings
            var updatedSettings = new Settings
            {
                ApiKey = "updated_key",
                Language = "",
                DirectUpload = true
            };

            Settings.SaveSettings(updatedSettings);

            var loadedSettings = Settings.LoadSettings();
            Assert.NotNull(loadedSettings);
            Assert.Equal("updated_key", loadedSettings.ApiKey);
            Assert.True(loadedSettings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefaultSettings()
        {
            // Arrange
            var file = Settings.GetSettingsFilename();
            var bak = file + ".bak";
            bool hadExistingSettings = File.Exists(file);

            try
            {
                if (hadExistingSettings)
                {
                    File.Move(file, bak);
                }

                // Double check that it's gone
                if (File.Exists(file))
                {
                    File.Delete(file);
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
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    File.Move(bak, file);
                }
            }
        }
    }
}
