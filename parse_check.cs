using System;
using System.IO;

class Program {
    static void Main() {
        var code = File.ReadAllText("uploader/uploader/UploadForm.cs");
        Console.WriteLine(code.Length);
    }
}
