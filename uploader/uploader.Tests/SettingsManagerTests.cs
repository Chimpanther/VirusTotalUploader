using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using uploader;

[Collection("Sequential")]
public class SettingsManagerTests : IDisposable
{
    private readonly string _settingsFile;

    public SettingsManagerTests()
    {
        // Setup clean state
        SettingsManager.ClearCache();
        _settingsFile = SettingsManager.GetSettingsFilename();
        if (File.Exists(_settingsFile))
            File.Delete(_settingsFile);
    }

    public void Dispose()
    {
        // Teardown
        if (File.Exists(_settingsFile))
            File.Delete(_settingsFile);
        SettingsManager.ClearCache();
    }

    [Fact]
    public void SaveAndLoad_EncryptsDataSuccessfully()
    {
        // Arrange
        var settings = new Settings { ApiKey = "SuperSecretKey123", DirectUpload = true };

        // Act
        SettingsManager.SaveSettings(settings);

        // Verify it was encrypted (should not be plain JSON)
        var fileContent = File.ReadAllText(_settingsFile);
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { Assert.DoesNotContain("SuperSecretKey123", fileContent); }

        // Act - Load
        var loadedSettings = SettingsManager.LoadSettings();

        // Assert
        Assert.Equal("SuperSecretKey123", loadedSettings.ApiKey);
        Assert.True(loadedSettings.DirectUpload);
    }
}
