using System;
using System.IO;
using Newtonsoft.Json;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests : IDisposable
    {
        private readonly string _originalCurrentDirectory;
        private readonly string _testDirectory;
        private readonly string _settingsFile;
        private readonly string _settingsBackup;
        private readonly bool _settingsExisted;

        public LocalizationHelperTests()
        {
            _originalCurrentDirectory = Environment.CurrentDirectory;
            _testDirectory = Path.Combine(Path.GetTempPath(), "vtu-localization-" + Guid.NewGuid());
            Directory.CreateDirectory(_testDirectory);
            Environment.CurrentDirectory = _testDirectory;

            SettingsManager.ClearCache();
            _settingsFile = Path.GetFullPath(SettingsManager.GetSettingsFilename());
            _settingsExisted = File.Exists(Path.GetFullPath(_settingsFile));
            _settingsBackup = _settingsExisted ? File.ReadAllText(Path.GetFullPath(_settingsFile)) : string.Empty;
            LocalizationHelper.Base = null!;
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = _originalCurrentDirectory;

            if (_settingsExisted)
            {
                File.WriteAllText(Path.GetFullPath(_settingsFile), _settingsBackup);
            }
            else if (File.Exists(Path.GetFullPath(_settingsFile)))
            {
                File.Delete(Path.GetFullPath(_settingsFile));
            }

            LocalizationHelper.Base = null!;
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void GetLanguages_DirectoryDoesNotExist_ReturnsArrayWithEmptyString()
        {
            var languages = LocalizationHelper.GetLanguages();

            Assert.NotNull(languages);
            Assert.Single(languages);
            Assert.Equal("", languages[0]);
        }

        [Fact]
        public void GetLanguages_DirectoryExistsButEmpty_ReturnsEmptyArray()
        {
            Directory.CreateDirectory("local");

            var languages = LocalizationHelper.GetLanguages();

            Assert.NotNull(languages);
            Assert.Empty(languages);
        }

        [Fact]
        public void GetLanguages_DirectoryHasFiles_ReturnsFilePaths()
        {
            Directory.CreateDirectory("local");
            var file1 = Path.Combine("local", "en.json");
            var file2 = Path.Combine("local", "fr.json");
            File.WriteAllText(file1, "{}");
            File.WriteAllText(file2, "{}");

            var languages = LocalizationHelper.GetLanguages();

            Assert.NotNull(languages);
            Assert.Equal(2, languages.Length);
            Assert.Contains(file1, languages);
            Assert.Contains(file2, languages);
        }

        [Fact]
        public void Load_ValidJson_SetsBaseProperty()
        {
            var languageFile = Path.Combine(_testDirectory, "language.json");
            File.WriteAllText(languageFile, "{\"MainForm_More\":\"Test More\"}");

            LocalizationHelper.Load(languageFile);

            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("Test More", LocalizationHelper.Base.MainForm_More);
        }

        [Fact]
        public void Load_EmptyFile_SetsBaseToNull()
        {
            var languageFile = Path.Combine(_testDirectory, "empty.json");
            File.WriteAllText(languageFile, "");

            LocalizationHelper.Load(languageFile);

            Assert.Null(LocalizationHelper.Base);
        }

        [Fact]
        public void Load_InvalidJson_ThrowsJsonReaderException()
        {
            var languageFile = Path.Combine(_testDirectory, "invalid.json");
            File.WriteAllText(languageFile, "not valid json");

            Assert.Throws<JsonReaderException>(() => LocalizationHelper.Load(languageFile));
        }

        [Fact]
        public void Load_MissingFile_ThrowsFileNotFoundException()
        {
            var languageFile = Path.Combine(_testDirectory, "missing.json");

            Assert.Throws<FileNotFoundException>(() => LocalizationHelper.Load(languageFile));
        }

        [Fact]
        public void Update_WithLanguageSettings_LoadsLanguage()
        {
            var languageFile = Path.Combine(_testDirectory, "configured.json");
            File.WriteAllText(languageFile, "{\"MainForm_More\":\"Configured More\"}");
            File.WriteAllText(Path.GetFullPath(_settingsFile), JsonConvert.SerializeObject(new Settings { Language = languageFile }));

            LocalizationHelper.Update();

            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("Configured More", LocalizationHelper.Base.MainForm_More);
        }

        [Fact]
        public void Update_WithoutLanguageSettings_UsesDefaultBase()
        {
            File.WriteAllText(Path.GetFullPath(_settingsFile), JsonConvert.SerializeObject(new Settings()));

            LocalizationHelper.Update();

            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("More", LocalizationHelper.Base.MainForm_More);
        }

        [Fact]
        public void Export_CreatesJsonFileWithDefaultValues()
        {
            LocalizationHelper.Export();

            Assert.True(File.Exists("export.json"));
            var json = File.ReadAllText("export.json");
            Assert.Contains("MainForm_More", json);
            Assert.Contains("More", json);
        }
    }
}
