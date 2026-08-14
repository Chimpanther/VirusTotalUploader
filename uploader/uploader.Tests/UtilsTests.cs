using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public sealed class UtilsTests
    {
        [Fact]
        public void GetMD5_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetMD5(tempFile);

                // Assert
                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", hash);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void GetMD5_EmptyFile_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "");

                // Act
                string hash = Utils.GetMD5(tempFile);

                // Assert
                Assert.Equal("D41D8CD98F00B204E9800998ECF8427E", hash);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void GetMD5_FileNotFound_ThrowsException()
        {
            string notExistingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            Assert.Throws<FileNotFoundException>(() => Utils.GetMD5(notExistingFile));
        }

        [Fact]
        public void GetMD5_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.GetMD5(null!));
        }

        [Fact]
        public void GetSHA256_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetSHA256(tempFile);

                // Assert
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hash);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void GetSHA1_ReturnsCorrectHash()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                string hash = Utils.GetSHA1(tempFile);

                // Assert
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", hash);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void GetMD5_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakePath = "this_file_does_not_exist.txt";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => Utils.GetMD5(fakePath));
        }
    }
}
