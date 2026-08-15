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
        [Fact]
        public void GetSHA256_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakePath = "this_file_does_not_exist_sha256.txt";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => Utils.GetSHA256(fakePath));
        }

        [Fact]
        public void GetSHA256_ValidFile_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetSHA256(tempFile);

                // Assert
                // SHA256 of "hello world" is B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

    }
}
