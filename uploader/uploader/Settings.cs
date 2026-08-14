using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uploader
{
    public class Settings
    {
        public string ApiKey = "";
        public string Language = "";
        public bool DirectUpload = false;

#pragma warning disable CS8632
        private static Settings? _cachedSettings = null;
#pragma warning restore CS8632
        private static readonly object _cacheLock = new object();

        public static string GetSettingsFilename()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vtu_settings.json");
        }

        public static void SaveSettings(Settings settings)
        {
            if (settings.Language != null && settings.Language.Contains("Default"))
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
                _cachedSettings = settings;
            }

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            lock (_cacheLock)
            {
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }
            }

            var file = GetSettingsFilename();

            if (!File.Exists(file))
            {
                var defaultSettings = new Settings();
                lock (_cacheLock)
                {
                    _cachedSettings = defaultSettings;
                }
                return defaultSettings;
            }

            var context = File.ReadAllText(file);
            var deserialized = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();

            lock (_cacheLock)
            {
                _cachedSettings = deserialized;
            }

            return deserialized;
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
