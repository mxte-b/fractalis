using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public abstract class Connection
    {
        public required Guid                Id      { get; init; }
        public required WebSocket           Socket  { get; init; }

        protected WebSocketMessageListener? _messageListener;

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            byte[] bytes = MessageSerializer.Serialize(message);
            await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }

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

        public async Task<ConnectionCloseReason> ListenAsync(Func<Message?, WebSocket, MessageHandlingResult> callback, CancellationToken cancellationToken = default)
        {
            return await ListenAsync((message, socket) => Task.FromResult(callback(message, socket)), cancellationToken);
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
