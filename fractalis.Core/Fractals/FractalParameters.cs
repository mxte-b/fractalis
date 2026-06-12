using fractalis.Core.Numbers;
using System.Text.Json.Serialization;

namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Base class for all fractal parameters.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(NoParameters), "noParameters")]
    [JsonDerivedType(typeof(JuliaParameters), "julia")]
    public abstract record FractalParameters;

    /// <summary>
    /// Indicates an empty parameter class.
    /// </summary>
    public record NoParameters : FractalParameters;

    /// <summary>
    /// Parameter class for the Julia set.
    /// </summary>
    /// <param name="Constant">The Julia constant.</param>
    public record JuliaParameters(BigComplex Constant) : FractalParameters;
}
