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
        public static string GetSHA384(string file)
        {
            using (var sha384 = SHA384.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hashBytes = sha384.ComputeHash(stream);
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

        public static string GetSHA512(string file)
        {
            using (var sha = SHA512.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }
    }
}
