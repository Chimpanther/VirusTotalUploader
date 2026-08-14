using System.IO;
using System.Linq;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests : System.IDisposable
    {
        private const string LocalFolder = "local";

        public LocalizationHelperTests()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (Directory.Exists(LocalFolder))
            {
                Directory.Delete(LocalFolder, true);
            }
        }

        [Fact]
        public void GetLanguages_DirectoryDoesNotExist_ReturnsArrayWithEmptyString()
        {
            // Act
            var result = LocalizationHelper.GetLanguages();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Length);
            Assert.Equal("", result[0]);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            // Result paths will have platform specific path separators, but Directory.GetFiles
            // returns them exactly as constructed if they are in the working directory.
            Assert.Equal(new[] { file1, file2 }.OrderBy(x => x), result.OrderBy(x => x));
        }
    }
}
