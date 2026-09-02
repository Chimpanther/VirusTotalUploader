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
            var fullPath = Utils.RequireRooted(path);
            if (!Path.IsPathRooted(fullPath))
                throw new ArgumentException("Path must be rooted", nameof(path));

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
