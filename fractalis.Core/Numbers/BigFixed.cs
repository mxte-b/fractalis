using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct BigFixed
    {
        // Number of fractional bits
        private static readonly int Precision = 860;
        private static readonly BigInteger Scale = BigInteger.Pow(10, Precision);
        private readonly BigInteger Value;

        public BigFixed(string value)
        {
            bool negative = value.StartsWith('-');
            string abs = negative ? value.Substring(1) : value;

            BigInteger intPart = BigInteger.Zero;
            BigInteger? decimalPart = BigInteger.Zero;

            bool exponential = value.IndexOf('e') > -1;

            if (exponential)
            {
                string[] parts = abs.Split('e');

                string[] mantissaParts = parts[0].Split('.');
                int exponent = int.Parse(parts[1]);

                if (exponent < 0)
                {
                    int shift = -exponent;

                    string whole = mantissaParts[0];
                    string fracPart = mantissaParts.Length > 1 ? mantissaParts[1] : "";

                    string digits = whole + fracPart;

                    if (shift >= whole.Length)
                    {
                        // Entire number becomes fractional
                        int zerosNeeded = shift - whole.Length;

                        string fractional = new string('0', zerosNeeded) + digits;

                        string fractionalPart = fractional.Substring(0, Math.Min(Precision, fractional.Length));

                        intPart = BigInteger.Zero;
                        decimalPart = BigInteger.Parse(fractionalPart) * BigInteger.Pow(10, Precision - fractionalPart.Length);
                    }
                    else
                    {
                        // Split into integer and fractional
                        int splitIndex = whole.Length - shift;

                        string intDigits = whole.Substring(0, splitIndex);
                        string fracDigits = whole.Substring(splitIndex) + fracPart;

                        string fractionalPart = fracDigits.Substring(0, Math.Min(Precision, fracDigits.Length));

                        intPart = BigInteger.Parse(intDigits);
                        decimalPart = BigInteger.Parse(fractionalPart) * BigInteger.Pow(10, Precision - fractionalPart.Length);
                    }
                }
                else
                {
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

        public static explicit operator double(BigFixed x)
        {
            return double.Parse(x.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        }

        public static implicit operator FloatExp(BigFixed x)
        {
            string[] parts = x.ToString().Split('.');

            bool negative = parts[0].StartsWith('-');
            string intPart = negative ? parts[0][1..] : parts[0];
            string fracPart = parts.Length > 1 ? parts[1] : "";

            // Find first non-zero digit in fractional part
            int fractionStartIdx = -1;
            for (int i = 0; i < fracPart.Length; i++)
            {
                if (fracPart[i] != '0') { fractionStartIdx = i; break; }
            }

            double mantissa;
            int exponentBase10;

            // If the number has an integer part, we parse the value by
            // just trimming the end of trailing zeros.
            if (intPart[0] != '0')
            {
                string digits = intPart + fracPart.TrimEnd('0');
                digits = digits[..Math.Min(17, digits.Length)];

                // Inserts a radix point at position 1 for parsing to work correctly.
                mantissa = double.Parse(digits[0] + "." + digits[1..], System.Globalization.CultureInfo.InvariantCulture);
                exponentBase10 = intPart.Length - 1;
            }
            // Else we have a number in [0, 1) range.
            else if (fractionStartIdx >= 0)
            {
                string digits = fracPart.Substring(fractionStartIdx, Math.Min(17, fracPart.Length - fractionStartIdx)).TrimEnd('0');

                if (digits.Length == 0) return FloatExp.Zero;

                mantissa = double.Parse(digits[0] + "." + (digits.Length > 1 ? digits[1..] : "0"), System.Globalization.CultureInfo.InvariantCulture);
                exponentBase10 = -(fractionStartIdx + 1);
            }
            else
            {
                return FloatExp.Zero;
            }

            if (negative) mantissa = -mantissa;

            // Convert exponent to base-2
            double binaryExp = exponentBase10 * Math.Log2(10);
            int binaryExpInt = (int)Math.Floor(binaryExp);
            double binaryExpFrac = binaryExp - binaryExpInt;

            // Since the conversion from base-10 to base-2 does not guarantee
            // that the exponent will be an integer, we will make it one and
            // correct the value by adjusting the mantissa.
            mantissa *= Math.Pow(2, binaryExpFrac);

            return new FloatExp(mantissa, binaryExpInt);
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
