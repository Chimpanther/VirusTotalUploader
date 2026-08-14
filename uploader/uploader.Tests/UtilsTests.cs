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
            Assert.Throws<FileNotFoundException>(() => Utils.GetMD5(fakePath));
        }

        [Fact]
        public void GetMD5_ValidFile_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetMD5(tempFile);

                // Assert
                // MD5 of "hello world" is 5EB63BBBE01EEED093CB22BB8F5ACDC3
                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetSHA1_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, System.Text.Encoding.ASCII.GetBytes("test"));
                var result = Utils.GetSHA1(tempFile);
                Assert.Equal("A94A8FE5CCB19BA61C4C0873D391E987982FBBD3", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetSHA256_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, System.Text.Encoding.ASCII.GetBytes("test"));
                var result = Utils.GetSHA256(tempFile);
                Assert.Equal("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
