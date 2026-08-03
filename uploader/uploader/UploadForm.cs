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
            var messageBox = new DarkMessageBox(error, LocalizationHelper.Base.UploadForm_Error, DarkMessageBoxIcon.Error, DarkDialogButton.Ok);
            messageBox.ShowDialog();
        }

        private void Upload()
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                MessageBox.Show(LocalizationHelper.Base.UploadForm_NoApiKey, LocalizationHelper.Base.UploadForm_InvalidKey, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                MessageBox.Show(LocalizationHelper.Base.UploadForm_InvalidLength, LocalizationHelper.Base.UploadForm_InvalidKey, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private List<Newtonsoft.Json.Linq.JToken> ParseReports(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            try
            {
                var parsedToken = Newtonsoft.Json.Linq.JToken.Parse(content);
                var reports = new List<Newtonsoft.Json.Linq.JToken>();
                if (parsedToken is Newtonsoft.Json.Linq.JArray)
                {
                    foreach (var r in parsedToken) reports.Add(r);
                }
                else
                {
                    reports.Add(parsedToken);
                }
                return reports;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return null;
            }
        }

        private void ProcessReport(string file, Newtonsoft.Json.Linq.JToken report)
        {
            bool hasPermalink = false;
            if (report != null)
            {
                var permalinkToken = report["permalink"];
                if (permalinkToken != null)
                {
                    var reportLink = permalinkToken.ToString();
                    Process.Start(reportLink);
                    hasPermalink = true;
                }
            }

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

            Newtonsoft.Json.Linq.JObject scanJson;
            try
            {
                scanJson = Newtonsoft.Json.Linq.JObject.Parse(scanContent);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                return;
            }

            try
            {
                var shaToken = scanJson["sha256"];
                var idToken = scanJson["scan_id"];
                if (shaToken != null && idToken != null)
                {
                    string sha256 = shaToken.ToString();
                    string scanId = idToken.ToString();

                    var scanLink = $"https://www.virustotal.com/gui/file/{sha256}/detection/{scanId}";

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