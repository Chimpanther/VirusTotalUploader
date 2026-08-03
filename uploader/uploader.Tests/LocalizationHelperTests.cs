using NUnit.Framework;
using System.IO;
using uploader;

namespace uploader.Tests
{
    [TestFixture]
    public class LocalizationHelperTests
    {
        [Test]
        public void GetLanguages_DirectoryDoesNotExist_ReturnsFallbackArray()
        {
            // Ensure the directory doesn't exist
            string localFolder = "local";
            bool dirExisted = Directory.Exists(localFolder);
            if (dirExisted)
            {
                Directory.Move(localFolder, localFolder + "_temp");
            }

            try
            {
                // Act
                string[] result = LocalizationHelper.GetLanguages();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Length, Is.EqualTo(1));
                Assert.That(result[0], Is.EqualTo(""));
            }
            finally
            {
                if (dirExisted)
                {
                    Directory.Move(localFolder + "_temp", localFolder);
                }
            }
        }
    }
}
