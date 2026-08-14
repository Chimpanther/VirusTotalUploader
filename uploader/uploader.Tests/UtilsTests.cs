namespace uploader.Tests;

using System;
using System.IO;

[TestClass]
public sealed class UtilsTests
{
    private void AssertHash(Func<string, string> hashFunc, string content, string expectedHash)
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, content);
            string hash = hashFunc(tempFile);
            Assert.AreEqual(expectedHash, hash);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void GetMD5_ReturnsCorrectHash()
    {
        AssertHash(Utils.GetMD5, "hello world", "5EB63BBBE01EEED093CB22BB8F5ACDC3");
    }

    [TestMethod]
    public void GetMD5_EmptyFile_ReturnsCorrectHash()
    {
        AssertHash(Utils.GetMD5, "", "D41D8CD98F00B204E9800998ECF8427E");
    }

    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void GetMD5_FileNotFound_ThrowsException()
    {
        string notExistingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        Utils.GetMD5(notExistingFile);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetMD5_NullArgument_ThrowsException()
    {
        Utils.GetMD5(null!);
    }

    [TestMethod]
    public void GetSHA256_ReturnsCorrectHash()
    {
        AssertHash(Utils.GetSHA256, "hello world", "B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9");
    }

    [TestMethod]
    public void GetSHA1_ReturnsCorrectHash()
    {
        AssertHash(Utils.GetSHA1, "hello world", "2AAE6C35C94FCFB415DBE95F408B9CE91EE846ED");
    }
}
