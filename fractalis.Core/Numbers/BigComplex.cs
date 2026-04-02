namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Represents an arbitrary-precision complex number with a real and imaginary component,
    /// each backed by <see cref="BigFloat"/>.
    /// </summary>
    /// <remarks>
    /// Use this type when <see cref="System.Numerics.Complex"/> (which is limited to
    /// <see langword="double"/> precision) is insufficient — for example, in deep fractal renders where
    /// accumulated floating-point error would otherwise cause visible artifacts.
    /// </remarks>
    public struct BigComplex
    {
        /// <summary>
        /// The real part of the complex number.
        /// </summary>
        public BigFloat Real { get; set; }

        /// <summary>
        /// The imaginary part of the complex number.
        /// </summary>
        public BigFloat Imaginary { get; set; }

        /// <summary>
        /// Gets the squared magnitude of the complex number.
        /// </summary>
        /// <remarks>
        /// Represents how large the number is in a relative sense, without taking a square root.
        /// Use this when you need a performance-friendly measure of size.
        /// </remarks>
        public BigFloat MagnitudeSquared => Real * Real + Imaginary * Imaginary;

        /// <summary>
        /// Gets the magnitude (absolute value) of the complex number.
        /// </summary>
        /// <remarks>
        /// Represents the overall size of the number. Computed from the real and imaginary parts.
        /// This involves a square root operation and may be more expensive than <see cref="MagnitudeSquared"/>.
        /// </remarks>
        public BigFloat Magnitude => BigFloat.Sqrt(MagnitudeSquared);

        /// <summary>
        /// Initializes a new instance of <see cref="BigComplex"/> with the given real and imaginary parts.
        /// </summary>
        /// <param name="r">The real component.</param>
        /// <param name="i">The imaginary component.</param>
        public BigComplex(BigFloat r, BigFloat i)
        {
            Real = r;
            Imaginary = i;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BigComplex"/> from double values.
        /// </summary>
        /// <param name="r">The real component as a <see langword="double"/>.</param>
        /// <param name="i">The imaginary component as a <see langword="double"/>.</param>
        public BigComplex(double r, double i)
        {
            Real = new BigFloat(r);
            Imaginary = new BigFloat(i);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BigComplex"/> from string representations of the components.
        /// </summary>
        /// <param name="r">The real component as a string.</param>
        /// <param name="i">The imaginary component as a string.</param>
        /// <remarks>
        /// The strings must be valid numeric representations compatible with <see cref="BigFloat(string)"/>.
        /// </remarks>
        public BigComplex(string r, string i)
        {
            Real = new BigFloat(r);
            Imaginary = new BigFloat(i);
        }

        /// <summary>
        /// Adds two <see cref="BigComplex"/> numbers.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>
        /// A new <see cref="BigComplex"/> representing the sum of <paramref name="a"/> and <paramref name="b"/>.
        /// </returns>
        public static BigComplex operator +(BigComplex a, BigComplex b)
        {
            return new BigComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        /// <summary>
        /// Converts this <see cref="BigComplex"/> to a standard-precision <see cref="Complex"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Complex"/> with components converted to <see langword="double"/>.
        /// </returns>
        /// <remarks>
        /// Precision loss may occur for very large or very small values.
        /// </remarks>
        public Complex ToComplex()
        {
            return new Complex(Real.ToDouble(), Imaginary.ToDouble());
        }

        /// <summary>
        /// Converts this <see cref="BigComplex"/> to a <see cref="ScaledComplex"/> representation.
        /// </summary>
        /// <returns>
        /// A <see cref="ScaledComplex"/> containing the scaled exponent representation of the components.
        /// </returns>
        public ScaledComplex ToScaledComplex()
        {
            return new ScaledComplex((FloatExp)Real, (FloatExp)Imaginary);
        }

        /// <summary>
        /// Returns a string representation of the complex number in the form "Real+Imaginaryi".
        /// </summary>
        /// <returns>A string representing this <see cref="BigComplex"/>.</returns>
        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
}