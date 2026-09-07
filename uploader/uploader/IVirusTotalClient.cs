using System;
using System.Threading;
using System.Threading.Tasks;

namespace uploader
{
    public interface IVirusTotalClient
    {
        Action<string> OnStatusChanged { get; set; }
        Action<string> OnError { get; set; }
        Task UploadAsync(UploadJob job, CancellationToken token);
    }
}
