using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
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
            Cleanup();
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
        public void GetLanguages_DirectoryDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("", result[0]);
        }

        [TestMethod]
        public void GetLanguages_DirectoryExists_ReturnsFiles()
        {
            // Arrange
            Directory.CreateDirectory(LocalFolder);
            string file1 = Path.Combine(LocalFolder, "English.json");
            string file2 = Path.Combine(LocalFolder, "Spanish.json");
            File.WriteAllText(file1, "{}");
            File.WriteAllText(file2, "{}");

            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            // Result paths will have platform specific path separators, but Directory.GetFiles
            // returns them exactly as constructed if they are in the working directory.
            CollectionAssert.AreEquivalent(new[] { file1, file2 }, result);
        }
    }
}
