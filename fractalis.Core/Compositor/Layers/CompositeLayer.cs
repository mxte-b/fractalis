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
    [JsonDerivedType(typeof(ExposureLayer), "exposure")]
    [JsonDerivedType(typeof(GammaLayer), "gamma")]
    //Color
    [JsonDerivedType(typeof(HueShiftLayer), "hueShift")]
    [JsonDerivedType(typeof(SaturationLayer), "saturation")]
    [JsonDerivedType(typeof(TemperatureLayer), "temperature")]
    [JsonDerivedType(typeof(VibranceLayer), "vibrance")]
    //Stylistic
    [JsonDerivedType(typeof(ASCIIArtLayer), "asciiArt")]
    [JsonDerivedType(typeof(BloomLayer), "bloom")]
    [JsonDerivedType(typeof(ChromaticAberrationLayer), "chromaticAberration")]
    [JsonDerivedType(typeof(VignetteLayer), "vignette")]
    [JsonDerivedType(typeof(WatermarkLayer), "watermark")]
    public abstract class CompositeLayer : ICompositeLayer
    {
        public abstract void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height);
    }
}
