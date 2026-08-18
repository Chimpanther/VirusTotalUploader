using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Newtonsoft.Json;
using uploader;

namespace uploader.Tests
{
    [TestClass]
    public class LocalizationHelperTests
    {
        [TestMethod]
        public void Export_CreatesValidJsonFile()
        {
            // Arrange
            string exportFileName = "export.json";
            if (File.Exists(exportFileName))
            {
                File.Delete(exportFileName);
            }

            try
            {
                // Act
                LocalizationHelper.Export();

                // Assert
                Assert.IsTrue(File.Exists(exportFileName), "The export.json file should be created.");

                string jsonContent = File.ReadAllText(exportFileName);
                var deserialized = JsonConvert.DeserializeObject<LocalizationBase>(jsonContent);

                Assert.IsNotNull(deserialized, "The JSON content should be deserializable to LocalizationBase.");
            }
            finally
            {
                // Clean up
                if (File.Exists(exportFileName))
                {
                    File.Delete(exportFileName);
                }
            }
        }
    }
}
