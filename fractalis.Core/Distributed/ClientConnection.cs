using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public class ClientConnection : Connection
    {
        public required string DisplayName { get; init; }

        private ClientConnection() { }

        public static async Task<ClientConnection?> NegotiateAsync(WebSocket socket, TimeSpan timeout)
        {
            ClientConnection? connection = null;
            WebSocketMessageListener messageListener = new(socket);

            // Wait for registration or reconnection with timeout
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
