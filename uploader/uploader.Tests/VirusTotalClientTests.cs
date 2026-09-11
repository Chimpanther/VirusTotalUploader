using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using uploader;
using RestSharp;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

namespace uploader.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public int Requests { get; private set; }
        public List<string> RequestUris { get; private set; } = new List<string>();

        public string ReportResponseContent { get; set; } = "{\"response_code\": 0}";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            RequestUris.Add(request.RequestUri?.ToString() ?? "");

            if (request.RequestUri?.ToString().Contains("vtapi/v2/file/report") == true)
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ReportResponseContent)
                });
            }

            if (request.RequestUri?.ToString().Contains("vtapi/v2/file/scan") == true)
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"response_code\": 1, \"sha256\": \"12345\", \"scan_id\": \"67890\"}")
                });
            }

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{}")
            });
        }
    }

    [Collection("Sequential")]
    public class VirusTotalClientTests
    {
        [Fact]
        public async Task UploadAsync_WhenPermalinkMissing_CallsScanFile()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            // Missing permalink to trigger RuntimeBinderException in dynamic json
            mockHandler.ReportResponseContent = "{\"response_code\": 0}";

            var options = new RestClientOptions("https://example.com")
            {
                ConfigureMessageHandler = _ => mockHandler
            };
            var client = new RestClient(options);

            var vtClient = new VirusTotalClient("fake_api_key", client);
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "test content");

            var job = new UploadJob
            {
                InitialPath = filePath,
                IsFolder = false,
                CachedSha256 = "dummyhash"
            };

            bool scanTriggered = false;
            vtClient.OnStatusChanged = (status) =>
            {
                if (status.Contains("Uploading"))
                {
                    scanTriggered = true;
                }
            };

            try
            {
                // Act
                await vtClient.UploadAsync(job, CancellationToken.None);

                // Assert
                Assert.True(scanTriggered);
                Assert.Equal(2, mockHandler.Requests);
                Assert.Contains(mockHandler.RequestUris, uri => uri.Contains("vtapi/v2/file/report"));
                Assert.Contains(mockHandler.RequestUris, uri => uri.Contains("vtapi/v2/file/scan"));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task UploadAsync_WhenPermalinkExists_DoesNotCallScanFile()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.ReportResponseContent = "{\"response_code\": 1, \"permalink\": \"https://virustotal.com/test\"}";

            var options = new RestClientOptions("https://example.com")
            {
                ConfigureMessageHandler = _ => mockHandler
            };
            var client = new RestClient(options);

            var vtClient = new VirusTotalClient("fake_api_key", client);
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "test content");

            var job = new UploadJob
            {
                InitialPath = filePath,
                IsFolder = false,
                CachedSha256 = "dummyhash"
            };

            bool scanTriggered = false;
            vtClient.OnStatusChanged = (status) =>
            {
                if (status.Contains("Uploading"))
                {
                    scanTriggered = true;
                }
            };

            try
            {
                // Act
                await vtClient.UploadAsync(job, CancellationToken.None);

                // Assert
                Assert.False(scanTriggered);
                Assert.Equal(1, mockHandler.Requests);
                Assert.Contains(mockHandler.RequestUris, uri => uri.Contains("vtapi/v2/file/report"));
                Assert.DoesNotContain(mockHandler.RequestUris, uri => uri.Contains("vtapi/v2/file/scan"));
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
