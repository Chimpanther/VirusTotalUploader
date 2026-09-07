using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace uploader.Tests
{
    public class MockUploadView : IUploadView
    {
        public bool ShowApiKeyMissingErrorCalled { get; private set; }
        public bool ShowApiKeyInvalidLengthErrorCalled { get; private set; }
        public bool ChangeStatusCalled { get; private set; }
        public bool FinishCalled { get; private set; }
        public string LastStatus { get; private set; }

        public void ChangeStatus(string text)
        {
            ChangeStatusCalled = true;
            LastStatus = text;
        }

        public void DisplayError(string error)
        {
        }

        public void Finish(bool resetText)
        {
            FinishCalled = true;
        }

        public void ShowApiKeyMissingError()
        {
            ShowApiKeyMissingErrorCalled = true;
        }

        public void ShowApiKeyInvalidLengthError()
        {
            ShowApiKeyInvalidLengthErrorCalled = true;
        }
    }

    public class MockVirusTotalClient : IVirusTotalClient
    {
        public Action<string> OnStatusChanged { get; set; }
        public Action<string> OnError { get; set; }
        public bool UploadAsyncCalled { get; private set; }

        public Task UploadAsync(UploadJob job, CancellationToken token)
        {
            UploadAsyncCalled = true;
            return Task.CompletedTask;
        }
    }
}

namespace uploader.Tests
{
    public class UploadPresenterTests
    {
        [Fact]
        public async Task UploadAsync_EmptyApiKey_ShowsMissingError()
        {
            var view = new MockUploadView();
            var settings = new Settings { ApiKey = "" };
            var presenter = new UploadPresenter(view, settings, apiKey => new MockVirusTotalClient());

            await presenter.UploadAsync(new UploadJob(), CancellationToken.None);

            Assert.True(view.ShowApiKeyMissingErrorCalled);
            Assert.False(view.ShowApiKeyInvalidLengthErrorCalled);
        }

        [Fact]
        public async Task UploadAsync_NullApiKey_ShowsMissingError()
        {
            var view = new MockUploadView();
            var settings = new Settings { ApiKey = null };
            var presenter = new UploadPresenter(view, settings, apiKey => new MockVirusTotalClient());

            await presenter.UploadAsync(new UploadJob(), CancellationToken.None);

            Assert.True(view.ShowApiKeyMissingErrorCalled);
            Assert.False(view.ShowApiKeyInvalidLengthErrorCalled);
        }

        [Fact]
        public async Task UploadAsync_InvalidApiKeyLength_ShowsInvalidLengthError()
        {
            var view = new MockUploadView();
            var settings = new Settings { ApiKey = "short_key" };
            var presenter = new UploadPresenter(view, settings, apiKey => new MockVirusTotalClient());

            await presenter.UploadAsync(new UploadJob(), CancellationToken.None);

            Assert.False(view.ShowApiKeyMissingErrorCalled);
            Assert.True(view.ShowApiKeyInvalidLengthErrorCalled);
        }

        [Fact]
        public async Task UploadAsync_ValidApiKey_StartsUploadAndFinishes()
        {
            var view = new MockUploadView();
            var settings = new Settings { ApiKey = new string('a', 64) };
            var mockClient = new MockVirusTotalClient();
            var presenter = new UploadPresenter(view, settings, apiKey => mockClient);

            // Ensure localization is loaded to avoid null ref in tests
            LocalizationHelper.Base = new LocalizationBase { Message_Init = "Initializing..." };

            await presenter.UploadAsync(new UploadJob(), CancellationToken.None);

            Assert.True(view.ChangeStatusCalled);
            Assert.Equal("Initializing...", view.LastStatus);
            Assert.True(mockClient.UploadAsyncCalled);
            Assert.True(view.FinishCalled);
        }

        [Fact]
        public async Task UploadAsync_TaskCanceled_CatchesExceptionAndFinishes()
        {
            var view = new MockUploadView();
            var settings = new Settings { ApiKey = new string('a', 64) };

            var presenter = new UploadPresenter(view, settings, apiKey => new CanceledMockVirusTotalClient());

            LocalizationHelper.Base = new LocalizationBase { Message_Init = "Initializing..." };

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await presenter.UploadAsync(new UploadJob(), tokenSource.Token);

            Assert.True(view.FinishCalled);
        }
    }

    public class CanceledMockVirusTotalClient : IVirusTotalClient
    {
        public Action<string> OnStatusChanged { get; set; }
        public Action<string> OnError { get; set; }

        public Task UploadAsync(UploadJob job, CancellationToken token)
        {
            throw new OperationCanceledException();
        }
    }
}
