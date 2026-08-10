using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace uploader
{
    public class Settings
    {
        [JsonProperty("ApiKey")]
        public string ObsoletePlaintextApiKey = "";

        [JsonIgnore]
        public string ApiKey = "";

        public string EncryptedApiKey = "";

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
            
            // Do not save the plaintext key
            settings.ObsoletePlaintextApiKey = "";

            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                try
                {
                    var plainBytes = Encoding.UTF8.GetBytes(settings.ApiKey);
                    var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                    settings.EncryptedApiKey = Convert.ToBase64String(encryptedBytes);
                }
                catch
                {
                    // Fallback or ignore if not on Windows / DPAPI fails
                }
            }
            else
            {
                settings.EncryptedApiKey = "";
            }

            var serialized = JsonConvert.SerializeObject(settings);
            var file = GetSettingsFilename();

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllText(file, serialized);

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            var file = GetSettingsFilename();

            if (!File.Exists(file))
                return new Settings();

            var context = File.ReadAllText(file);
            var settings = JsonConvert.DeserializeObject<Settings>(context);
            if (settings == null)
            {
                return new Settings();
            }

            if (!string.IsNullOrEmpty(settings.EncryptedApiKey))
            {
                try
                {
                    var encryptedBytes = Convert.FromBase64String(settings.EncryptedApiKey);
                    var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                    settings.ApiKey = Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    // Decryption failed, possibly different user/machine
                    settings.ApiKey = "";
                }
            }
            else if (!string.IsNullOrEmpty(settings.ObsoletePlaintextApiKey))
            {
                // Migrate from old plaintext
                settings.ApiKey = settings.ObsoletePlaintextApiKey;
            }

            return settings;
        }
    }
}