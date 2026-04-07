using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Represents a client connected to the server via WebSocket.
    /// </summary>
    /// <remarks>
    /// Stored on the server to track active clients. Contains the client’s display name, unique ID, and socket.
    /// Instances are created via <see cref="NegotiateAsync"/> when a client connects or registers.
    /// </remarks>
    public class ClientConnection : Connection
    {
        /// <summary>
        /// Display name of the connected client.
        /// </summary>
        public required string      DisplayName { get; init; }
        
        /// <summary>
        /// Role assigned to the client, determining its permissions and behavior on the server.
        /// </summary>
        public required ClientRole  Role        { get; init; }

        private ClientConnection() { }

        /// <summary>
        /// Performs initial negotiation with a client over WebSocket.
        /// </summary>
        /// <param name="socket">WebSocket to communicate with.</param>
        /// <param name="timeout">Timeout for negotiation.</param>
        /// <returns>
        /// A fully initialized <see cref="ClientConnection"/> if negotiation succeeded; otherwise, <see langword="null"/>.
        /// </returns>
        public static async Task<ClientConnection?> NegotiateAsync(WebSocket socket, TimeSpan timeout)
        {
            ClientConnection? connection = null;
            WebSocketMessageListener messageListener = new(socket);

            CancellationTokenSource source = new();
            source.CancelAfter(timeout);

            try
            {
                await messageListener.ListenAsync((message, socket) =>
                {
                    switch (message)
                    {
                        case RegistrationMessage reg:
                            connection = new ClientConnection()
                            {
                                DisplayName = reg.DisplayName,
                                Id = Guid.NewGuid(),
                                Socket = socket,
                                Role = reg.Role
                            };
                            return MessageHandlingResult.Stop;

                        case ReconnectMessage rec:
                            throw new NotImplementedException();

                        default:
                            return MessageHandlingResult.Continue;
                    }
                }, source.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            if (connection is null) return null;

            await connection.SendMessageAsync(new RegistrationAcknowledgedMessage() { ClientId = connection.Id });

            return connection;
        }
    }
}
