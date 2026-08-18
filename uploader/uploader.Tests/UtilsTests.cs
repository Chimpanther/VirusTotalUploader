using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetMD5_NonExistentFile_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => Utils.GetMD5("this_file_does_not_exist.txt"));
        }

        [Fact]
        public void GetMD5_ValidFile_ReturnsCorrectHash()
        {
            TestHashFunction("hello world", Utils.GetMD5, "5EB63BBBE01EEED093CB22BB8F5ACDC3");
        }

        private void TestHashFunction(string content, Func<string, string> hashFunc, string expectedHash)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, content);
                string hash = hashFunc(tempFile);
                Assert.Equal(expectedHash, hash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
