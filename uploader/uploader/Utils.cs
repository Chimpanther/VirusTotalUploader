using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace uploader
{
    public static class Utils
    {
        public static string RequireRooted(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var fullPath = Path.GetFullPath(path);
            if (!Path.IsPathRooted(fullPath))
                throw new ArgumentException("Path must be rooted", nameof(path));

            return fullPath;
        }

        public static void RevealInExplorer(string path)
        {
            var file = RequireRooted(path);
            var pidl = ILCreateFromPathW(file);
            if (pidl == IntPtr.Zero)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });
                return;
            }

            try
            {
                SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
            }
            finally
            {
                ILFree(pidl);
            }
        }

        public static void OpenUrlSafe(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return;
            }

            bool isHttp = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            if (isHttp)
            {
                var host = uri.Host.ToLowerInvariant();
                bool isAllowedHost = host == "virustotal.com" || host == "www.virustotal.com" || host == "developers.virustotal.com";

                if (!isAllowedHost)
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

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);
    }
}
