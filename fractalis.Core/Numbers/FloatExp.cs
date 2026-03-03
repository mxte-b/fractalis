using System;

namespace fractalis.Core.Numbers
{
    public struct FloatExp
    {
        public static FloatExp Zero = new(0.0, 0);
        public static FloatExp One = new(1.0, 0);

        public double Mantissa;
        public int Exponent;

        public FloatExp(double mantissa, int exponent)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
        }

        public void Normalize()
        {
            if (Mantissa == 0 || double.IsNaN(Mantissa) || double.IsInfinity(Mantissa))
            {
                Mantissa = 0;
                Exponent = 0;
                return;
            }

            int shift = Math.ILogB(Mantissa);
            Mantissa = Math.ScaleB(Mantissa, -shift);
            Exponent += shift;
        }

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

        public readonly FloatExp Abs() => new(Math.Abs(Mantissa), Exponent);

        public static FloatExp operator *(FloatExp left, FloatExp right) {
            return new FloatExp(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static FloatExp operator *(FloatExp left, double right)
        {
            if (right == 0) return Zero;
            return new FloatExp(left.Mantissa * right, left.Exponent);
        }

        public static FloatExp operator *(double left, FloatExp right)
        {
            if (left == 0) return Zero;
            return new FloatExp(right.Mantissa * left, right.Exponent);
        }

        public static FloatExp operator /(FloatExp left, FloatExp right)
        {
            return new FloatExp(left.Mantissa / right.Mantissa, left.Exponent - right.Exponent);
        }

        public static FloatExp operator +(FloatExp left, FloatExp right)
        {
            if (left.Mantissa == 0) return right;
            if (right.Mantissa == 0) return left;

            int exponentDiff = left.Exponent - right.Exponent;

            // Optimization: If one of the terms differ by several
            // orders of magnitude, just return the bigger one.
            if (exponentDiff > 53) return left;
            if (exponentDiff < -53) return right;

            if (exponentDiff >= 0)
            {
                return new FloatExp(left.Mantissa + Math.ScaleB(right.Mantissa, -exponentDiff), left.Exponent);
            }
            else
            {
                return new FloatExp(Math.ScaleB(left.Mantissa, exponentDiff) + right.Mantissa, right.Exponent);
            }
        }

        public static FloatExp operator -(FloatExp x) => new FloatExp(-x.Mantissa, x.Exponent);

        public static FloatExp operator -(FloatExp left, FloatExp right) => left + (-right);

        public static bool operator >(FloatExp left, FloatExp right)
        {
            bool lz = left.Mantissa == 0, rz = right.Mantissa == 0;

            // Quick zero check
            if (lz && rz) return false;
            if (lz) return right.Mantissa < 0;
            if (rz) return left.Mantissa > 0;

            // Sign difference check
            if (left.Mantissa > 0 && right.Mantissa < 0) return true;
            if (left.Mantissa < 0 && right.Mantissa > 0) return false;

            if (left.Mantissa > 0)
            {
                // Exponent difference
                if (left.Exponent != right.Exponent) return left.Exponent > right.Exponent;

                // Mantissa difference
                return left.Mantissa > right.Mantissa;
            }
            else
            {
                if (left.Exponent != right.Exponent) return left.Exponent < right.Exponent;
                return left.Mantissa > right.Mantissa;
            }
        }

        public static bool operator <(FloatExp left, FloatExp right) => right > left;
        public static bool operator >=(FloatExp left, FloatExp right) => !(left < right);
        public static bool operator <=(FloatExp left, FloatExp right) => !(left > right);
        public static implicit operator double(FloatExp x) => Math.ScaleB(x.Mantissa, x.Exponent);
        public readonly override string ToString() => $"{Mantissa}*2^{Exponent}";
    }
}