using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace uploader
{
    public class FileHashesResult
    {
        public string MD5 = "";
        public string SHA1 = "";
        public string SHA256 = "";
    }

    public static class Utils
    {
        public static FileHashesResult GetFileHashes(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var md5 = MD5.Create())
            using (var sha1 = SHA1.Create())
            using (var sha256 = SHA256.Create())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                }

                md5.TransformFinalBlock(buffer, 0, 0);
                sha1.TransformFinalBlock(buffer, 0, 0);
                sha256.TransformFinalBlock(buffer, 0, 0);

                return new FileHashesResult
                {
                    MD5 = BitConverter.ToString(md5.Hash != null ? md5.Hash : new byte[0]).Replace("-", string.Empty),
                    SHA1 = BitConverter.ToString(sha1.Hash != null ? sha1.Hash : new byte[0]).Replace("-", string.Empty),
                    SHA256 = BitConverter.ToString(sha256.Hash != null ? sha256.Hash : new byte[0]).Replace("-", string.Empty)
                };
            }
        }

        public static string GetMD5(string file)
        {
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }
        }

        public static string GetSHA256(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA256.Create())
            {
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        public static string GetSHA1(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA1.Create())
            {
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
    }
}
