using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public class ClientConnection
    {
        public required Guid                Id          { get; init; }
        public required string              DisplayName { get; init; }

        public required WebSocket           Socket      { get; init; }

        private WebSocketMessageListener?   _messageListener;

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
                            return true;

                        case ReconnectMessage rec:
                            throw new NotImplementedException();

                        default:
                            return false;
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

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            byte[] bytes = MessageSerializer.Serialize(message);
            await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task<ConnectionCloseReason> ListenAsync(Func<Message?, WebSocket, bool> callback, CancellationToken cancellationToken = default)
        {
            _messageListener = new WebSocketMessageListener(Socket);

            try
            {
                await _messageListener.ListenAsync(callback, cancellationToken);
            }
            catch (WebSocketException)
            {
                return ConnectionCloseReason.Error;
            }
            catch (OperationCanceledException)
            {
                return ConnectionCloseReason.Cancelled;
            }

            return ConnectionCloseReason.NormalClosure;
        }

        public async Task Close()
        {
            await Socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                null,
                CancellationToken.None
            );
        }
    }
}
