namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Represents a standard-precision complex number with real and imaginary components.
    /// </summary>
    /// <remarks>
    /// Use this type for operations where <see langword="double"/> precision is sufficient.
    /// This struct supports basic arithmetic and provides properties to measure the size of the number.
    /// </remarks>
    public struct Complex
    {
        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        public double Real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        public double Imaginary;

        /// <summary>
        /// Initializes a new instance of <see cref="Complex"/> with the specified real and imaginary components.
        /// </summary>
        /// <param name="r">The real part of the number.</param>
        /// <param name="i">The imaginary part of the number.</param>
        public Complex(double r, double i)
        {
            Real = r;
            Imaginary = i;
        }

        /// <summary>
        /// Gets a value representing the size of the number without applying a square root.
        /// </summary>
        /// <remarks>
        /// This property is useful when comparing magnitudes for ordering or thresholds,
        /// as it avoids the extra computation of a square root.
        /// </remarks>
        public double MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }

        /// <summary>
        /// Gets a value representing the overall size of the complex number.
        /// </summary>
        /// <remarks>
        /// Use this property when the actual magnitude is needed for distance calculations
        /// or scaling.
        /// </remarks>
        public double Magnitude
        {
            get
            {
                return Math.Sqrt(Real * Real + Imaginary * Imaginary);
            }
        }

        /// <summary>
        /// Computes a measure of similarity between two complex numbers.
        /// </summary>
        /// <param name="a">The first complex number.</param>
        /// <param name="b">The second complex number.</param>
        /// <returns>
        /// A value representing the combined contribution of both components of the numbers.
        /// </returns>
        public static double Dot(Complex a, Complex b)
        {
            return a.Real * b.Real + a.Imaginary * b.Imaginary;
        }

        #region Arithmetic operators

        /// <summary>
        /// Multiplies a complex number by a scalar.
        /// </summary>
        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.Real * b, a.Imaginary * b);
        }

        /// <summary>
        /// Multiplies two complex numbers.
        /// </summary>
        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        }

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        /// <summary>
        /// Subtracts one complex number from another.
        /// </summary>
        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        /// <summary>
        /// Multiplies a scalar by a complex number.
        /// </summary>
        public static Complex operator *(double a, Complex b)
        {
            return new Complex(b.Real * a, b.Imaginary * a);
        }

        #endregion

        /// <summary>
        /// Returns a string representation of the complex number in the form "Real+Imaginaryi".
        /// </summary>
        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
}