using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {
        private void AssertThrowsFileNotFound(Action<string> hashFunc)
        {
            string fakePath = Path.Combine(Path.GetTempPath(), "this_file_does_not_exist_" + Guid.NewGuid() + ".txt");
            Assert.Throws<FileNotFoundException>(() => hashFunc(fakePath));
        }

        private void AssertThrowsArgumentNull(Action<string> hashFunc)
        {
            Assert.Throws<ArgumentNullException>(() => hashFunc(null!));
        }

        private void AssertValidFileHash(Func<string, string> hashFunc, string expectedHash)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");
                string hash = hashFunc(tempFile);
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
            AssertThrowsFileNotFound(path => Utils.GetMD5(path));
        }

        [Fact]
        public void GetMD5_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(path => Utils.GetMD5(path));
        }

        [Fact]
        public void GetMD5_EmptyFile_ReturnsCorrectHash()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                string hash = Utils.GetMD5(tempFile);
                Assert.Equal("D41D8CD98F00B204E9800998ECF8427E", hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetMD5_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetMD5, "5EB63BBBE01EEED093CB22BB8F5ACDC3");
        }

        [Fact]
        public void GetSHA256_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(path => Utils.GetSHA256(path));
        }

        [Fact]
        public void GetSHA256_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(path => Utils.GetSHA256(path));
        }

        [Fact]
        public void GetSHA256_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetSHA256, "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9");
        }

        [Fact]
        public void GetSHA1_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(path => Utils.GetSHA1(path));
        }

        [Fact]
        public void GetSHA1_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(path => Utils.GetSHA1(path));
        }

        [Fact]
        public void GetSHA1_ValidFile_ReturnsCorrectHash()
        {
            AssertValidFileHash(Utils.GetSHA1, "2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED");
        }
    }
}
