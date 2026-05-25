using System.Runtime.CompilerServices;

namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Represents a floating-point number in normalized mantissa-exponent form.
    /// </summary>
    /// <remarks>
    /// This type separates the number into a <see cref="Mantissa"/> and an <see cref="Exponent"/>
    /// to allow for high-dynamic-range calculations without losing precision.
    /// </remarks>
    public struct FloatExp
    {
        /// <summary>
        /// Represents the value zero.
        /// </summary>
        public static FloatExp Zero = new(0.0, 0);

        /// <summary>
        /// Represents the value one.
        /// </summary>
        public static FloatExp One = new(1.0, 0);

        /// <summary>
        /// The normalized mantissa of the number.
        /// </summary>
        public double Mantissa;

        /// <summary>
        /// The exponent of the number.
        /// </summary>
        public int Exponent;

        /// <summary>
        /// Initializes a new <see cref="FloatExp"/> with the given mantissa and exponent.
        /// </summary>
        /// <param name="mantissa">The base value component.</param>
        /// <param name="exponent">The scale factor component.</param>
        public FloatExp(double mantissa, int exponent)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
        }

        /// <summary>
        /// Normalizes the value so the mantissa is within a standard range.
        /// </summary>
        /// <remarks>
        /// Ensures consistent representation, handling zero, infinity, and NaN values.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(Mantissa);
            int biasedExp = (int)((bits >> 52) & 0x7FF);

            if (biasedExp == 0)
            {
                Mantissa = 0.0; Exponent = 0; return;
            }
            if (biasedExp == 0x7FF)
            {
                Mantissa = 0.0; Exponent = 0; return;
            }

            Mantissa = BitConverter.Int64BitsToDouble((long)((bits & 0x800F_FFFF_FFFF_FFFFUL) | 0x3FF0_0000_0000_0000UL));
            Exponent += biasedExp - 1023;
        }

        /// <summary>
        /// Returns the absolute value of the number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FloatExp Abs() => new() { Mantissa = Math.Abs(Mantissa), Exponent = Exponent };

        /// <summary>
        /// Returns a square root of the number.
        /// </summary>
        /// <returns>A new <see cref="FloatExp"/> representing the square root.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the value is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FloatExp Sqrt()
        {
            if (Mantissa < 0) throw new InvalidOperationException("Cannot take square root of a negative FloatExp");
            if (Mantissa == 0) return Zero;

            double m = Mantissa;
            int e = Exponent;

            // If exponent is odd, we have to multiply by 2 to make it even
            if ((e & 1) != 0)
            {
                m *= 2.0;
                e--;
            }

            return new FloatExp(Math.Sqrt(m), e / 2);
        }

        #region Arithmetic operators

        /// <summary>
        /// Multiplies two <see cref="FloatExp"/> numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator *(FloatExp left, FloatExp right)
        {
            double m = left.Mantissa * right.Mantissa;
            int e = left.Exponent + right.Exponent;

            if (m != 0 && Math.Abs(m) < 0.5) { m *= 2.0; e--; }

            return new FloatExp { Mantissa = m, Exponent = e };
        }

        /// <summary>
        /// Multiplies a <see cref="FloatExp"/> by a scalar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator *(FloatExp left, double right)
        {
            if (right == 0) return Zero;
            double m = left.Mantissa * right;
            int e = left.Exponent;
            if (m != 0)
            {
                if (m >= 1.0 || m <= -1.0) { m *= 0.5; e++; }
                else if (m > -0.5 && m < 0.5) { m *= 2.0; e--; }
            }
            return new FloatExp { Mantissa = m, Exponent = e };
        }

        /// <summary>
        /// Multiplies a scalar by a <see cref="FloatExp"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator *(double left, FloatExp right) => right * left;

        /// <summary>
        /// Divides one <see cref="FloatExp"/> by another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator /(FloatExp left, FloatExp right)
        {
            return new FloatExp(left.Mantissa / right.Mantissa, left.Exponent - right.Exponent);
        }

        /// <summary>
        /// Adds two <see cref="FloatExp"/> numbers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator +(FloatExp left, FloatExp right)
        {
            if (left.Mantissa == 0) return right;
            if (right.Mantissa == 0) return left;

            int exponentDiff = left.Exponent - right.Exponent;

            if (exponentDiff > 53) return left;
            if (exponentDiff < -53) return right;

            if (exponentDiff == 0) return new FloatExp(left.Mantissa + right.Mantissa, left.Exponent);

            if (exponentDiff > 0)
            {
                return new FloatExp(left.Mantissa + Math.ScaleB(right.Mantissa, -exponentDiff), left.Exponent);
            }
            else
            {
                return new FloatExp(Math.ScaleB(left.Mantissa, exponentDiff) + right.Mantissa, right.Exponent);
            }
        }

        /// <summary>
        /// Negates a <see cref="FloatExp"/> number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator -(FloatExp x) => new FloatExp(-x.Mantissa, x.Exponent);

        /// <summary>
        /// Subtracts one <see cref="FloatExp"/> from another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FloatExp operator -(FloatExp left, FloatExp right) => left + (-right);

        #endregion

        #region Comparison operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(FloatExp left, FloatExp right)
        {
            bool lz = left.Mantissa == 0, rz = right.Mantissa == 0;

            if (lz && rz) return false;
            if (lz) return right.Mantissa < 0;
            if (rz) return left.Mantissa > 0;

            if (left.Mantissa > 0 && right.Mantissa < 0) return true;
            if (left.Mantissa < 0 && right.Mantissa > 0) return false;

            if (left.Mantissa > 0)
            {
                if (left.Exponent != right.Exponent) return left.Exponent > right.Exponent;
                return left.Mantissa > right.Mantissa;
            }
            else
            {
                if (left.Exponent != right.Exponent) return left.Exponent < right.Exponent;
                return left.Mantissa > right.Mantissa;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(FloatExp left, FloatExp right) => right > left;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(FloatExp left, FloatExp right) => !(left < right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(FloatExp left, FloatExp right) => !(left > right);

        #endregion

        #region Conversions

        /// <summary>
        /// Converts the <see cref="FloatExp"/> to a <see langword="double"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator double(FloatExp x) => Math.ScaleB(x.Mantissa, x.Exponent);

        #endregion

        /// <summary>
        /// Returns a string representing the mantissa and exponent.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString() => $"{Mantissa}*2^{Exponent}";
    }
}