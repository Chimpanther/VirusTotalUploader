using System;

namespace uploader
{
    public class FileHashesResult
    {
        public string MD5 { get; set; } = string.Empty;
        public string SHA1 { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
    }
}
