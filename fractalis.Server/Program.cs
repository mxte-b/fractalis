using fractalis.Core.Distributed.Orchestrator;

namespace fractalis.ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Orchestrator orchestrator = new Orchestrator();
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
            builder.WebHost.UseUrls("http://localhost:5059");

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
