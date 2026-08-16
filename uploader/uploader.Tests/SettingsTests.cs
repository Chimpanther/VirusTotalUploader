using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class SettingsTests : IDisposable
    {
        public SettingsTests()
        {
            Settings.ClearCache();
        }

        public void Dispose()
        {
            Settings.ClearCache();
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
                else if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
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
                else if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
        }
    }
}
