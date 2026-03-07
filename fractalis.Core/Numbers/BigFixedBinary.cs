using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct BigFixedBinary
    {
        private static readonly int         Precision   = 1163; // In bits
        private static readonly BigInteger  Scale       = BigInteger.One << Precision;

        private readonly BigInteger Value;

        public BigFixedBinary(string value)
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

        private BigFixedBinary(BigInteger raw)
        {
            Value = raw;
        }

        public static BigFixedBinary operator +(BigFixedBinary left, BigFixedBinary right) => new(left.Value + right.Value);

        public static BigFixedBinary operator -(BigFixedBinary left, BigFixedBinary right) => new(left.Value - right.Value);

        public static BigFixedBinary operator *(BigFixedBinary left, BigFixedBinary right) => new((left.Value * right.Value) >> Precision);

        public static BigFixedBinary operator /(BigFixedBinary left, BigFixedBinary right) => new((left.Value >> Precision) / right.Value);

        public static bool operator >(BigFixedBinary left, BigFixedBinary right) => left.Value > right.Value;

        public static bool operator <(BigFixedBinary left, BigFixedBinary right) => left.Value < right.Value;

        public static implicit operator BigFixedBinary(double value)
        {
            return new BigFixedBinary(value.ToString("0." + new string('#', 339)));
        }

        public static explicit operator double(BigFixedBinary x)
        {
            return double.Parse(x.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        }

        public static implicit operator FloatExp(BigFixedBinary x)
        {
            if (x.Value.IsZero) return FloatExp.Zero;

            bool negative = x.Value.Sign == -1;
            BigInteger abs = BigInteger.Abs(x.Value);

            int exp;
            double mantissa;

            return FloatExp.One;
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
