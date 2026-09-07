using System;
using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class UtilsTests
    {
        private static void AssertThrowsFileNotFound(Func<string, string> hashFunction)
        {
            var missingPath = Path.Combine(Path.GetTempPath(), "vtu-missing-" + Guid.NewGuid() + ".txt");
            Assert.Throws<FileNotFoundException>(() => hashFunction(missingPath));
        }

        private static void AssertThrowsArgumentNull(Func<string, string> hashFunction)
        {
            Assert.Throws<ArgumentNullException>(() => hashFunction(null!));
        }

        private static void AssertFileHash(Func<string, string> hashFunction, string content, string expectedHash)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                if (content != null)
                {
                    File.WriteAllText(tempFile, content);
                }
                var hash = hashFunction(tempFile);

                Assert.Equal(expectedHash, hash, ignoreCase: true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetSHA256_NonExistentFile_ThrowsFileNotFoundException()
        {
            AssertThrowsFileNotFound(Utils.GetSHA256);
        }

        [Fact]
        public void GetSHA256_NullFile_ThrowsArgumentNullException()
        {
            AssertThrowsArgumentNull(Utils.GetSHA256);
        }

        [Fact]
        public void GetSHA256_ValidFile_ReturnsCorrectHash()
        {
            AssertFileHash(Utils.GetSHA256, "hello world", "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9");
        }

        [Fact]
        public void GetSHA256_EmptyFile_ReturnsCorrectHash()
        {
            AssertFileHash(Utils.GetSHA256, null, "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
        }

        [Fact]
        public void OpenUrlSafe_ValidUrl_ExecutesProcessStart()
        {
            var originalProcessStart = Utils.ProcessStart;
            bool processStarted = false;
            try
            {
                Utils.ProcessStart = psi =>
                {
                    processStarted = true;
                    Assert.Equal("https://developers.virustotal.com/reference", psi.FileName);
                    Assert.True(psi.UseShellExecute);
                };

                Utils.OpenUrlSafe("https://developers.virustotal.com/reference");

                Assert.True(processStarted);
            }
            finally
            {
                Utils.ProcessStart = originalProcessStart;
            }
        }

        [Fact]
        public void OpenUrlSafe_ProcessStartThrows_InvokesOnError()
        {
            var originalProcessStart = Utils.ProcessStart;
            Exception caughtException = null;
            try
            {
                Utils.ProcessStart = psi =>
                {
                    throw new System.ComponentModel.Win32Exception("No application is associated with the specified file for this operation");
                };

                Utils.OpenUrlSafe("https://developers.virustotal.com/reference", ex =>
                {
                    caughtException = ex;
                });

                Assert.NotNull(caughtException);
                Assert.IsType<System.ComponentModel.Win32Exception>(caughtException);
            }
            finally
            {
                Utils.ProcessStart = originalProcessStart;
            }
        }
    }
}
