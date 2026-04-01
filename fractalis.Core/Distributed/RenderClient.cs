using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace fractalis.Core.Distributed
{
    public class RenderClient
    {
        private ServerConnection?           _connection;
        private readonly ClientRuntime      _runtime = new ClientRuntime();
        private static readonly TimeSpan    RegistrationTimeout = TimeSpan.FromSeconds(10);
        public bool Connected   { get; private set; } = false;

        public async Task Connect(Uri uri, string displayName)
        {
            ClientWebSocket ws = new ClientWebSocket();

            await ws.ConnectAsync(uri, default);
            if (ws.State != WebSocketState.Open)
            {
                return;
            }

            _connection = await ServerConnection.RegisterAsync(displayName, ws, RegistrationTimeout);
            if (_connection != null)
            {
                Connected = true;
            }
        }

        public async Task Start()
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Cannot start client because it is not connected.");
            }

            await _connection!.ListenAsync(async (message, _) => 
            {
                if (message != null)
                {
                    await _runtime.HandleMessage(message);
                } 

                return MessageHandlingResult.Continue;
            }, CancellationToken.None);
        }

        public async Task SendMessageToServerAsync(Message message)
        {
            if (!Connected) return;

            await _connection!.SendMessageAsync(message);
        }

        public async Task Disconnect()
        {
            if (!Connected) return;

            await _connection!.Close();
        }
    }
}
