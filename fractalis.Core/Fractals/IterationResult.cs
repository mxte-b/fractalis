namespace fractalis.Core.Fractals
{
    /// <summary>
    /// Represents the result of a fractal iteration.
    /// </summary>
    /// <remarks>
    /// Contains the number of iterations performed, the magnitude of the point at escape, 
    /// and whether the point escaped the bailout threshold.
    /// </remarks>
    public struct IterationResult
    {
        /// <summary>
        /// Indicates whether the point escaped the fractal’s bailout threshold.
        /// </summary>
        public readonly bool Escaped => !double.IsNaN(MagnitudeSquared);

        /// <summary>
        /// The number of iterations performed for this point.
        /// </summary>
        public int Iteration;

        /// <summary>
        /// The squared magnitude of the point when it escaped, or NaN if it did not escape.
        /// </summary>
        public double MagnitudeSquared;

        /// <summary>
        /// Initializes a new <see cref="IterationResult"/>.
        /// </summary>
        /// <param name="iteration">Number of iterations performed.</param>
        /// <param name="magnitudeSquared">Magnitude value at escape or at final iteration.</param>
        /// <param name="escaped">Whether the point escaped the bailout threshold. Defaults to <see langword="true"/>.</param>
        public IterationResult(int iteration, double magnitudeSquared)
        {
            Iteration = iteration;
            MagnitudeSquared = magnitudeSquared;
        }
    }
}