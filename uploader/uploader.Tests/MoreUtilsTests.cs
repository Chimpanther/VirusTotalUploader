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

                var hashes = Utils.GetHashes(tempFile);

                Assert.Equal("5EB63BBBE01EEED093CB22BB8F5ACDC3", hashes.md5);
                Assert.Equal("2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED", hashes.sha1);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", hashes.sha256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
