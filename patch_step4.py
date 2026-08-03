with open("uploader/uploader/UploadForm.cs", "r", encoding="utf-8") as f:
    content = f.read()

old_upload = """        private void Upload()
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

            foreach (var file in _filesToUpload)
            {
                UploadFile(file);
            }

            Finish(true);
        }"""

new_upload = """        private async Task UploadAsync(CancellationToken token)
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
        }"""

content = content.replace(old_upload, new_upload)

with open("uploader/uploader/UploadForm.cs", "w", encoding="utf-8") as f:
    f.write(content)
