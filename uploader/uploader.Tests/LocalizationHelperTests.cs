using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class LocalizationHelperTests : IDisposable
    {
        private const string LocalFolder = "local";

        public LocalizationHelperTests()
        {
            // Clean up before tests
            LocalizationHelper.Base = null;
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        public void Dispose()
        {
            // Clean up after tests
            LocalizationHelper.Base = null;
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

        [Fact]
        public void Load_ValidJson_SetsBaseProperties()
        {
            // Arrange
            string testFilePath = "test_localization.json";
            string jsonContent = @"{ ""MainForm_DragFile"": ""Drag here custom"" }";
            File.WriteAllText(testFilePath, jsonContent);

            try
            {
                // Act
                LocalizationHelper.Load(testFilePath);

                // Assert
                Assert.NotNull(LocalizationHelper.Base);
                Assert.Equal("Drag here custom", LocalizationHelper.Base.MainForm_DragFile);
                // Assert that default value is maintained for not specified properties
                Assert.Equal("More", LocalizationHelper.Base.MainForm_More);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Fact]
        public void Load_FileNotFound_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistentPath = "non_existent_localization.json";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => LocalizationHelper.Load(nonExistentPath));
        }
    }
}
