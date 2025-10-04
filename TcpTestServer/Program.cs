using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class TcpTestServer
{
    static async Task Main()
    {
        var listener = new TcpListener(IPAddress.Loopback, 5000);
        listener.Start();
        Console.WriteLine("TCP Server started on port 5000");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClient(client);
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        var rnd = new Random();
        byte[] buffer = new byte[8192];

        // ارسال 500MB داده تصادفی
        long total = 0;
        while (total < 500_000_000)
        {
            rnd.NextBytes(buffer);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            total += buffer.Length;
        }

        client.Close();
    }
}
