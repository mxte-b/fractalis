using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;
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
            var path = Prompts.FilePath(
                $"[{ThemeColor.Accent}]Path[/] of the watermark image?",
                allowResources: true,
                defaultValue: "default:white",
                hint: "[grey]Hint: Use 'default:white' or 'default:black' for built-in watermarks.[/]",
                allowedFormats: AppConfigurator.ImageFormats,
                alsoAccept: ["default:white", "default:black"]
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
                    Enum.GetValues<Alignment>(),
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
