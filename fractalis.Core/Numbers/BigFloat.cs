using Sdcb.Arithmetic.Mpfr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    internal class BigFloat
    {
        private readonly MpfrFloat          _value;
        private static int                  _precision  = 1500;
        private static readonly double      LOG2_10     = Math.Log2(10);
        private static readonly double      LOG10_2     = Math.Log10(2);
        public static int                   Precision
        {
            get => (int)Math.Floor(_precision * LOG10_2);
            set => _precision = (int)Math.Ceiling(value * LOG2_10);
        }

        public BigFloat(string s) => _value = MpfrFloat.Parse(s, 10, _precision, MpfrRounding.ToEven);
        public BigFloat(double d) => _value = MpfrFloat.From(d, _precision, MpfrRounding.ToEven);
        public BigFloat(MpfrFloat raw) => _value = raw;

        public void AddInPlace(BigFloat value) => MpfrFloat.AddInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void SubtractInPlace(BigFloat value) => MpfrFloat.SubtractInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void MultiplyInPlace(BigFloat value) => MpfrFloat.MultiplyInplace(_value, _value, value._value, MpfrRounding.ToEven);
        public void DivideInPlace(BigFloat value) => MpfrFloat.DivideInplace(_value, _value, value._value, MpfrRounding.ToEven);

        public static bool operator ==(BigFloat left, BigFloat right) => left._value.Equals(right._value);
        public static bool operator !=(BigFloat left, BigFloat right) => !left._value.Equals(right._value);
        public static BigFloat operator +(BigFloat left, BigFloat right) => new BigFloat(left._value + right._value);
        public static BigFloat operator -(BigFloat left, BigFloat right) => new BigFloat(left._value - right._value);
        public static BigFloat operator *(BigFloat left, BigFloat right) => new BigFloat(left._value * right._value);
        public static BigFloat operator /(BigFloat left, BigFloat right) => new BigFloat(left._value / right._value);
        public static explicit operator BigFloat(double d) => new BigFloat(d);

        public override string ToString() => _value.ToString();
        public override bool Equals(object? obj) => obj is BigFloat other && _value.Equals(other._value);
        public override int GetHashCode() => _value.GetHashCode();
    }
}
