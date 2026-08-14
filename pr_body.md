## Consolidated Integration of Maintenance and Test PRs

This pull request consolidates 9 verified and clean PRs that were sitting open on `master` with no merge conflicts. 

### Included PRs
* #38 - 🧹 Remove dead commented code in SettingsForm.cs
* #39 - Replace MessageBox with DarkMessageBox in SettingsForm
* #41 - 🧪 Add test for LocalizationHelper.GetLanguages
* #42 - 🧪 Add test for Settings.LoadSettings
* #47 - 🧹 Use DarkMessageBox instead of standard MessageBox in SettingsForm
* #49 - 🧹 Remove unused using directives in LocalizationHelper
* #50 - ⚡ Optimize UploadForm Instantiation on Drag and Drop
* #61 - 🧪 Add test for Utils.GetSHA1
* #63 - ⚡ Optimize LoadSettings caching to prevent excessive disk IO

### Conflict Resolutions
* Resolusjon av `<Compile Include>` overlapp i `uploader.Tests.csproj`.
* PR-ene var 100% "CLEAN/MERGEABLE" pre-fletting. Resten av the clean PRs (f.eks 43, 44) ble ekskludert pga. gjensidige tekst-konflikter i the test suite (`LocalizationHelperTests.cs`).

All local verification complete and branch pushed cleanly.
