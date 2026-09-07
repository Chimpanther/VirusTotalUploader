using System;
using System.Diagnostics;
using System.IO;

namespace uploader
{
    public class SettingsFormLocalization
    {
        public string General { get; set; }
        public string Key { get; set; }
        public string Get { get; set; }
        public string Language { get; set; }
        public string Save { get; set; }
        public string Open { get; set; }
        public string Title { get; set; }
        public string DirectUpload { get; set; }

        public static SettingsFormLocalization FromBase(LocalizationBase loc)
        {
            return new SettingsFormLocalization
            {
                General = loc.SettingsForm_General,
                Key = loc.SettingsForm_Key,
                Get = loc.SettingsForm_Get,
                Language = loc.SettingsForm_Language,
                Save = loc.SettingsForm_Save,
                Open = loc.SettingsForm_Open,
                Title = loc.SettingsForm_Title,
                DirectUpload = loc.SettingsForm_DirectUpload
            };
        }
    }

    public interface ISettingsView
    {
        Settings CurrentSettings { get; set; }
        void LoadLanguages(string[] languages);
        void SelectLanguageOrDefault(string language);
        void SetLocalization(SettingsFormLocalization loc);
        void ShowStatusMessage(string message);
        void ShowSuccessMessage(string message);
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

            _view.CurrentSettings = settings;

            var languages = _localizationHelper.GetLanguages();
            _view.LoadLanguages(languages);
            _view.SelectLanguageOrDefault(settings.Language);

            var formLoc = SettingsFormLocalization.FromBase(_localizationHelper.Base);
            _view.SetLocalization(formLoc);
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
            var settings = _view.CurrentSettings;
            settings.ApiKey = settings.ApiKey?.Trim() ?? string.Empty;

            _settingsManager.SaveSettings(settings);

            _view.ShowSuccessMessage(_localizationHelper.Base.Message_Saved);
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
