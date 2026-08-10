using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace uploader
{
    internal class Utils
    {
        public static string GetMD5(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hashBytes = md5.ComputeHash(stream);
                    var sb = new StringBuilder();
                    foreach (var t in hashBytes)
                    {
                        sb.Append(t.ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
        }

        public static string GetSHA256(string file)
        {
            using (var sha = SHA256.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }

        public static string GetSHA1(string file)
        {
            using (var sha = SHA1.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }

        public static (string md5, string sha1, string sha256) GetHashes(string file)
        {
            using (var md5 = MD5.Create())
            using (var sha1 = SHA1.Create())
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(file))
            {
                byte[] buffer = new byte[81920]; // 80KB buffer is optimal for FileStream
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

                var sbMd5 = new StringBuilder(32);
                foreach (var t in md5.Hash ?? Array.Empty<byte>())
                {
                    sbMd5.Append(t.ToString("X2"));
                }
                string md5String = sbMd5.ToString();

                string sha1String = BitConverter.ToString(sha1.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty);
                string sha256String = BitConverter.ToString(sha256.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty);

                return (md5String, sha1String, sha256String);
            }
        }
    }
}
