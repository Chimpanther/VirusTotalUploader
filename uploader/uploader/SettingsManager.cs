using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

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

            WriteSettingsContent(file, serialized);

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
                    return _cachedSettings.Clone();
                }

                var file = Utils.RequireRooted(GetSettingsFilename());
                if (!Path.IsPathRooted(file))
                    throw new InvalidOperationException("Settings path must be rooted");

                if (!File.Exists(file))
                {
                    _cachedSettings = new Settings();
                    return _cachedSettings.Clone();
                }

                var context = ReadSettingsContent(file);
                _cachedSettings = JsonConvert.DeserializeObject<Settings>(context) ?? new Settings();
                return _cachedSettings.Clone();
            }
        }

        private static void WriteSettingsContent(string file, string serialized)
        {
            byte[] data = Encoding.UTF8.GetBytes(serialized);
            try
            {
                byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(file, encrypted);
            }
            catch (PlatformNotSupportedException)
            {
                // Non-Windows / unsupported runtimes keep plaintext so settings still work in CI tests.
                File.WriteAllText(file, serialized);
            }
        }

        private static string ReadSettingsContent(string file)
        {
            byte[] raw = File.ReadAllBytes(file);
            if (raw.Length == 0)
                return "{}";

            try
            {
                byte[] decrypted = ProtectedData.Unprotect(raw, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (CryptographicException)
            {
                // Legacy plaintext settings written before DPAPI encryption.
                return Encoding.UTF8.GetString(raw);
            }
            catch (PlatformNotSupportedException)
            {
                return Encoding.UTF8.GetString(raw);
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
