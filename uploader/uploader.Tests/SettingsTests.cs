using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefault()
        {
            var settingsFile = Settings.GetSettingsFilename();

            // Backup existing if any
            string? backup = null;
            if (File.Exists(settingsFile))
            {
                backup = File.ReadAllText(settingsFile);
                File.Delete(settingsFile);
            }

            try
            {
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
            finally
            {
                // Restore backup
                if (backup != null)
                {
                    File.WriteAllText(settingsFile, backup);
                }
            }
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

        [Fact]
        public void SaveSettings_ValidSettings_WritesToFile()
        {
            var settingsFile = Settings.GetSettingsFilename();
            string? backup = null;

            if (File.Exists(settingsFile))
            {
                backup = File.ReadAllText(settingsFile);
                File.Delete(settingsFile);
            }

            try
            {
                var settings = new Settings
                {
                    ApiKey = "test_api_key",
                    Language = "",
                    DirectUpload = true
                };

                Settings.SaveSettings(settings);

                Assert.True(File.Exists(settingsFile));
                var content = File.ReadAllText(settingsFile);
                Assert.Contains("test_api_key", content);
                Assert.Contains("\"DirectUpload\":true", content.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);

                var loaded = Settings.LoadSettings();
                Assert.Equal("test_api_key", loaded.ApiKey);
                Assert.True(loaded.DirectUpload);
            }
            finally
            {
                if (backup != null)
                {
                    File.WriteAllText(settingsFile, backup);
                }
                else if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
        }

        [Fact]
        public void SaveSettings_LanguageContainsDefault_LanguageIsCleared()
        {
            var settingsFile = Settings.GetSettingsFilename();
            string? backup = null;

            if (File.Exists(settingsFile))
            {
                backup = File.ReadAllText(settingsFile);
                File.Delete(settingsFile);
            }

            try
            {
                var settings = new Settings
                {
                    ApiKey = "test_api_key_2",
                    Language = "en_Default",
                    DirectUpload = false
                };

                Settings.SaveSettings(settings);

                Assert.Equal("", settings.Language);

                var loaded = Settings.LoadSettings();
                Assert.Equal("", loaded.Language);
            }
            finally
            {
                if (backup != null)
                {
                    File.WriteAllText(settingsFile, backup);
                }
                else if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
        }
    }
}
