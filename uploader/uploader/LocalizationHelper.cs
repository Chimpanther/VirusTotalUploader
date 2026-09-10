using System;
using System.IO;
using Newtonsoft.Json;

namespace uploader
{
    internal class LocalizationHelper
    {
        private const string LocalFolder = "local";
        public static LocalizationBase Base;

        public static string[] GetLanguages()
        {
            return Directory.Exists(LocalFolder) ? Directory.GetFiles(LocalFolder) : new []{ "" };
        }

        public static void Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must not be empty", nameof(path));

            var localRoot = Path.GetFullPath(LocalFolder);
            if (!localRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !localRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                localRoot += Path.DirectorySeparatorChar;
            }

            var fullPath = Path.GetFullPath(path);
            if (!fullPath.StartsWith(localRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Language file must be under the local folder.");
            }

            var context = File.ReadAllText(fullPath);
            Base = JsonConvert.DeserializeObject<LocalizationBase>(context);
        }

        public static void Update()
        {
            var settings = SettingsManager.LoadSettings();
            if (!string.IsNullOrEmpty(settings.Language))
            {
                Load(settings.Language);
            }
            else
            {
                Base = new LocalizationBase();
            }
        }

        // Used to create Json for new version
        public static void Export()
        {
            Base = new LocalizationBase();
            var serialized = JsonConvert.SerializeObject(LocalizationHelper.Base);
            File.WriteAllText("export.json", serialized);
        }
    }
}
