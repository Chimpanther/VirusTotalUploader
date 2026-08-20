using System;
using System.IO;
using uploader;
using Xunit;
using Newtonsoft.Json;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

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
        public void LoadSettings_ExistingFile_ReturnsDeserializedSettings()
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

                var expectedSettings = new Settings
                {
                    ApiKey = "TestApiKey123",
                    Language = "English",
                    DirectUpload = true
                };

                var json = JsonConvert.SerializeObject(expectedSettings);
                File.WriteAllText(settingsFile, json);

                // Act
                var settings = Settings.LoadSettings();

                // Assert
                Assert.NotNull(settings);
                Assert.Equal("TestApiKey123", settings.ApiKey);
                Assert.Equal("English", settings.Language);
                Assert.True(settings.DirectUpload);
            }
            finally
            {
                // Clean up the test file
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }

                // Restore
                if (hadExistingSettings)
                {
                    File.Move(backupFile, settingsFile);
                }
            }
        }
    }
}
