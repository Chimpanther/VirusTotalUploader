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

                var file = Utils.RequireRooted(GetSettingsFilename());
                if (!Path.IsPathRooted(file))
                    throw new InvalidOperationException("Settings path must be rooted");

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

        private static void WriteSettingsContent(string file, string serialized)
        {
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
        }

        private static string ReadSettingsContent(string file)
        {
            try
            {
                byte[] encrypted = File.ReadAllBytes(file);
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (CryptographicException)
            {
                // Fallback for previously unencrypted files
                string context = File.ReadAllText(file);
                // Resave to encrypt it
                var tempSettings = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();
                SaveSettings(tempSettings);
                return context;
            }
            catch (PlatformNotSupportedException)
            {
                return File.ReadAllText(file);
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
