using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests : IDisposable
    {
        private const string LocalFolder = "local";

        public LocalizationHelperTests()
        {
            // Clean up before tests
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        public void Dispose()
        {
            // Clean up after tests
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        [Fact]
        public void GetLanguages_DirectoryDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Single(languages);
            Assert.Equal("", languages[0]);
        }

        [Fact]
        public void GetLanguages_DirectoryExistsButEmpty_ReturnsEmptyArray()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);

            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Empty(languages);
        }

        [Fact]
        public void GetLanguages_DirectoryHasFiles_ReturnsFilePaths()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);
            string file1 = Path.Combine(LocalFolder, "en.json");
            string file2 = Path.Combine(LocalFolder, "fr.json");
            File.WriteAllText(file1, "{}");
            File.WriteAllText(file2, "{}");

            // Act
            var languages = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.Equal(2, languages.Length);
            Assert.Contains(file1, languages);
            Assert.Contains(file2, languages);
        }
    }
}
