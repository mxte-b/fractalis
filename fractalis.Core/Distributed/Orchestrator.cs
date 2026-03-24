using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
        public readonly static TimeSpan                     RegistrationTimeout = TimeSpan.FromSeconds(10);
        public ConcurrentDictionary<Guid, ClientConnection> Clients             = [];

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

        private static async Task CloseConnection(WebSocket socket)
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                null,
                CancellationToken.None
            );
        }

        private static async Task CloseConnection(WebSocket socket, WebSocketReceiveResult result)
        {
            await socket.CloseAsync(
                result.CloseStatus != null ? result.CloseStatus.Value : WebSocketCloseStatus.NormalClosure,
                result.CloseStatusDescription,
                CancellationToken.None
            );
        }

        public async Task Listen(WebSocket webSocket, Func<string, WebSocket, bool> callback, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnection(webSocket, result);
                    break;
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // If the registration/reconnection is successful, return
                    if (callback(message, webSocket)) break;
                }
            }
        }

        public async Task HandleClient(WebSocket webSocket)
        {
            Console.WriteLine("<#> New client connected! Awaiting registration...");

            // Wait for registration or reconnection with timeout
            CancellationTokenSource source = new();
            source.CancelAfter(RegistrationTimeout);
            try
            {
                await Listen(webSocket, (m, _) => 
                {
                    Console.WriteLine($"Got message: {m}");
                    JsonDocument doc = JsonDocument.Parse(m);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("type", out JsonElement typeProp)) return false;
                    if (!Enum.TryParse(typeProp.ToString(), true, out MessageType type)) return false;

                    switch (type)
                    {
                        case MessageType.Registration:
                            RegistrationMessage reg = JsonSerializer.Deserialize<RegistrationMessage>(m)!;
                            RegisterClient(webSocket, reg.DisplayName);

                            // Remove timeout
                            source.Dispose();
                            break;
                        case MessageType.Reconnect:
                            ReconnectMessage rec = JsonSerializer.Deserialize<ReconnectMessage>(m)!;

                            // Remove timeout
                            source.Dispose();
                            break;
                        default: 
                            return false;
                    }

                    return true;
                }, source.Token);       
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   - Client didn't send registration request in time, closing connnection.");
            }

            // Listen for job polls
            Console.WriteLine("Successful registration, awaiting messages");
            await Listen(webSocket, (m, _) =>
            {
                Console.WriteLine($"Got message: {m}");
                return false;
            }, CancellationToken.None);
        }
    }
}
