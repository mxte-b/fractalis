using fractalis.Core.Numbers;
using System.Runtime.Intrinsics;

namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Defines the basic contract for a fractal type.
    /// </summary>
    /// <remarks>
    /// A fractal implementing this interface must support iteration for a complex point
    /// and provide a continuous iteration value useful for smooth coloring.
    /// </remarks>
    public interface IFractal
    {
        /// <summary>
        /// Performs a fractal iteration for a given complex coordinate.
        /// </summary>
        /// <param name="c">The complex coordinate to iterate.</param>
        /// <param name="maxIterations">Maximum number of iterations to perform.</param>
        /// <returns>
        /// An <see cref="IterationResult"/> indicating the number of iterations and magnitude at escape.
        /// </returns>
        IterationResult Iteration(Complex c, int maxIterations);

        /// <summary>
        /// Computes a continuous iteration value for smooth coloring.
        /// </summary>
        /// <param name="result">The iteration result from a fractal calculation.</param>
        /// <returns>
        /// A double representing the continuous iteration value.
        /// </returns>
        double GetContinousValue(IterationResult result);
    }

    /// <summary>
    /// Defines a fractal that supports perturbation techniques.
    /// </summary>
    public interface IPerturbableFractal : IFractal
    {
        /// <summary>
        /// Calculates a reference orbit for perturbation.
        /// </summary>
        /// <param name="center">The central complex coordinate.</param>
        /// <param name="maxIterations">Maximum number of iterations for the reference orbit.</param>
        /// <param name="referenceOrbit">Outputs the calculated <see cref="ReferenceOrbit"/>.</param>
        void CalculateReferenceOrbit(BigComplex center, int maxIterations, out ReferenceOrbit referenceOrbit);

        /// <summary>
        /// Performs perturbation iteration for a single point.
        /// </summary>
        /// <param name="deltaR">Delta real component relative to reference orbit.</param>
        /// <param name="deltaI">Delta imaginary component relative to reference orbit.</param>
        /// <param name="maxIterations">Maximum iterations.</param>
        /// <param name="referenceOrbit">Reference orbit for perturbation.</param>
        /// <returns>An <see cref="IterationResult"/> representing escape or final iteration.</returns>
        IterationResult IterationPerturbed(double deltaR, double deltaI, int maxIterations, in ReferenceOrbit referenceOrbit);

        /// <summary>
        /// Performs perturbation iteration using high-dynamic-range <see cref="ScaledComplex"/> for accuracy.
        /// </summary>
        /// <param name="delta">Delta from the reference orbit.</param>
        /// <param name="maxIterations">Maximum iterations.</param>
        /// <param name="referenceOrbit">Reference orbit for perturbation calculations.</param>
        /// <returns>An <see cref="IterationResult"/> representing escape or final iteration.</returns>
        IterationResult IterationFloatExp(ScaledComplex delta, int maxIterations, in ReferenceOrbit referenceOrbit);
    }

    /// <summary>
    /// Defines a fractal capable of SIMD-accelerated iterations.
    /// </summary>
    public interface ISimdFractal : IFractal
    {
        /// <summary>
        /// Performs SIMD-based iteration for four points simultaneously.
        /// </summary>
        /// <param name="cr">Vector of real components.</param>
        /// <param name="ci">Vector of imaginary components.</param>
        /// <param name="maxIterations">Maximum iterations per point.</param>
        /// <returns>
        /// A tuple of <see cref="IterationResult"/> for each point.
        /// </returns>
        (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3)
            IterationSIMD(Vector256<double> cr, Vector256<double> ci, int maxIterations);
    }

    /// <summary>
    /// Defines a fractal that supports both SIMD and perturbation-based iterations.
    /// </summary>
    public interface ISimdPerturbableFractal : ISimdFractal, IPerturbableFractal
    {
        /// <summary>
        /// Performs SIMD-based perturbation iteration for four points.
        /// </summary>
        /// <param name="ndcX">Normalized X coordinates vector.</param>
        /// <param name="ndcY">Normalized Y coordinate.</param>
        /// <param name="pixelSpacing">Distance between pixels in fractal space.</param>
        /// <param name="maxIterations">Maximum iterations per point.</param>
        /// <param name="referenceOrbit">Reference orbit for perturbation calculations.</param>
        /// <returns>
        /// A tuple of <see cref="IterationResult"/> for each point.
        /// </returns>
        (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3)
            IterationPerturbedSIMD(Vector256<double> ndcX, double ndcY, double pixelSpacing, int maxIterations, in ReferenceOrbit referenceOrbit);
    }
}