using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    internal class WebSocketMessageListener
    {
        private WebSocket _socket;
        public WebSocketMessageListener(WebSocket socket)
        {
            _socket = socket;
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

        public async Task ListenAsync(Func<Message?, WebSocket, bool> callback, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnection(_socket, result);
                    break;
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Message? parsed = JsonSerializer.Deserialize<Message>(message);

                    if (callback.Invoke(parsed, _socket)) break;
                }
            }
        }
    }
}
