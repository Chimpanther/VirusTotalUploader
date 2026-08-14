using Xunit;
using System.IO;
using uploader;

namespace uploader.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetSHA1_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, System.Text.Encoding.ASCII.GetBytes("test"));
                var result = Utils.GetSHA1(tempFile);
                Assert.Equal("A94A8FE5CCB19BA61C4C0873D391E987982FBBD3", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetMD5_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, System.Text.Encoding.ASCII.GetBytes("test"));
                var result = Utils.GetMD5(tempFile);
                Assert.Equal("098F6BCD4621D373CADE4E832627B4F6", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetSHA256_ReturnsCorrectHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile, System.Text.Encoding.ASCII.GetBytes("test"));
                var result = Utils.GetSHA256(tempFile);
                Assert.Equal("9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08", result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
