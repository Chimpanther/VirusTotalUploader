using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private IEnumerable<string> _filesToUpload;
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
                _filesToUpload = Directory.EnumerateFiles(_path, "*.*", SearchOption.AllDirectories);
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
                var host = uri.Host.ToLowerInvariant();
                if (host != "virustotal.com" && host != "www.virustotal.com")
                {
                    Debug.WriteLine($"Blocked attempt to open non-whitelisted URL: {url}");
                    return;
                }

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = uri.AbsoluteUri,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
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
            string fileSha256 = (!_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedSha256)) ? _cachedSha256 : Utils.GetSHA256(fullPath);
            var reportRequest = new RestRequest($"api/v3/files/{fileSha256}", Method.Get);
            reportRequest.AddHeader("x-apikey", _settings.ApiKey);

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            if (reportResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var reportLink = $"https://www.virustotal.com/gui/file/{reportJson.data.id}";
                    OpenUrlSafe(reportLink);
                }
                catch (Exception ex)
                {
                    DisplayError($"Failed to parse report for {fileName}. Error: {ex.Message}");
                }
            }
            else
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                ChangeStatus($"Uploading {fileName}...");
                var scanRequest = new RestRequest("api/v3/files", Method.Post);
                scanRequest.AddHeader("x-apikey", _settings.ApiKey);
                scanRequest.AddFile("file", fullPath);

                var scanResponse = await _client.ExecuteAsync(scanRequest, token);
                var scanContent = scanResponse.Content;

                token.ThrowIfCancellationRequested();

                dynamic scanJson = JsonConvert.DeserializeObject(scanContent);

                try
                {
                    string id = scanJson.data.id.ToString();
                    var scanLink = $"https://www.virustotal.com/gui/file-analysis/{id}";
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
                sha2Textbox.Text = "N/A (Folder)";
            }
            else
            {
                _cachedSha256 = Utils.GetSHA256(_path);
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
