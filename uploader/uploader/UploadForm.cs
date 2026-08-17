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
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => DisplayError(error)));
                return;
            }

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
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Process.Start can throw e.g. Win32Exception if there is no default handler for HTTP/HTTPS URLs.
                    // Silently ignoring is safer than crashing the background thread.
                    Debug.WriteLine($"Failed to open URL: {ex.Message}");
                }
            }
        }

        private async Task<bool> CheckFileReportAsync(string fullPath, string fileName, CancellationToken token)
        {
            ChangeStatus($"Checking {fileName}...");
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);

            bool isMainFile = (!_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedMd5));
            string fileSha256;
            string fileMd5;

            if (isMainFile)
            {
                var hashes = Utils.GetHashes(fullPath);
                fileSha256 = hashes.SHA256;
                fileMd5 = _cachedMd5;
            }
            else
            {
                var hashes = Utils.GetHashes(fullPath);
                fileSha256 = hashes.SHA256;
                fileMd5 = hashes.MD5;
            }

            reportRequest.AddParameter("resource", fileSha256);
            reportRequest.AddParameter("resource", fileMd5);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);

            token.ThrowIfCancellationRequested();

            if (reportResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var reportContent = reportResponse.Content;
                    dynamic reportJson = JsonConvert.DeserializeObject(reportContent);
                    var reportLink = reportJson.permalink.ToString();
                    OpenUrlSafe(reportLink);
                    return true;
                }
                catch (RuntimeBinderException)
                {
                    // Json does not contain permalink
                }
            }

            return false;
        }

        private async Task ScanFileAsync(string fullPath, string fileName, CancellationToken token)
        {
            ChangeStatus($"Uploading {fileName}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _settings.ApiKey);
            scanRequest.AddFile("file", fullPath);

            var scanResponse = await _client.ExecuteAsync(scanRequest, token);

            token.ThrowIfCancellationRequested();

            if (scanResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var scanContent = scanResponse.Content;
                    dynamic scanJson = JsonConvert.DeserializeObject(scanContent);

                    string sha256 = Uri.EscapeDataString(scanJson.sha256.ToString());
                    string scanId = Uri.EscapeDataString(scanJson.scan_id.ToString());

                    var scanLink = $"https://www.virustotal.com/gui/file/{sha256}/detection/{scanId}";
                    OpenUrlSafe(scanLink);
                }
                catch (Exception ex)
                {
                    DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
            else
            {
                DisplayError($"Failed to upload {fileName}. Error code: {scanResponse.StatusCode}");
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

            bool reportParsed = await CheckFileReportAsync(fullPath, fileName, token);

            if (!reportParsed)
            {
                await ScanFileAsync(fullPath, fileName, token);
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
                shaTextbox.Text = "N/A (Folder)";
                sha2Textbox.Text = "N/A (Folder)";
            }
            else
            {
                var hashes = Utils.GetHashes(_path);
                _cachedMd5 = hashes.MD5;
                mdTextbox.Text = _cachedMd5;
                shaTextbox.Text = hashes.SHA1;
                sha2Textbox.Text = hashes.SHA256;
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