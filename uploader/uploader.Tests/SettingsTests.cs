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
