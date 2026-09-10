using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;

namespace uploader
{

    public class UploadJob
    {
        public string InitialPath { get; set; }
        public bool IsFolder { get; set; }
        public string CachedSha256 { get; set; }
    }

    public class VirusTotalClient

    {
        private const string VirusTotalUrl = "https://www.virustotal.com";
        private const int MaxConcurrentUploads = 4;

        private readonly string _apiKey;
        private readonly RestClient _client;
        public Action<string> OnStatusChanged { get; set; }
        public Action<string> OnError { get; set; }

        public VirusTotalClient(string apiKey)
        {
            _apiKey = apiKey;
            _client = new RestClient(VirusTotalUrl);
        }

        public async Task UploadAsync(UploadJob job, CancellationToken token)
        {
            IEnumerable<string> filesToUpload;
            if (job.IsFolder)
            {
                filesToUpload = Directory.EnumerateFiles(job.InitialPath, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                filesToUpload = new List<string> { job.InitialPath };
            }

            var throttler = new SemaphoreSlim(MaxConcurrentUploads);
            try
            {
                var tasks = filesToUpload.Select(async file =>
                {
                    await throttler.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        await UploadFileAsync(file, job, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                throttler.Dispose();
            }
        }

        private async Task UploadFileAsync(string fullPath, UploadJob job, CancellationToken token)
        {
            if (!File.Exists(fullPath))
            {
                OnError?.Invoke($"File {fullPath} does not exist.");
                return;
            }

            token.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(fullPath);
            OnStatusChanged?.Invoke($"Checking {fileName}...");

            bool hasReport = await CheckFileReportAsync(fullPath, job, token).ConfigureAwait(false);

            if (!hasReport)
            {
                await ScanFileAsync(fullPath, fileName, token).ConfigureAwait(false);
            }
        }

        private async Task<bool> CheckFileReportAsync(string fullPath, UploadJob job, CancellationToken token)
        {
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _apiKey);

            string fileSha256 = (!job.IsFolder && fullPath == job.InitialPath && !string.IsNullOrEmpty(job.CachedSha256))
                ? job.CachedSha256
                : await Utils.GetSHA256Async(fullPath).ConfigureAwait(false);
            reportRequest.AddParameter("resource", fileSha256);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token).ConfigureAwait(false);

            if (!reportResponse.IsSuccessful || string.IsNullOrEmpty(reportResponse.Content))
            {
                OnError?.Invoke($"API request failed: {reportResponse.StatusCode} - {reportResponse.ErrorMessage}");
                return false;
            }

            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                Utils.OpenUrlSafe(reportLink);
                return true;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        private async Task ScanFileAsync(string fullPath, string fileName, CancellationToken token)
        {
            OnStatusChanged?.Invoke($"Uploading {fileName}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _apiKey);
            scanRequest.AddFile("file", fullPath);

            var scanResponse = await _client.ExecuteAsync(scanRequest, token).ConfigureAwait(false);

            if (!scanResponse.IsSuccessful || string.IsNullOrEmpty(scanResponse.Content))
            {
                OnError?.Invoke($"Upload failed: {scanResponse.StatusCode} - {scanResponse.ErrorMessage}");
                return;
            }

            var scanContent = scanResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic scanJson = JsonConvert.DeserializeObject(scanContent);

            try
            {
                string sha256 = scanJson.sha256.ToString();
                string scanId = scanJson.scan_id.ToString();

                var scanLink = $"{VirusTotalUrl}/gui/file/{sha256}/detection/{scanId}";
                Utils.OpenUrlSafe(scanLink);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to get link for {fileName}. Error: {ex.Message}");
            }
        }
    }
}
