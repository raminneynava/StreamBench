using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Start...");

        // داده تستی: 100MB بایت تصادفی
        byte[] testData = new byte[100_000_000];
        new Random().NextBytes(testData);

        // Stream سنتی
        using var ms1 = new MemoryStream(testData);
        var sw1 = Stopwatch.StartNew();
        long total1 = await ReadWithStreamAsync(ms1);
        sw1.Stop();
        Console.WriteLine($"Stream: {sw1.ElapsedMilliseconds}ms, total read byte: {total1}");

        // Pipelines
        using var ms2 = new MemoryStream(testData);
        var sw2 = Stopwatch.StartNew();
        long total2 = await ReadWithPipelinesAsync(ms2);
        sw2.Stop();
        Console.WriteLine($"Pipelines: {sw2.ElapsedMilliseconds}ms, total read byte: {total2}");
    }

    // خواندن با Stream سنتی
    static async Task<long> ReadWithStreamAsync(Stream stream)
    {
        byte[] buffer = new byte[8192];
        long total = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            total += read;
            // شبیه‌سازی پردازش
            _ = buffer[0];
        }
        return total;
    }

    // خواندن با Pipelines
    static async Task<long> ReadWithPipelinesAsync(Stream stream)
    {
        var pipe = new Pipe();
        long total = 0;

        // Producer
        var writing = Task.Run(async () =>
        {
            while (true)
            {
                var memory = pipe.Writer.GetMemory(8192);
                int bytesRead = await stream.ReadAsync(memory);
                if (bytesRead == 0) break;
                pipe.Writer.Advance(bytesRead);
                var result = await pipe.Writer.FlushAsync();
                if (result.IsCompleted) break;
            }
            pipe.Writer.Complete();
        });

        // Consumer
        var reading = Task.Run(async () =>
        {
            while (true)
            {
                var result = await pipe.Reader.ReadAsync();
                var buffer = result.Buffer;
                total += buffer.Length;

                // پردازش ساده (بدون کپی)
                buffer = buffer.Slice(buffer.End);

                pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }
            pipe.Reader.Complete();
        });

        await Task.WhenAll(writing, reading);
        return total;
    }
}
