import os

def read_file(path):
    with open(path, 'r', encoding='utf-8-sig') as f:
        return f.read()

def write_file(path, content):
    with open(path, 'w', encoding='utf-8-sig') as f:
        content = content.replace('\r\n', '\n').replace('\n', '\r\n')
        f.write(content)

content = read_file('uploader/uploader/UploadForm.cs')

old_methods = """        private void UploadFile(string fullPath, CancellationToken token)
        {
            if (!File.Exists(fullPath))
            {
                DisplayError($"File {fullPath} does not exist.");
                return;
            }

            var fileName = Path.GetFileName(fullPath);
            ChangeStatus($"Checking {fileName}...");

            if (CheckFileReport(fullPath, fileName))
            {
                return;
            }

            if (token.IsCancellationRequested) return;

            ScanNewFile(fullPath, fileName);
        }

        private bool CheckFileReport(string fullPath, string fileName)
        {
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            reportRequest.AddParameter("resource", Utils.GetSHA256(fullPath));

            string fileMd5 = (!_isFolder && fullPath == _path && !string.IsNullOrEmpty(_cachedMd5)) ? _cachedMd5 : Utils.GetMD5(fullPath);
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

        private void ScanNewFile(string fullPath, string fileName)
        {
            ChangeStatus($"Uploading {fileName}...");
            var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
            scanRequest.AddParameter("apikey", _settings.ApiKey);
            scanRequest.AddFile("file", fullPath);

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
                DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
            }
        }"""

new_methods = """        private void UploadFile(string fullPath, CancellationToken token)
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
        }"""

content = content.replace(old_methods, new_methods)
write_file('uploader/uploader/UploadForm.cs', content)
