using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class LocalizationHelperTests : IDisposable
    {
        private const string LocalFolder = "local";

        public LocalizationHelperTests()
        {
            CleanupTestState();
        }

        public void Dispose()
        {
            CleanupTestState();
        }

        private void CleanupTestState()
        {
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
            if (File.Exists("export.json"))
            {
                File.Delete("export.json");
            }
            LocalizationHelper.Base = null;
        }

        [Fact]
        public void Load_ValidJson_SetsBaseProperty()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                var json = "{\"MainForm_More\": \"Test More\"}";
                File.WriteAllText(tempFile, json);

                // Act
                LocalizationHelper.Load(tempFile);

                // Assert
                Assert.NotNull(LocalizationHelper.Base);
                Assert.Equal("Test More", LocalizationHelper.Base.MainForm_More);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private void RunWithMockSettings(Settings tempSettings, Action testAction)
        {
            var settingsFile = Settings.GetSettingsFilename();
            var backupFile = settingsFile + ".bak";
            bool hadExistingSettings = File.Exists(settingsFile);

            if (hadExistingSettings)
            {
                File.Move(settingsFile, backupFile);
            }

            try
            {
                Settings.SaveSettings(tempSettings);
                testAction();
            }
            finally
            {
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }

                if (hadExistingSettings)
                {
                    File.Move(backupFile, settingsFile);
                }
            }
        }

        [Fact]
        public void Update_WithLanguageSettings_LoadsLanguage()
        {
            Directory.CreateDirectory(LocalFolder);
            string langFile = Path.Combine(LocalFolder, "test_lang.json");
            File.WriteAllText(langFile, "{\"MainForm_More\": \"Language More\"}");

            RunWithMockSettings(new Settings { Language = langFile }, () =>
            {
                LocalizationHelper.Update();
                Assert.NotNull(LocalizationHelper.Base);
                Assert.Equal("Language More", LocalizationHelper.Base.MainForm_More);
            });
        }

        [Fact]
        public void Update_WithoutLanguageSettings_SetsDefaultBase()
        {
            RunWithMockSettings(new Settings { Language = "" }, () =>
            {
                LocalizationHelper.Update();
                Assert.NotNull(LocalizationHelper.Base);
                Assert.Equal("More", LocalizationHelper.Base.MainForm_More);
            });
        }

        [Fact]
        public void Export_CreatesJsonFileWithDefaultValues()
        {
            // Act
            LocalizationHelper.Export();

            // Assert
            Assert.True(File.Exists("export.json"));
            var json = File.ReadAllText("export.json");
            Assert.Contains("MainForm_More", json);
            Assert.Contains("More", json);
        }

        [Fact]
        public void GetLanguages_DirectoryDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Single(languages);
            Assert.Equal("", languages[0]);
        }

        [Fact]
        public void GetLanguages_DirectoryExistsButEmpty_ReturnsEmptyArray()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);

            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Empty(languages);
        }

        [Fact]
        public void GetLanguages_DirectoryHasFiles_ReturnsFilePaths()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);
            string file1 = Path.Combine(LocalFolder, "en.json");
            string file2 = Path.Combine(LocalFolder, "fr.json");
            File.WriteAllText(file1, "{}");
            File.WriteAllText(file2, "{}");

            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Equal(2, languages.Length);
            Assert.Contains(file1, languages);
            Assert.Contains(file2, languages);
        }
    }
}
