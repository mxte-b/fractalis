using fractalis.Core.Distributed;
using System.Net.WebSockets;

namespace fractalis.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Uri uri = new Uri("ws://localhost:5059/ws");

            using RenderClient client = new RenderClient();
            await client.Connect(uri);

            Console.WriteLine($"Client state: {client.Connected}");
            _ = Task.Run(client.Listen);
            
            string? message = null;
            while (true)
            {
                message = Console.ReadLine();
                if (message == "") break;

                await client.ReportToOrchestrator(message);
            }

            
        }
    }
}
