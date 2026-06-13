using fractalis.Core.Numbers;
using System.Text.Json.Serialization;

namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Base class for all fractal parameters.
    /// </summary>
    public abstract record FractalParameters;

    /// <summary>
    /// Indicates an empty parameter class.
    /// </summary>
    public record NoParameters : FractalParameters;

    /// <summary>
    /// Parameter class for the Julia set.
    /// </summary>
    public record JuliaParameters : FractalParameters
    {
        public required BigComplex Constant { get; init; }
    }

    /// <summary>
    /// Parameter class for the generalized Mandelbrot set.
    /// </summary>
    public record GeneralizedMandelbrotParameters : FractalParameters
    {
        public required double Exponent { get; init; }
    }

    /// <summary>
    /// Parameter class for the generalized Julia set.
    /// </summary>
    public record GeneralizedJuliaParameters : GeneralizedMandelbrotParameters
    {
        public required BigComplex Constant { get; init; }
    };
}
