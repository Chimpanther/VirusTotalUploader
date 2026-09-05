using System;
using System.IO;

class Program
{
    static void Main()
    {
        try
        {
            var stream = new FileStream("", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType().Name);
        }
    }
}
