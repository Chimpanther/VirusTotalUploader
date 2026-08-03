using System;
using System.IO;

class Program {
    static void Main() {
        Console.WriteLine(File.ReadAllText("uploader/uploader/UploadForm.cs").Length);
    }
}
