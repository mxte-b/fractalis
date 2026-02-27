using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
{
    public struct BigFixed
    {
        // Number of fractional bits
        private static readonly int Precision = 350;
        private static readonly BigInteger Scale = BigInteger.Pow(10, Precision);
        private readonly BigInteger Value;

        public BigFixed(string value)
        {
            bool negative = value.StartsWith("-");
            string abs = value.TrimStart('-');

            bool exponential = value.IndexOf('e') > -1;

            BigInteger intPart;
            BigInteger? decimalPart = null;

            if (exponential)
            {
                string[] parts = abs.Split('e');

                string[] mantissaParts = parts[0].Split('.');
                int exponent = int.Parse(parts[1]);
                
                // If the mantissa is an integer, just pad the mantissa with exponent zeros
                if (mantissaParts.Length < 2)
                {
                    string zeros = new string('0', exponent);
                    intPart = BigInteger.Parse(mantissaParts[0] + zeros);
                }
                // If the precision of the mantissa is less than the exponent,
                // then just concat and convert to BigInteger
                else if (mantissaParts[1].Length <= exponent)
                {
                    string zeros = new string('0', exponent - mantissaParts[1].Length);
                    intPart = BigInteger.Parse(mantissaParts[0] + mantissaParts[1] + zeros);
                }
                // Else, we need to manually handle the remaining fractional digits
                else
                {
                    string intDigits = mantissaParts[1].Substring(0, exponent);
                    string fracDigits = mantissaParts[1].Substring(exponent);

                    // Clamping fractional part to at most Precision digits
                    string fractionalPart = fracDigits.Substring(0, Math.Min(Precision, fracDigits.Length));

                    intPart = BigInteger.Parse(mantissaParts[0] + intDigits);
                    decimalPart = BigInteger.Parse(fractionalPart) * BigInteger.Pow(10, Precision - fractionalPart.Length);
                }
            }
            else
            {
                string[] parts = abs.Split('.');
                intPart = BigInteger.Parse(parts[0]);

                decimalPart = BigInteger.Zero;
                if (parts.Length > 1 && parts[1].Length > 0)
                {
                    // Clamping fractional part to at most Precision digits
                    string fractionalPart = parts[1].Substring(0, Math.Min(Precision, parts[1].Length));
                    decimalPart = BigInteger.Parse(fractionalPart) * BigInteger.Pow(10, Precision - fractionalPart.Length);
                }
            }


            Value = intPart * Scale + (decimalPart == null ? 0 : (BigInteger)decimalPart);
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
            return left.Value < right.Value;
        }

        public static implicit operator BigFixed(double value)
        {
            return new BigFixed(value.ToString("0." + new string('#', 339)));
        }

        public static implicit operator double(BigFixed x)
        {
            return double.Parse(x.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            if (Value.IsZero) return "0." + new string('0', Precision);

            bool negative = Value.Sign < 0;
            BigInteger absValue = BigInteger.Abs(Value);

            BigInteger intPart = absValue / Scale;
            BigInteger decimalPart = absValue % Scale;

            string result = intPart.ToString() + "." + decimalPart.ToString().PadLeft(Precision, '0');

            return negative ? "-" + result : result;
        }
    }
}
