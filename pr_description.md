## Summary
- Added a new unit test for `Settings.SaveSettings` to ensure that saving with a `Language` value of "Default" correctly persists as an empty string.
- Created `uploader.Tests.csproj` and `SettingsTests.cs` using the .NET 8 MSTest framework.
- Linked source files directly into the .NET 8 test project to support testing a .NET Framework 4.8 legacy project.
- Configured the test Setup/Teardown methods to backup any existing configuration file from the host system environment so that it is properly restored post-test run, avoiding inadvertent destruction of developer configurations.
- Added test project into solution file `uploader.sln`.

## Verification
- `cd uploader && dotnet test uploader.Tests/uploader.Tests.csproj` runs and passes tests correctly on Linux environments.

## Notes
- `uploader.Tests` relies on linked source files because the main project (`uploader.csproj`) utilizes legacy .NET 4.8 `packages.config` style which is not compatible with direct `<ProjectReference>` for native dotnet tools.
