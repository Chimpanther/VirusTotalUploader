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

        private static void AssertFileHash(Func<string, string> hashFunction, string content, string expectedHash)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                if (content != null)
                {
                    File.WriteAllText(tempFile, content);
                }
                var hash = hashFunction(tempFile);

                Assert.Equal(expectedHash, hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
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
            AssertFileHash(Utils.GetSHA256, "hello world", "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9");
        }

        [Fact]
        public void GetSHA256_EmptyFile_ReturnsCorrectHash()
        {
            AssertFileHash(Utils.GetSHA256, null, "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSHA256Async_NonExistentFile_ThrowsFileNotFoundException()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), "vtu-missing-" + Guid.NewGuid() + ".txt");
            await Assert.ThrowsAsync<FileNotFoundException>(() => Utils.GetSHA256Async(missingPath));
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSHA256Async_NullFile_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Utils.GetSHA256Async(null!));
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSHA256Async_ValidFile_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");
                var hash = await Utils.GetSHA256Async(tempFile);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSHA256Async_EmptyFile_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var hash = await Utils.GetSHA256Async(tempFile);
                Assert.Equal("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855", hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
