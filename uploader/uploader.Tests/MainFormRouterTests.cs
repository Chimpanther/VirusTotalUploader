using System;
using System.Collections.Generic;
using Xunit;
using uploader;

namespace uploader.Tests
{
    public class MainFormRouterTests
    {
        private readonly MockMainFormView _view;
        private readonly MainFormRouter _router;
        private readonly Settings _settings;

        public MainFormRouterTests()
        {
            _view = new MockMainFormView();
            _router = new MainFormRouter(_view);
            _settings = new Settings();
        }

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

            // Act
            _router.HandleFilesDropped(_settings, null!);

            // Assert
            Assert.Empty(_view.ShowUploadFormCalls);
            Assert.Equal(0, _view.HideFormCalls);
        }

        [Fact]
        public void HandleFilesDropped_WithEmptyFiles_DoesNothing()
        {
            // Arrange

            // Act
            _router.HandleFilesDropped(_settings, Array.Empty<string>());

            // Assert
            Assert.Empty(_view.ShowUploadFormCalls);
            Assert.Equal(0, _view.HideFormCalls);
        }

        [Fact]
        public void HandleFilesDropped_WithValidFiles_CallsShowAndHideForEach()
        {
            // Arrange
            var files = new[] { "file1.txt", "file2.txt" };

            // Act
            _router.HandleFilesDropped(_settings, files);

            // Assert
            Assert.Equal(2, _view.ShowUploadFormCalls.Count);
            Assert.Equal("file1.txt", _view.ShowUploadFormCalls[0].file);
            Assert.True(_view.ShowUploadFormCalls[0].reopen);
            Assert.Equal(_settings, _view.ShowUploadFormCalls[0].settings);

            Assert.Equal("file2.txt", _view.ShowUploadFormCalls[1].file);
            Assert.True(_view.ShowUploadFormCalls[1].reopen);
            Assert.Equal(_settings, _view.ShowUploadFormCalls[1].settings);

            Assert.Equal(2, _view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithNullArgs_DoesNothing()
        {
            // Arrange

            // Act
            _router.HandleCommandLineArgs(_settings, null!);

            // Assert
            Assert.Empty(_view.ShowUploadFormCalls);
            Assert.Equal(0, _view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithInvalidLengthArgs_DoesNothing()
        {
            // Arrange
            var args = new[] { "program.exe" }; // Length 1

            // Act
            _router.HandleCommandLineArgs(_settings, args);

            // Assert
            Assert.Empty(_view.ShowUploadFormCalls);
            Assert.Equal(0, _view.HideFormCalls);
        }

        [Fact]
        public void HandleCommandLineArgs_WithValidLengthArgs_CallsShowAndHide()
        {
            // Arrange
            var args = new[] { "program.exe", "file.txt" }; // Length 2

            // Act
            _router.HandleCommandLineArgs(_settings, args);

            // Assert
            Assert.Single(_view.ShowUploadFormCalls);
            Assert.Equal("file.txt", _view.ShowUploadFormCalls[0].file);
            Assert.False(_view.ShowUploadFormCalls[0].reopen);
            Assert.Equal(_settings, _view.ShowUploadFormCalls[0].settings);

            Assert.Equal(1, _view.HideFormCalls);
        }
    }
}
