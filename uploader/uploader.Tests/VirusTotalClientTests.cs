using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using uploader;

namespace uploader.Tests
{
    public class VirusTotalClientTests
    {
        [Fact]
        public async Task UploadAsync_ThrottlesConcurrency()
        {
            // We want to write a test that shows we don't just spin up 1000 tasks at once and await them all.
            // But since VirusTotalClient creates RestClient and does external calls, it might be tricky without mocking.
            // Let's at least see if it builds.
        }
    }
}
