using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class BloomLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(BloomLayer);

        public CompositeLayer Configure()
        {
            var intensity = Prompts.TextValidated<float>(
                $"Bloom [{ThemeColor.Accent}]intensity[/]?",
                b => b >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                1
            );

            var radius = Prompts.TextValidated(
                $"Blur kernel [{ThemeColor.Accent}]radius[/]?",
                b => b > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be greater than zero.[/]"),
                6
            );

            var start = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Lower[/] luminance threshold?",
                b => b >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                0.6f
            );

            var end = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Upper[/] luminance threshold?",
                b => b >= start
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be greater than the lower threshold.[/]"),
                0.8f
            );

            var sigma = Prompts.TextValidated<float>(
                $"Bloom [{ThemeColor.Accent}]sigma[/] value?",
                b => b > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be greater than zero.[/]"),
                8
            );

            return new BloomLayer(intensity, radius, start, end, sigma);
        }
    }
}
