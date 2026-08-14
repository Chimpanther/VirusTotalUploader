using System.IO;
using uploader;
using Newtonsoft.Json;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests
    {
        [Fact]
        public void Load_ValidJson_SetsBase()
        {
            // Arrange
            var testPath = "test_localization.json";
            var expectedBase = new LocalizationBase
            {
                MainForm_DragFile = "Test Drag File",
                MainForm_More = "Test More"
            };
            var json = JsonConvert.SerializeObject(expectedBase);
            File.WriteAllText(testPath, json);

            try
            {
                // Act
                LocalizationHelper.Load(testPath);

                // Assert
                Assert.NotNull(LocalizationHelper.Base);
                Assert.Equal("Test Drag File", LocalizationHelper.Base.MainForm_DragFile);
                Assert.Equal("Test More", LocalizationHelper.Base.MainForm_More);

                // Verify default values remain for properties not in JSON
                Assert.Equal("Settings", LocalizationHelper.Base.SettingsForm_Title);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testPath))
                {
                    File.Delete(testPath);
                }
            }
        }
    }
}
