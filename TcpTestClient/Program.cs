using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

class TcpTestClient
{
    static async Task Main()
    {
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 5000);
        var stream = client.GetStream();

        Console.WriteLine("Running Stream benchmark...");
        var sw1 = Stopwatch.StartNew();
        long total1 = await ReadWithStreamAsync(stream);
        sw1.Stop();
        Console.WriteLine($"Stream: {sw1.ElapsedMilliseconds}ms, {total1 / 100_000} MB read");

        client.Close();

        // دوباره وصل می‌شویم برای Pipeline
        using var client2 = new TcpClient();
        await client2.ConnectAsync("127.0.0.1", 5000);
        var stream2 = client2.GetStream();

        Console.WriteLine("Running Pipelines benchmark...");
        var sw2 = Stopwatch.StartNew();
        long total2 = await ReadWithPipelinesAsync(stream2);
        sw2.Stop();
        Console.WriteLine($"Pipelines: {sw2.ElapsedMilliseconds}ms, {total2 / 100_000} MB read");
        Console.ReadKey();
    }

    static async Task<long> ReadWithStreamAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[8192];
        long total = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            total += read;
            _ = buffer[0]; // شبیه‌سازی پردازش
        }
        return total;
    }

    static async Task<long> ReadWithPipelinesAsync(NetworkStream stream)
    {
        var pipe = new Pipe();
        long total = 0;

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

        var reading = Task.Run(async () =>
        {
            while (true)
            {
                var result = await pipe.Reader.ReadAsync();
                var buffer = result.Buffer;
                total += buffer.Length;

                // پردازش ساده بدون کپی
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
