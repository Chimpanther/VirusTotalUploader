# AGENTS.md

Repository-level instructions for Factory Droid, Codex, Claude Code, Cursor, Aider, Gemini CLI, and other coding agents working in `Chimpanther/VirusTotalUploader`.

Follow this file before planning, editing, committing, or opening a PR. When instructions conflict, the user's latest explicit chat instruction wins. Do not invent missing requirements.

## Project Overview

- This is an unofficial Windows desktop uploader for VirusTotal.
- The app is a legacy WinForms desktop application, not a web service.
- Primary runtime target: `.NET Framework 4.8`.
- Primary language: C#.
- UI framework: Windows Forms with DarkUI.
- Main workflow: user drags/drops a file or passes one file path as an argument, the app computes hashes, checks VirusTotal, uploads only when needed, and opens the resulting VirusTotal URL.
- Treat this as a security-adjacent utility: privacy, user consent, API-key handling, and safe test behavior matter more than flashy changes.

## Repository Layout

- `README.md` - human-facing project description and usage notes.
- `LICENSE.txt` - GPLv3 project license. Do not replace or weaken it.
- `LICENSE_3rd.txt` - third-party licensing information.
- `assets/` - README/demo assets.
- `darkui/` - bundled DarkUI dependency used by the WinForms app.
- `uploader/uploader.sln` - Visual Studio solution. Open/build this, not a guessed root solution.
- `uploader/uploader/uploader.csproj` - .NET Framework 4.8 WinForms project.
- `uploader/uploader/Program.cs` - WinForms entry point.
- `uploader/uploader/MainForm.cs` - main drag/drop shell.
- `uploader/uploader/UploadForm.cs` - upload/check workflow and VirusTotal requests.
- `uploader/uploader/Settings.cs` - local settings serialization, including API key storage.
- `uploader/uploader/Localization*.cs` and `local/` runtime folder behavior - localization support.
- `uploader/packages.config` or `uploader/uploader/packages.config` - NuGet packages for the legacy project format.

## Factory Droid Operating Mode

Use a conservative Factory workflow:

1. Explore the repo and confirm paths before editing.
2. State a focused plan with files to touch.
3. Make the smallest coherent change.
4. Build/verify, or clearly explain why verification could not be run.
5. Open a PR for Magnus/Chimpanther to review.

Factory Droid should prefer explicit, copy-pasteable commands. Avoid broad autonomous rewrites, speculative modernization, or changing project format unless specifically requested.

## GitHub, Branching, PR, and Identity Rules

Hard rules:

- Never push directly to `master`.
- Never merge your own work.
- Never enable auto-merge.
- Never approve your own PR.
- All changes must be delivered as a pull request for Chimpanther to review and approve.
- Use `master` as the PR base branch unless the user explicitly names another base.
- Prefer branch names like `chimpanther/<short-task-slug>` or `agent/<short-task-slug>`.
- PR title should be short and descriptive, e.g. `docs: add agent instructions`.
- PR body must include summary, verification, and any limitations.
- Do not add `Co-authored-by`, `Generated-by`, `Signed-off-by`, AI signature trailers, bot credits, or assistant attribution to commits or PR text.
- When the platform allows setting commit author identity, use Chimpanther as the author/user identity.
- Do not claim human authorship for work you did not do; just keep metadata clean and do not add AI/bot trailers.

Commit style:

- Use short English conventional-style messages: `docs: ...`, `fix: ...`, `refactor: ...`, `test: ...`, `chore: ...`.
- Keep commits atomic and scoped.
- Do not combine unrelated cleanup with feature/fix work.

## Build and Verification Commands

Run from Windows, preferably in **Developer PowerShell for Visual Studio 2022**.

Restore dependencies:

```powershell
cd uploader
nuget restore .\uploader.sln
```

Build Debug:

```powershell
cd uploader
msbuild .\uploader.sln /p:Configuration=Debug /p:Platform=x64 /m
```

Build Release:

```powershell
cd uploader
msbuild .\uploader.sln /p:Configuration=Release /p:Platform=x64 /m
```

If `msbuild` is unavailable but Visual Studio is installed:

```powershell
cd uploader
devenv .\uploader.sln /Build "Release|x64"
```

Notes:

- This is a legacy .NET Framework project, so do not assume `dotnet build` is the correct primary command.
- If NuGet restore/build fails because the environment lacks Visual Studio Build Tools or the .NET Framework 4.8 Developer Pack, report that explicitly in the PR.
- There is no known automated test suite in this repo at the time this file was added. For logic changes, prefer adding tests where practical; otherwise provide compile evidence plus manual verification steps.

## Manual Verification Checklist

For UI or upload-flow changes, verify as much of this as possible:

