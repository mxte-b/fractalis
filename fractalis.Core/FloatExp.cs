using System;

namespace fractalis.Core
{
    public struct FloatExp
    {
        public static FloatExp Zero = new(0, 0);
        public static FloatExp One = new(1, 0);

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
            if (Mantissa == 0)
            {
                Exponent = 0;
                return;
            }

            int shift = (int)Math.Floor(Math.Log10(Math.Abs(Mantissa)));
            Mantissa /= Math.Pow(10, shift);
            Exponent += shift;
        }

        public static FloatExp operator *(FloatExp left, FloatExp right)
        {
            return new FloatExp(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static FloatExp operator /(FloatExp left, FloatExp right)
        {
            return new FloatExp(left.Mantissa / right.Mantissa, left.Exponent - right.Exponent);
        }

        public static FloatExp operator +(FloatExp left, FloatExp right)
        {
            int exponentDiff = left.Exponent - right.Exponent;

            //if (Math.Abs(exponentDiff) > 16)
            //    return exponentDiff > 0 ? left : right;

            // Align exponents
            if (exponentDiff > 0)
            {
                double scaled = right.Mantissa * Math.Pow(10, -exponentDiff);
                return new FloatExp(left.Mantissa + scaled, left.Exponent);
            }
            else
            {
                double scaled = left.Mantissa * Math.Pow(10, exponentDiff);
                return new FloatExp(right.Mantissa + scaled, right.Exponent);
            }
        }

        public static FloatExp operator -(FloatExp x)
        {
            return new FloatExp(-x.Mantissa, x.Exponent);
        }

        public static FloatExp operator -(FloatExp left, FloatExp right)
        {
            return left + (-right);
        }

        public override string ToString()
        {
            return $"{Mantissa:0.00}e{Exponent}";
        }
    }
}