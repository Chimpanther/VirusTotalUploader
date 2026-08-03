with open("uploader/uploader/UploadForm.cs", "r", encoding="utf-8") as f:
    content = f.read()

old_uploadfile = """        private void UploadFile(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                DisplayError($"File {fullPath} does not exist.");
                return;
            }

            var fileName = Path.GetFileName(fullPath);
            ChangeStatus($"Checking {fileName}...");
            var reportRequest = new RestRequest("vtapi/v2/file/report", Method.Post);
            reportRequest.AddParameter("apikey", _settings.ApiKey);
            reportRequest.AddParameter("resource", Utils.GetMD5(fullPath));

            var reportResponse = _client.Execute(reportRequest);
            var reportContent = reportResponse.Content;
            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                Process.Start(reportLink);
            }
            catch (RuntimeBinderException)
            {
                // Json does not contain permalink which means it's a new file (or the request failed)
                ChangeStatus($"Uploading {fileName}...");
                var scanRequest = new RestRequest("vtapi/v2/file/scan", Method.Post);
                scanRequest.AddParameter("apikey", _settings.ApiKey);
                scanRequest.AddFile("file", fullPath);

                var scanResponse = _client.Execute(scanRequest);
                var scanContent = scanResponse.Content;
                dynamic scanJson = JsonConvert.DeserializeObject(scanContent);

                try
                {
                    string sha256 = scanJson.sha256.ToString();
                    string scanId = scanJson.scan_id.ToString();

                    var scanLink = $"https://www.virustotal.com/gui/file/{sha256}/detection/{scanId}";

                    Process.Start(scanLink);
                }
                catch (Exception ex)
                {
                    DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
        }"""

new_uploadfile = """        private async Task UploadFileAsync(string fullPath, CancellationToken token)
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
            reportRequest.AddParameter("resource", Utils.GetMD5(fullPath));

            var reportResponse = await _client.ExecuteAsync(reportRequest, token);
            var reportContent = reportResponse.Content;

            token.ThrowIfCancellationRequested();

            dynamic reportJson = JsonConvert.DeserializeObject(reportContent);

            try
            {
                var reportLink = reportJson.permalink.ToString();
                Process.Start(reportLink);
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

                    Process.Start(scanLink);
                }
                catch (Exception ex)
                {
                    DisplayError($"Failed to get link for {fileName}. Error: {ex.Message}");
                }
            }
        }"""

content = content.replace(old_uploadfile, new_uploadfile)

with open("uploader/uploader/UploadForm.cs", "w", encoding="utf-8") as f:
    f.write(content)
