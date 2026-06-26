using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class ZoomValueLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(ZoomValueLayer);

        private static string GetPositionName(string t) =>
            Regex.Replace(t, "(?<=[a-z])(?=[A-Z])", " ");

        public CompositeLayer Configure()
        {
            var position = Prompts.Selection(
                $"[{ThemeColor.Accent}]Position[/] of the overlay?",
                Enum.GetValues<Alignment>(),
                value => GetPositionName(value.ToString())
            );

            var opacity = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Opacity[/] of the overlay background?",
                o => o >= 0 && o <= 1
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be in the range 0-1.[/]"),
                0.5f
            );

            var scale = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Scale[/] of the overlay?",
                o => o >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                1f
            );

            return new ZoomValueLayer(scale, opacity, position);
        }
    }
}
