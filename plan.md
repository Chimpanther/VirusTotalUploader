1. **Refactor Helper Methods to Fix CodeScene CI Check (Primitive Obsession)**
   - **Problem:** CodeScene complains about `Primitive Obsession`. This could be because we pass multiple strings to `DisplayError` and `ChangeStatus` directly from the background thread or we are passing `fileHashes` along with `fullPath`. Let's create an `UploadJob` record or struct to represent a file being processed to encapsulate state (`FullPath`, `FileName`, `Hashes`).
   - **Fix:** Add a private class/struct `UploadJob` containing `FullPath`, `FileName`, and `FileHashesResult Hashes`. Update `UploadFileAsync`, `CheckFileReportAsync`, and `ScanNewFileAsync` to accept a single `UploadJob` object instead of multiple string arguments.
   - **Verify:** Run tests and check compilation.
2. **Pre-commit Checks**
   - Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.
3. **Submit Changes**
   - Submit the PR containing these verified improvements.
