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
        public void GetMD5_NullFile_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Utils.GetMD5(null!));
        }

        [Fact]
        public void GetMD5_EmptyFile_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // File.WriteAllText(tempFile, ""); // Temp files are empty by default

                // Act
                string hash = Utils.GetMD5(tempFile);

                // Assert
                Assert.Equal("D41D8CD98F00B204E9800998ECF8427E", hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
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
        public void GetSHA256_NonExistentFile_ThrowsFileNotFoundException()
        {
            string fakePath = "this_file_does_not_exist_sha256.txt";
            Assert.Throws<FileNotFoundException>(() => Utils.GetSHA256(fakePath));
        }

        [Fact]
        public void GetSHA256_NullFile_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.GetSHA256(null!));
        }

        [Fact]
        public void GetSHA256_ValidFile_ReturnsCorrectHash()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");
                string hash = Utils.GetSHA256(tempFile);
                // SHA256 of "hello world" is b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetSHA1_NonExistentFile_ThrowsFileNotFoundException()
        {
            string fakePath = "this_file_does_not_exist_sha1.txt";
            Assert.Throws<FileNotFoundException>(() => Utils.GetSHA1(fakePath));
        }

        [Fact]
        public void GetSHA1_NullFile_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.GetSHA1(null!));
        }

        [Fact]
        public void GetSHA1_ValidFile_ReturnsCorrectHash()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");
                string hash = Utils.GetSHA1(tempFile);
                // SHA1 of "hello world" is 2aae6c35c94fcfb415dbe95f408b9ce91ee846ed
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
