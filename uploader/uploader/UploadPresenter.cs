using System;
using System.Threading;
using System.Threading.Tasks;

namespace uploader
{
    public interface IUploadView
    {
        void ChangeStatus(string text);
        void DisplayError(string error);
        void Finish(bool resetText);
        void ShowApiKeyMissingError();
        void ShowApiKeyInvalidLengthError();
    }

    public class UploadPresenter
    {
        private readonly IUploadView _view;
        private readonly Settings _settings;
        private readonly Func<string, IVirusTotalClient> _clientFactory;

        public UploadPresenter(IUploadView view, Settings settings, Func<string, IVirusTotalClient> clientFactory)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task UploadAsync(UploadJob job, CancellationToken token)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                _view.ShowApiKeyMissingError();
                return;
            }

            if (_settings.ApiKey.Length != 64)
            {
                _view.ShowApiKeyInvalidLengthError();
                return;
            }

            _view.ChangeStatus(LocalizationHelper.Base.Message_Init);

            var client = _clientFactory(_settings.ApiKey);
            client.OnStatusChanged = _view.ChangeStatus;
            client.OnError = _view.DisplayError;

            try
            {
                await client.UploadAsync(job, token);
            }
            catch (OperationCanceledException)
            {
                // Cancellation was requested, do nothing special here.
            }

            _view.Finish(true);
        }
    }
}
