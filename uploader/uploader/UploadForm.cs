using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Forms;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;

namespace uploader
{
    public partial class UploadForm : DarkForm
    {
        private readonly bool _reopen;
        private readonly string _path;
        private readonly MainForm _mainForm;
        private readonly Settings _settings;
        private CancellationTokenSource _cancellationTokenSource;
        private RestClient _client;
        private bool _isFolder;
        private List<string> _filesToUpload;
        private string _cachedMd5;
        private string _cachedSha256;

        public UploadForm(MainForm mainForm, Settings settings, bool reopen, string path)
        {
            _path = path;
            _mainForm = mainForm;
            _settings = settings;
            _reopen = reopen;
            _isFolder = Directory.Exists(_path);

            InitializeComponent();
        }

        private void ChangeStatus(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => ChangeStatus(text)));
                return;
            }

            statusLabel.Text = text;
        }

        private void Finish(bool resetText)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => Finish(resetText)));
                return;
            }

            if (resetText)
            {
                ChangeStatus(LocalizationHelper.Base.Message_Idle);
            }

            uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
        }

        private void CloseWindow()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => CloseWindow()));
                return;
            }

            this.Close();
        }

        private void DisplayError(string error)
        {
            using (var messageBox = new DarkMessageBox(error, LocalizationHelper.Base.UploadForm_Error, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
            {
                messageBox.ShowDialog();
            }
        }

        private async Task UploadAsync(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_NoApiKey, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                {
                    messageBox.ShowDialog();
                }
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_InvalidLength, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                {
                    messageBox.ShowDialog();
                }
                return;
            }

            ChangeStatus(LocalizationHelper.Base.Message_Init);
            _client = new RestClient("https://www.virustotal.com");

            if (_isFolder)
            {
                _filesToUpload = Directory.GetFiles(_path, "*.*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                _filesToUpload = new List<string> { _path };
            }

            var tasks = new List<Task>();
            foreach (var file in _filesToUpload)
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

            Finish(true);
        }

        private void OpenUrlSafe(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return;
            }

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                // It is recommended to further restrict allowed hosts using a whitelist of known-safe domains.
                try
                {
                    Process.Start(uri.AbsoluteUri);
                }
                catch (Exception ex)
                {
                    // Process.Start can throw e.g. Win32Exception if there is no default handler for HTTP/HTTPS URLs.
                    // Silently ignoring is safer than crashing the background thread.
                    Debug.WriteLine($"Failed to open URL: {ex.Message}");
                }
            }
        }

        private async Task UploadFileAsync(string fullPath, CancellationToken token)
        {
            if (!File.Exists(fullPath))
            {
                DisplayError($"File {fullPath} does not exist.");
                return;
            }

            token.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(fullPath);
            ChangeStatus($"Checking {fileName}...");
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            string fileMd5;
            string fileSha256;

            bool canUseCachedHashes = !_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedMd5) && !string.IsNullOrEmpty(_cachedSha256);
            if (canUseCachedHashes)
            {
                fileMd5 = _cachedMd5;
                fileSha256 = _cachedSha256;
            }
            else
            {
                var hashes = Utils.GetHashes(fullPath);
                fileMd5 = hashes.MD5;
                fileSha256 = hashes.SHA256;
            }

            reportRequest.AddParameter("resource", fileSha256);
            reportRequest.AddParameter("resource", fileMd5);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                OpenUrlSafe(reportLink);
            }
            catch (RuntimeBinderException)
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                ChangeStatus($"Uploading {fileName}...");
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
                    OpenUrlSafe(scanLink);
                }
                catch (Exception ex)
                {
                    DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
        }

        private void StartUploadThread()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
                return;
            }

            uploadButton.Text = LocalizationHelper.Base.UploadForm_Cancel;
            _cancellationTokenSource = new CancellationTokenSource();

            var token = _cancellationTokenSource.Token;
            Task.Run(async () => await UploadAsync(token));
        }

        private void UploadForm_Load(object sender, EventArgs e)
        {
            if (_isFolder)
            {
                mdTextbox.Text = "N/A (Folder)";
                sha2Textbox.Text = "N/A (Folder)";
            }
            else
            {
                var hashes = Utils.GetHashes(_path);
                _cachedMd5 = hashes.MD5;
                _cachedSha256 = hashes.SHA256;
                mdTextbox.Text = _cachedMd5;
                sha2Textbox.Text = _cachedSha256;
            }

            settingsGroup.Text = LocalizationHelper.Base.UploadForm_Info;
            uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
            statusLabel.Text = LocalizationHelper.Base.Message_Idle;

            if (_settings.DirectUpload)
            {
                StartUploadThread();
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            StartUploadThread();
        }

        private void UploadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_reopen)
            {
                _mainForm.Show();
            }
            else
            {
                _mainForm.Close();
            }
        }
    }
}
