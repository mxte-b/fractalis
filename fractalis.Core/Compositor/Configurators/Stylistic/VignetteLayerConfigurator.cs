using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class VignetteLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(VignetteLayer);

        public CompositeLayer Configure()
        {
            var strength = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Strength[/] of the vignette effect?",
                s => s >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                10f
            );

            var extent = Prompts.TextValidated(
                $"[{ThemeColor.Accent}]Extent[/] of the vignette effect?",
                s => s >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                0.9f
            );

            return new VignetteLayer(strength, extent);
        }
    }
}
