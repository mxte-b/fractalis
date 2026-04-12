using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace fractalis.Core.Distributed.Networking
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
        public async Task ListenAsync(Func<Message?, Task<MessageHandlingResult>> callback, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            using MemoryStream ms = new();

            while (_socket.State == WebSocketState.Open)
            {
                ms.SetLength(0);

                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(buffer, cancellationToken);
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnection(_socket, result);
                    break;
                }

                try
                {
                    string message = Encoding.UTF8.GetString(ms.ToArray());
                    Message? parsed = MessageSerializer.Deserialize(message);
                    if (await callback.Invoke(parsed) == MessageHandlingResult.Stop) break;
                }
                catch (JsonException)
                {
                    // Ignore non-JSON messages
                }
            }
        }

        /// <summary>
        /// Overload of <see cref="ListenAsync(Func{Message?,WebSocket,Task{MessageHandlingResult}},CancellationToken)"/> for synchronous callbacks.
        /// </summary>
        public async Task ListenAsync(Func<Message?, MessageHandlingResult> callback, CancellationToken cancellationToken)
        {
            await ListenAsync((message) => Task.FromResult(callback(message)), cancellationToken);
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
