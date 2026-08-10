using System;
using System.Diagnostics;
using System.IO;
using uploader;
using Xunit;
using Xunit.Abstractions;

namespace uploader.Tests
{
    public class UtilsTests
    {
        private readonly ITestOutputHelper _output;

        public UtilsTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
                // Hashes of "hello world" without newlines
                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", hashes.md5);
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", hashes.sha1);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hashes.sha256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void BenchmarkHashing()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                // Create a 50MB file
                byte[] data = new byte[50 * 1024 * 1024];
                new Random(42).NextBytes(data);
                File.WriteAllBytes(tempFile, data);

                Stopwatch sw1 = Stopwatch.StartNew();
                string md5 = Utils.GetMD5(tempFile);
                string sha1 = Utils.GetSHA1(tempFile);
                string sha256 = Utils.GetSHA256(tempFile);
                sw1.Stop();

                Stopwatch sw2 = Stopwatch.StartNew();
                var hashes = Utils.GetHashes(tempFile);
                sw2.Stop();

                _output.WriteLine($"Baseline (Sequential): {sw1.ElapsedMilliseconds} ms");
                _output.WriteLine($"Optimized (Single-pass): {sw2.ElapsedMilliseconds} ms");

                Assert.Equal(md5, hashes.md5);
                Assert.Equal(sha1, hashes.sha1);
                Assert.Equal(sha256, hashes.sha256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
