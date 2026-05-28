using fractalis.Core.Numbers;

namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Stores reference orbit data for perturbation theory rendering.
    /// </summary>
    /// <param name="maxIterations">Maximum number of iterations in the reference orbit.</param>
    public struct ReferenceOrbit(int maxIterations)
    {
        /// <summary>Real components of orbit points.</summary>
        public readonly double[] PointsR = new double[maxIterations];

        /// <summary>Imaginary components of orbit points.</summary>
        public readonly double[] PointsI = new double[maxIterations];

        /// <summary>Scaled complex points for high-precision rendering.</summary>
        public readonly ScaledComplex[] ScaledPoints = new ScaledComplex[maxIterations];

        /// <summary>The iteration at which escape occurred, or 0 if not escaped.</summary>
        public int EscapeIteration = 0;
    }
}
