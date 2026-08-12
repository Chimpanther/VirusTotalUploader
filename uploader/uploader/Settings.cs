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

        private static Settings _cachedSettings;
        private static readonly object _lock = new object();

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

            lock (_lock)
            {
                if (File.Exists(file))
                    File.Delete(file);

                File.WriteAllText(file, serialized);

                _cachedSettings = settings;
            }

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            lock (_lock)
            {
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }

                var file = GetSettingsFilename();

                if (!File.Exists(file))
                {
                    _cachedSettings = new Settings();
                    return _cachedSettings;
                }

                var context = File.ReadAllText(file);
                _cachedSettings = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();
                return _cachedSettings;
            }
        }

        internal static void ClearCache()
        {
            lock (_lock)
            {
                _cachedSettings = null;
            }
        }
    }
}
