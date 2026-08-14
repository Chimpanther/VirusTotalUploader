using System;
using System.IO;
using Newtonsoft.Json;
using uploader;
using Xunit;

namespace uploader.Tests
{
    [Collection("SettingsCollection")]
    public class SettingsTests : IDisposable
    {
        private readonly string _settingsFile;
        private readonly string _backupSettingsContent;
        private readonly bool _backupSettingsExists;
        private readonly LocalizationBase _backupLocalizationBase;
        private readonly string _testLangFile;

        public SettingsTests()
        {
            _settingsFile = Settings.GetSettingsFilename();
            _backupSettingsExists = File.Exists(_settingsFile);
            if (_backupSettingsExists)
            {
                _backupSettingsContent = File.ReadAllText(_settingsFile);
            }

            _backupLocalizationBase = LocalizationHelper.Base;

            _testLangFile = Path.Combine(Directory.GetCurrentDirectory(), "test_lang.json");

            // Clean up potentially existing test files
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }
            if (File.Exists(_testLangFile))
            {
                File.Delete(_testLangFile);
            }
        }

        public void Dispose()
        {
            if (_backupSettingsExists)
            {
                File.WriteAllText(_settingsFile, _backupSettingsContent);
            }
            else if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            LocalizationHelper.Base = _backupLocalizationBase;

            if (File.Exists(_testLangFile))
            {
                File.Delete(_testLangFile);
            }
        }

        [Fact]
        public void SaveSettings_WithDefaultLanguage_ClearsLanguageAndSaves()
        {
            // Arrange
            var settings = new Settings
            {
                ApiKey = "test_key",
                DirectUpload = true,
                Language = "Default (English)"
            };

            // Act
            Settings.SaveSettings(settings);

            // Assert
            Assert.True(File.Exists(_settingsFile));
            var savedContent = File.ReadAllText(_settingsFile);
            var loadedSettings = JsonConvert.DeserializeObject<Settings>(savedContent);

            Assert.Equal("test_key", loadedSettings.ApiKey);
            Assert.True(loadedSettings.DirectUpload);
            Assert.Equal("", loadedSettings.Language);
            Assert.Equal("", settings.Language);
        }

        [Fact]
        public void SaveSettings_WithValidLanguage_UpdatesLocalization()
        {
            // Arrange
            var dummyLang = new LocalizationBase { MainForm_DragFile = "Test Drag File" };
            File.WriteAllText(_testLangFile, JsonConvert.SerializeObject(dummyLang));

            var settings = new Settings
            {
                ApiKey = "test_key",
                DirectUpload = true,
                Language = _testLangFile
            };

            // Act
            Settings.SaveSettings(settings);

            // Assert
            Assert.True(File.Exists(_settingsFile));
            var savedContent = File.ReadAllText(_settingsFile);
            var loadedSettings = JsonConvert.DeserializeObject<Settings>(savedContent);

            Assert.Equal(_testLangFile, loadedSettings.Language);

            // Check if LocalizationHelper.Base was updated
            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("Test Drag File", LocalizationHelper.Base.MainForm_DragFile);
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
