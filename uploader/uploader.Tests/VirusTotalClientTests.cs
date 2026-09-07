using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RestSharp;
using uploader;
using System.Collections.Generic;

namespace uploader.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private int _callCount = 0;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _callCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (_callCount == 1) // First call is CheckFileReport
            {
                response.Content = new StringContent("{}"); // no permalink, trigger runtimebinderexception
            }
            else // Second call is ScanFile
            {
                response.Content = new StringContent("{ \"invalid\": \"json\" }"); // missing sha256 to trigger Exception block
            }
            return Task.FromResult(response);
        }
    }

    public class VirusTotalClientTests
    {
        [Fact]
        public async Task UploadAsync_ScanFailsWithException_InvokesOnError()
        {
            // Arrange
            var httpClient = new HttpClient(new MockHttpMessageHandler()) { BaseAddress = new Uri("https://www.virustotal.com") };
            var restClient = new RestClient(httpClient);

            var vtClient = new VirusTotalClient("fake_api_key", restClient);
            var errorInvoked = false;
            string errorMessage = null;
            vtClient.OnError = (msg) =>
            {
                errorInvoked = true;
                errorMessage = msg;
            };

            var testFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(testFilePath, "test content");
                var job = new UploadJob { InitialPath = testFilePath, IsFolder = false };

                // Act
                await vtClient.UploadAsync(job, CancellationToken.None);

                // Assert
                Assert.True(errorInvoked, "OnError should be invoked");
                Assert.Contains("Failed to get link for", errorMessage);
            }
            finally
            {
                File.Delete(testFilePath);
            }
        }
    }
}
