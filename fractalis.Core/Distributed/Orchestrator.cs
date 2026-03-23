using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public class Orchestrator
    {
        public static async Task Echo(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(
                        result.CloseStatus != null ? result.CloseStatus.Value : WebSocketCloseStatus.NormalClosure, 
                        result.CloseStatusDescription, 
                        CancellationToken.None
                    );
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine($"Orchestrator received message: {message}");

                    byte[] response = Encoding.UTF8.GetBytes("Echo: " + message);

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(response),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
        }
    }
}
