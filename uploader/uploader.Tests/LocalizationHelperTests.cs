using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests
    {
        private const string LocalFolder = "local";

        public LocalizationHelperTests()
        {
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        [Fact]
        public void GetLanguages_WhenLocalFolderDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Length);
            Assert.Equal("", result[0]);
        }

        [Fact]
        public void GetLanguages_WhenLocalFolderExistsButEmpty_ReturnsEmptyArray()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);

            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Length);
        }

        [Fact]
        public void GetLanguages_WhenLocalFolderExistsWithFiles_ReturnsFilePaths()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);
            File.WriteAllText(Path.Combine(LocalFolder, "en.json"), "{}");
            File.WriteAllText(Path.Combine(LocalFolder, "fr.json"), "{}");

            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Contains(Path.Combine(LocalFolder, "en.json"), result);
            Assert.Contains(Path.Combine(LocalFolder, "fr.json"), result);
        }
    }
}
