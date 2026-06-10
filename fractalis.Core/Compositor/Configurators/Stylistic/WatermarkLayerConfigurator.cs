using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class WatermarkLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(WatermarkLayer);

        private static string GetPositionName(string t) =>
            Regex.Replace(t, "(?<=[a-z])(?=[A-Z])", " ");

        public CompositeLayer Configure()
        {
            var path = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Path[/] of the watermark image?",
                path => File.Exists(path) || path == "default:white" || path == "default:black"
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Please enter a valid path.[/]"),
                "default:white",
                "[grey]Hint: The default watermarks can be accessed by using 'default:white' or 'default:black' values below.[/]"
            ).Convert(path => 
                path.StartsWith("default:") 
                    ? path switch
                        {
                            "default:white" => Watermarks.FractalisWhite,
                            "default:black" => Watermarks.FractalisBlack,
                            _ => Watermarks.FractalisWhite,
                        } 
                    : path
            );

            WatermarkOptions? options = null;

            if (Prompts.Confirm($"[{ThemeColor.Accent}]Customize[/] watermark settings?"))
            {
                var position = Prompts.Selection(
                    $"[{ThemeColor.Accent}]Position[/] of the watermark?",
                    Enum.GetValues<WatermarkPosition>(),
                    value => GetPositionName(value.ToString())
                );

                var opacity = Prompts.TextValidated<float>(
                    $"[{ThemeColor.Accent}]Opacity[/] of the watermark?",
                    o => o >= 0 && o <= 1
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Must be in the range 0-1.[/]"),
                    0.5f
                );

                var scale = Prompts.TextValidated<float>(
                    $"[{ThemeColor.Accent}]Scale[/] of the watermark?",
                    o => o >= 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Must be non-negative.[/]"),
                    1
                );

                var marginX = Prompts.Text<int>($"[{ThemeColor.Accent}]Horizontal margin[/] value?");
                var marginY = Prompts.Text<int>($"[{ThemeColor.Accent}]Vertical margin[/] value?"); ;

                options = new()
                {
                    Position = position,
                    Opacity = opacity,
                    Scale = scale,
                    Margin = new(marginX, marginY)
                };
            }

            return new WatermarkLayer(path, options);
        }
    }
}
