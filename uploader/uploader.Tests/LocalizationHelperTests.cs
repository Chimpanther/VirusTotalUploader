using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using uploader;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [TestClass]
    public class LocalizationHelperTests
    {
        [TestMethod]
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
                Assert.IsNotNull(LocalizationHelper.Base);
                Assert.AreEqual("Test Drag File", LocalizationHelper.Base.MainForm_DragFile);
                Assert.AreEqual("Test More", LocalizationHelper.Base.MainForm_More);

                // Verify default values remain for properties not in JSON
                Assert.AreEqual("Settings", LocalizationHelper.Base.SettingsForm_Title);
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
