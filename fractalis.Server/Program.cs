using fractalis.Core.Distributed.Orchestrator;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace fractalis.ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var localIp = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork))
                .Select(ni => ni.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses)
                .FirstOrDefault(a =>
                    a.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(a.Address))
                ?.Address.ToString() ?? "localhost";

            Orchestrator orchestrator = new Orchestrator($"ws://{localIp}:5059/ws");
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
            builder.WebHost.UseUrls($"http://{localIp}:5059");

            var app = builder.Build();

            WebSocketOptions options = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(1)
            };

            app.UseWebSockets(options);

            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var websocket = await context.WebSockets.AcceptWebSocketAsync();
                    await orchestrator.HandleClient(websocket);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });

            app.Run();
        }
    }
}