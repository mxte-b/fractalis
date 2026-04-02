namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Represents a complex number using high-dynamic-range <see cref="FloatExp"/> components.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ScaledComplex"/> for computations where standard floating-point precision
    /// is insufficient and scaling of components is required to prevent overflow or underflow.
    /// </remarks>
    public struct ScaledComplex
    {
        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        public FloatExp Real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        public FloatExp Imaginary;

        /// <summary>
        /// Gets a measure of the size of the number without taking a square root.
        /// </summary>
        /// <remarks>
        /// Useful for performance-sensitive comparisons or threshold checks.
        /// </remarks>
        public FloatExp MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }

        /// <summary>
        /// Gets a measure of the overall size of the number.
        /// </summary>
        /// <remarks>
        /// Represents how large the number is in absolute terms. More expensive to compute
        /// than <see cref="MagnitudeSquared"/> because it calculates a square root.
        /// </remarks>
        public FloatExp Magnitude
        {
            get
            {
                return (Real * Real + Imaginary * Imaginary).Sqrt();
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ScaledComplex"/> with specified <see cref="FloatExp"/> components.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imaginary">The imaginary part.</param>
        public ScaledComplex(FloatExp real, FloatExp imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        /// <summary>
        /// Initializes a new <see cref="ScaledComplex"/> from double-precision values.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imaginary">The imaginary part.</param>
        public ScaledComplex(double real, double imaginary)
        {
            Real = new FloatExp(real, 0);
            Imaginary = new FloatExp(imaginary, 0);
        }

        #region Arithmetic operators

        /// <summary>
        /// Adds two <see cref="ScaledComplex"/> numbers.
        /// </summary>
        public static ScaledComplex operator +(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        /// <summary>
        /// Subtracts one <see cref="ScaledComplex"/> number from another.
        /// </summary>
        public static ScaledComplex operator -(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        /// <summary>
        /// Multiplies two <see cref="ScaledComplex"/> numbers.
        /// </summary>
        public static ScaledComplex operator *(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(
                a.Real * b.Real - a.Imaginary * b.Imaginary,
                a.Real * b.Imaginary + a.Imaginary * b.Real
            );
        }

        /// <summary>
        /// Multiplies a <see cref="ScaledComplex"/> number by a scalar.
        /// </summary>
        public static ScaledComplex operator *(ScaledComplex a, double b)
        {
            return new ScaledComplex(a.Real * b, a.Imaginary * b);
        }

        /// <summary>
        /// Multiplies a scalar by a <see cref="ScaledComplex"/> number.
        /// </summary>
        public static ScaledComplex operator *(double a, ScaledComplex b)
        {
            return new ScaledComplex(b.Real * a, b.Imaginary * a);
        }

        #endregion

        /// <summary>
        /// Converts this <see cref="ScaledComplex"/> to a standard-precision <see cref="Complex"/>.
        /// </summary>
        /// <returns>A <see cref="Complex"/> with double-precision components.</returns>
        public Complex ToComplex() => new Complex((double)Real, (double)Imaginary);

        /// <summary>
        /// Returns a string representation with components converted to double-precision.
        /// </summary>
        /// <remarks>
        /// Positive or negative sign is included for the imaginary part.
        /// </remarks>
        public readonly string ToDoubleString()
        {
            return $"{(double)Real} {((double)Imaginary >= 0 ? "+" : "-")} {(double)(Imaginary.Abs())}i";
        }

        /// <summary>
        /// Returns a string representation of the <see cref="ScaledComplex"/> number.
        /// </summary>
        /// <remarks>
        /// Includes mantissa-exponent formatting of each component with proper sign for the imaginary part.
        /// </remarks>
        public readonly override string ToString()
        {
            return $"{Real.ToString()} {((double)Imaginary >= 0 ? "+" : "-")} {Imaginary.Abs().ToString()}i";
        }
    }
}