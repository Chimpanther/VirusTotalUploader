1. **F-01: Correctness / Test Fix**
   - **Problem:** `Utils.GetMD5` returns a lowercase hex string using a `StringBuilder`, which is inconsistent with `GetSHA1` and `GetSHA256` (which return uppercase) and causes `GetMD5_ValidFile_ReturnsCorrectHash` to fail.
   - **Fix:** Update `Utils.GetMD5` to use `BitConverter.ToString(hash).Replace("-", string.Empty)`.

2. **F-02: Performance / Resource Efficiency**
   - **Problem:** `UploadForm` computes MD5, SHA1, and SHA256 sequentially for a single file, resulting in three separate disk reads.
   - **Fix:** Add a new `FileHashesResult GetHashes(string file)` method in `Utils.cs` to compute all three hashes in a single pass using `TransformBlock`/`TransformFinalBlock`. Update `UploadForm` to use this method.

3. **F-03: Performance / Configuration**
   - **Problem:** `Settings.LoadSettings()` reads the configuration file from disk every time it is invoked, causing redundant I/O.
   - **Fix:** Add a thread-safe static cache (`_cachedSettings`) to `Settings.cs` that is populated on first load and updated during `SaveSettings()`. Update `SettingsTests.cs` to clear the cache during setup/teardown.

4. **F-04: Security / Reliability**
   - **Problem:** `UploadForm.OpenUrlSafe` directly passes a raw URL string to `Process.Start()`.
   - **Fix:** Update it to pass an explicitly configured `ProcessStartInfo` object with `UseShellExecute = true` to safely launch the default browser.

5. **F-05: Reliability / External Integrations**
   - **Problem:** VirusTotal API responses are parsed as JSON in `UploadForm` without checking the HTTP status code, leading to crashes when rate-limited (returns 204) or on API errors.
   - **Fix:** Check `response.IsSuccessful` and `StatusCode == System.Net.HttpStatusCode.OK` before deserialization. Display a clear error if the API request fails.

6. **F-06: Test Quality / Concurrency**
   - **Problem:** `SettingsTests` modify the real settings file, risking test failures during concurrent execution.
   - **Fix:** Apply `[assembly: CollectionBehavior(DisableTestParallelization = true)]` in the test project and isolate state.

7. **Pre-commit Checks**
   - Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.

8. **Submit Changes**
   - Submit the PR containing these verified improvements.
