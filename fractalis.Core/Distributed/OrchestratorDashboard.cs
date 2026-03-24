using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    internal class OrchestratorDashboard
    {
        private static readonly OrchestratorDashboard           _instance   = new();
        private ConcurrentQueue<string>                         _logs       = new();
        private ConcurrentDictionary<Guid, ClientConnection>?   _clients;
        private Layout                                          _layout;
        private readonly Panel                                  _header     = new Panel(Banner.V1 + "\n[bold]Orchestrator Dashboard[/]").Border(BoxBorder.Rounded);
        private static readonly int                             _maxLogs    = 8;
        public static OrchestratorDashboard                     Instance    => _instance;
        
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

        public void Initialize(ConcurrentDictionary<Guid, ClientConnection> clients)
        {
            _clients = clients;
        }

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

        public void AddLog(string message)
        {
            _logs.Enqueue($"[grey]{DateTime.Now:HH:mm:ss}[/] {message}");

            while (_logs.Count > _maxLogs) _logs.TryDequeue(out _);
        }

        public void AddLog(ClientConnection connection, string message)
        {
            _logs.Enqueue($"[grey]{DateTime.Now:HH:mm:ss}[/] [cyan]{connection.DisplayName}[/] [grey]({connection.Id.ToString()[..8]}) [/]{message}");

            while (_logs.Count > _maxLogs) _logs.TryDequeue(out _);
        }

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

        private Panel BuildLogPanel()
        {
            var text = string.Join("\n", _logs);
            if (text.Length == 0) text = "[grey]No logs to show[/]";
            return new Panel(text).Header("[bold]Logs[/]").Border(BoxBorder.Rounded).Expand();
        }
    }
}
