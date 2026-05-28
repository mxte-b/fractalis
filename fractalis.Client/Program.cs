using fractalis.Core;
using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Miscellaneous;
using System.Text;

namespace fractalis.ClientApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(Banner.V1);
            ClientSettings settings = ClientConfigurator.Configure(args);

            WorkerClient client = new(settings.ProcessorUsageLimit);

            // Initiate server connection
            Console.WriteLine("<#> Establishing connection to the Orchestrator...");
            try
            {
                await client.Connect(settings.OrchestratorUri, settings.DisplayName);
            }
            catch
            {
                Prompts.Warn("Couldn't establish a connection to the server. Please check the settings and try again.");
                return;
            }

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
