using System.Runtime.CompilerServices;
using fractalis.Core.Numbers;

namespace fractalis.Core.Fractals;

public class BurningShip : IFractal
{
    /// <summary>
    /// Constant for converting logarithms to base 2.
    /// </summary>
    private const double ILOG2 = 1.4426950408889634;

    /// <summary>
    /// Maximum magnitude before a point is considered escaped in high-precision iterations.
    /// </summary>
    private static readonly FloatExp BAILOUT = new(1, 7);

    /// <summary>
    /// Maximum magnitude before a point is considered escaped in double-precision iterations.
    /// </summary>
    private static readonly double BAILOUT_DOUBLE = Math.Pow(2, 7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IterationResult Iteration(Complex c, int maxIterations)
    {
        double zr = 0;
        double zi = 0;
        double escapeMag = 0;

        int i = 0;
        for (; i < maxIterations; i++)
        {
            double zrTemp = zr;
            zr = zr * zr - zi * zi + c.Real;
            zi = 2 * Math.Abs(zrTemp * zi) - c.Imaginary;

            double mag = zr * zr + zi * zi;
            if (mag > BAILOUT_DOUBLE)
            {
                escapeMag = mag;
                break;
            }
        }
        
        return i == maxIterations 
            ? new IterationResult(i, double.NaN) 
            : new IterationResult(i, escapeMag);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetContinousValue(IterationResult result)
    {
        if (!result.Escaped) return result.Iteration;
        return result.Iteration + 1 - Math.Log(Math.Log(result.MagnitudeSquared) * 0.5) * ILOG2;
    }
}