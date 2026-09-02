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
            try
            {
                var fileName = Path.GetFileName(path);
                var fullLocalFolder = Path.GetFullPath(LocalFolder);
                var fullTempPath = Path.GetTempPath();

                bool isSimpleFilename = string.Equals(fileName, path, StringComparison.Ordinal);
                if (!isSimpleFilename)
                {
                    var fullPath = Path.GetFullPath(path);

                    bool isOutsideLocalFolder = !fullPath.StartsWith(fullLocalFolder);
                    bool isOutsideTempPath = !fullPath.StartsWith(fullTempPath);

                    if (isOutsideLocalFolder && isOutsideTempPath)
                    {
                        throw new ArgumentException("Invalid path");
                    }
                }

                var resolvedPath = Path.GetFullPath(Path.Combine(fullLocalFolder, fileName));
                var context = File.ReadAllText(resolvedPath);
                Base = JsonConvert.DeserializeObject<LocalizationBase>(context);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid localization path", ex);
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