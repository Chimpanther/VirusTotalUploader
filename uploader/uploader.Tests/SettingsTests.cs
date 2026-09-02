using System;
using System.IO;
using Newtonsoft.Json;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsFile;
        private readonly string _settingsBackup;
        private readonly bool _settingsExisted;
        private readonly LocalizationBase _localizationBackup;
        private readonly string _originalCurrentDirectory;
        private readonly string _testDirectory;

        public SettingsTests()
        {
            _settingsFile = Path.GetFullPath(Path.GetFullPath(SettingsManager.GetSettingsFilename()));
            _settingsExisted = File.Exists(Path.GetFullPath(_settingsFile));
            _settingsBackup = _settingsExisted ? File.ReadAllText(Path.GetFullPath(_settingsFile)) : string.Empty;
            _localizationBackup = LocalizationHelper.Base;
            _originalCurrentDirectory = Environment.CurrentDirectory;
            _testDirectory = Path.Combine(Path.GetTempPath(), "vtu-settings-" + Guid.NewGuid());
            Directory.CreateDirectory(_testDirectory);
            Environment.CurrentDirectory = _testDirectory;

            SettingsManager.ClearCache();
            if (File.Exists(Path.GetFullPath(_settingsFile)))
            {
                File.Delete(Path.GetFullPath(_settingsFile));
            }
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

            LocalizationHelper.Base = _localizationBackup;
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void GetSettingsFilename_ReturnsExpectedApplicationDataPath()
        {
            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "vtu_settings.json");

            Assert.Equal(expectedPath, Path.GetFullPath(SettingsManager.GetSettingsFilename()));
        }

        [Fact]
        public void LoadSettings_MissingFile_ReturnsDefault()
        {
            var settings = SettingsManager.LoadSettings();

            Assert.NotNull(settings);
            Assert.Equal("", settings.ApiKey);
            Assert.Equal("", settings.Language);
            Assert.False(settings.DirectUpload);
        }

        [Fact]
        public void LoadSettings_ExistingFile_RestoresApiKey()
        {
            var settings = LoadExistingSettings();
            Assert.Equal("TestApiKey123", settings.ApiKey);
        }

        [Fact]
        public void LoadSettings_ExistingFile_RestoresLanguage()
        {
            var settings = LoadExistingSettings();
            Assert.Equal("English", settings.Language);
        }

        [Fact]
        public void LoadSettings_ExistingFile_RestoresDirectUpload()
        {
            var settings = LoadExistingSettings();
            Assert.True(settings.DirectUpload);
        }

        private Settings LoadExistingSettings()
        {
            var expected = new Settings
            {
                ApiKey = "TestApiKey123",
                Language = "English",
                DirectUpload = true
            };

            File.WriteAllText(Path.GetFullPath(_settingsFile), JsonConvert.SerializeObject(expected));

            return SettingsManager.LoadSettings();
        }

        [Fact]
        public void SaveSettings_ValidSettings_WritesFileAndLoadsLocalization()
        {
            var languageFile = Path.Combine(_testDirectory, "en.json");
            File.WriteAllText(languageFile, JsonConvert.SerializeObject(new LocalizationBase
            {
                MainForm_More = "Test More"
            }));
            var settings = new Settings
            {
                ApiKey = "test-api-key",
                Language = languageFile,
                DirectUpload = true
            };

            SettingsManager.SaveSettings(settings);

            Assert.True(File.Exists(Path.GetFullPath(_settingsFile)));
            var persisted = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.GetFullPath(_settingsFile)));
            Assert.NotNull(persisted);
            Assert.Equal("test-api-key", persisted.ApiKey);
            Assert.Equal(languageFile, persisted.Language);
            Assert.True(persisted.DirectUpload);
            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("Test More", LocalizationHelper.Base.MainForm_More);
        }

        [Fact]
        public void SaveSettings_DefaultLanguageClearsLanguageBeforePersisting()
        {
            var settings = new Settings
            {
                ApiKey = "test-api-key",
                Language = "Default (English)",
                DirectUpload = true
            };

            SettingsManager.SaveSettings(settings);

            Assert.Equal("", settings.Language);
            var persisted = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.GetFullPath(_settingsFile)));
            Assert.NotNull(persisted);
            Assert.Equal("", persisted.Language);
            Assert.Equal("test-api-key", persisted.ApiKey);
            Assert.True(persisted.DirectUpload);
        }
    }
}
