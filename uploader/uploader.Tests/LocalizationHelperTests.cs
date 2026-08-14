using System.IO;
using Newtonsoft.Json;
using uploader;
using Xunit;

namespace uploader.Tests
{
    public class LocalizationHelperTests
    {
        [Fact]
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
                Assert.True(File.Exists(exportFileName), "The export.json file should be created.");

                string jsonContent = File.ReadAllText(exportFileName);
                var deserialized = JsonConvert.DeserializeObject<LocalizationBase>(jsonContent);

                Assert.NotNull(deserialized);
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
