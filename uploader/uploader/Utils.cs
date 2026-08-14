using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
<<<<<<< HEAD
            using (var sha = SHA256.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
=======
            using (var stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
>>>>>>> origin/master
            }
        }

        public static string GetSHA1(string file)
        {
<<<<<<< HEAD
            using (var sha = SHA1.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
=======
            using (var stream = File.OpenRead(file))
            {
                var sha = new SHA1Managed();
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
>>>>>>> origin/master
            }
        }
    }
}
