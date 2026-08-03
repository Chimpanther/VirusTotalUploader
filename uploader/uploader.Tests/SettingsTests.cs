using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uploader;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [TestClass]
    public class SettingsTests
    {
        private string? originalAppData;
        private string? originalXdgConfigHome;
        private string testAppDataPath = "";
        private string testSettingsFile = "";

        [TestInitialize]
        public void Setup()
        {
            testAppDataPath = Path.Combine(Path.GetTempPath(), "uploader_test_appdata_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(testAppDataPath);

            // Set Environment to our test folder temporarily
            originalAppData = Environment.GetEnvironmentVariable("APPDATA");
            Environment.SetEnvironmentVariable("APPDATA", testAppDataPath);

            originalXdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testAppDataPath);

            testSettingsFile = Path.Combine(testAppDataPath, "vtu_settings.json");

            // Note: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            // On Linux uses XDG_CONFIG_HOME, on Windows uses APPDATA or USERPROFILE\AppData\Roaming.
            // Since on Windows APPDATA might not change GetFolderPath if it's cached,
            // we may need to use reflection if we want to change it. But setting XDG_CONFIG_HOME usually works for .NET Core on Linux.
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Restore original environment
            Environment.SetEnvironmentVariable("APPDATA", originalAppData);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", originalXdgConfigHome);

            if (Directory.Exists(testAppDataPath))
            {
                Directory.Delete(testAppDataPath, true);
            }
        }

        [TestMethod]
        public void LoadSettings_WhenFileDoesNotExist_ReturnsDefaultSettings()
        {
            var file = Settings.GetSettingsFilename();
            if (File.Exists(file))
                File.Delete(file);

            var settings = Settings.LoadSettings();

            Assert.IsNotNull(settings);
            Assert.AreEqual("", settings.ApiKey);
            Assert.AreEqual("", settings.Language);
            Assert.IsFalse(settings.DirectUpload);
        }

        [TestMethod]
        public void LoadSettings_WhenFileExists_LoadsSettingsCorrectly()
        {
            var testSettings = new Settings
            {
                ApiKey = "test_api_key_12345",
                Language = "English",
                DirectUpload = true
            };

            var file = Settings.GetSettingsFilename();
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, JsonConvert.SerializeObject(testSettings));

            var loadedSettings = Settings.LoadSettings();

            Assert.IsNotNull(loadedSettings);
            Assert.AreEqual("test_api_key_12345", loadedSettings.ApiKey);
            Assert.AreEqual("English", loadedSettings.Language);
            Assert.IsTrue(loadedSettings.DirectUpload);
        }
    }
}
