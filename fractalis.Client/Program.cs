using fractalis.Core;
using fractalis.Core.Distributed;
using System.Net.WebSockets;

namespace fractalis.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(Banner.V1);

            string displayName = "User's Laptop";
            Uri uri = new Uri("ws://localhost:5059/ws");

            using RenderClient client = new RenderClient();

            // Initiate server connection
            Console.WriteLine("<#> Establishing connection to the Orchestrator...");
            await client.Connect(uri);
            if (client.Connected)
            {
                Console.WriteLine("   - Done!");
            }
            else
            {
                throw new Exception("Couldn't establish a connection to the server.");
            }

            // Start listen loop
            _ = Task.Run(client.Listen);

            // Register this client to the server
            Console.WriteLine("<#> Sending registration request...");
            await client.Register(displayName);
            if (client.Registered)
            {
                Console.WriteLine("   - Registration confirmed!");
            }

            // Poll currently available jobs and start asking for work

            // Test echo
            string? message = null;
            while (true)
            {
                message = Console.ReadLine();
                if (message == "") break;

                await client.SendRawStringToServer(message);
            }

            client.Dispose();
        }
    }
}
