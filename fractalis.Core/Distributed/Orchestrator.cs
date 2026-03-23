using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// This record uniquely represents a render client's connection.
    /// </summary>
    /// <param name="id">A randomly generated GUID.</param>
    /// <param name="displayName">A user-chosen display name</param>
    public record ClientConnection()
    {
        public required Guid        Id          { get; init; }
        public required string      DisplayName { get; init; }
        public required WebSocket   Socket      { get; init; }
    }

    public class Orchestrator
    {
        public ConcurrentDictionary<Guid, ClientConnection> Clients = [];

        public ClientConnection RegisterClient(WebSocket socket, string displayName)
        {
            ClientConnection c = new ClientConnection()
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName,
                Socket = socket,
            };

            Clients.TryAdd(c.Id, c);

            return c;
        }

        public void UnregisterClient(Guid id)
        {
            Clients.TryRemove(id, out _);
        }

        public static async Task Echo(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];
            Console.WriteLine("New client connected! Awaiting registration...");
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
