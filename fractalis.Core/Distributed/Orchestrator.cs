using SixLabors.ImageSharp.Processing.Processors.Transforms;
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

        public ClientConnection ReconnectClient(WebSocket socket, Guid clientId)
        {
            //if (!Clients.TryGetValue(clientId, out ClientConnection? c))
            //{
            //    RegisterClient(socket)
            //}

            throw new NotImplementedException();
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
            ClientConnection? currentConnection = null;

            // Wait for registration or reconnection with timeout
            CancellationTokenSource source = new();
            source.CancelAfter(RegistrationTimeout);
            try
            {
                await Listen(webSocket, (m, _) => 
                {
                    JsonDocument doc = JsonDocument.Parse(m);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("type", out JsonElement typeProp)) return false;
                    if (!Enum.TryParse(typeProp.ToString(), true, out MessageType type)) return false;

                    switch (type)
                    {
                        case MessageType.Registration:
                            RegistrationMessage reg = JsonSerializer.Deserialize<RegistrationMessage>(m)!;
                            currentConnection = RegisterClient(webSocket, reg.DisplayName);

                            // Remove timeout
                            source.Dispose();
                            break;
                        case MessageType.Reconnect:
                            ReconnectMessage rec = JsonSerializer.Deserialize<ReconnectMessage>(m)!;
                            currentConnection = ReconnectClient(webSocket, rec.ClientId);

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
                Console.WriteLine("   - Client registration timed out.");
            }

            if (currentConnection == null) return;

            // Listen for job polls
            Console.WriteLine("Successful registration, awaiting messages");
            await Listen(webSocket, (m, _) =>
            {
                Console.WriteLine($"[{currentConnection.Id}]: {m}");
                return false;
            }, CancellationToken.None);
        }
    }
}
