using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        public void Dispose()
        {
            LocalizationHelper.Base = null;
        }
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
                if (File.Exists("en")) File.Delete("en");
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
                if (File.Exists("en")) File.Delete("en");
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
            var backupFile = settingsFile + ".bak";
            bool hadExistingSettings = File.Exists(settingsFile);

            try
            {
                // Backup
                if (File.Exists("en")) File.Delete("en");
                if (hadExistingSettings)
                {
                    File.Move(settingsFile, backupFile);
                }
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }

                // Arrange
                var settings = new Settings
                {
                    ApiKey = "test-api-key",
                    Language = "en",
                    DirectUpload = true
                };

                // Act
                File.WriteAllText("en", "{}");
                Settings.SaveSettings(settings);

                // Assert
                Assert.True(File.Exists(settingsFile));
                var fileContent = File.ReadAllText(settingsFile);
                Assert.Contains("test-api-key", fileContent);
                Assert.Contains("en", fileContent);
                Assert.Contains("true", fileContent.ToLower()); // DirectUpload: true
                Assert.NotNull(LocalizationHelper.Base);
            }
            finally
            {
                // Restore
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
                if (File.Exists("en")) File.Delete("en");
                if (hadExistingSettings)
                {
                    File.Move(backupFile, settingsFile);
                }
            }
        }

        [Fact]
        public void SaveSettings_LanguageContainsDefault_LanguageIsCleared()
        {
            var settingsFile = Settings.GetSettingsFilename();
            var backupFile = settingsFile + ".bak";
            bool hadExistingSettings = File.Exists(settingsFile);

            try
            {
                // Backup
                if (File.Exists("en")) File.Delete("en");
                if (hadExistingSettings)
                {
                    File.Move(settingsFile, backupFile);
                }
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }

                // Arrange
                var settings = new Settings
                {
                    ApiKey = "test-api-key",
                    Language = "English (Default)",
                    DirectUpload = false
                };

                // Act
                File.WriteAllText("en", "{}");
                Settings.SaveSettings(settings);

                // Assert
                Assert.Equal("", settings.Language);
                Assert.True(File.Exists(settingsFile));
                var fileContent = File.ReadAllText(settingsFile);
                Assert.DoesNotContain("English (Default)", fileContent);
                Assert.Contains("\"Language\":\"\"", fileContent.Replace(" ", "")); // Ensure empty language is saved
                Assert.NotNull(LocalizationHelper.Base);
            }
            finally
            {
                // Restore
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
                if (File.Exists("en")) File.Delete("en");
                if (hadExistingSettings)
                {
                    File.Move(backupFile, settingsFile);
                }
            }
        }
    }
}