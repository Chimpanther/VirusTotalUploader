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
    public class VirusTotalClient
    {
        private readonly string _apiKey;
        private readonly RestClient _client;
        public Action<string> OnStatusChanged { get; set; }
        public Action<string> OnError { get; set; }

        public VirusTotalClient(string apiKey)
        {
            _apiKey = apiKey;
            _client = new RestClient("https://www.virustotal.com");
        }

        public async Task UploadAsync(string initialPath, bool isFolder, string cachedSha256, CancellationToken token)
        {
            IEnumerable<string> filesToUpload;
            if (isFolder)
            {
                filesToUpload = Directory.EnumerateFiles(initialPath, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                filesToUpload = new List<string> { initialPath };
            }

            var tasks = new List<Task>();
            foreach (var file in filesToUpload)
            {
                tasks.Add(UploadFileAsync(file, initialPath, isFolder, cachedSha256, token));
            }

            await Task.WhenAll(tasks);
        }

        private async Task UploadFileAsync(string fullPath, string initialPath, bool isFolder, string cachedSha256, CancellationToken token)
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

            string fileSha256 = (!isFolder && fullPath == initialPath && !string.IsNullOrEmpty(cachedSha256)) ? cachedSha256 : Utils.GetSHA256(fullPath);
            reportRequest.AddParameter("resource", fileSha256);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
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
