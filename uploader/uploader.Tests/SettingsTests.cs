using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void GetSettingsFilename_ReturnsCorrectPath()
        {
            string expectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vtu_settings.json");
            string actualPath = Settings.GetSettingsFilename();
            Assert.Equal(expectedPath, actualPath);
        }
    }
}
