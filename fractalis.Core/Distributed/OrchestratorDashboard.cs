using Spectre.Console;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace fractalis.Core.Distributed
{
    /// <summary>
    /// Singleton dashboard for monitoring and interacting with connected clients
    /// in the orchestrator system.
    /// </summary>
    /// <remarks>
    /// Provides a live console UI using Spectre.Console, showing connected client
    /// status and recent log messages. Thread-safe for log updates and supports
    /// concurrent client tracking.
    /// </remarks>
    internal class OrchestratorDashboard
    {
        private static readonly OrchestratorDashboard           _instance   = new();
        private ConcurrentQueue<string>                         _logs       = new();
        private ConcurrentDictionary<Guid, ClientConnection>?   _clients;
        private Layout                                          _layout;
        private readonly Panel                                  _header     = new Panel(Banner.V1 + "\n[bold]Orchestrator Dashboard[/]").Border(BoxBorder.Rounded);
        private static readonly int                             _maxLogs    = 8;

        /// <summary>
        /// Gets the singleton instance of the <see cref="OrchestratorDashboard"/>.
        /// </summary>
        public static OrchestratorDashboard                     Instance    => _instance;

        /// <summary>
        /// Initializes the layout and header for the console dashboard.
        /// </summary>
        private OrchestratorDashboard()
        {
            _layout = new Layout().SplitRows(
                new Layout("header"),
                new Layout("top"),
                new Layout("bottom")
            );

            _layout["header"].Size(10);
            _layout["top"].Ratio(3);
            _layout["bottom"].Size(10);
        }

        /// <summary>
        /// Initializes the dashboard with the concurrent dictionary of connected clients.
        /// </summary>
        /// <param name="clients">A thread-safe dictionary containing client connections keyed by GUID.</param>
        public void Initialize(ConcurrentDictionary<Guid, ClientConnection> clients)
        {
            _clients = clients;
        }

        /// <summary>
        /// Starts the live console UI to display client statuses and recent logs.
        /// </summary>
        /// <remarks>
        /// This method runs asynchronously and continuously refreshes the console every 100ms.
        /// </remarks>
        public void Start()
        {
            _ = Task.Run(() =>
            {
                AnsiConsole.Live(_layout)
                    .Start(ctx =>
                    {
                        _layout["header"].Update(Align.Center(_header));

                        while (true)
                        {
                            _layout["top"].Update(BuildTable());
                            _layout["bottom"].Update(BuildLogPanel());

                            ctx.Refresh();
                            Thread.Sleep(100);
                        }
                    });
            });
        }

        /// <summary>
        /// Adds a timestamped log message to the dashboard.
        /// </summary>
        /// <param name="message">The log message text.</param>
        public void AddLog(string message)
        {
            _logs.Enqueue($"[grey]{DateTime.Now:HH:mm:ss}[/] {message}");

            while (_logs.Count > _maxLogs) _logs.TryDequeue(out _);
        }

        /// <summary>
        /// Adds a timestamped log message associated with a specific client.
        /// </summary>
        /// <param name="connection">The client connection related to the log.</param>
        /// <param name="message">The log message text.</param>
        public void AddLog(ClientConnection connection, string message)
        {
            _logs.Enqueue($"[grey]{DateTime.Now:HH:mm:ss}[/] [cyan]{connection.DisplayName}[/] [darkorange][[{connection.Role}]][/] [grey]({connection.Id.ToString()[..8]}) [/]{message}");

            while (_logs.Count > _maxLogs) _logs.TryDequeue(out _);
        }

        /// <summary>
        /// Builds a <see cref="Table"/> representing all connected clients.
        /// </summary>
        /// <returns>A table with columns for GUID, display name, status, and state.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the dashboard has not been initialized with clients.</exception>
        private Table BuildTable()
        {
            if (_clients == null) throw new InvalidOperationException("Dashboard is not initialized.");

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold]Clients[/]")
                .Caption($"[grey]Refreshed: {DateTime.Now:HH:mm:ss}[/]")
                .Expand();

            table.AddColumn(new TableColumn("[bold]GUID[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Display Name[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
            table.AddColumn(new TableColumn("[bold]State[/]").Centered());

            foreach (var c in _clients.Values)
            {
                bool isOpen = c.Socket.State == WebSocketState.Open;

                string status = isOpen ? "[DarkOliveGreen2]● Online[/]" : "[red]○ Offline[/]";
                string state = isOpen ? "[grey]Connected[/]" : $"[grey]{c.Socket.State}[/]";

                table.AddRow(
                    $"[grey]{c.Id.ToString()[..8]}[/]",
                    $"[cyan]{c.DisplayName}[/]",
                    status,
                    state
                );
            }

            if (_clients.IsEmpty)
            {
                table.AddRow("[grey]-[/]", "[grey]No clients connected[/]", "[grey]-[/]", "[grey]-[/]");
            }

            return table;
        }

        /// <summary>
        /// Builds a <see cref="Panel"/> displaying recent log messages.
        /// </summary>
        /// <returns>A panel with the last <c>_maxLogs</c> messages, or a placeholder if no logs exist.</returns>
        private Panel BuildLogPanel()
        {
            var text = string.Join("\n", _logs);
            if (text.Length == 0) text = "[grey]No logs to show[/]";
            return new Panel(text).Header("[bold]Logs[/]").Border(BoxBorder.Rounded).Expand();
        }
    }
}