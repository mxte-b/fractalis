using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace fractalis.Core.Distributed
{
    public class RenderClient : IDisposable
    {
        private ClientWebSocket? _ws;
        public bool Connected { get; private set; } = false;
        public bool Registered { get; private set; } = false;

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
            await SendRawStringToServer(JsonSerializer.Serialize(message));
        }

        public async Task SendRawStringToServer(string message)
        {
            if (_ws == null || !Connected) return;

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, default);
        }

        public async Task Listen()
        {
            if (_ws == null || !Connected) return;

            byte[] buffer = new byte[1024];

            try
            {
                while (Connected)
                {
                    var result = await _ws.ReceiveAsync(buffer, default);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(
                            result.CloseStatus != null ? result.CloseStatus.Value : WebSocketCloseStatus.NormalClosure,
                            result.CloseStatusDescription,
                            CancellationToken.None
                        );
                        Connected = false;
                        break;
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        Console.WriteLine($"Client received message: {message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"WebSocket error: {e}");
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
