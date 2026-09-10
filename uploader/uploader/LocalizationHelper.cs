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

        /// <summary>
        /// Resolves <paramref name="path"/> and ensures the final path is strictly under
        /// the <c>local</c> directory (after GetFullPath normalization). Rejects
        /// prefix tricks like <c>local_evil</c> via a trailing-separator root check.
        /// </summary>
        public static void Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must not be empty", nameof(path));

            var fullPath = Path.GetFullPath(path);
            if (!IsPathStrictlyUnderLocalFolder(fullPath))
            {
                throw new UnauthorizedAccessException("Language file must be under the local folder.");
            }

            var context = File.ReadAllText(fullPath);
            Base = JsonConvert.DeserializeObject<LocalizationBase>(context);
        }

        internal static bool IsPathStrictlyUnderLocalFolder(string candidateFullPath)
        {
            if (string.IsNullOrWhiteSpace(candidateFullPath))
                return false;

            // Normalize both sides the same way (separators, .. segments, mixed slashes).
            var localRoot = Path.GetFullPath(LocalFolder);
            localRoot = localRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        + Path.DirectorySeparatorChar;

            var fullPath = Path.GetFullPath(candidateFullPath);

            // Must be a path strictly inside local/ (not the directory itself, not a sibling prefix).
            if (fullPath.Length <= localRoot.Length)
                return false;

            return fullPath.StartsWith(localRoot, StringComparison.OrdinalIgnoreCase);
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
