using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Xunit;
using uploader;
using System.Reflection;

namespace uploader.Tests
{
    public class MockUploadCallbacks : IUploadCallbacks
    {
        public List<string> StatusChanges = new List<string>();
        public List<string> Errors = new List<string>();
        public List<string> OpenedUrls = new List<string>();
        public bool Finished;
        public bool FinishedResetText;

        public void ChangeStatus(string text) => StatusChanges.Add(text);
        public void DisplayError(string error) => Errors.Add(error);
        public void Finish(bool resetText)
        {
            Finished = true;
            FinishedResetText = resetText;
        }
        public void OpenUrlSafe(string url) => OpenedUrls.Add(url);
    }

    public class MockRestHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> HandlerFunc;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(HandlerFunc(request));
        }
    }

    public class UploadLogicTests : IDisposable
    {
        private string _tempDir;
        private string _testFilePath;
        private string _appDataDir;
        private string _settingsPath;

        private Settings GetSetupSettings()
        {
            var settings = Settings.LoadSettings();
            LocalizationHelper.Base = new LocalizationBase();
            return settings;
        }

        private void ResetSettingsCache()
        {
            var field = typeof(Settings).GetField("_cachedSettings", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, null);
            }
        }


        private RestClient CreateMockClient(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            var handler = new MockRestHandler { HandlerFunc = handlerFunc };
            return new RestClient(new RestClientOptions("https://api.test.com") { ConfigureMessageHandler = _ => handler });
        }

        public UploadLogicTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _testFilePath = Path.Combine(_tempDir, "test.txt");
            File.WriteAllText(_testFilePath, "test content");

            _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            _settingsPath = Path.Combine(_appDataDir, "vtu_settings.json");

            if (File.Exists(_settingsPath))
            {
                File.Move(_settingsPath, _settingsPath + ".bak");
            }

            ResetSettingsCache();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }

            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }
            if (File.Exists(_settingsPath + ".bak"))
            {
                File.Move(_settingsPath + ".bak", _settingsPath);
            }

            ResetSettingsCache();
        }

        [Fact]
        public async Task UploadAsync_NoApiKey_DisplaysError()
        {
            var settings = GetSetupSettings();
            var callbacks = new MockUploadCallbacks();
            var logic = new UploadLogic(new UploadLogicConfig { Settings = settings, IsFolder = false, Path = _testFilePath }, callbacks);

            await logic.UploadAsync(CancellationToken.None);

            Assert.Contains(callbacks.Errors, e => e.Contains("API key"));
        }

        [Fact]
        public async Task UploadAsync_InvalidApiKeyLength_DisplaysError()
        {
            var settings = GetSetupSettings();
            settings.ApiKey = "short";
            Settings.SaveSettings(settings);
            var callbacks = new MockUploadCallbacks();
            var logic = new UploadLogic(new UploadLogicConfig { Settings = settings, IsFolder = false, Path = _testFilePath }, callbacks);

            await logic.UploadAsync(CancellationToken.None);

            Assert.Contains(callbacks.Errors, e => e.Contains("length"));
        }

        [Fact]
        public async Task UploadAsync_ValidFile_UploadsAndOpensUrl()
        {
            var settings = GetSetupSettings();
            settings.ApiKey = new string('A', 64);
            Settings.SaveSettings(settings);
            var callbacks = new MockUploadCallbacks();

            var client = CreateMockClient(req =>
            {
                var content = new StringContent("{\"permalink\": \"https://www.virustotal.com/gui/file/12345\"}");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            });

            var logic = new UploadLogic(new UploadLogicConfig { Settings = settings, IsFolder = false, Path = _testFilePath }, callbacks, client);

            await logic.UploadAsync(CancellationToken.None);

            Assert.Contains(callbacks.OpenedUrls, u => u == "https://www.virustotal.com/gui/file/12345");
            Assert.True(callbacks.Finished);
            Assert.True(callbacks.FinishedResetText);
        }

        [Fact]
        public async Task UploadAsync_ValidFile_ScanFallback()
        {
            var settings = GetSetupSettings();
            settings.ApiKey = new string('A', 64);
            Settings.SaveSettings(settings);
            var callbacks = new MockUploadCallbacks();

            var client = CreateMockClient(req =>
            {
                if (req.RequestUri.ToString().Contains("report")) {
                    var content = new StringContent("{}"); // No permalink -> throws RuntimeBinderException
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                } else {
                    var content = new StringContent("{\"sha256\": \"testsha\", \"scan_id\": \"testid\"}");
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                }
            });

            var logic = new UploadLogic(new UploadLogicConfig { Settings = settings, IsFolder = false, Path = _testFilePath }, callbacks, client);

            await logic.UploadAsync(CancellationToken.None);

            Assert.Contains(callbacks.OpenedUrls, u => u == "https://www.virustotal.com/gui/file/testsha/detection/testid");
            Assert.True(callbacks.Finished);
        }
    }
}
