🎯 **What:** Addressed missing tests for SettingsForm.cs by extracting business logic (such as populating the UI, getting options, getting CLI args, and saving settings) from the Windows Forms UI file. Testing the underlying UI directly was impossible in the CI headless setup, so this logic was moved into SettingsFormHelpers.cs.
📊 **Coverage:** Wrote SettingsFormTests.cs to test loading settings values into UI options, default empty cases, resolving Windows Explorer command args, and saving form values.
✨ **Result:** Enhanced project coverage without breaking compatibility or CI.
