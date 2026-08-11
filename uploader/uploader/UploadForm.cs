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
        private Thread _uploadThread;
        private RestClient _client;
        private bool _isFolder;
        private List<string> _filesToUpload;

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

        private void Upload()
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                DisplayError(LocalizationHelper.Base.UploadForm_NoApiKey);
                Finish(true);
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                DisplayError(LocalizationHelper.Base.UploadForm_InvalidLength);
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

            for (int i = 0; i < _filesToUpload.Count; i += 4)
            {
                var chunk = _filesToUpload.Skip(i).Take(4).ToList();
                UploadFiles(chunk);
            }

            Finish(true);
        }

        private void OpenUrlSafe(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return;
            }

            try
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    Process.Start(url);
                }
            }
            catch (Exception ex)
            {
                // Process.Start can throw e.g. Win32Exception if there is no default handler for HTTP/HTTPS URLs.
                // Silently ignoring is safer than crashing the background thread.
                Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }

        private void UploadFile(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                DisplayError($"File {fullPath} does not exist.");
                return;
            }

        private class VTReport
        {
            [JsonProperty("permalink")]
            public string Permalink { get; set; }
        }

        private class VTScanResponse
        {
            [JsonProperty("sha256")]
            public string Sha256 { get; set; }
            [JsonProperty("scan_id")]
            public string ScanId { get; set; }
        }

        private void UploadFiles(List<string> files)
        {
            var validFiles = files.Where(f =>
            {
                if (!File.Exists(f))
                {
                    DisplayError($"File {f} does not exist.");
                    return false;
                }
                return true;
            }).ToList();

            if (validFiles.Count == 0) return;

            ChangeStatus($"Checking {validFiles.Count} files...");
            var hashes = validFiles.Select(f => Utils.GetMD5(f)).ToList();
            var resourceString = string.Join(",", hashes);

            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            reportRequest.AddParameter("resource", Utils.GetSHA256(fullPath));
            reportRequest.AddParameter("resource", resourceString);

            var reportResponse = _client.Execute(reportRequest);
            var reports = ParseReports(reportResponse.Content);

            for (int j = 0; j < validFiles.Count; j++)
            {
                var file = validFiles[j];
                var report = reports != null && reports.Count > j ? reports[j] : null;
                ProcessReport(file, report);
            }
        }

        private List<VTReport> ParseReports(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            try
            {
                var reportLink = reportJson.permalink.ToString();
                OpenUrlSafe(reportLink);
                var parsedToken = Newtonsoft.Json.Linq.JToken.Parse(content);
                var reports = new List<VTReport>();
                if (parsedToken is Newtonsoft.Json.Linq.JArray)
                {
                    foreach (var r in parsedToken)
                    {
                        reports.Add(r.ToObject<VTReport>());
                    }
                }
                else
                {
                    reports.Add(parsedToken.ToObject<VTReport>());
                }
                return reports;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }

        private void ProcessReport(string file, VTReport report)
        {
            bool hasPermalink = false;
            if (report != null && !string.IsNullOrEmpty(report.Permalink))
            {
                try
                {
                    Process.Start(report.Permalink);
                    hasPermalink = true;
                }
                catch (Exception)
                {
                }
            }

                    var scanLink = $"https://www.virustotal.com/gui/file/{sha256}/detection/{scanId}";
                    OpenUrlSafe(scanLink);
            if (!hasPermalink)
            {
                ScanFile(file);
            }
        }

        private void ScanFile(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath);
            ChangeStatus($"Uploading {fileName}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _settings.ApiKey);
            scanRequest.AddFile("file", fullPath);

            var scanResponse = _client.Execute(scanRequest);
            var scanContent = scanResponse.Content;

            if (string.IsNullOrEmpty(scanContent))
            {
                DisplayError($"Failed to get link for {fileName}. Empty response from VirusTotal.");
                return;
            }

            VTScanResponse scanJson;
            try
            {
                scanJson = JsonConvert.DeserializeObject<VTScanResponse>(scanContent);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                return;
            }

            try
            {
                if (scanJson != null && !string.IsNullOrEmpty(scanJson.Sha256) && !string.IsNullOrEmpty(scanJson.ScanId))
                {
                    var scanLink = $"https://www.virustotal.com/gui/file/{scanJson.Sha256}/detection/{scanJson.ScanId}";
                    Process.Start(scanLink);
                }
                else
                {
                    DisplayError($"Failed to get link for {fileName}. Missing required properties in response.");
                }
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
            }
        }

        private void StartUploadThread()
        {
            if (_uploadThread != null && _uploadThread.IsAlive)
            {
                _uploadThread.Abort();
                uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
                return;
            }
            uploadButton.Text = LocalizationHelper.Base.UploadForm_Cancel;

            _uploadThread = new Thread(Upload);
            _uploadThread.Start();
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
                mdTextbox.Text = Utils.GetMD5(_path);
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