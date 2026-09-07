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
            _presenter.Load();

            _mockView.VerifySet(v => v.CurrentSettings = It.Is<Settings>(s => s.ApiKey == "test-api-key" && s.DirectUpload == true));
            _mockView.Verify(v => v.LoadLanguages(It.Is<string[]>(l => l.Length == 2 && l[0] == "English" && l[1] == "Spanish")), Times.Once);
            _mockView.Verify(v => v.SelectLanguageOrDefault("English"), Times.Once);
            _mockView.Verify(v => v.SetLocalization(It.IsAny<SettingsFormLocalization>()), Times.Once);
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

            _presenter.Load();

            _mockView.Verify(v => v.SelectLanguageOrDefault(""), Times.Once);
        }

        [Fact]
        public void SaveSettings_SavesTrimmedApiKeyAndShowsMessageAndRestarts()
        {
            var viewSettings = new Settings
            {
                ApiKey = "  new-api-key  ",
                Language = "Spanish",
                DirectUpload = false
            };
            _mockView.SetupGet(v => v.CurrentSettings).Returns(viewSettings);

            _presenter.SaveSettings();

            _mockSettingsManager.Verify(m => m.SaveSettings(It.Is<Settings>(s =>
                s.ApiKey == "new-api-key" &&
                s.Language == "Spanish" &&
                s.DirectUpload == false
            )), Times.Once);

            _mockView.Verify(v => v.ShowSuccessMessage(It.Is<MessageBoxOptions>(o => o.Message == "Settings saved successfully." && o.Caption == "Ok")), Times.Once);
            _mockView.Verify(v => v.RestartApplication(), Times.Once);
        }

        [Fact]
        public void GetApiKey_OpensUrl()
        {
            _presenter.GetApiKey();

            _mockView.Verify(v => v.OpenUrl(It.Is<MessageBoxOptions>(o => o.Message == "https://developers.virustotal.com/reference")), Times.Once);
        }
    }
}
