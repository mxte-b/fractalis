using System.Collections.Concurrent;
using System.Data;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace fractalis.Core.Distributed
{
    public class Orchestrator
    {
        private readonly OrchestratorDashboard              _dashboard          = OrchestratorDashboard.Instance;
        public readonly static TimeSpan                     RegistrationTimeout = TimeSpan.FromSeconds(10);
        public ConcurrentDictionary<Guid, ClientConnection> Clients             = [];

        public Orchestrator()
        {
            _dashboard.Initialize(Clients);
            _dashboard.Start();
        }   

        public ClientConnection ReconnectClient(WebSocket socket, Guid clientId)
        {
            //if (!Clients.TryGetValue(clientId, out ClientConnection? c))
            //{
            //    RegisterClient(socket)
            //}

            throw new NotImplementedException();
        }

        public async Task BroadcastMessage(Message message)
        {
            var tasks = Clients.Values.Select(c => c.SendMessageAsync(message));
            await Task.WhenAll(tasks);
        }

        public async Task HandleClient(WebSocket webSocket)
        {
            _dashboard.AddLog($"New client connected! ");

            // Wait for client registration
            ClientConnection? connection = await ClientConnection.NegotiateAsync(webSocket, RegistrationTimeout);
            if (connection is null)
            {
                _dashboard.AddLog("   - Client registration timed out.");
                return;
            }

            // Register the connection
            Clients.TryAdd(connection.Id, connection);
            _dashboard.AddLog(connection, "Registered.");

            // Listen for job polls
            ConnectionCloseReason reason = await connection.ListenAsync((m, _) =>
            {
                if (m == null)
                {
                    _dashboard.AddLog(connection, "Received invalid message");
                }

                _dashboard.AddLog(connection, "Received a good message");
                return false;
            }, CancellationToken.None);

            if (reason != ConnectionCloseReason.NormalClosure)
            { 
                _dashboard.AddLog(connection, "Disconnected unexpectedly.");
            }

            Clients.TryRemove(connection.Id, out _);
        }
    }
}
