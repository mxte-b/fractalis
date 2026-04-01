using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    internal class ServerConnection : Connection
    {
        private ServerConnection() { }

        private static async Task SendMessageInternal(Message message, WebSocket socket, CancellationToken cancellationToken)
        {
            byte[] bytes = MessageSerializer.Serialize(message);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }

        public static async Task<ServerConnection?> RegisterAsync(string displayName, WebSocket socket, TimeSpan timeout)
        {
            ServerConnection? connection = null;
            WebSocketMessageListener messageListener = new(socket);

            CancellationTokenSource source = new();
            source.CancelAfter(timeout);

            // Send registration request
            await SendMessageInternal(new RegistrationMessage() { DisplayName = displayName }, socket, source.Token);

            // Listen for registration acknowledgement
            await messageListener.ListenAsync((message, socket) =>
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
