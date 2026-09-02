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
            var fileName = Path.GetFileName(path);
            var safePath = Path.Combine(LocalFolder, fileName);

            bool isSimpleFilename = fileName == path;
            if (!isSimpleFilename)
            {
                var fullPath = Path.GetFullPath(path);
                var fullLocalFolder = Path.GetFullPath(LocalFolder);
                var fullTempPath = Path.GetTempPath();

                bool isOutsideLocalFolder = !fullPath.StartsWith(fullLocalFolder);
                bool isOutsideTempPath = !fullPath.StartsWith(fullTempPath);

                if (isOutsideLocalFolder && isOutsideTempPath)
                {
                    throw new ArgumentException("Invalid path");
                }

                safePath = fullPath;
            }

            var context = File.ReadAllText(safePath);
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