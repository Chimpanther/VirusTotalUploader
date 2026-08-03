import re
with open("uploader/uploader/UploadForm.cs", "r") as f:
    content = f.read()

# Add CancellationTokenSource
content = content.replace("private Thread _uploadThread;", "private CancellationTokenSource _cancellationTokenSource;")
content = content.replace("using System.Windows.Forms;", "using System.Windows.Forms;\r\nusing System.Threading;\r\nusing System.Threading.Tasks;")

with open("uploader/uploader/UploadForm.cs", "w") as f:
    f.write(content)
