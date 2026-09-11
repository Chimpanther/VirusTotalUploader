using System;
using System.IO;

namespace uploader
{
    public static class SettingsFormHelpers
    {
        public static string[] GetLanguageOptions()
        {
            var languages = LocalizationHelper.GetLanguages();
            var options = new string[languages.Length + 1];
            options[0] = "Default (Build-in English)";
            Array.Copy(languages, 0, options, 1, languages.Length);
            return options;
        }

        public static int GetSelectedLanguageIndex(Settings settings, string[] options)
        {
            if (string.IsNullOrEmpty(settings.Language))
            {
                return 0; // Default (Build-in English) is at index 0
            }

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == settings.Language)
                {
                    return i;
                }
            }

            return 0;
        }

        public static string GetExplorerArgsForSettings()
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var safeFile = Path.GetFileName(Settings.GetSettingsFilename());
            var file = Path.Combine(baseDir, safeFile);

            if (!File.Exists(file))
            {
                return string.Empty;
            }

            var safePath = file.Replace("\"", "\\\"");
            return $"/e, /select, \"{safePath}\"";
        }

        public static void SaveSettings(string apiKey, string language, bool directUpload)
        {
            var settings = new Settings
            {
                ApiKey = apiKey.Trim(),
                Language = language,
                DirectUpload = directUpload
            };
            Settings.SaveSettings(settings);
        }
    }
}
