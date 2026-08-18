using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {
        private static void AssertThrowsFileNotFound(Func<string, string> hashFunction)
        {
            var missingPath = Path.Combine(Path.GetTempPath(), "vtu-missing-" + Guid.NewGuid() + ".txt");
            Assert.Throws<FileNotFoundException>(() => hashFunction(missingPath));
        }

        private static void AssertThrowsArgumentNull(Func<string, string> hashFunction)
        {
            Assert.Throws<ArgumentNullException>(() => hashFunction(null!));
        }

        private static void AssertValidFileHash(Func<string, string> hashFunction, string expectedHash)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");
                var hash = hashFunction(tempFile);

                Assert.Equal(expectedHash, hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private static void AssertEmptyFileHash(Func<string, string> hashFunction, string expectedHash)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var hash = hashFunction(tempFile);

                Assert.Equal(expectedHash, hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetMD5_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(Utils.GetMD5);
        }

        [Fact]
        public void GetMD5_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(Utils.GetMD5);
        }

        [Fact]
        public void GetMD5_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetMD5, "5EB63BBBE01EEED093CB22BB8F5ACDC3");
        }

        [Fact]
        public void GetMD5_EmptyFile_ReturnsCorrectHash()
        {
            AssertEmptyFileHash(Utils.GetMD5, "D41D8CD98F00B204E9800998ECF8427E");
        }

        [Fact]
        public void GetSHA1_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(Utils.GetSHA1);
        }

        [Fact]
        public void GetSHA1_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(Utils.GetSHA1);
        }

        [Fact]
        public void GetSHA1_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetSHA1, "2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED");
        }

        [Fact]
        public void GetSHA1_EmptyFile_ReturnsCorrectHash()
        {
            AssertEmptyFileHash(Utils.GetSHA1, "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709");
        }

        [Fact]
        public void GetSHA256_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(Utils.GetSHA256);
        }

        [Fact]
        public void GetSHA256_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(Utils.GetSHA256);
        }

        [Fact]
        public void GetSHA256_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetSHA256, "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9");
        }

        [Fact]
        public void GetSHA256_EmptyFile_ReturnsCorrectHash()
        {
            AssertEmptyFileHash(Utils.GetSHA256, "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
        }
    }
}
