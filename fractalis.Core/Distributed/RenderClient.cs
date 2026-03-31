using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace fractalis.Core.Distributed
{
    public class RenderClient : IDisposable
    {
        private ClientWebSocket?            _ws;
        private WebSocketMessageListener?   _messageListener;
        public bool Connected   { get; private set; } = false;
        public bool Registered  { get; private set; } = false;

        public async Task Connect(Uri uri)
        {
            _ws = new();
            await _ws.ConnectAsync(uri, default);
            Connected = _ws.State == WebSocketState.Open;
        }

        public async Task Register(string displayName)
        {
            Message message = new RegistrationMessage()
            {
                DisplayName = displayName,
            };

            await SendMessageToServer(message);
        }

        public async Task Disconnect()
        {
            if (_ws == null || !Connected) return;
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default);
        }

        public async Task SendMessageToServer(Message message)
        {
            if (_ws is null || !Connected) return;

            byte[] bytes = MessageSerializer.Serialize(message);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, default);
        }

        public async Task Listen()
        {
            if (_ws == null || !Connected) return;

            _messageListener = new WebSocketMessageListener(_ws);

            try
            {
                await _messageListener.ListenAsync((message, _) =>
                {
                    Console.WriteLine(JsonSerializer.Serialize(message));
                    return false;
                }, default);
            }
            catch (WebSocketException)
            {
                Console.WriteLine("Lost connection to the server.");
                Connected = false;
            }
        }

        public async void Dispose()
        {
            await Disconnect();
            _ws?.Dispose();
            _ws = null;
            Connected = false;
        }

        ~RenderClient()
        {
            Dispose();
        }
    }
}
