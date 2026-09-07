using System;
using System.Collections.Generic;
using Moq;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsPresenterTests
    {
        private readonly Mock<ISettingsView> _mockView;
        private readonly Mock<ISettingsManager> _mockSettingsManager;
        private readonly Mock<ILocalizationHelper> _mockLocalizationHelper;
        private readonly SettingsPresenter _presenter;

        public SettingsPresenterTests()
        {
            _mockView = new Mock<ISettingsView>();
            _mockSettingsManager = new Mock<ISettingsManager>();
            _mockLocalizationHelper = new Mock<ILocalizationHelper>();

            var localizationBase = new LocalizationBase
            {
                SettingsForm_General = "General settings",
                SettingsForm_Key = "API key:",
                SettingsForm_Get = "Get API key",
                SettingsForm_Language = "Language:",
                SettingsForm_Save = "Save",
                SettingsForm_Open = "Open settings file",
                SettingsForm_Title = "Settings",
                SettingsForm_DirectUpload = "Direct file upload",
                Message_NoSettings = "Settings file not found.",
                Message_Saved = "Settings saved successfully."
            };

            _mockLocalizationHelper.Setup(l => l.Base).Returns(localizationBase);
            _mockLocalizationHelper.Setup(l => l.GetLanguages()).Returns(new[] { "English", "Spanish" });

            _mockSettingsManager.Setup(m => m.LoadSettings()).Returns(new Settings
            {
                ApiKey = "test-api-key",
                DirectUpload = true,
                Language = "English"
            });
            _mockSettingsManager.Setup(m => m.GetSettingsFilename()).Returns("C:\\mock\\path\\settings.json");

            _presenter = new SettingsPresenter(_mockView.Object, _mockSettingsManager.Object, _mockLocalizationHelper.Object);
        }

        [Fact]
        public void Load_SetsViewPropertiesFromSettingsAndLocalization()
        {
            _mockView.Setup(v => v.GetLanguageIndexOf("English")).Returns(0);

            _presenter.Load();

            _mockView.VerifySet(v => v.ApiKey = "test-api-key");
            _mockView.VerifySet(v => v.DirectUpload = true);
            _mockView.Verify(v => v.ClearLanguages(), Times.Once);
            _mockView.Verify(v => v.AddLanguage("English"), Times.Once);
            _mockView.Verify(v => v.AddLanguage("Spanish"), Times.Once);
            _mockView.Verify(v => v.SetLanguageSelectedIndex(0), Times.Once);
            _mockView.Verify(v => v.SetLocalization(It.IsAny<LocalizationBase>()), Times.Once);
        }

        [Fact]
        public void Load_EmptyLanguage_SetsDefaultLanguage()
        {
            _mockSettingsManager.Setup(m => m.LoadSettings()).Returns(new Settings
            {
                ApiKey = "test-api-key",
                DirectUpload = true,
                Language = ""
            });
            _mockView.Setup(v => v.AddLanguageAndGetIndex("Default (Build-in English)")).Returns(2);

            _presenter.Load();

            _mockView.Verify(v => v.AddLanguageAndGetIndex("Default (Build-in English)"), Times.Once);
            _mockView.Verify(v => v.SetLanguageSelectedIndex(2), Times.Once);
        }

        [Fact]
        public void Load_LanguageNotFound_DoesNotSetSelectedIndex()
        {
            _mockSettingsManager.Setup(m => m.LoadSettings()).Returns(new Settings
            {
                ApiKey = "test-api-key",
                DirectUpload = true,
                Language = "French"
            });
            _mockView.Setup(v => v.GetLanguageIndexOf("French")).Returns(-1);

            _presenter.Load();

            _mockView.Verify(v => v.SetLanguageSelectedIndex(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void SaveSettings_SavesTrimmedApiKeyAndShowsMessageAndRestarts()
        {
            _mockView.SetupGet(v => v.ApiKey).Returns("  new-api-key  ");
            _mockView.SetupGet(v => v.Language).Returns("Spanish");
            _mockView.SetupGet(v => v.DirectUpload).Returns(false);

            _presenter.SaveSettings();

            _mockSettingsManager.Verify(m => m.SaveSettings(It.Is<Settings>(s =>
                s.ApiKey == "new-api-key" &&
                s.Language == "Spanish" &&
                s.DirectUpload == false
            )), Times.Once);

            _mockView.Verify(v => v.ShowMessageBox("Settings saved successfully.", "Ok"), Times.Once);
            _mockView.Verify(v => v.RestartApplication(), Times.Once);
        }

        [Fact]
        public void GetApiKey_OpensUrl()
        {
            _presenter.GetApiKey();

            _mockView.Verify(v => v.OpenUrl("https://developers.virustotal.com/reference"), Times.Once);
        }
    }
}
