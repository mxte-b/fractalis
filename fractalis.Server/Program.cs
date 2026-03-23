using fractalis.Core.Distributed;

namespace fractalis.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Orchestrator orchestrator = new Orchestrator();
            var builder = WebApplication.CreateBuilder(args);
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
                    await Orchestrator.Echo(websocket);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });

            Console.WriteLine("<#> Orchestrator running...");
            Console.WriteLine("    - WebSocket server running at ws://localhost:5059/ws");
            app.Run();
        }
    }
}
