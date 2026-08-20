using System;
using System.IO;
using System.Security.Cryptography;

namespace uploader
{
    public class FileHashesResult
    {
        public string MD5 { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
    }

    public static class Utils
    {
        public static FileHashesResult GetHashes(string file)
        {
            using (var md5 = MD5.Create())
            using (var sha256 = SHA256.Create())
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                }

                md5.TransformFinalBlock(new byte[0], 0, 0);
                sha256.TransformFinalBlock(new byte[0], 0, 0);

                return new FileHashesResult
                {
                    MD5 = BitConverter.ToString(md5.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty),
                    SHA256 = BitConverter.ToString(sha256.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty)
                };
            }
        }
    }
}
