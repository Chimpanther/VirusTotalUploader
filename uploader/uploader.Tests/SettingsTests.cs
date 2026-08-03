using Microsoft.VisualStudio.TestTools.UnitTesting;
using uploader;
using System.IO;
using System;

namespace uploader.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void LoadSettings_MissingFile_ReturnsDefault()
        {
            var settingsFile = Settings.GetSettingsFilename();

            // Backup existing if any
            string? backup = null;
            if (File.Exists(settingsFile))
            {
                backup = File.ReadAllText(settingsFile);
                File.Delete(settingsFile);
            }

            try
            {
                // Ensure it does not exist
                Assert.IsFalse(File.Exists(settingsFile));

                // Act
                var settings = Settings.LoadSettings();

                // Assert default properties
                Assert.IsNotNull(settings);
                Assert.AreEqual("", settings.ApiKey);
                Assert.AreEqual("", settings.Language);
                Assert.IsFalse(settings.DirectUpload);
            }
            finally
            {
                // Restore backup
                if (backup != null)
                {
                    File.WriteAllText(settingsFile, backup);
                }
            }
        }
    }
}
