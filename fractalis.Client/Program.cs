using fractalis.Core;
using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking;

namespace fractalis.ClientApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Banner.V1);

            string displayName = "User's Laptop";
            Uri uri = new Uri("ws://localhost:5059/ws");

            WorkerClient client = new WorkerClient();

            // Initiate server connection
            Console.WriteLine("<#> Establishing connection to the Orchestrator...");
            await client.Connect(uri, displayName);
            if (client.Connected)
            {
                Console.WriteLine("   - Done!");
            }
            else
            {
                throw new Exception("Couldn't establish a connection to the server.");
            }

            _ = Task.Run(client.Start);

            // Test echo
            string? message = null;
            while (true)
            {
                message = Console.ReadLine();
                if (message == "") break;

                await client.SendMessageToServerAsync(new DebugMessage() { Content = message });
            }
        }
    }
}
