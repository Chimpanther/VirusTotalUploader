using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetMD5_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakePath = "this_file_does_not_exist.txt";

            // Act & Assert
            Xunit.Assert.Throws<FileNotFoundException>(() => Utils.GetMD5(fakePath));
        }

        [Fact]
        public void GetMD5_ValidFile_ReturnsCorrectHash()
        {
            VerifyHashMatch("hello world", "5EB63BBBE01EEED093CB22BB8F5ACDC3");
        }

        private void VerifyHashMatch(string content, string expectedHash)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, content);
                string hash = Utils.GetMD5(tempFile);
                Xunit.Assert.Equal(expectedHash, hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
