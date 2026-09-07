using System;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace uploader
{
    public static class SettingsManager
    {
        private static Settings _cachedSettings;
        private static readonly object _cacheLock = new object();

        public static string GetSettingsFilename()
        {
            var combined = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vtu_settings.json");
            return Utils.RequireRooted(combined);
        }

        public static void SaveSettings(Settings settings)
        {
            if (settings.Language?.Contains("Default") ?? false)
            {
                settings.Language = "";
            }

            var serialized = JsonConvert.SerializeObject(settings);
            var file = Utils.RequireRooted(GetSettingsFilename());
            if (!Path.IsPathRooted(file))
                throw new InvalidOperationException("Settings path must be rooted");

            if (File.Exists(file))
                File.Delete(file);

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(serialized);
                byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(file, encrypted);
            }
            catch (PlatformNotSupportedException)
            {
                File.WriteAllText(file, serialized);
            }

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

                var file = Utils.RequireRooted(GetSettingsFilename());
                if (!Path.IsPathRooted(file))
                    throw new InvalidOperationException("Settings path must be rooted");

                if (!File.Exists(file))
                {
                    _cachedSettings = new Settings();
                    return JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(_cachedSettings)) ?? new Settings();
                }

                string context;
                try
                {
                    byte[] encrypted = File.ReadAllBytes(file);
                    byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    context = Encoding.UTF8.GetString(decrypted);
                }
                catch (CryptographicException)
                {
                    // Fallback for previously unencrypted files
                    context = File.ReadAllText(file);
                    // Resave to encrypt it
                    var tempSettings = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();
                    SaveSettings(tempSettings);
                }
                catch (PlatformNotSupportedException)
                {
                    context = File.ReadAllText(file);
                }
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
