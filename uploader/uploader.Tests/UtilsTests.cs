using NUnit.Framework;
using System;
using System.IO;
using uploader;

namespace uploader.Tests
{
    [TestFixture]
    public class UtilsTests
    {
        private string testFilePath;

        [SetUp]
        public void Setup()
        {
            testFilePath = Path.GetTempFileName();
            File.WriteAllText(testFilePath, "Hello, World!");
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Test]
        public void GetSHA256_ReturnsExpectedHash()
        {
            // Expected SHA256 for "Hello, World!" is dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f
            string expectedHash = "DFFD6021BB2BD5B0AF676290809EC3A53191DD81C7F70A4B28688A362182986F";
            string actualHash = Utils.GetSHA256(testFilePath);

            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }

        [Test]
        public void GetSHA256_ThrowsExceptionForNonExistentFile()
        {
            Assert.Throws<FileNotFoundException>(() => Utils.GetSHA256("non_existent_file.txt"));
        }

        [Test]
        public void GetSHA256_EmptyFile()
        {
            string emptyFilePath = Path.GetTempFileName();
            try
            {
                // SHA256 for empty string is e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
                string expectedHash = "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855";
                string actualHash = Utils.GetSHA256(emptyFilePath);

                Assert.That(actualHash, Is.EqualTo(expectedHash));
            }
            finally
            {
                File.Delete(emptyFilePath);
            }
        }
    }
}
