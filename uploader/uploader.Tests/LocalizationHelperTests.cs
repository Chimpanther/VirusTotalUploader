using System.IO;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests
    {
        [Fact]
        public void GetLanguages_NoLocalFolder_ReturnsArrayWithEmptyString()
        {
            // Arrange
            string localFolder = "local";
            string backupFolder = "local_backup_" + System.Guid.NewGuid().ToString();
            bool folderExisted = Directory.Exists(localFolder);

            try
            {
                if (folderExisted)
                {
                    Directory.Move(localFolder, backupFolder);
                }

                // Act
                string[] result = LocalizationHelper.GetLanguages();

                // Assert
                Assert.Single(result);
                Assert.Equal("", result[0]);
            }
            finally
            {
                if (folderExisted)
                {
                    if (Directory.Exists(localFolder))
                    {
                        Directory.Delete(localFolder, true);
                    }
                    Directory.Move(backupFolder, localFolder);
                }
            }
        }
    }
}
