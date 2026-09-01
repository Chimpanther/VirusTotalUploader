using System;
using System.IO;
using Newtonsoft.Json;

namespace uploader
{
    public static class SettingsManager
    {
        private static Settings _cachedSettings;
        private static readonly object _cacheLock = new object();

        public static string GetSettingsFilename()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vtu_settings.json");
        }

        public static void SaveSettings(Settings settings)
        {
            if (settings.Language.Contains("Default"))
            {
                settings.Language = "";
            }

            var serialized = JsonConvert.SerializeObject(settings);
            var file = GetSettingsFilename();

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllText(file, serialized);

            lock (_cacheLock)
            {
                _cachedSettings = JsonConvert.DeserializeObject<Settings>(serialized) ?? new Settings();
            }

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            lock (_cacheLock)
            {
                if (_cachedSettings != null)
                {
                    return JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(_cachedSettings)) ?? new Settings();
                }

                var file = GetSettingsFilename();

                if (!File.Exists(file))
                {
                    _cachedSettings = new Settings();
                    return JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(_cachedSettings)) ?? new Settings();
                }

                var context = File.ReadAllText(file);
                _cachedSettings = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();
                return JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(_cachedSettings)) ?? new Settings();
            }
        }

        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                _cachedSettings = null;
            }
        }
    }
}
