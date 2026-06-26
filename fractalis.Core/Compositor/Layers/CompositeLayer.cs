using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Compositor.Layers.Stylistic;
using fractalis.Core.Compositor.Layers.Tone;
using System.Numerics;
using System.Text.Json.Serialization;

namespace fractalis.Core.Compositor.Layers
{
    /// <summary>
    /// Base class for compositing image effect layers.
    /// </summary>
    /// <remarks>
    /// Defines the contract for all post-processing layers that operate on image buffers.
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    //Tone
    [JsonDerivedType(typeof(BrightnessLayer), "brightness")]
    [JsonDerivedType(typeof(ContrastLayer), "contrast")]
    [JsonDerivedType(typeof(GammaLayer), "gamma")]
    //Color
    [JsonDerivedType(typeof(HueShiftLayer), "hueShift")]
    [JsonDerivedType(typeof(SaturationLayer), "saturation")]
    [JsonDerivedType(typeof(TemperatureLayer), "temperature")]
    [JsonDerivedType(typeof(VibranceLayer), "vibrance")]
    //Stylistic
    [JsonDerivedType(typeof(AsciiArtLayer), "asciiArt")]
    [JsonDerivedType(typeof(BloomLayer), "bloom")]
    [JsonDerivedType(typeof(ChromaticAberrationLayer), "chromaticAberration")]
    [JsonDerivedType(typeof(VignetteLayer), "vignette")]
    [JsonDerivedType(typeof(WatermarkLayer), "watermark")]
    [JsonDerivedType(typeof(ZoomValueLayer), "zoomValue")]
    public abstract class CompositeLayer : ICompositeLayer
    {
        public abstract void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height);

        public static readonly IEnumerable<Type> AllLayers = [
            typeof(BrightnessLayer),
            typeof(ContrastLayer),
            typeof(GammaLayer),
            typeof(HueShiftLayer),
            typeof(SaturationLayer),
            typeof(TemperatureLayer),
            typeof(VibranceLayer),
            typeof(AsciiArtLayer),
            typeof(BloomLayer),
            typeof(ChromaticAberrationLayer),
            typeof(VignetteLayer),
            typeof(WatermarkLayer),
            typeof(ZoomValueLayer),
        ];
    }
}
