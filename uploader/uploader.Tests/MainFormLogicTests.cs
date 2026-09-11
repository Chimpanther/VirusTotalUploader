using System;
using Xunit;
using uploader;

namespace uploader.Tests
{
    public class MainFormLogicTests
    {
        [Fact]
        public void GetDroppedFiles_NullData_ReturnsNull()
        {
            var result = MainFormLogic.GetDroppedFiles(null);
            Assert.Null(result);
        }

        [Fact]
        public void GetDroppedFiles_WrongType_ReturnsNull()
        {
            var result = MainFormLogic.GetDroppedFiles(new object[] { "test" });
            Assert.Null(result);
        }

        [Fact]
        public void GetDroppedFiles_EmptyStringArray_ReturnsNull()
        {
            var result = MainFormLogic.GetDroppedFiles(Array.Empty<string>());
            Assert.Null(result);
        }

        [Fact]
        public void GetDroppedFiles_ValidStringArray_ReturnsArray()
        {
            var input = new string[] { "file1.txt", "file2.txt" };
            var result = MainFormLogic.GetDroppedFiles(input);
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal("file1.txt", result[0]);
            Assert.Equal("file2.txt", result[1]);
        }

        [Fact]
        public void TryGetFileFromArgs_NullArgs_ReturnsNull()
        {
            var result = MainFormLogic.TryGetFileFromArgs(null);
            Assert.Null(result);
        }

        [Fact]
        public void TryGetFileFromArgs_EmptyArgs_ReturnsNull()
        {
            var result = MainFormLogic.TryGetFileFromArgs(Array.Empty<string>());
            Assert.Null(result);
        }

        [Fact]
        public void TryGetFileFromArgs_LengthOne_ReturnsNull()
        {
            var result = MainFormLogic.TryGetFileFromArgs(new string[] { "program.exe" });
            Assert.Null(result);
        }

        [Fact]
        public void TryGetFileFromArgs_LengthTwo_ReturnsSecondArgument()
        {
            var result = MainFormLogic.TryGetFileFromArgs(new string[] { "program.exe", "file.txt" });
            Assert.Equal("file.txt", result);
        }

        [Fact]
        public void TryGetFileFromArgs_LengthThree_ReturnsNull()
        {
            var result = MainFormLogic.TryGetFileFromArgs(new string[] { "program.exe", "file1.txt", "file2.txt" });
            Assert.Null(result);
        }
    }
}
