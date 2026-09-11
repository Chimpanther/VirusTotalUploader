
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, string> ResponseFactory { get; set; } = _ => "{}";
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseFactory(request))
            };
            return Task.FromResult(response);
        }
    }

    public class VirusTotalClientTests : IDisposable
    {
        private string _tempFile;
        private Action<System.Diagnostics.ProcessStartInfo> _originalProcessStarter;

        public VirusTotalClientTests()
        {
            _tempFile = Path.GetTempFileName();
            File.WriteAllText(_tempFile, "test content");
            _originalProcessStarter = Utils.ProcessStarter;
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
            Utils.ProcessStarter = _originalProcessStarter;
        }

        [Fact]
        public async Task UploadFileAsync_WithPermalink_DoesNotTriggerFallback()
        {
            string urlOpened = null;
            Utils.ProcessStarter = psi => urlOpened = psi.FileName;
            var mockHandler = new MockHttpMessageHandler
            {
                ResponseFactory = req =>
                {
                    if (req.RequestUri.ToString().Contains("report"))
                    {
                        return "{\"permalink\":\"https://www.virustotal.com/gui/file/12345/detection/67890\"}";
                    }
                    return "{}";
                }
            };

            var options = new RestClientOptions("https://www.virustotal.com")
            {
                ConfigureMessageHandler = _ => mockHandler
            };
            var restClient = new RestClient(options);
            var client = new VirusTotalClient("fake_api_key", restClient);

            var job = new UploadJob { InitialPath = _tempFile, IsFolder = false, CachedSha256 = "dummy" };
            await client.UploadAsync(job, CancellationToken.None);

            Assert.Equal("https://www.virustotal.com/gui/file/12345/detection/67890", urlOpened);
            Assert.Equal(1, mockHandler.RequestCount); // Only hits /file/report
        }

        [Fact]
        public async Task UploadFileAsync_WithoutPermalink_TriggersFallback()
        {
            string urlOpened = null;
            Utils.ProcessStarter = psi => urlOpened = psi.FileName;
            var mockHandler = new MockHttpMessageHandler
            {
                ResponseFactory = req =>
                {
                    if (req.RequestUri.ToString().Contains("report"))
                    {
                        return "{}"; // No permalink, should trigger fallback
                    }
                    if (req.RequestUri.ToString().Contains("scan"))
                    {
                        return "{\"sha256\":\"dummy_sha256\",\"scan_id\":\"dummy_scan_id\"}";
                    }
                    return "{}";
                }
            };

            var options = new RestClientOptions("https://www.virustotal.com")
            {
                ConfigureMessageHandler = _ => mockHandler
            };
            var restClient = new RestClient(options);
            var client = new VirusTotalClient("fake_api_key", restClient);

            var job = new UploadJob { InitialPath = _tempFile, IsFolder = false, CachedSha256 = "dummy" };
            await client.UploadAsync(job, CancellationToken.None);

            Assert.Equal("https://www.virustotal.com/gui/file/dummy_sha256/detection/dummy_scan_id", urlOpened);
            Assert.Equal(2, mockHandler.RequestCount); // Hits /file/report, then /file/scan
        }

        [Fact]
        public async Task UploadFileAsync_InvalidJson_TriggersFallback()
        {
            string urlOpened = null;
            Utils.ProcessStarter = psi => urlOpened = psi.FileName;
            var mockHandler = new MockHttpMessageHandler
            {
                ResponseFactory = req =>
                {
                    if (req.RequestUri.ToString().Contains("report"))
                    {
                        return "invalid json"; // Invalid JSON
                    }
                    if (req.RequestUri.ToString().Contains("scan"))
                    {
                        return "{\"sha256\":\"dummy_sha256_invalid\",\"scan_id\":\"dummy_scan_id_invalid\"}";
                    }
                    return "{}";
                }
            };

            var options = new RestClientOptions("https://www.virustotal.com")
            {
                ConfigureMessageHandler = _ => mockHandler
            };
            var restClient = new RestClient(options);
            var client = new VirusTotalClient("fake_api_key", restClient);

            var job = new UploadJob { InitialPath = _tempFile, IsFolder = false, CachedSha256 = "dummy" };
            await client.UploadAsync(job, CancellationToken.None);

            Assert.Equal("https://www.virustotal.com/gui/file/dummy_sha256_invalid/detection/dummy_scan_id_invalid", urlOpened);
            Assert.Equal(2, mockHandler.RequestCount); // Hits /file/report, then /file/scan
        }
    }
}
