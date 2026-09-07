using System;
using System.Collections.Generic;
using Xunit;
using uploader;

namespace uploader.Tests
{
    public class MainFormRouterTests
    {
        private class MockMainFormView : IMainFormView
        {
            public List<(Settings settings, bool reopen, string file)> ShowUploadFormCalls = new();
            public int HideFormCalls = 0;

            public void ShowUploadForm(Settings settings, bool reopen, string file)
            {
                ShowUploadFormCalls.Add((settings, reopen, file));
            }

            public void HideForm()
            {
                HideFormCalls++;
            }
        }

        [Fact]
        public void HandleFilesDropped_WithNullFiles_DoesNothing()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();

            // Act
            router.HandleFilesDropped(settings, null!);

            // Assert
            Assert.Empty(view.ShowUploadFormCalls);
            Assert.Equal(0, view.HideFormCalls);
        }

        [Fact]
        public void HandleFilesDropped_WithEmptyFiles_DoesNothing()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();

            // Act
            router.HandleFilesDropped(settings, Array.Empty<string>());

            // Assert
            Assert.Empty(view.ShowUploadFormCalls);
            Assert.Equal(0, view.HideFormCalls);
        }

        [Fact]
        public void HandleFilesDropped_WithValidFiles_CallsShowAndHideForEach()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();
            var files = new[] { "file1.txt", "file2.txt" };

            // Act
            router.HandleFilesDropped(settings, files);

            // Assert
            Assert.Equal(2, view.ShowUploadFormCalls.Count);
            Assert.Equal("file1.txt", view.ShowUploadFormCalls[0].file);
            Assert.True(view.ShowUploadFormCalls[0].reopen);
            Assert.Equal(settings, view.ShowUploadFormCalls[0].settings);

            Assert.Equal("file2.txt", view.ShowUploadFormCalls[1].file);
            Assert.True(view.ShowUploadFormCalls[1].reopen);
            Assert.Equal(settings, view.ShowUploadFormCalls[1].settings);

            Assert.Equal(2, view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithNullArgs_DoesNothing()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();

            // Act
            router.HandleCommandLineArgs(settings, null!);

            // Assert
            Assert.Empty(view.ShowUploadFormCalls);
            Assert.Equal(0, view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithInvalidLengthArgs_DoesNothing()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();
            var args = new[] { "program.exe" }; // Length 1

            // Act
            router.HandleCommandLineArgs(settings, args);

            // Assert
            Assert.Empty(view.ShowUploadFormCalls);
            Assert.Equal(0, view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithValidLengthArgs_CallsShowAndHide()
        {
            // Arrange
            var view = new MockMainFormView();
            var router = new MainFormRouter(view);
            var settings = new Settings();
            var args = new[] { "program.exe", "file.txt" }; // Length 2

            // Act
            router.HandleCommandLineArgs(settings, args);

            // Assert
            Assert.Single(view.ShowUploadFormCalls);
            Assert.Equal("file.txt", view.ShowUploadFormCalls[0].file);
            Assert.False(view.ShowUploadFormCalls[0].reopen);
            Assert.Equal(settings, view.ShowUploadFormCalls[0].settings);

            Assert.Equal(1, view.HideFormCalls);
        }
    }
}
