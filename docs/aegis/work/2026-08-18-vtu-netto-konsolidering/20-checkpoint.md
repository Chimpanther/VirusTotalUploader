# VirusTotalUploader netto-innholds-kartlegging - Checkpoint

- Task ID: 2026-08-18-vtu-netto-konsolidering
- Current todo: Define first execution slice.
- Active slice: initial
- Blocked on: none
- Next step: Read baseline refs and start the next safe slice.

## Checkpoint Update (1 - Crypto Tests)

- Current todo: Review and consolidate the remaining unique PR slices
- Active slice: Crypto-utils tests: SHA-256 coverage, null/empty edge cases, and MD5 output case alignment
- Completed todos:
- Baseline review; crypto-utils test slice
- Evidence refs:
- dotnet test uploader/uploader.Tests/uploader.Tests.csproj --no-restore (17 passed)
- Blocked on: none
- Next step: Review LocalizationHelper Load/Export/Update candidates against the baseline

## DriftCheckDraft (1 - Crypto Tests)

- Scope status: within crypto-utils test boundary
- Compatibility status: hash runtime semantics unchanged; test-only except expected-value correction
- Retirement status: no new fallback or compatibility path
- New risk signals:
- remaining PRs include overlapping UploadForm, Settings security, and API URL changes requiring separate review
- Advisory decision: continue

## Checkpoint Update (2 - Localization Tests)

- Current todo: Review and consolidate the remaining unique PR slices
- Active slice: Review Settings and UploadForm candidate PRs for contract-safe consolidation
- Completed todos:
- Baseline review; crypto-utils tests; LocalizationHelper Load/Update/Export tests
- Evidence refs:
- dotnet test uploader/uploader.Tests/uploader.Tests.csproj --no-restore (24 passed)
- Blocked on: none
- Next step: Inspect Settings DPAPI and Save/Load test candidates; stop if contract or compatibility is unresolved

## DriftCheckDraft (2 - Localization Tests)
- Scope status: within test-owner boundary; no runtime source changed
- Compatibility status: existing LocalizationHelper semantics exercised without change
- Retirement status: no new fallback or compatibility path
- New risk signals:
- Settings and UploadForm candidates remain contract-sensitive and Windows-build dependent
- Advisory decision: continue

## Checkpoint Update (3 - Final Review)
- Current todo: Windows/MSBuild verification and approval for remaining risky units
- Active slice: Completion boundary review: available tests pass, legacy app build is blocked by missing MSBuild/Visual Studio resource tooling
- Completed todos:
- Baseline review; crypto-utils tests; LocalizationHelper tests; Settings tests; safe Process.Start URL hardening
- Evidence refs:
- Blocked on: Windows/MSBuild verification
- Next step: Obtain Windows Developer PowerShell/MSBuild verification and explicit scope decision for DPAPI/API-URL/hash-removal/UploadForm slices

## DriftCheckDraft (3 - Final Review)

- Scope status: safe test and URL slices only; remaining plan units intentionally out of scope
- Compatibility status: legacy app compatibility remains unverified because native MSBuild is unavailable
- Retirement status: no fallback retirement needed; risky migrations not started
- New risk signals:
- explicit approval and Windows build evidence required for settings storage, API URL, hash removal, and UploadForm concurrency changes
- Advisory decision: pause-for-user
