using System;
using System.IO;
using Xunit;
using uploader;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class LocalizationHelperTests : IDisposable
    {
        private readonly string _tempFilePath;

        public LocalizationHelperTests()
        {
            _tempFilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }

            // Clean up static state to avoid affecting other tests
            LocalizationHelper.Base = null;
        }

        [Fact]
        public void Load_ReadsJsonFileAndPopulatesBase()
        {
            // Arrange
            var expectedBase = new LocalizationBase
            {
                MainForm_DragFile = "Test Drag file here",
                MainForm_More = "Test More",
                Message_Idle = "Test Idle."
            };

            var json = JsonConvert.SerializeObject(expectedBase);
            File.WriteAllText(_tempFilePath, json);

            // Act
            LocalizationHelper.Load(_tempFilePath);

            // Assert
            Assert.NotNull(LocalizationHelper.Base);
            Assert.Equal("Test Drag file here", LocalizationHelper.Base.MainForm_DragFile);
            Assert.Equal("Test More", LocalizationHelper.Base.MainForm_More);
            Assert.Equal("Test Idle.", LocalizationHelper.Base.Message_Idle);
        }

        [Fact]
        public void Load_EmptyFile_ReturnsNullBase()
        {
            // Arrange
            File.WriteAllText(_tempFilePath, "");

            // Act
            LocalizationHelper.Load(_tempFilePath);

            // Assert
            Assert.Null(LocalizationHelper.Base);
        }

        [Fact]
        public void Load_InvalidJson_ThrowsJsonReaderException()
        {
            // Arrange
            File.WriteAllText(_tempFilePath, "invalid json");

            // Act & Assert
            Assert.Throws<JsonReaderException>(() => LocalizationHelper.Load(_tempFilePath));
        }

        [Fact]
        public void Load_FileNotFound_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => LocalizationHelper.Load(nonExistentFile));
        }
    }
}
