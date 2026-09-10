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

        // App-specific DPAPI entropy (defense in depth; not a secret by itself).
        private static readonly byte[] DpapiEntropy = Encoding.UTF8.GetBytes("VirusTotalUploader.Settings.v1");

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
                byte[] encrypted = ProtectedData.Protect(data, DpapiEntropy, DataProtectionScope.CurrentUser);
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

            // Prefer DPAPI with current entropy; also try null entropy for older encrypted files.
            try
            {
                byte[] decrypted = ProtectedData.Unprotect(raw, DpapiEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (CryptographicException)
            {
                try
                {
                    byte[] decryptedLegacy = ProtectedData.Unprotect(raw, null, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(decryptedLegacy);
                }
                catch (CryptographicException)
                {
                    // Only treat as legacy plaintext when the file looks like JSON.
                    if (LooksLikeJsonObject(raw))
                        return Encoding.UTF8.GetString(raw);
                    throw;
                }
            }
            catch (PlatformNotSupportedException)
            {
                return Encoding.UTF8.GetString(raw);
            }
        }

        private static bool LooksLikeJsonObject(byte[] raw)
        {
            // Trim leading whitespace, then require a JSON object opener.
            var text = Encoding.UTF8.GetString(raw).TrimStart();
            return text.StartsWith("{", StringComparison.Ordinal);
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
