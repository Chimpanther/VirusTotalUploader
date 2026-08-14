using NUnit.Framework;
using System;
using System.IO;
using uploader;

namespace uploader.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void GetSettingsFilename_ReturnsCorrectPath()
        {
            // Arrange
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string expectedPath = Path.Combine(appDataPath, "vtu_settings.json");

            // Act
            string actualPath = Settings.GetSettingsFilename();

            // Assert
            Assert.That(actualPath, Is.EqualTo(expectedPath), "The generated settings filename should be in the ApplicationData folder with the name 'vtu_settings.json'.");
        }

        [Test]
        public void GetSettingsFilename_IsNotNullOrEmpty()
        {
            // Act
            string actualPath = Settings.GetSettingsFilename();

            // Assert
            Assert.That(actualPath, Is.Not.Null.And.Not.Empty);
        }
    }
}
