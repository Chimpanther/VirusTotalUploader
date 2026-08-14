using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests
    {
        [Fact]
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
                Assert.NotNull(result);
                Assert.Equal(1, result.Length);
                Assert.Equal("", result[0]);
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
