using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uploader
{
    public class Settings
    {
        public string ApiKey = "";
        public string Language = "";
        public bool DirectUpload = false;

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

            string originalKey = settings.ApiKey;

            if (!string.IsNullOrEmpty(settings.ApiKey) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(settings.ApiKey);
                    var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                    settings.ApiKey = Convert.ToBase64String(encrypted);
                }
                catch
                {
                    // Ignore encryption errors and fall back
                }
            }

            var serialized = JsonConvert.SerializeObject(settings);
            var file = GetSettingsFilename();

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllText(file, serialized);

            settings.ApiKey = originalKey; // Restore the unencrypted key in memory

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            var file = GetSettingsFilename();

            if (!File.Exists(file))
                return new Settings();

            var context = File.ReadAllText(file);
            var settings = JsonConvert.DeserializeObject<Settings>(context);

            if (settings != null && !string.IsNullOrEmpty(settings.ApiKey) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                try
                {
                    var encryptedBytes = Convert.FromBase64String(settings.ApiKey);
                    var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                    settings.ApiKey = Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    // Failed to decrypt, could be plaintext or invalid
                }
            }

            return settings ?? new Settings();
        }
    }
}
