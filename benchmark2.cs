using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        int numFiles = 10;
        Console.WriteLine($"Simulating {numFiles} file uploads...");

        // Synchronous (Simulated)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < numFiles; i++)
        {
            SimulateSync();
        }
        sw.Stop();
        Console.WriteLine($"[BASELINE] Synchronous I/O took {sw.ElapsedMilliseconds} ms");

        // Parallel Async (Simulated)
        sw.Restart();
        var tasks = new List<Task>();
        for (int i = 0; i < numFiles; i++)
        {
            tasks.Add(Task.Run(() => SimulateAsync()));
        }
        await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"[OPTIMIZED] Parallel Async I/O took {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Speedup factor: " + (double)(numFiles * 1000) / sw.ElapsedMilliseconds + "x");
    }

    static void SimulateSync()
    {
        Thread.Sleep(500); // report
        Thread.Sleep(500); // scan
    }

    static async Task SimulateAsync()
    {
        await Task.Delay(500); // report
        await Task.Delay(500); // scan
    }
}
