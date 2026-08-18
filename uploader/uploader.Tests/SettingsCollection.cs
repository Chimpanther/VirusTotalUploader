using Xunit;

namespace uploader.Tests
{
    [CollectionDefinition("SettingsCollection", DisableParallelization = true)]
    public class SettingsCollection : ICollectionFixture<object>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
