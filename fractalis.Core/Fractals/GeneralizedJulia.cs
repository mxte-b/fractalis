using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using fractalis.Core.Numbers;

namespace fractalis.Core.Fractals;

[method: JsonConstructor]
public class GeneralizedJulia(BigComplex constant, double exponent) : IFractal
{
    /// <summary>
    /// Maximum magnitude before a point is considered escaped in high-precision iterations.
    /// </summary>
    private static readonly FloatExp BAILOUT = new(1, 7);

    /// <summary>
    /// Maximum magnitude before a point is considered escaped in double-precision iterations.
    /// </summary>
    private static readonly double BAILOUT_DOUBLE = Math.Pow(2, 7);
    
    private readonly BigComplex _constant = constant;
    private readonly Complex _constantDouble = constant.ToComplex();
    private readonly double _exponent = exponent;
    private readonly double _invExpLog = 1 / Math.Log(exponent);

    public BigComplex Constant => _constant;
    public double Exponent => _exponent;
    public GeneralizedJulia(GeneralizedJuliaParameters p) : this(p.Constant, p.Exponent) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IterationResult Iteration(Complex c, int maxIterations)
    {
        // Convert to polar form
        double zr = c.Real;
        double zi = c.Imaginary;
        double escapeMag = 0;

        int i = 0;
        for (; i < maxIterations; i++)
        {
            double zRad = Math.Pow(Math.Sqrt(zr * zr + zi * zi), _exponent);
            double zTheta = Math.Atan2(zi, zr) * _exponent;

            zr = zRad * Math.Cos(zTheta);
            zi = zRad * Math.Sin(zTheta);

            zr += _constantDouble.Real;
            zi += _constantDouble.Imaginary;

            double mag = zr * zr + zi * zi;
            if (mag > BAILOUT_DOUBLE)
            {
                escapeMag = mag;
                break;
            }
        }
        
        if (i == maxIterations) return new IterationResult(i, double.NaN);

        return new IterationResult(i, escapeMag);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetContinousValue(IterationResult result)
    {
        if (!result.Escaped) return result.Iteration;
        return result.Iteration + 1 - Math.Log(Math.Log(result.MagnitudeSquared) * 0.5) * _invExpLog;
    }
}