using fractalis.Core.Distributed.Clients;
using System.Net.WebSockets;

namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Represents a connection from the client to a server.
    /// </summary>
    /// <remarks>
    /// Stored on the client to communicate with a remote server via WebSocket.
    /// Handles registration/handshake and provides a WebSocket for sending messages to the server.
    /// </remarks>
    internal class ServerConnection : Connection
    {
        private ServerConnection() { }

        private static async Task SendMessageInternal(Message message, WebSocket socket, CancellationToken cancellationToken)
        {
            byte[] bytes = MessageSerializer.Serialize(message);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// Performs a handshake and registers the client with the server.
        /// </summary>
        /// <param name="displayName">The client's display name to present to the server.</param>
        /// <param name="socket">The WebSocket for communication with the server.</param>
        /// <param name="timeout">Timeout for the registration handshake.</param>
        /// <returns>
        /// A <see cref="ServerConnection"/> representing the server connection if registration succeeds; otherwise, <see langword="null"/>.
        /// </returns>
        public static async Task<ServerConnection?> RegisterAsync(WebSocket socket, string displayName, ClientRole role, TimeSpan timeout)
        {
            ServerConnection? connection = null;
            WebSocketMessageListener messageListener = new(socket);

            CancellationTokenSource source = new();
            source.CancelAfter(timeout);

            // Send registration request
            await SendMessageInternal(new RegistrationMessage() { DisplayName = displayName, Role = role }, socket, source.Token);

            // Listen for registration acknowledgement
            await messageListener.ListenAsync((message) =>
            {
                if (message is RegistrationAcknowledgedMessage ack)
                {
                    connection = new ServerConnection()
                    {
                        Id = ack.ClientId,
                        Socket = socket
                    };

                    return MessageHandlingResult.Stop;
                }

                return MessageHandlingResult.Continue;
            }, source.Token);

            return connection;
        }
    }
}
