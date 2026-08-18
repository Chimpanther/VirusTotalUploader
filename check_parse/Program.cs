using System;
using System.IO;

class Program
{
    static void Main()
    {
        string[] lines = File.ReadAllLines("../uploader/uploader/UploadForm.cs");
        int brackets = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            for (int j = 0; j < line.Length; j++) {
                if (line[j] == '{') brackets++;
                else if (line[j] == '}') brackets--;
            }
        }
        Console.WriteLine($"Final brackets: {brackets}");
    }
}
