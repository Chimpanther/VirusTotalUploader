using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly string _apiKey;
        private readonly RestClient _client;
        public Action<string> OnStatusChanged { get; set; }
        public Action<string> OnError { get; set; }

        public VirusTotalClient(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            if (apiKey.Length != 64)
                throw new ArgumentException("API key must be 64 characters", nameof(apiKey));

            _apiKey = apiKey;
            _client = new RestClient("https://www.virustotal.com");
        }

        public async Task UploadAsync(UploadJob job, CancellationToken token)
        {
            IEnumerable<string> filesToUpload;
            if (job.IsFolder)
            {
                try
                {
                    filesToUpload = Directory.EnumerateFiles(job.InitialPath, "*.*", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Failed to enumerate folder {job.InitialPath}: {ex.Message}");
                    return;
                }
            }
            else
            {
                filesToUpload = new List<string> { job.InitialPath };
            }

            foreach (var file in filesToUpload)
            {
                await UploadFileAsync(file, job, token);
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
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _apiKey);

            string fileSha256;
            try
            {
                fileSha256 = (!job.IsFolder && fullPath == job.InitialPath && !string.IsNullOrEmpty(job.CachedSha256)) ? job.CachedSha256 : Utils.GetSHA256(fullPath);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to hash {fileName}: {ex.Message}");
                return;
            }
            reportRequest.AddParameter("resource", fileSha256);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
            if (!reportResponse.IsSuccessful)
            {
                OnError?.Invoke($"API request failed for {fileName}: {reportResponse.StatusCode} - {reportResponse.ErrorMessage}");
                return;
            }
            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                Utils.OpenUrlSafe(reportLink);
            }
            catch (RuntimeBinderException)
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                OnStatusChanged?.Invoke($"Uploading {fileName}...");
                var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
                scanRequest.AddParameter("apikey", _apiKey);
                scanRequest.AddFile("file", fullPath);

                var scanResponse = await _client.ExecuteAsync(scanRequest, token);
                if (!scanResponse.IsSuccessful)
                {
                    OnError?.Invoke($"Upload failed for {fileName}: {scanResponse.StatusCode} - {scanResponse.ErrorMessage}");
                    return;
                }
                var scanContent = scanResponse.Content;

                token.ThrowIfCancellationRequested();

                dynamic scanJson = JsonConvert.DeserializeObject(scanContent);

                try
                {
                    string sha256 = scanJson.sha256.ToString();
                    string scanId = scanJson.scan_id.ToString();

                    var scanLink = $"https://www.virustotal.com/gui/file/{sha256}/detection/{scanId}";
                    Utils.OpenUrlSafe(scanLink);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
        }
    }
}
