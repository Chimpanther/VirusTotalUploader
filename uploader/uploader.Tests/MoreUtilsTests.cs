using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class MoreUtilsTests
    {
        [Fact]
        public void GetHashes_ValidFile_ReturnsCorrectHashes()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "hello world");

                string md5 = Utils.GetMD5(tempFile);
                string sha1 = Utils.GetSHA1(tempFile);
                string sha256 = Utils.GetSHA256(tempFile);

                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", md5);
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", sha1);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", sha256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
