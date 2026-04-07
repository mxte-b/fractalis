using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Represents a base WebSocket connection with messaging capabilities.
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        /// Unique identifier for the connection.
        /// </summary>
        public required Guid                Id      { get; init; }

        /// <summary>
        /// The underlying WebSocket for this connection.
        /// </summary>
        public required WebSocket           Socket  { get; init; }

        protected WebSocketMessageListener? _messageListener;

        /// <summary>
        /// Sends a message asynchronously to the connected client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            byte[] bytes = MessageSerializer.Serialize(message);
            await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// Starts listening for incoming messages asynchronously.
        /// </summary>
        /// <param name="callback">Callback invoked for each received message.</param>
        /// <param name="cancellationToken">Token to cancel the listener.</param>
        /// <returns>Reason why the connection ended.</returns>
        public async Task<ConnectionCloseReason> ListenAsync(Func<Message?, WebSocket, Task<MessageHandlingResult>> callback, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Overload of <see cref="ListenAsync(Func{Message?,WebSocket,Task{MessageHandlingResult}},CancellationToken)"/> for synchronous callbacks.
        /// </summary>
        public async Task<ConnectionCloseReason> ListenAsync(Func<Message?, WebSocket, MessageHandlingResult> callback, CancellationToken cancellationToken = default)
        {
            return await ListenAsync((message, socket) => Task.FromResult(callback(message, socket)), cancellationToken);
        }

        /// <summary>
        /// Closes the WebSocket connection gracefully.
        /// </summary>
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
