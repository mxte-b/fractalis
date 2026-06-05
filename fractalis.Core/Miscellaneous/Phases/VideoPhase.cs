using fractalis.Core.Numbers;
using fractalis.Core.Video;
using Spectre.Console;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record VideoPhaseResult(VideoMode VideoMode, DistributedRendererConfig? DistributedRendererSettings, VideoConfig VideoConfig);
    internal class VideoPhase : IPromptPhase<VideoPhaseResult>
    {
        private static DistributedRendererConfig ConfigureDistributedRenderer()
        {
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

            //var port = Prompts.TextValidated(
            //    $"What port [{ThemeColor.Accent}]port[/] should the frame listener use (1024-65535)?",

            //    choice => choice >= 1024 && choice <= 65535
            //        ? ValidationResult.Success() : ValidationResult.Error("[red]Must be a port in the range.[/]"),

            //    5059
            //);

            return new() { 
                OrchestratorUri = uri,
                //FrameListenerPort = port,
            };
        }

        private static VideoConfig ConfigureVideoRenderer()
        {
            var duration = Prompts.TextValidated<double>(
                $"[{ThemeColor.Accent}]Duration[/] of the video?",
                duration => duration > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be greater than zero.[/]"),
                60
            );

            var zoomStart = Prompts.TextValidated<BigFloat>(
                $"[{ThemeColor.Accent}]Starting[/] zoom value?",
                zoom => zoom > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be greater than zero.[/]"),
                BigFloat.One
            );

            var zoomEnd = Prompts.TextValidated<BigFloat>(
                $"[{ThemeColor.Accent}]Final[/] zoom value?",
                zoom => zoom > zoomStart ? ValidationResult.Success() : ValidationResult.Error("[red]Must be greater than the starting zoom value.[/]"),
                new BigFloat(1e20)
            );

            return new()
            {
                Duration = duration,
                ZoomStart = zoomStart,
                ZoomEnd = zoomEnd
            };
        }

        public VideoPhaseResult Run()
        {
            Prompts.Section("Video rendering");

            var videoMode = Prompts.Selection(
                $"[{ThemeColor.Accent}]How[/] should the video be rendered?",
                ["Local rendering", "Distributed rendering"]
            ).Convert(choice => choice switch
            {
                "Local rendering" => VideoMode.Local,
                "Distributed rendering" => VideoMode.Distributed,
                _ => VideoMode.Local
            });

            var distributedSettings = videoMode == VideoMode.Distributed ? ConfigureDistributedRenderer() : null;
            var videoConfig = ConfigureVideoRenderer();

            Prompts.Done();

            return new(videoMode, distributedSettings, videoConfig);
        }
    }
}
