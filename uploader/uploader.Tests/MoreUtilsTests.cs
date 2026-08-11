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

                string sha384 = Utils.GetSHA384(tempFile);
                string sha512 = Utils.GetSHA512(tempFile);
                string sha256 = Utils.GetSHA256(tempFile);

                Assert.Equal("FDBD8E75A67F29F701A4E040385E2E23986303EA10239211AF907FCBB83578B3E417CB71CE646EFD0819DD8C088DE1BD", sha384);
                Assert.Equal("309ECC489C12D6EB4CC40F50C902F2B4D0ED77EE511A7C7A9BCD3CA86D4CD86F989DD35BC5FF499670DA34255B45B0CFD830E81F605DCF7DC5542E93AE9CD76F", sha512);
                Assert.Equal("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9", sha256);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
