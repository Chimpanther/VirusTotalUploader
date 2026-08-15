using Xunit;
using System.IO;
using uploader;
using Newtonsoft.Json;

namespace uploader.Tests
{
    [Collection("Sequential")]
    public class SettingsTests
    {
        [Fact]
        public void Settings_ApiKey_Encryption_RoundTrips()
        {
            var settings = new Settings { ApiKey = "my_secret_key" };
            string json = JsonConvert.SerializeObject(settings);

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Check that plaintext key isn't in JSON on Windows where DPAPI works
                Assert.DoesNotContain("my_secret_key", json);
            }

            Assert.Contains("ApiKey", json);

            // Check that it can be deserialized properly
            var deserialized = JsonConvert.DeserializeObject<Settings>(json);
            Assert.NotNull(deserialized);
            Assert.Equal("my_secret_key", deserialized.ApiKey);
        }

        [Fact]
        public void Settings_OldPlaintextJson_DeserializesCorrectly()
        {
            // Simulate reading an old JSON file where "ApiKey" was stored as plaintext
            string json = "{\"ApiKey\":\"old_secret_key\",\"Language\":\"en\"}";

            var deserialized = JsonConvert.DeserializeObject<Settings>(json);
            Assert.NotNull(deserialized);
            Assert.Equal("old_secret_key", deserialized.ApiKey);
        }
    }
}