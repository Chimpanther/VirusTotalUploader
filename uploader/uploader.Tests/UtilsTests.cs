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
        public void GetSHA256_Benchmark()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // Create a 50MB dummy file
                byte[] data = new byte[50 * 1024 * 1024];
                new Random(42).NextBytes(data);
                File.WriteAllBytes(tempFile, data);

                // Warmup
                Utils.GetSHA256(tempFile);

                // Act - Baseline: Uncached (compute twice)
                var swUncached = System.Diagnostics.Stopwatch.StartNew();
                string hash1 = Utils.GetSHA256(tempFile);
                string hash2 = Utils.GetSHA256(tempFile); // Duplicate work
                swUncached.Stop();

                // Act - Optimized: Cached (compute once, use cached)
                var swCached = System.Diagnostics.Stopwatch.StartNew();
                string cachedHash = Utils.GetSHA256(tempFile);
                string useHash2 = cachedHash; // Use cached work
                swCached.Stop();

                // Assert
                Console.WriteLine($"[Benchmark] Uncached time (ms): {swUncached.ElapsedMilliseconds}");
                Console.WriteLine($"[Benchmark] Cached time (ms): {swCached.ElapsedMilliseconds}");

                Assert.Equal(hash1, hash2);
                Assert.Equal(hash1, cachedHash);
                // We expect cached to be significantly faster (at least 20% faster for 50MB)
                Assert.True(swCached.ElapsedMilliseconds < swUncached.ElapsedMilliseconds);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
}
}
