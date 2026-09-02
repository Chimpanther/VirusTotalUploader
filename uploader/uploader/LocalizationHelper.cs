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
            if (path.Contains("..") || path.Contains("/") || path.Contains("\\"))
            {
                var fullPath = Path.GetFullPath(path);
                var fullLocalFolder = Path.GetFullPath(LocalFolder);
                var fullTempPath = Path.GetTempPath();

                bool isOutsideLocalFolder = !fullPath.StartsWith(fullLocalFolder);
                bool isOutsideTempPath = !fullPath.StartsWith(fullTempPath);
                bool isInvalidPath = isOutsideLocalFolder && isOutsideTempPath;

                if (isInvalidPath)
                {
                    throw new ArgumentException("Invalid path");
                }

                var context = File.ReadAllText(fullPath);
                Base = JsonConvert.DeserializeObject<LocalizationBase>(context);
            }
            else
            {
                var safePath = Path.Combine(LocalFolder, path);
                var context = File.ReadAllText(safePath);
                Base = JsonConvert.DeserializeObject<LocalizationBase>(context);
            }
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