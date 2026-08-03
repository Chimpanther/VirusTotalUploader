with open("uploader/uploader/UploadForm.cs", "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace("using System.Windows.Forms;\nusing System.Threading;\nusing System.Threading.Tasks;", "using System.Windows.Forms;")

with open("uploader/uploader/UploadForm.cs", "w", encoding="utf-8") as f:
    f.write(content)
