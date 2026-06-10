using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Tone;
using fractalis.Core.Miscellaneous;
using Spectre.Console;

namespace fractalis.Core.Compositor.Configurators.Tone
{
    internal class ContrastLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(ContrastLayer);

        public CompositeLayer Configure()
        {
            var contrast = Prompts.TextValidated<float>(
                $"Desired [{ThemeColor.Accent}]contrast[/]?",
                b => b >= 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be non-negative.[/]"),
                1);

            return new ContrastLayer(contrast);
        }
    }
}
