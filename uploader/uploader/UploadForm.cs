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
        private Task _uploadTask;
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

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
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

        private bool CheckApiKey(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                ShowErrorDialog(LocalizationHelper.Base.UploadForm_NoApiKey, LocalizationHelper.Base.UploadForm_InvalidKey, token);
                return false;
            }

            if (_settings.ApiKey.Length != 64)
            {
                ShowErrorDialog(LocalizationHelper.Base.UploadForm_InvalidLength, LocalizationHelper.Base.UploadForm_InvalidKey, token);
                return false;
            }

            return true;
        }

        private void ShowErrorDialog(string message, string caption, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                Invoke(new Action(() =>
                {
                    using (var messageBox = new DarkMessageBox(message, caption, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                    {
                        messageBox.ShowDialog();
                    }
                }));
            }
        }

        private void Upload(CancellationToken token)
        {
            if (!CheckApiKey(token))
            {
                Finish(true);
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

            foreach (var file in _filesToUpload)
            {
                if (token.IsCancellationRequested)
                    break;

                UploadFile(file, token);
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
                    var info = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(info);
                }
                catch (Win32Exception ex)
                {
                    Invoke(new Action(() => DisplayError($"Failed to open URL. Error: {ex.Message}")));
                }
            }
        }

        private class ScanResult
        {
            public string Sha256 { get; set; }
            public string ScanId { get; set; }
        }

        private void ProcessScanResponse(string scanContent, string fileName)
        {
            try
            {
                dynamic scanJson = JsonConvert.DeserializeObject(scanContent);
                var result = new ScanResult
                {
                    Sha256 = scanJson.sha256.ToString(),
                    ScanId = scanJson.scan_id.ToString()
                };

                var safeSha256 = Uri.EscapeDataString(result.Sha256);
                var safeScanId = Uri.EscapeDataString(result.ScanId);

                var scanLink = $"https://www.virustotal.com/gui/file/{safeSha256}/detection/{safeScanId}";
                OpenUrlSafe(scanLink);
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}")));
            }
        }

        private void UploadNewFile(string fileName, string fullPath, CancellationToken token)
        {
            ChangeStatus($"Uploading {fileName}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _settings.ApiKey);
            scanRequest.AddFile("file", fullPath);

            if (token.IsCancellationRequested) return;

            var scanResponse = _client.Execute(scanRequest);
            if (token.IsCancellationRequested) return;

            ProcessScanResponse(scanResponse.Content, fileName);
        }

        private void UploadFile(string fullPath, CancellationToken token)
        {
            if (!File.Exists(fullPath))
            {
                Invoke(new Action(() => DisplayError($"File {fullPath} does not exist.")));
                return;
            }

            var fileName = Path.GetFileName(fullPath);
            ChangeStatus($"Checking {fileName}...");
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            reportRequest.AddParameter("resource", Utils.GetSHA256(fullPath));

            string fileMd5 = (!_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedMd5)) ? _cachedMd5 : Utils.GetMD5(fullPath);
            reportRequest.AddParameter("resource", fileMd5);

            if (token.IsCancellationRequested) return;

            var reportResponse = _client.Execute(reportRequest);
            if (token.IsCancellationRequested) return;

            var reportContent = reportResponse.Content;
            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                OpenUrlSafe(reportLink);
            }
            catch (RuntimeBinderException)
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                UploadNewFile(fileName, fullPath, token);
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

            _uploadTask = Task.Run(() => Upload(token), token);
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
                _cachedMd5 = Utils.GetMD5(_path);
                mdTextbox.Text = _cachedMd5;
                shaTextbox.Text = Utils.GetSHA1(_path);
                sha2Textbox.Text = Utils.GetSHA256(_path);
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
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

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