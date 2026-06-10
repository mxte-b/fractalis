using Spectre.Console;
using System.Text.Json;

namespace fractalis.Core.Miscellaneous
{
    public static class ClientConfigurator
    {
        private static ClientSettings? LoadConfig(string path) => JsonSerializer.Deserialize<ClientSettings>(File.ReadAllText(path), FractalisJsonOptions.Default);

        public static ClientSettings Configure(string[] args)
        {
            AnsiConsole.MarkupLine($"[bold {ThemeColor.Title}]Welcome to the Fractalis Client Configurator![/]");
            AnsiConsole.WriteLine();

            Prompts.Section("Connection");

            var displayName = Prompts.Text<string>($"What should the [{ThemeColor.Accent}]display name[/] be?", DisplayNameGenerator.Generate());

            var uri = Prompts.TextValidated(
                $"What is the [{ThemeColor.Accent}]WebSocket URI[/] of the Orchestrator?",

                choice => {
                    bool valid =
                        Uri.TryCreate(choice, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == "ws" || uri.Scheme == "wss");

                    return valid ? ValidationResult.Success() : ValidationResult.Error("[red]Must be a valid WebSocket URI.[/]");
                },

                "ws://localhost:5059/ws"
            ).Convert(choice => new Uri(choice));

            var cpu = Prompts.Selection(
                $"What should the [{ThemeColor.Accent}]CPU usage limit[/] be?",
                [1, 0.75, 0.5, 0.25, 0.1],
                converter: x => $"{x:p0}");

            Prompts.Done();

            return new()
            {
                DisplayName = displayName,
                OrchestratorUri = uri,
                ProcessorUsageLimit = cpu,
            };
        }
    }
}
