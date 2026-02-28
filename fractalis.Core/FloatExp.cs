using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    internal struct FloatExp
    {
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

            int shift = Math.ILogB(Mantissa);
            Mantissa = Math.ScaleB(Mantissa, -shift);
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

            // Optimization: If the difference in magnitude is bigger than
            // what doubles can handle, just return the bigger one.
            if (Math.Abs(exponentDiff) > 54)
            {
                return exponentDiff > 0 ? left : right;
            }

            // Aligning exponents
            // Left number is bigger
            if (exponentDiff > 0)
            {
                return new FloatExp(left.Mantissa + Math.ScaleB(right.Mantissa, -exponentDiff), left.Exponent);
            }
            // Right number is bigger
            else
            {
                return new FloatExp(right.Mantissa + Math.ScaleB(left.Mantissa, exponentDiff), right.Exponent);
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
            return $"{Mantissa:0.00} * 2^{Exponent}";
        }
    }
}
