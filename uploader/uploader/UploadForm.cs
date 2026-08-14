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
        private CancellationTokenSource _uploadCts;
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

        private void Upload(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                this.Invoke(new Action(() => {
                    using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_NoApiKey, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                    {
                        messageBox.ShowDialog();
                    }
                }));
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                this.Invoke(new Action(() => {
                    using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_InvalidLength, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                    {
                        messageBox.ShowDialog();
                    }
                }));
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
                if (token.IsCancellationRequested) return;
                UploadFile(file, token);
            }

            if (token.IsCancellationRequested) return;
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
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }
            }
        }

        private void UploadFile(string fullPath, CancellationToken token)
        {
            var fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
            {
                DisplayError($"File {fullPath} does not exist.");
                return;
            }

            ChangeStatus($"Checking {fileInfo.Name}...");

            if (CheckFileReport(fileInfo))
            {
                return;
            }

            if (token.IsCancellationRequested) return;

            ScanNewFile(fileInfo);
        }

        private bool CheckFileReport(FileInfo fileInfo)
        {
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            reportRequest.AddParameter("resource", Utils.GetSHA256(fileInfo.FullName));

            string fileMd5 = (!_isFolder && fileInfo.FullName == _path && !string.IsNullOrEmpty(_cachedMd5)) ? _cachedMd5 : Utils.GetMD5(fileInfo.FullName);
            reportRequest.AddParameter("resource", fileMd5);

            var reportResponse = _client.Execute(reportRequest);
            if (reportResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                DisplayError($"VT API returned {reportResponse.StatusCode}");
                return true; // Stop processing on error
            }

            var reportContent = reportResponse.Content;
            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                OpenUrlSafe(reportLink);
                return true;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        private void ScanNewFile(FileInfo fileInfo)
        {
            ChangeStatus($"Uploading {fileInfo.Name}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _settings.ApiKey);
            scanRequest.AddFile("file", fileInfo.FullName);

            var scanResponse = _client.Execute(scanRequest);
            if (scanResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                DisplayError($"VT API returned {scanResponse.StatusCode}");
                return;
            }

            var scanContent = scanResponse.Content;
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
                DisplayError($"Failed to get link for {fileInfo.Name}. Error: {ex.Message}");
            }
        }

        private void StartUploadThread()
        {
            if (_uploadCts != null && !_uploadCts.IsCancellationRequested)
            {
                _uploadCts.Cancel();
                uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
                return;
            }
            uploadButton.Text = LocalizationHelper.Base.UploadForm_Cancel;

            _uploadCts = new CancellationTokenSource();
            var token = _uploadCts.Token;

            Task.Run(() => Upload(token), token);
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
            if (_uploadCts != null && !_uploadCts.IsCancellationRequested)
            {
                _uploadCts.Cancel();
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