using System;
using System.Diagnostics;
using System.IO;

namespace uploader
{
    public interface ISettingsView
    {
        string ApiKey { get; set; }
        bool DirectUpload { get; set; }
        string Language { get; set; }
        void ClearLanguages();
        void AddLanguage(string language);
        int AddLanguageAndGetIndex(string language);
        void SetLanguageSelectedIndex(int index);
        int GetLanguageIndexOf(string language);
        void SetLocalization(LocalizationBase loc);
        void ShowStatusMessage(string message);
        void ShowMessageBox(string message, string caption);
        void RevealInExplorer(string path);
        void OpenUrl(string url);
        void RestartApplication();
    }

    public class SettingsPresenter
    {
        private readonly ISettingsView _view;
        private readonly ISettingsManager _settingsManager;
        private readonly ILocalizationHelper _localizationHelper;

        public SettingsPresenter(ISettingsView view, ISettingsManager? settingsManager = null, ILocalizationHelper? localizationHelper = null)
        {
            _view = view;
            _settingsManager = settingsManager ?? new DefaultSettingsManager();
            _localizationHelper = localizationHelper ?? new DefaultLocalizationHelper();
        }

        public void Load()
        {
            var settings = _settingsManager.LoadSettings();

            _view.ApiKey = settings.ApiKey;
            _view.DirectUpload = settings.DirectUpload;

            var languages = _localizationHelper.GetLanguages();
            _view.ClearLanguages();
            foreach (var language in languages)
            {
                _view.AddLanguage(language);
            }

            if (string.IsNullOrEmpty(settings.Language))
            {
                var defaultLanguage = _view.AddLanguageAndGetIndex("Default (Build-in English)");
                _view.SetLanguageSelectedIndex(defaultLanguage);
            }
            else
            {
                var index = _view.GetLanguageIndexOf(settings.Language);
                if (index != -1)
                {
                    _view.SetLanguageSelectedIndex(index);
                }
            }

            _view.SetLocalization(_localizationHelper.Base);
        }

        public void OpenSettingsFile()
        {
            var file = Utils.RequireRooted(_settingsManager.GetSettingsFilename());
            if (!Path.IsPathRooted(file))
                return;

            if (!File.Exists(file))
            {
                _view.ShowStatusMessage(_localizationHelper.Base.Message_NoSettings);
                return;
            }

            _view.RevealInExplorer(file);
        }

        public void SaveSettings()
        {
            var apiKey = _view.ApiKey?.Trim() ?? string.Empty;

            var settings = new Settings
            {
                ApiKey = apiKey,
                Language = _view.Language,
                DirectUpload = _view.DirectUpload
            };

            _settingsManager.SaveSettings(settings);

            _view.ShowMessageBox(_localizationHelper.Base.Message_Saved, "Ok");
            _view.RestartApplication();
        }

        public void GetApiKey()
        {
            try
            {
                _view.OpenUrl("https://developers.virustotal.com/reference");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open URL: {ex.Message}");
                _view.ShowStatusMessage("Failed to open URL. Please check your browser settings.");
            }
        }
    }

    public interface ISettingsManager
    {
        Settings LoadSettings();
        void SaveSettings(Settings settings);
        string GetSettingsFilename();
    }

    public class DefaultSettingsManager : ISettingsManager
    {
        public Settings LoadSettings() => SettingsManager.LoadSettings();
        public void SaveSettings(Settings settings) => SettingsManager.SaveSettings(settings);
        public string GetSettingsFilename() => SettingsManager.GetSettingsFilename();
    }

    public interface ILocalizationHelper
    {
        string[] GetLanguages();
        LocalizationBase Base { get; }
    }

    public class DefaultLocalizationHelper : ILocalizationHelper
    {
        public string[] GetLanguages() => LocalizationHelper.GetLanguages();
        public LocalizationBase Base => LocalizationHelper.Base;
    }
}
