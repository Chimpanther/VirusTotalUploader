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
        private bool _isFolder;
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
            this.InvokeIfRequired(() => statusLabel.Text = text);
        }

        private void Finish(bool resetText)
        {
            this.InvokeIfRequired(() =>
            {
                if (resetText)
                {
                    ChangeStatus(LocalizationHelper.Base.Message_Idle);
                }

                uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
            });
        }

        private void CloseWindow()
        {
            this.InvokeIfRequired(() => this.Close());
        }

        private void DisplayError(string error)
        {
            this.InvokeIfRequired(() =>
            {
                using (var messageBox = new DarkMessageBox(error, LocalizationHelper.Base.UploadForm_Error, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                {
                    messageBox.ShowDialog();
                }
            });
        }

        private async Task UploadAsync(CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                this.InvokeIfRequired(() =>
                {
                    using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_NoApiKey, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                    {
                        messageBox.ShowDialog();
                    }
                });
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                this.InvokeIfRequired(() =>
                {
                    using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.UploadForm_InvalidLength, LocalizationHelper.Base.UploadForm_InvalidKey, DarkMessageBoxIcon.Error, DarkDialogButton.Ok))
                    {
                        messageBox.ShowDialog();
                    }
                });
                return;
            }

            ChangeStatus(LocalizationHelper.Base.Message_Init);

            var client = new VirusTotalClient(_settings.ApiKey);
            client.OnStatusChanged = ChangeStatus;
            client.OnError = DisplayError;

            try
            {
                var job = new UploadJob { InitialPath = _path, IsFolder = _isFolder, CachedSha256 = _cachedSha256 };
                await client.UploadAsync(job, token);
            }
            catch (OperationCanceledException)
            {
                // Cancellation was requested, do nothing special here.
            }

            Finish(true);
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

        private async void UploadForm_Load(object sender, EventArgs e)
        {
            settingsGroup.Text = LocalizationHelper.Base.UploadForm_Info;
            uploadButton.Text = LocalizationHelper.Base.UploadForm_Upload;
            statusLabel.Text = LocalizationHelper.Base.Message_Idle;

            if (_isFolder)
            {
                sha2Textbox.Text = "N/A (Folder)";
            }
            else
            {
                sha2Textbox.Text = "Calculating...";
                uploadButton.Enabled = false;

                _cachedSha256 = await Task.Run(() => Utils.GetSHA256(_path));
                sha2Textbox.Text = _cachedSha256;
                uploadButton.Enabled = true;
            }

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
