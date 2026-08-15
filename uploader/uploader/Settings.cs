using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uploader
{

    public class Settings
    {
        [JsonIgnore]
        public string ApiKey = "";

        [JsonProperty("ApiKey")]
        public string EncryptedApiKey
        {
            get => Protect(ApiKey);
            set => ApiKey = Unprotect(value);
        }

        public string Language = "";
        public bool DirectUpload = false;

        private static string Protect(string clearText)
        {
            if (string.IsNullOrEmpty(clearText))
                return clearText;
            try
            {
                byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
                byte[] encryptedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback for non-Windows platforms in tests
                return clearText;
            }
            catch (CryptographicException)
            {
                return clearText;
            }
        }

        private static string Unprotect(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(clearBytes);
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback for non-Windows platforms in tests
                return encryptedText;
            }
            catch (CryptographicException)
            {
                // Fallback for old plaintext settings
                return encryptedText;
            }
            catch (FormatException)
            {
                // Fallback if not valid base64 (old plaintext settings)
                return encryptedText;
            }
        }


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

            LocalizationHelper.Update();
        }

        public static Settings LoadSettings()
        {
            var file = GetSettingsFilename();

            if (!File.Exists(file))
                return new Settings();

            var context = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<Settings>(context);
        }
    }
}
