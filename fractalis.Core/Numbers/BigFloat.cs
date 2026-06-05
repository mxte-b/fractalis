using fractalis.Core.Converters;
using Sdcb.Arithmetic.Gmp;
using Sdcb.Arithmetic.Mpfr;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace fractalis.Core.Numbers
{
    /// <summary>
    /// Represents a high-precision floating-point number backed by <see cref="MpfrFloat"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="BigFloat"/> when standard floating-point types (<see langword="double"/>)
    /// do not provide sufficient precision, such as in deep fractal computations
    /// or numerical simulations requiring hundreds or thousands of digits of accuracy.
    /// </remarks>
    [TypeConverter(typeof(BigFloatConverter))]
    [JsonConverter(typeof(BigFloatJsonConverter))]
    public class BigFloat
    {
        private readonly MpfrFloat _value;
        private static int _precision = 2048;
        private static readonly double LOG2_10 = Math.Log2(10);
        private static readonly double LOG10_2 = Math.Log10(2);

        /// <summary>
        /// Gets or sets the global precision used for all <see cref="BigFloat"/> instances.
        /// </summary>
        /// <remarks>
        /// Setting this value adjusts the internal precision in bits to approximate the desired
        /// decimal digit count.
        /// </remarks>
        public static int Precision
        {
            get => (int)Math.Floor(_precision * LOG10_2);
            set => _precision = (int)Math.Ceiling(value * LOG2_10);
        }

        public static readonly BigFloat One = new(1);
        public static readonly BigFloat Ten = new(10);

        /// <summary>
        /// Initializes a new <see cref="BigFloat"/> from a string representation.
        /// </summary>
        /// <param name="s">A string containing a numeric value.</param>
        public BigFloat(string s) => _value = MpfrFloat.Parse(s, 10, _precision, MpfrRounding.ToEven);

        /// <summary>
        /// Initializes a new <see cref="BigFloat"/> from a <see langword="double"/> value.
        /// </summary>
        /// <param name="d">The numeric value.</param>
        public BigFloat(double d) => _value = MpfrFloat.From(d, _precision, MpfrRounding.ToEven);

        /// <summary>
        /// Initializes a new <see cref="BigFloat"/> from an existing <see cref="MpfrFloat"/>.
        /// </summary>
        /// <param name="raw">The underlying high-precision value.</param>
        public BigFloat(MpfrFloat raw) => _value = raw;

        /// <summary>Indicates whether the value is exactly zero.</summary>
        public bool IsZero => _value.IsZero;

        /// <summary>Indicates whether the value is negative.</summary>
        public bool IsNegative => _value.Sign < 0;

        /// <summary>Indicates whether the value is positive.</summary>
        public bool IsPositive => _value.Sign > 0;

        /// <summary>Gets the sign of the value (-1, 0, or 1).</summary>
        public int Sign => _value.Sign;

        public static bool TryParse(string s, out BigFloat? value)
        {
            value = null;

            try
            {
                value = new BigFloat(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether this value is approximately equal to another <see cref="BigFloat"/> 
        /// within a given precision.
        /// </summary>
        /// <param name="other">The value to compare with.</param>
        /// <param name="precision">The number of bits of precision to consider.</param>
        /// <returns><see langword="true"/> if the values are considered equal within the specified precision; otherwise, <see langword="false"/>.</returns>
        public bool ApproximatelyEquals(BigFloat other, uint precision)
        {
            BigFloat diff = Abs(this - other);

            if (diff.IsZero) return true;

            BigFloat absA = Abs(this);
            BigFloat absB = Abs(other);
            BigFloat scale = absA >= absB ? absA : absB;
            BigFloat one = new BigFloat(1.0);

            if (scale < one) scale = one;

            BigFloat threshold = new BigFloat(2.0) ^ (-(double)precision);
            return diff <= threshold * scale;
        }

        #region Comparison operators
        public static bool operator ==(BigFloat left, BigFloat right) => left._value.Equals(right._value);
        public static bool operator !=(BigFloat left, BigFloat right) => !left._value.Equals(right._value);
        public static bool operator <(BigFloat left, BigFloat right) => left._value < right._value;
        public static bool operator <(double left, BigFloat right) => left < right._value;
        public static bool operator <(BigFloat left, double right) => left._value < right;
        public static bool operator >(BigFloat left, BigFloat right) => left._value > right._value;
        public static bool operator >(double left, BigFloat right) => left > right._value;
        public static bool operator >(BigFloat left, double right) => left._value > right;
        public static bool operator <=(BigFloat left, BigFloat right) => left._value <= right._value;
        public static bool operator <=(double left, BigFloat right) => left <= right._value;
        public static bool operator <=(BigFloat left, double right) => left._value <= right;
        public static bool operator >=(BigFloat left, BigFloat right) => left._value >= right._value;
        public static bool operator >=(double left, BigFloat right) => left >= right._value;
        public static bool operator >=(BigFloat left, double right) => left._value >= right;
        #endregion

        #region Arithmetic operators
        public static BigFloat operator +(BigFloat left, BigFloat right) => new(left._value + right._value);
        public static BigFloat operator -(BigFloat left, BigFloat right) => new(left._value - right._value);
        public static BigFloat operator *(BigFloat left, BigFloat right) => new(left._value * right._value);
        public static BigFloat operator *(double left, BigFloat right) => new(left * right._value);
        public static BigFloat operator *(BigFloat left, double right) => new(left._value * right);
        public static BigFloat operator /(BigFloat left, BigFloat right) => new(left._value / right._value);
        public static BigFloat operator ^(BigFloat left, BigFloat right) => new(left._value ^ right._value);
        public static BigFloat operator ^(BigFloat left, double right) => new(left._value ^ right);
        public static BigFloat operator ^(double left, BigFloat right) => new(left ^ right._value);
        public static BigFloat operator -(BigFloat value) => new(-value._value);
        #endregion

        #region Static functions
        public static BigFloat Sqrt(BigFloat value) => new(MpfrFloat.Sqrt(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Abs(BigFloat value) => new(MpfrFloat.Abs(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log(BigFloat value) => new(MpfrFloat.Log(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log2(BigFloat value) => new(MpfrFloat.Log2(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log10(BigFloat value) => new(MpfrFloat.Log10(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Pow(BigFloat baseValue, BigFloat power) => new(MpfrFloat.PowerR(baseValue._value, power._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Exp(BigFloat value) => new(MpfrFloat.Exp(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Sin(BigFloat value) => new(MpfrFloat.Sin(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Cos(BigFloat value) => new(MpfrFloat.Cos(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Tan(BigFloat value) => new(MpfrFloat.Tan(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Floor(BigFloat value) => new(MpfrFloat.Floor(value._value, _precision));
        public static BigFloat Ceiling(BigFloat value) => new(MpfrFloat.Ceiling(value._value, _precision));
        public static BigFloat Truncate(BigFloat value) => new(MpfrFloat.RIntTruncate(value._value, MpfrRounding.ToZero, _precision));
        #endregion

        #region In-place operations
        public void AddInPlace(BigFloat value) => MpfrFloat.AddInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void SubtractInPlace(BigFloat value) => MpfrFloat.SubtractInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void MultiplyInPlace(BigFloat value) => MpfrFloat.MultiplyInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void MultiplyInPlace(double value) => MpfrFloat.MultiplyInplace(_value, _value, value, MpfrRounding.ToEven);
        public void DivideInPlace(BigFloat value) => MpfrFloat.DivideInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void PowerInPlace(BigFloat value) => MpfrFloat.PowerInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void PowerInPlace(double value) => MpfrFloat.PowerInplace(_value, _value, value, MpfrRounding.ToEven);
        public void NegateInPlace() => MpfrFloat.NegateInplace(_value, _value, MpfrRounding.ToEven);
        public void SqrtInPlace() => MpfrFloat.SqrtInplace(_value, _value, MpfrRounding.ToEven);
        public void AbsInPlace() => MpfrFloat.AbsInplace(_value, _value, MpfrRounding.ToEven);
        public void LogInPlace() => MpfrFloat.LogInplace(_value, _value, MpfrRounding.ToEven);
        public void Log2InPlace() => MpfrFloat.Log2Inplace(_value, _value, MpfrRounding.ToEven);
        public void Log10InPlace() => MpfrFloat.Log10Inplace(_value, _value, MpfrRounding.ToEven);
        public void ExpInPlace() => MpfrFloat.ExpInplace(_value, _value, MpfrRounding.ToEven);
        public void SinInPlace() => MpfrFloat.SinInplace(_value, _value, MpfrRounding.ToEven);
        public void CosInPlace() => MpfrFloat.CosInplace(_value, _value, MpfrRounding.ToEven);
        public void TanInPlace() => MpfrFloat.TanInplace(_value, _value, MpfrRounding.ToEven);
        public void FloorInPlace() => MpfrFloat.FloorInplace(_value, _value);
        public void CeilingInPlace() => MpfrFloat.CeilingInplace(_value, _value);
        public void TruncateInPlace() => MpfrFloat.TruncateInplace(_value, _value);
        #endregion

        #region Conversions
        public static explicit operator BigFloat(double d) => new(d);

        public static explicit operator FloatExp(BigFloat x)
        {
            ExpDouble converted = x._value.ToExpDouble();
            return new FloatExp(converted.Value, converted.Exp);
        }

        public double ToDouble() => _value.ToDouble(MpfrRounding.ToEven);
        #endregion

        /// <summary>Returns a simple string representation of the number.</summary>
        public override string ToString()
        {
            string raw = _value.ToString("E5");

            // If it's in scientific notation, then remove the + sign and the trailing zeros
            // at the end of the mantissa
            if (raw.Contains('e') || raw.Contains('E'))
            {
                int eIdx = raw.IndexOfAny(['e', 'E']);

                string mantissa = raw[..eIdx].TrimEnd('0').TrimEnd('.');
                string exponent = raw[(eIdx + 1)..].TrimStart('+');
                exponent = int.Parse(exponent).ToString();

                return $"{mantissa}e{exponent}";
            }

            //If the mantissa is a decimal in normal notation, then remove trailing zeros
            if (raw.Contains('.'))
            {
                string trimmed = raw.TrimEnd('0');
                return trimmed.EndsWith('.') ? trimmed[..^1] : trimmed;
            }

            return raw;
        }

        public string ToFullString() => _value.ToString();

        public override bool Equals(object? obj) => obj is BigFloat other && _value.Equals(other._value);

        public override int GetHashCode() => _value.GetHashCode();
    }
}