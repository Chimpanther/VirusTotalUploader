using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using uploader;

namespace uploader.Tests
{
    [TestClass]
    public class LocalizationHelperTests
    {
        private const string LocalFolder = "local";

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        [TestMethod]
        public void GetLanguages_WhenLocalFolderDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("", result[0]);
        }

        [TestMethod]
        public void GetLanguages_WhenLocalFolderExistsButEmpty_ReturnsEmptyArray()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);

            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void GetLanguages_WhenLocalFolderExistsWithFiles_ReturnsFilePaths()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);
            File.WriteAllText(Path.Combine(LocalFolder, "en.json"), "{}");
            File.WriteAllText(Path.Combine(LocalFolder, "fr.json"), "{}");

            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.Contains(result, Path.Combine(LocalFolder, "en.json"));
            CollectionAssert.Contains(result, Path.Combine(LocalFolder, "fr.json"));
        }
    }
}
