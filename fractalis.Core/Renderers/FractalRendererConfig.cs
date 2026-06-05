using fractalis.Core.Compositor;
using fractalis.Core.Converters;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using System.Text.Json.Serialization;

namespace fractalis.Core.Renderers
{
    /// <summary>
    /// Configuration parameters for the <see cref="FractalRenderer"/>.
    /// </summary>
    public record FractalRendererConfig
    {
        /// <summary>Fractal instance to render.</summary>
        public required IFractal Fractal { get; init; }

        /// <summary>Maximum number of iterations for escape-time calculation.</summary>
        public required int Iterations { get; init; }

        /// <summary>Width of the output image in pixels.</summary>
        public required int Width { get; init; }

        /// <summary>Height of the output image in pixels.</summary>
        public required int Height { get; init; }

        /// <summary>Zoom level for the fractal view.</summary>
        [JsonConverter(typeof(BigFloatJsonConverter))]
        public required BigFloat Zoom { get; init; }

        /// <summary>Center coordinate in the complex plane.</summary>
        public required BigComplex Center { get; init; }

        /// <summary>Antialiasing (supersampling) level.</summary>
        public AntiAliasing AntiAliasing { get; init; } = AntiAliasing.NoAntialiasing;

        /// <summary>Maximum usage percentage while rendering (uses available CPU core count).</summary>
        /// <remarks>
        /// When this field is defined while using distributed video rendering,
        /// the value will be overwritten by the worker's runtime preferences.
        /// </remarks>
        public double ProcessorUsageLimit { get; init; } = 1;

        /// <summary>Color palette used for rendering.</summary>
        public ColorPalette ColorPalette { get; init; } = ColorPalette.FromPreset(PalettePreset.BB);

        /// <summary>The layer compositor to use when rendering images.</summary>
        public LayerCompositor? LayerCompositor { get; init; } = null;
    }
}
