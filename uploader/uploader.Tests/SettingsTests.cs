using System;
using System.IO;
using System.Text;
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
            var file = Utils.RequireRooted(SettingsManager.GetSettingsFilename());
            if (!Path.IsPathRooted(file))
                throw new InvalidOperationException("Settings path must be rooted");
            _settingsFile = file;
            _settingsExisted = File.Exists(_settingsFile);
            _settingsBackup = _settingsExisted ? Convert.ToBase64String(File.ReadAllBytes(_settingsFile)) : string.Empty;
            _localizationBackup = LocalizationHelper.Base;
            _originalCurrentDirectory = Environment.CurrentDirectory;
            _testDirectory = Path.Combine(Path.GetTempPath(), "vtu-settings-" + Guid.NewGuid());
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(Path.Combine(_testDirectory, "local"));
            Environment.CurrentDirectory = _testDirectory;

            SettingsManager.ClearCache();
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = _originalCurrentDirectory;

            var file = Utils.RequireRooted(_settingsFile);
            if (!Path.IsPathRooted(file))
                throw new InvalidOperationException("Settings path must be rooted");

            if (_settingsExisted)
            {
                File.WriteAllBytes(file, Convert.FromBase64String(_settingsBackup));
            }
            else if (File.Exists(file))
            {
                File.Delete(file);
            }

            LocalizationHelper.Base = _localizationBackup;
            SettingsManager.ClearCache();
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

            Assert.Equal(Utils.RequireRooted(expectedPath), SettingsManager.GetSettingsFilename());
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

        [Fact]
        public void LoadSettings_InvalidJson_ThrowsException()
        {
            File.WriteAllText(_settingsFile, "{ invalid }");
            SettingsManager.ClearCache();

            var ex = Record.Exception(() => SettingsManager.LoadSettings());

            Assert.NotNull(ex);
            Assert.IsType<JsonReaderException>(ex);
        }

        [Fact]
        public void LoadSettings_LegacyPlaintext_StillLoads()
        {
            var expected = new Settings
            {
                ApiKey = "LegacyPlainKey",
                Language = "",
                DirectUpload = true
            };
            File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(expected));
            SettingsManager.ClearCache();

            var loaded = SettingsManager.LoadSettings();
            Assert.Equal("LegacyPlainKey", loaded.ApiKey);
            Assert.True(loaded.DirectUpload);
        }

        private Settings LoadExistingSettings()
        {
            var expected = new Settings
            {
                ApiKey = "TestApiKey123",
                Language = "English",
                DirectUpload = true
            };

            var file = Utils.RequireRooted(_settingsFile);
            if (!Path.IsPathRooted(file))
                throw new InvalidOperationException("Settings path must be rooted");
            File.WriteAllText(file, JsonConvert.SerializeObject(expected));
            SettingsManager.ClearCache();

            return SettingsManager.LoadSettings();
        }

        [Fact]
        public void SaveSettings_ValidSettings_WritesFileAndLoadsLocalization()
        {
            var languageFile = Path.Combine("local", "en.json");
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

            var file = Utils.RequireRooted(_settingsFile);
            if (!Path.IsPathRooted(file))
                throw new InvalidOperationException("Settings path must be rooted");
            Assert.True(File.Exists(file));

            SettingsManager.ClearCache();
            var persisted = SettingsManager.LoadSettings();
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
            SettingsManager.ClearCache();
            var persisted = SettingsManager.LoadSettings();
            Assert.NotNull(persisted);
            Assert.Equal("", persisted.Language);
            Assert.Equal("test-api-key", persisted.ApiKey);
            Assert.True(persisted.DirectUpload);
        }

        [Fact]
        public void ClearCache_NextLoadReadsFromFile()
        {
            var initialSettings = new Settings
            {
                ApiKey = "InitialKey",
                Language = "",
                DirectUpload = false
            };
            File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(initialSettings));
            SettingsManager.ClearCache();

            var firstLoad = SettingsManager.LoadSettings();
            Assert.Equal("InitialKey", firstLoad.ApiKey);

            var updatedSettings = new Settings
            {
                ApiKey = "UpdatedKey",
                Language = "",
                DirectUpload = true
            };
            File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(updatedSettings));

            SettingsManager.ClearCache();

            var secondLoad = SettingsManager.LoadSettings();
            Assert.Equal("UpdatedKey", secondLoad.ApiKey);
            Assert.True(secondLoad.DirectUpload);
        }

        [Fact]
        public void SaveAndLoad_RoundTripsApiKey()
        {
            var settings = new Settings { ApiKey = "SuperSecretKey123", DirectUpload = true };
            SettingsManager.SaveSettings(settings);

            // On Windows DPAPI encrypts the file so the raw secret should not appear as UTF-8 plaintext.
            if (OperatingSystem.IsWindows())
            {
                var raw = File.ReadAllBytes(_settingsFile);
                var asText = Encoding.UTF8.GetString(raw);
                Assert.DoesNotContain("SuperSecretKey123", asText);
            }

            SettingsManager.ClearCache();
            var loaded = SettingsManager.LoadSettings();
            Assert.Equal("SuperSecretKey123", loaded.ApiKey);
            Assert.True(loaded.DirectUpload);
        }

        [Fact]
        public void Clone_ReturnsIndependentCopy()
        {
            var original = new Settings { ApiKey = "a", Language = "b", DirectUpload = true };
            var clone = original.Clone();
            clone.ApiKey = "changed";
            Assert.Equal("a", original.ApiKey);
            Assert.Equal("changed", clone.ApiKey);
        }
    }
}
