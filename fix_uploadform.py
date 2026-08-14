import sys
content = open('uploader/uploader/UploadForm.cs').read()

new_content = []
lines = content.split('\n')
skip = False
for i, line in enumerate(lines):
    if line.startswith('<<<<<<< HEAD'):
        skip = True
        new_content.extend([
            "            try",
            "            {",
            "                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)",
            "                {",
            "                    var info = new ProcessStartInfo { FileName = uri.AbsoluteUri, UseShellExecute = true };",
            "                    Process.Start(info);",
            "                }",
            "            }",
            "            catch (Exception ex)",
            "            {",
            "                // Process.Start can throw e.g. Win32Exception if there is no default handler for HTTP/HTTPS URLs.",
            "                // Silently ignoring is safer than crashing the background thread.",
            "                Debug.WriteLine($\"Failed to open URL: {ex.Message}\");"
        ])
    elif line.startswith('>>>>>>> origin/master'):
        skip = False
    elif not skip:
        new_content.append(line)

open('uploader/uploader/UploadForm.cs', 'w').write('\n'.join(new_content))
