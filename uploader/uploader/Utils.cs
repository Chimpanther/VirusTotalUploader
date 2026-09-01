using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace uploader
{
    public static class Utils
    {
        public static string GetSHA256(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA256.Create())
            {
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public static async Task<string> GetSHA256Async(string file, CancellationToken token = default)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, FileOptions.Asynchronous))
            using (var sha = SHA256.Create())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                {
                    sha.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }
                sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return BitConverter.ToString(sha.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty);
            }
        }
    }
}
