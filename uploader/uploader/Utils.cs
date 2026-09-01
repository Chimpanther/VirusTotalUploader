using System;
using System.IO;
using System.Security.Cryptography;

namespace uploader
{
    public static class Utils
    {
        public static void OpenUrlSafe(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return;
            }

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                var host = uri.Host.ToLowerInvariant();
                if (host != "virustotal.com" && host != "www.virustotal.com" && host != "developers.virustotal.com")
                {
                    System.Diagnostics.Debug.WriteLine($"Blocked attempt to open non-whitelisted URL: {url}");
                    return;
                }

                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = uri.AbsoluteUri,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    // Process.Start can throw e.g. Win32Exception if there is no default handler for HTTP/HTTPS URLs.
                    // Silently ignoring is safer than crashing the background thread.
                    System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
                }
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
    }
}
