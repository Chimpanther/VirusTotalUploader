using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;

namespace uploader
{
    public interface IUploadCallbacks
    {
        void ChangeStatus(string text);
        void Finish(bool resetText);
        void DisplayError(string error);
        void OpenUrlSafe(string url);
    }

    public class UploadLogic
    {
        private readonly Settings _settings;
        private readonly bool _isFolder;
        private readonly string _path;
        private readonly string _cachedSha256;
        private readonly IUploadCallbacks _callbacks;
        private RestClient _client;

        public UploadLogic(Settings settings, bool isFolder, string path, string cachedSha256, IUploadCallbacks callbacks, RestClient client = null)
        {
            _settings = settings;
            _isFolder = isFolder;
            _path = path;
            _cachedSha256 = cachedSha256;
            _callbacks = callbacks;
            _client = client ?? new RestClient("https://www.virustotal.com");
        }

        public async Task UploadAsync(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                _callbacks.DisplayError(LocalizationHelper.Base.UploadForm_NoApiKey);
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                _callbacks.DisplayError(LocalizationHelper.Base.UploadForm_InvalidLength);
                return;
            }

            _callbacks.ChangeStatus(LocalizationHelper.Base.Message_Init);

            IEnumerable<string> filesToUpload;
            if (_isFolder)
            {
                filesToUpload = Directory.EnumerateFiles(_path, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                filesToUpload = new List<string> { _path };
            }

            var tasks = new List<Task>();
            foreach (var file in filesToUpload)
            {
                tasks.Add(UploadFileAsync(file, token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Cancellation was requested, do nothing special here.
            }

            _callbacks.Finish(true);
        }

        public async Task UploadFileAsync(string fullPath, CancellationToken token)
        {
            if (!File.Exists(fullPath))
            {
                _callbacks.DisplayError($"File {fullPath} does not exist.");
                return;
            }

            token.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(fullPath);
            _callbacks.ChangeStatus($"Checking {fileName}...");
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);

            string fileSha256 = (!_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedSha256)) ? _cachedSha256 : Utils.GetSHA256(fullPath);
            reportRequest.AddParameter("resource", fileSha256);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                _callbacks.OpenUrlSafe(reportLink);
            }
            catch (RuntimeBinderException)
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                _callbacks.ChangeStatus($"Uploading {fileName}...");
                var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
                scanRequest.AddParameter("apikey", _settings.ApiKey);
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
                    _callbacks.OpenUrlSafe(scanLink);
                }
                catch (Exception ex)
                {
                    _callbacks.DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
        }
    }
}
