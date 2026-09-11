using System;
using System.IO;
using Xunit;
using uploader;
using System.Linq;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class SettingsFormTests : IDisposable
    {
        private readonly string _settingsFile;
        private readonly string _settingsBackup;
        private readonly bool _settingsExisted;
        private readonly LocalizationBase _localizationBackup;
        private readonly string _originalCurrentDirectory;
        private readonly string _testDirectory;

        public SettingsFormTests()
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var safeFile = Path.GetFileName(Settings.GetSettingsFilename());
            _settingsFile = Path.GetFullPath(Path.Combine(baseDir, safeFile));

            if (!_settingsFile.StartsWith(Path.GetFullPath(baseDir))) throw new Exception("Invalid Path");

            _settingsExisted = File.Exists(_settingsFile);
            _settingsBackup = _settingsExisted ? File.ReadAllText(_settingsFile) : string.Empty;
            _localizationBackup = LocalizationHelper.Base;
            _originalCurrentDirectory = Environment.CurrentDirectory;

            var tempBase = Path.GetFullPath(Path.GetTempPath());
            var dirName = "vtu-settingsform-" + Guid.NewGuid();
            _testDirectory = Path.GetFullPath(Path.Combine(tempBase, dirName));
            if (!_testDirectory.StartsWith(tempBase)) throw new Exception("Invalid Path");
            Directory.CreateDirectory(_testDirectory);
            Environment.CurrentDirectory = _testDirectory;

            Settings.ClearCache();
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            // LocalizationHelper checks the "local" folder
            var localDir = Path.GetFullPath(Path.Combine(_testDirectory, "local"));
            if (!localDir.StartsWith(_testDirectory)) throw new Exception("Invalid Path");
            Directory.CreateDirectory(localDir);

            var f1 = Path.GetFullPath(Path.Combine(localDir, "TestLang.json"));
            if (!f1.StartsWith(localDir)) throw new Exception("Invalid Path");
            File.WriteAllText(f1, "{}");

            var f2 = Path.GetFullPath(Path.Combine(localDir, "TestLang2.json"));
            if (!f2.StartsWith(localDir)) throw new Exception("Invalid Path");
            File.WriteAllText(f2, "{}");
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = _originalCurrentDirectory;
            if (Directory.Exists(_testDirectory))
            {
                try { Directory.Delete(_testDirectory, true); } catch { }
            }

            if (_settingsExisted)
            {
                File.WriteAllText(_settingsFile, _settingsBackup);
            }
            else if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            Settings.ClearCache();
            LocalizationHelper.Base = _localizationBackup;
        }

        [Fact]
        public void GetLanguageOptions_IncludesDefaultAndAvailableLanguages()
        {
            var options = SettingsFormHelpers.GetLanguageOptions();

            Assert.Contains("Default (Build-in English)", options);
            Assert.Equal("Default (Build-in English)", options[0]);

            var availableLanguages = LocalizationHelper.GetLanguages();
            foreach (var lang in availableLanguages)
            {
                Assert.Contains(lang, options);
            }
        }

        [Fact]
        public void GetSelectedLanguageIndex_ReturnsZero_WhenLanguageIsEmpty()
        {
            var settings = new Settings { Language = string.Empty };
            var options = SettingsFormHelpers.GetLanguageOptions();

            var index = SettingsFormHelpers.GetSelectedLanguageIndex(settings, options);

            Assert.Equal(0, index);
        }

        [Fact]
        public void GetSelectedLanguageIndex_ReturnsCorrectIndex_WhenLanguageIsSet()
        {
            var localDir = Path.Combine(_testDirectory, "local");
            var options = SettingsFormHelpers.GetLanguageOptions();

            // Just use the one it actually picked up to avoid slash inconsistencies
            var validLang = options.FirstOrDefault(o => o != "Default (Build-in English)");
            Assert.NotNull(validLang);

            var settings = new Settings { Language = validLang };

            var index = SettingsFormHelpers.GetSelectedLanguageIndex(settings, options);

            Assert.True(index > 0);
            Assert.Equal(validLang, options[index]);
        }

        [Fact]
        public void GetExplorerArgsForSettings_ReturnsNull_WhenFileDoesNotExist()
        {
            if (File.Exists(_settingsFile))
                File.Delete(_settingsFile);

            var args = SettingsFormHelpers.GetExplorerArgsForSettings();

            Assert.Equal(string.Empty, args);
        }

        [Fact]
        public void GetExplorerArgsForSettings_ReturnsArgs_WhenFileExists()
        {
            Settings.SaveSettings(new Settings());

            var args = SettingsFormHelpers.GetExplorerArgsForSettings();

            Assert.NotNull(args);
            Assert.Contains("/e, /select, ", args);
            Assert.Contains(_settingsFile.Replace("\"", "\\\""), args);
        }

        [Fact]
        public void SaveSettings_SavesProvidedValues()
        {
            var localDir = Path.Combine(_testDirectory, "local");
            var langPath = Path.Combine(localDir, "TestLang.json");

            // Settings form helper trims api key
            SettingsFormHelpers.SaveSettings("  test_api_key  ", langPath, true);

            var savedSettings = Settings.LoadSettings();

            Assert.Equal("test_api_key", savedSettings.ApiKey);
            Assert.Equal(langPath, savedSettings.Language);
            Assert.True(savedSettings.DirectUpload);
        }
    }
}