- App starts without crashing.
- Drag/drop of a normal file opens `UploadForm`.
- Launching with a single file path argument opens `UploadForm`.
- MD5, SHA1, and SHA256 fields populate for files.
- Folder input does not attempt to hash the folder itself.
- Settings dialog opens and saves expected values.
- Missing API key shows a clear error.
- Invalid API key length shows a clear error.
- Existing localization behavior is preserved.
- No real user file is uploaded to VirusTotal during tests unless the user explicitly approved that exact test.

Use harmless local test files only. Do not use malware, suspicious samples, private files, credentials, dumps, logs, or customer/user data as upload-test material.

## Security and Privacy Rules

- Never commit API keys, sample keys, secrets, `.user` files, logs, build output, or local settings.
- The VirusTotal API key is sensitive even if it is only used client-side.
- Do not print API keys to logs, exceptions, screenshots, PR bodies, or test output.
- Do not upload files to VirusTotal automatically in tests. VirusTotal submissions may be shared with security vendors and should be treated as external disclosure.
- Do not introduce telemetry, analytics, crash reporting, auto-update, remote config, or background network calls without explicit approval.
- Do not change the VirusTotal endpoint, request semantics, or upload behavior without explaining privacy and compatibility impact in the PR.
- Use TLS endpoints only.
- Validate file existence and handle IO/network failures gracefully.
- Avoid shell execution. If opening URLs, ensure values originate from trusted VirusTotal response fields or are safely constructed.
- Do not add malware-analysis, evasion, unpacking, persistence, credential collection, or offensive capabilities. This project is only an uploader/checker utility.

## Coding Conventions

- Preserve the existing C# style unless a broader style task is requested.
- Keep namespace `uploader` unless explicitly renaming the project.
- Use explicit, readable WinForms code over clever abstractions.
- Keep UI text localization-friendly; do not hard-code new user-facing strings when they belong in localization resources.
- Keep changes compatible with C# 7.3 unless the project file is intentionally updated.
- Avoid new runtime dependencies unless they are clearly justified in the PR.
- Do not migrate from `packages.config` to `PackageReference` unless explicitly requested.
- Do not migrate from .NET Framework to modern .NET unless explicitly requested.
- Do not reformat designer-generated files except when Visual Studio intentionally updates them for a UI change.
- Do not edit `.resx`, `.Designer.cs`, or `.settings` files by hand unless necessary and explain why.

## Dependency and Packaging Rules

- Treat `darkui/DarkUI.dll` as a bundled dependency. Do not replace it casually.
- Keep NuGet versions pinned unless the task is dependency maintenance.
- For dependency updates, include reason, affected packages, build result, and any breaking-change notes.
- Do not commit `bin/`, `obj/`, `.vs/`, `x64/`, `Debug/`, `Release/`, logs, profiler output, or generated installers unless explicitly requested.
- Respect the existing Visual Studio `.gitignore` behavior.

## Architecture Notes and Gotchas

- `Program.Main()` enables WinForms visual styles and starts `MainForm`.
- `MainForm` sets the working directory to the executable location so localization files can be found at runtime.
- Drag/drop supports both files and folders; folders enumerate files recursively.
- `UploadForm` currently uses a background `Thread` for upload work and marshals UI changes back to the UI thread with `Invoke`.
- `Settings.GetSettingsFilename()` stores settings at `%APPDATA%\vtu_settings.json`.
- `Settings.ApiKey` is currently validated as exactly 64 characters.
- VirusTotal API integration currently uses v2-style endpoints under `https://www.virustotal.com`.
- Hash helpers are in `Utils.cs`; do not duplicate hash logic.

## Safe Refactoring Boundaries

Good agent tasks:

- Add guardrails and docs.
- Improve error handling around network/JSON failures.
- Reduce UI freezes while preserving WinForms behavior.
- Add tests or testable helpers for pure logic.
- Improve localization consistency.
- Fix clear bugs with narrow diffs.

Risky tasks that require explicit approval:

- Replacing RestSharp or JSON library.
- Changing VirusTotal API version.
- Changing settings storage/encryption behavior.
- Migrating target framework/project format.
- Reworking threading/async model across the app.
- Modifying installer/release packaging.
- Uploading real files to VirusTotal as validation.

## PR Evidence Requirements

Every PR should include:

```markdown
## Summary
- What changed
- Why it changed

## Verification
- Command(s) run and result
- Manual checks performed, if any

## Notes
- Limitations, skipped checks, or environment blockers
```

For code changes, include exact build command output status. For docs-only changes, state `Docs-only change; no build run` unless a build was actually performed.

## Agent Drift Recovery

Stop and ask for a narrower task if any of these happen:

- You are about to touch unrelated files.
- You need to upload real samples or use a real API key.
- The change requires project migration.
- Build errors appear unrelated to your diff.
- You cannot verify a security-sensitive behavior.

Prefer a small PR with honest limitations over a broad PR with unverified claims.