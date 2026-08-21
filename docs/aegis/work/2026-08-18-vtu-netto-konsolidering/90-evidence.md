# VirusTotalUploader netto-innholds-kartlegging - Evidence

Evidence bundles recorded during consolidation execution:

## EvidenceBundleDraft (Crypto Tests)

- Artifact key: crypto-tests
- Type: test
- Source: dotnet test uploader/uploader.Tests/uploader.Tests.csproj --no-restore --verbosity minimal
- Summary: 17 tests passed after adding SHA-256/null/empty coverage and aligning MD5 expected case; existing nullable warnings remain.
- Verifier: local dotnet test

## EvidenceBundleDraft (Localization Tests)

- Artifact key: localization-tests
- Type: test
- Source: dotnet test uploader/uploader.Tests/uploader.Tests.csproj --no-restore --verbosity minimal
- Summary: 24 tests passed after isolating LocalizationHelper tests in unique temp directories and adding Load/Update/Export coverage.
- Verifier: local dotnet test

## EvidenceBundleDraft (Legacy Build Status)

- Artifact key: legacy-build-blocked
- Type: build
- Source: dotnet build uploader/uploader/uploader.csproj --no-restore --configuration Debug --verbosity minimal
- Summary: Build did not reach source compilation; dotnet SDK stopped on existing non-string .resx resources with MSB3822/MSB3823. No msbuild/devenv/vswhere command is available.
- Verifier: local command inspection

## EvidenceBundleDraft (Final Test and Diff Check)

- Artifact key: final-test-diff
- Type: verification
- Source: dotnet test uploader/uploader.Tests/uploader.Tests.csproj --no-restore --verbosity minimal; git diff --check
- Summary: 26 tests passed and diff whitespace check returned exit 0.
- Verifier: local dotnet test and git
