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
    /// <summary>
    /// Listens to incoming messages on a <see cref="WebSocket"/> and invokes callbacks for each message.
    /// </summary>
    public class WebSocketMessageListener
    {
        private readonly WebSocket _socket;

        /// <summary>
        /// Initializes a new <see cref="WebSocketMessageListener"/> for the given socket.
        /// </summary>
        /// <param name="socket">The <see cref="WebSocket"/> to listen on.</param>
        public WebSocketMessageListener(WebSocket socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// Starts listening for messages asynchronously.
        /// </summary>
        /// <param name="callback">
        /// A callback function invoked for each received message.
        /// Returning <see cref="MessageHandlingResult.Stop"/> will end the listener.
        /// </param>
        /// <param name="cancellationToken">Token to cancel the listener.</param>
        public async Task ListenAsync(Func<Message?, WebSocket, Task<MessageHandlingResult>> callback, CancellationToken cancellationToken)
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
                    try
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Message? parsed = JsonSerializer.Deserialize<Message>(message);

                        if (await callback.Invoke(parsed, _socket) == MessageHandlingResult.Stop) break;
                    }
                    catch (JsonException)
                    {
                        // Ignore non-JSON messages
                    }
                }
            }
        }

        /// <summary>
        /// Overload of <see cref="ListenAsync(Func{Message?,WebSocket,Task{MessageHandlingResult}},CancellationToken)"/> for synchronous callbacks.
        /// </summary>
        public async Task ListenAsync(Func<Message?, WebSocket, MessageHandlingResult> callback, CancellationToken cancellationToken)
        {
            await ListenAsync((message, socket) => Task.FromResult(callback(message, socket)), cancellationToken);
        }

        /// <summary>
        /// Closes the underlying WebSocket connection.
        /// </summary>
        private static async Task CloseConnection(WebSocket socket, WebSocketReceiveResult result)
        {
            await socket.CloseAsync(
                result.CloseStatus != null ? result.CloseStatus.Value : WebSocketCloseStatus.NormalClosure,
                result.CloseStatusDescription,
                CancellationToken.None
            );
        }
    }
}
