🔒 [Security Fix] Replace weak hash functions MD5 and SHA-1

🎯 **What:**
The application was using `MD5.Create()` and `SHA1Managed()` for hashing file contents, and `SHA256Managed()` which is obsolete and issues a `SYSLIB0021` warning. Additionally, `SHA1Managed` and `SHA256Managed` were not disposed in `using` blocks, potentially causing resource leaks.

⚠️ **Risk:**
MD5 and SHA-1 are cryptographically broken algorithms susceptible to collision attacks. If these hashes are used to verify file integrity or for security contexts, an attacker could supply a malicious file that generates the same hash as a legitimate file. Additionally, using unmanaged cryptographic resources without `using` statements can cause memory leaks.

🛡️ **Solution:**
1. Replaced `GetMD5` with `GetSHA384` utilizing `SHA384.Create()`.
2. Replaced `GetSHA1` with `GetSHA512` utilizing `SHA512.Create()`.
3. Updated `GetSHA256` to use `SHA256.Create()` instead of the obsolete `SHA256Managed()`.
4. Ensured all hashing functions wrap the hash instances in `using` blocks to prevent unmanaged resource leaks.
5. Updated UI labels and text boxes in `UploadForm` to display the new SHA384 and SHA512 hashes.
6. Updated the xUnit test suite (`UtilsTests.cs`) to correctly test `GetSHA384` and updated the expected hash assertion. Tests passed successfully.
