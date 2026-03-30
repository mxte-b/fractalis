using Sdcb.Arithmetic.Gmp;
using Sdcb.Arithmetic.Mpfr;

namespace fractalis.Core.Numbers
{
    public class BigFloat
    {
        private readonly MpfrFloat _value;
        private static int _precision = 2048;
        private static readonly double LOG2_10 = Math.Log2(10);
        private static readonly double LOG10_2 = Math.Log10(2);
        public static int Precision
        {
            get => (int)Math.Floor(_precision * LOG10_2);
            set => _precision = (int)Math.Ceiling(value * LOG2_10);
        }

        public BigFloat(string s) => _value = MpfrFloat.Parse(s, 10, _precision, MpfrRounding.ToEven);
        public BigFloat(double d) => _value = MpfrFloat.From(d, _precision, MpfrRounding.ToEven);
        public BigFloat(MpfrFloat raw) => _value = raw;

        public bool IsZero => _value.IsZero;
        public bool IsNegative => _value.Sign < 0;
        public bool IsPositive => _value.Sign > 0;
        public int Sign => _value.Sign;

        public bool ApproximatelyEquals(BigFloat other, uint precision)
        {
            var diff = Abs(this - other);
            if (diff.IsZero) return true;
            var absA = Abs(this);
            var absB = Abs(other);
            var scale = absA >= absB ? absA : absB;
            var one = new BigFloat(1.0);
            if (scale < one) scale = one;
            var threshold = new BigFloat(2.0) ^ (-(double)precision);
            return diff <= threshold * scale;
        }

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

        public static BigFloat Sqrt(BigFloat value) => new(MpfrFloat.Sqrt(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Abs(BigFloat value) => new(MpfrFloat.Abs(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log(BigFloat value) => new(MpfrFloat.Log(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log2(BigFloat value) => new(MpfrFloat.Log2(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Log10(BigFloat value) => new(MpfrFloat.Log10(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Exp(BigFloat value) => new(MpfrFloat.Exp(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Sin(BigFloat value) => new(MpfrFloat.Sin(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Cos(BigFloat value) => new(MpfrFloat.Cos(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Tan(BigFloat value) => new(MpfrFloat.Tan(value._value, _precision, MpfrRounding.ToEven));
        public static BigFloat Floor(BigFloat value) => new(MpfrFloat.Floor(value._value, _precision));
        public static BigFloat Ceiling(BigFloat value) => new(MpfrFloat.Ceiling(value._value, _precision));
        public static BigFloat Truncate(BigFloat value) => new(MpfrFloat.RIntTruncate(value._value, MpfrRounding.ToZero, _precision));

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

        public static explicit operator BigFloat(double d) => new(d);
        public static explicit operator FloatExp(BigFloat x) 
        {
            ExpDouble converted = x._value.ToExpDouble();
            return new FloatExp(converted.Value, converted.Exp);
        }

        public double ToDouble() => _value.ToDouble(MpfrRounding.ToEven);

        public override string ToString() => _value.ToString();
        public override bool Equals(object? obj) => obj is BigFloat other && _value.Equals(other._value);
        public override int GetHashCode() => _value.GetHashCode();
    }
}