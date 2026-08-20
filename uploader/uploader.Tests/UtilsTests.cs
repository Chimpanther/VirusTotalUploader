using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {

        [Fact]
        public void GetSHA1_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakePath = "this_file_does_not_exist_sha1.txt";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => Utils.GetSHA1(fakePath));
        }

        [Fact]
        public void GetSHA1_ValidFile_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetSHA1(tempFile);

                // Assert
                // SHA1 of "hello world" is 2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
