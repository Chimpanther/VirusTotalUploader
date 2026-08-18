using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uploader;

namespace uploader.Tests
{
    [TestClass]
    public class UtilsTests
    {
        private string testFile = "test_hash.txt";

        [TestInitialize]
        public void Setup()
        {
            File.WriteAllText(testFile, "test content");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }

        [TestMethod]
        public void Test_MD5()
        {
            var hash = Utils.GetMD5(testFile);
            Assert.AreEqual("9473FDD0D880A43C21B7778D34872157", hash);
        }

        [TestMethod]
        public void Test_SHA1()
        {
            var hash = Utils.GetSHA1(testFile);
            Assert.AreEqual("1EEBDF4FDC9FC7BF283031B93F9AEF3338DE9052", hash);
        }

        [TestMethod]
        public void Test_SHA256()
        {
            var hash = Utils.GetSHA256(testFile);
            Assert.AreEqual("6AE8A75555209FD6C44157C0AED8016E763FF435A19CF186F76863140143FF72", hash);
        }
    }
}
