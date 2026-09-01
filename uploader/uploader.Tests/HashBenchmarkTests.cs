using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace uploader.Tests
{
    public class HashBenchmarkTests
    {
        private readonly ITestOutputHelper _output;

        public HashBenchmarkTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CompareHashingMethods()
        {
            // Create a 100MB dummy file
            var filePath = Path.Combine(Path.GetTempPath(), "dummy_100mb.bin");
            if (!File.Exists(filePath))
            {
                var rng = new Random(42);
                byte[] buffer = new byte[1024 * 1024]; // 1MB
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        rng.NextBytes(buffer);
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
            }

            try
            {
                // Warm up
                Utils.GetSHA256(filePath);
                await Utils.GetSHA256Async(filePath);

                var swSync = Stopwatch.StartNew();
                string syncHash = Utils.GetSHA256(filePath);
                swSync.Stop();

                _output.WriteLine($"Sync Hash: {syncHash} took {swSync.ElapsedMilliseconds} ms");

                var swAsync = Stopwatch.StartNew();
                string asyncHash = await Utils.GetSHA256Async(filePath);
                swAsync.Stop();

                _output.WriteLine($"Async Hash: {asyncHash} took {swAsync.ElapsedMilliseconds} ms");

                Assert.Equal(syncHash, asyncHash);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
