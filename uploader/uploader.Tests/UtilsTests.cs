using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class UtilsTests
    {
        [Fact]
        public void GetHashes_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string fakePath = "this_file_does_not_exist.txt";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => Utils.GetHashes(fakePath));
        }

        [Fact]
        public void GetHashes_ValidFile_ReturnsCorrectHashes()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                // Act
                var hashes = Utils.GetHashes(tempFile);

                // Assert
                // MD5 of "hello world" is 5EB63BBBE01EEED093CB22BB8F5ACDC3
                // SHA256 of "hello world" is B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9
                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", hashes.MD5);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hashes.SHA256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
