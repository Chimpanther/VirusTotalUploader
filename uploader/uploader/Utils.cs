using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace uploader
{
    public class FileHashesResult
    {
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
        public string SHA256 { get; set; }
    }

    public static class Utils
    {
        public static FileHashesResult GetHashes(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var md5 = MD5.Create())
            using (var sha1 = SHA1.Create())
            using (var sha256 = SHA256.Create())
            {
                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                md5.TransformFinalBlock(buffer, 0, 0);
                sha1.TransformFinalBlock(buffer, 0, 0);
                sha256.TransformFinalBlock(buffer, 0, 0);

                return new FileHashesResult
                {
                    MD5 = BitConverter.ToString(md5.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty).ToLowerInvariant(),
                    SHA1 = BitConverter.ToString(sha1.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty).ToLowerInvariant(),
                    SHA256 = BitConverter.ToString(sha256.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty).ToLowerInvariant()
                };
            }
        }

        public static string GetMD5(string file)
        {
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        public static string GetSHA256(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA256.Create())
            {
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        public static string GetSHA1(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA1.Create())
            {
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
