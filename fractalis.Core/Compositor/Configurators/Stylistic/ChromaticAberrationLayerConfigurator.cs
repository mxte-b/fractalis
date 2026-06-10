using fractalis.Core.Compositor.Layers;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Miscellaneous;

namespace fractalis.Core.Compositor.Configurators.Stylistic
{
    internal class ChromaticAberrationLayerConfigurator : ILayerConfigurator
    {
        public Type TargetType => typeof(ChromaticAberrationLayer);

        public CompositeLayer Configure()
        {
            var r = Prompts.Text(
                $"Displacement of the [{ThemeColor.Accent}]red channel[/]?",
                0.006f
            );

            var g = Prompts.Text(
                $"Displacement of the [{ThemeColor.Accent}]green channel[/]?",
                0.003f
            );

            var b = Prompts.Text(
                $"Displacement of the [{ThemeColor.Accent}]blue channel[/]?",
                0.003f
            );

            return new ChromaticAberrationLayer(new(r, g, b));
        }
    }
}
