<div><p align="center"><img src="https://raw.githubusercontent.com/SamuelTulach/VirusTotalUploader/master/uploader/uploader/icon.ico" width="75" height="75" /></p><h3 align="center">VirusTotal Uploader</h3></div>
<p align="center">Open-source desktop uploader for VirusTotal.</p>

## Purpose
This is an unofficial, open-source Windows desktop uploader for VirusTotal. It provides an alternative to the unmaintained official desktop application, enabling users to easily check and upload files or folders for malware analysis via the VirusTotal API.

## Capabilities
- **File and Folder Uploads:** Drag and drop files or entire folders to upload.
- **CLI Support:** Launch the app with a file path as an argument.
- **Hash Checks:** Computes SHA256 checksums locally and queries VirusTotal before uploading.
- **Automatic Uploads:** Supports direct file uploads for unknown files or when configured.
- **Result Navigation:** Safely opens the generated VirusTotal report in the default browser.
- **Localization:** Supports multi-language UI via translation resources.

## Prerequisites
- **Operating System:** Windows
- **Runtime:** .NET Framework 4.8
- **VirusTotal API Key:** Required for all operations. Create a free account at [VirusTotal](https://www.virustotal.com/) and copy your 64-character API key.

## Quick Start
1. Go to the [releases page](https://github.com/SamuelTulach/VirusTotalUploader/releases) to download the compiled executable or installer.
2. Launch the application.
3. Open Settings and enter your 64-character VirusTotal API Key.
4. Drag and drop a file onto the application to check it.

## Installation & Build
If you prefer to build the application from source:
1. Ensure you have **Visual Studio 2022** with the **.NET Framework 4.8 Developer Pack** installed.
2. Clone the repository and open **Developer PowerShell for Visual Studio 2022**.
3. Restore dependencies and build the solution:
   ```powershell
   cd uploader
   nuget restore .\uploader.sln
   msbuild .\uploader.sln /p:Configuration=Release /p:Platform=x64 /m
   ```
4. The executable will be generated in the respective `bin/x64/Release` directory.

## Configuration
Application settings are securely stored locally in your Application Data folder:
`%APPDATA%\vtu_settings.json`

Configurable settings include:
- `ApiKey`: Your 64-character VirusTotal API Key.
- `Language`: Selected localization (defaults to system UI).
- `DirectUpload`: Enable/disable automatic uploading of files without manual prompt.

Modifying settings should be done via the app's Settings menu to ensure the internal memory cache updates simultaneously.

## Usage & Examples
### Drag and Drop
- Drag a single file or a folder onto the main window to initiate the upload and scan workflow. The application will compute the SHA256 hash and check if the file is known to VirusTotal.

### Command Line Interface
- You can launch the application by passing a file path directly as a command-line argument:
  ```powershell
  uploader.exe "C:\path\to\your\file.exe"
  ```
  This immediately opens the upload status window for the specified file.

## Architecture
- **Language/Framework:** C# / .NET Framework 4.8
- **UI Framework:** Windows Forms paired with the DarkUI library for styling.
- **Main Components:**
  - `Program.cs`: WinForms entry point.
  - `MainForm.cs`: Handles drag-and-drop mechanics and CLI argument parsing.
  - `UploadForm.cs`: Manages asynchronous file hashing (SHA256), VirusTotal API requests (via RestSharp), and UI status updates.
  - `Settings.cs`: Thread-safe caching and JSON serialization for application settings.

## Troubleshooting
- **Invalid API Key Length:** The application requires exactly a 64-character API key.
- **No Default Browser Configured:** If clicking or automatically opening a VirusTotal report fails, ensure a default web browser is set in Windows.
- **Build Errors:** If you encounter NuGet or build errors, ensure `.NET Framework 4.8 Developer Pack` is installed and you are using `msbuild` from a Visual Studio Developer Command Prompt. Linux builds are not supported for the main WinForms project.

## Warning
This is not an officially supported application. Please do not download it from any third-party sites other than this GitHub repository. Treat your VirusTotal API key as a sensitive secret.

## Contributing
If you have ideas for improvement, please [create a pull request](https://github.com/SamuelTulach/VirusTotalUploader/compare). If you encounter any bugs, [create an issue](https://github.com/SamuelTulach/VirusTotalUploader/issues/new).

## License
This project is licensed under the GPLv3 license. See [LICENSE.txt](LICENSE.txt) and [LICENSE_3rd.txt](LICENSE_3rd.txt) for details.
