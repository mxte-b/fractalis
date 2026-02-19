using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fractalis
{
    internal class BigFixed
    {
        // Number of fractional bits
        private static readonly int Precision = 500;
        private static readonly BigInteger Scale = BigInteger.Pow(10, Precision);
        private readonly BigInteger Value;

        public BigFixed(string value)
        {
            bool negative = value.StartsWith("-");
            string abs = value.TrimStart('-');

            string[] parts = abs.Split('.');
            BigInteger intPart = BigInteger.Parse(parts[0]);

            BigInteger decimalPart = BigInteger.Zero;
            if (parts.Length > 1 && parts[1].Length > 0)
            {
                // Clamping fractional part to at most Precision digits
                string fractionalPart = parts[1].Substring(0, Math.Min(Precision, parts[1].Length));
                decimalPart = BigInteger.Parse(fractionalPart) * BigInteger.Pow(10, Precision - fractionalPart.Length);
            }

            Value = intPart * Scale + decimalPart;
            if (negative) Value = -Value;
        }

        private BigFixed(BigInteger raw)
        {
            Value = raw;
        }

        public static BigFixed operator +(BigFixed left, BigFixed right)
        {
            return new BigFixed(left.Value + right.Value);
        }

        public static BigFixed operator -(BigFixed left, BigFixed right)
        {
            return new BigFixed(left.Value - right.Value);
        }

        public static BigFixed operator *(BigFixed left, BigFixed right)
        {
            return new BigFixed(left.Value * right.Value / Scale);
        }

        public static BigFixed operator /(BigFixed left, BigFixed right)
        {
            return new BigFixed(left.Value * Scale / right.Value);
        }

        public static bool operator >(BigFixed left, BigFixed right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(BigFixed left, BigFixed right)
        {
            return left.Value > right.Value;
        }

        public static implicit operator BigFixed(double value)
        {
            return new BigFixed(value.ToString("0." + new string('#', 339)));
        }

        public override string ToString()
        {
            BigInteger intPart = Value / Scale;
            BigInteger decimalPart = BigInteger.Abs(Value % Scale);

            return intPart.ToString() + "." + decimalPart.ToString().PadLeft(Precision, '0');
        }
    }
}
