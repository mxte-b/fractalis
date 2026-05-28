using fractalis.Core.Numbers;
using System;
using Xunit;

namespace fractalis.Test
{
    public class BigFloatTest
    {
        private const uint DefaultPrecision = 10;

        private static void AssertApproxEqual(BigFloat expected, BigFloat actual, uint precision = DefaultPrecision)
        {
            Assert.True(actual.ApproximatelyEquals(expected, precision), $"Expected approximately {expected}, but got {actual}");
        }

        [Theory]
        [InlineData("1.5", "1.5")]
        [InlineData("-1.5", "-1.5")]
        [InlineData("0", "0")]
        [InlineData("1073741824", "1073741824")]
        [InlineData("3.1415", "3.1415")]
        public void InitializesCorrectly(string s, string expected)
        {
            BigFloat a = new BigFloat(s);
            AssertApproxEqual(new BigFloat(expected), a);
        }

        [Theory]
        [InlineData("1.4", "2.43", "3.83")]
        [InlineData("5", "5", "10")]
        [InlineData("-2", "3", "1")]
        [InlineData("0.1", "0.2", "0.3")]
        [InlineData("-1.5", "-2.5", "-4")]
        public void Addition(string a, string b, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) + new BigFloat(b));
        }

        [Theory]
        [InlineData("1.4", "2.43", "3.83")]
        [InlineData("5", "5", "10")]
        [InlineData("-2", "3", "1")]
        [InlineData("0.1", "0.2", "0.3")]
        [InlineData("-1.5", "-2.5", "-4")]
        public void Addition_InPlace(string a, string b, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.AddInPlace(new BigFloat(b));
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1.4", "2.43", "-1.03")]
        [InlineData("10", "5", "5")]
        [InlineData("5", "10", "-5")]
        [InlineData("0.5", "0.25", "0.25")]
        [InlineData("-3", "-2", "-1")]
        public void Subtraction(string a, string b, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) - new BigFloat(b));
        }

        [Theory]
        [InlineData("1.4", "2.43", "-1.03")]
        [InlineData("10", "5", "5")]
        [InlineData("5", "10", "-5")]
        [InlineData("0.5", "0.25", "0.25")]
        [InlineData("-3", "-2", "-1")]
        public void Subtraction_InPlace(string a, string b, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.SubtractInPlace(new BigFloat(b));
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1.5", "2", "3")]
        [InlineData("2", "3", "6")]
        [InlineData("-2", "3", "-6")]
        [InlineData("0.5", "0.5", "0.25")]
        [InlineData("-1.5", "-2", "3")]
        public void Multiplication(string a, string b, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) * new BigFloat(b));
        }

        [Theory]
        [InlineData("1.5", "2", "3")]
        [InlineData("2", "3", "6")]
        [InlineData("-2", "3", "-6")]
        [InlineData("0.5", "0.5", "0.25")]
        [InlineData("-1.5", "-2", "3")]
        public void Multiplication_InPlace(string a, string b, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.MultiplyInPlace(new BigFloat(b));
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1", "2", "0.5")]
        [InlineData("10", "2", "5")]
        [InlineData("-6", "3", "-2")]
        [InlineData("0.5", "0.5", "1")]
        [InlineData("-9", "-3", "3")]
        public void Division(string a, string b, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) / new BigFloat(b));
        }

        [Theory]
        [InlineData("1", "2", "0.5")]
        [InlineData("10", "2", "5")]
        [InlineData("-6", "3", "-2")]
        [InlineData("0.5", "0.5", "1")]
        [InlineData("-9", "-3", "3")]
        public void Division_InPlace(string a, string b, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.DivideInPlace(new BigFloat(b));
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("2", 3, "8")]
        [InlineData("5", 0, "1")]
        [InlineData("10", 2, "100")]
        [InlineData("1.5", 2, "2.25")]
        [InlineData("-2", 3, "-8")]
        public void Power(string a, int exponent, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) ^ exponent);
        }

        [Theory]
        [InlineData("2", "3", "8")]
        [InlineData("5", "0", "1")]
        [InlineData("10", "2", "100")]
        [InlineData("1.5", "2", "2.25")]
        [InlineData("-2", "3", "-8")]
        public void Power_InPlace(string a, string exponent, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.PowerInPlace(new BigFloat(exponent));
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("2", "100", "1267650600228229401496703205376")]
        public void Power_BigFloatExponent_Precision(string a, string exponent, string expected)
        {
            AssertApproxEqual(new BigFloat(expected),
                new BigFloat(a) ^ new BigFloat(exponent), precision: 100);
        }

        [Theory]
        [InlineData("10", "400", "1e400")]
        [InlineData("2", "100", "1267650600228229401496703205376")]
        public void Power_BigFloatExponent_Precision_InPlace(string a, string exponent, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.PowerInPlace(new BigFloat(exponent));
            AssertApproxEqual(new BigFloat(expected), x, precision: 100);
        }

        [Theory]
        [InlineData("3", "-3")]
        [InlineData("-5", "5")]
        [InlineData("0", "0")]
        [InlineData("1.25", "-1.25")]
        public void Negate(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), -new BigFloat(a));
        }

        [Theory]
        [InlineData("3", "-3")]
        [InlineData("-5", "5")]
        [InlineData("0", "0")]
        [InlineData("1.25", "-1.25")]
        public void Negate_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.NegateInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("3", "3")]
        [InlineData("-3", "3")]
        [InlineData("0", "0")]
        [InlineData("-1.5", "1.5")]
        [InlineData("1.5", "1.5")]
        public void Abs(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Abs(new BigFloat(a)));
        }

        [Theory]
        [InlineData("3", "3")]
        [InlineData("-3", "3")]
        [InlineData("0", "0")]
        [InlineData("-1.5", "1.5")]
        [InlineData("1.5", "1.5")]
        public void Abs_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.AbsInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("4", "2")]
        [InlineData("9", "3")]
        [InlineData("0.25", "0.5")]
        [InlineData("1", "1")]
        [InlineData("0", "0")]
        [InlineData("2", "1.41421356237309504880168872420969807856967187537694")]
        public void Sqrt(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Sqrt(new BigFloat(a)));
        }

        [Theory]
        [InlineData("4", "2")]
        [InlineData("9", "3")]
        [InlineData("0.25", "0.5")]
        [InlineData("1", "1")]
        [InlineData("0", "0")]
        [InlineData("2", "1.41421356237309504880168872420969807856967187537694")]
        public void Sqrt_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.SqrtInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("10", "2.30258509299404568401799145468436420760110148862877")]
        public void Log(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Log(new BigFloat(a)));
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("10", "2.30258509299404568401799145468436420760110148862877")]
        public void Log_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.LogInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("2", "1")]
        [InlineData("8", "3")]
        [InlineData("0.5", "-1")]
        [InlineData("10", "3.32192809488736234787031942948939017880316906514190")]
        public void Log2(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Log2(new BigFloat(a)));
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("2", "1")]
        [InlineData("8", "3")]
        [InlineData("0.5", "-1")]
        [InlineData("10", "3.32192809488736234787031942948939017880316906514190")]
        public void Log2_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.Log2InPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("10", "1")]
        [InlineData("100", "2")]
        [InlineData("0.1", "-1")]
        [InlineData("2", "0.30102999566398119521373889472449302676818596998558")]
        public void Log10(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Log10(new BigFloat(a)));
        }

        [Theory]
        [InlineData("1", "0")]
        [InlineData("10", "1")]
        [InlineData("100", "2")]
        [InlineData("0.1", "-1")]
        [InlineData("2", "0.30102999566398119521373889472449302676818596998558")]
        public void Log10_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.Log10InPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("0", "1")]
        [InlineData("1", "2.71828182845904523536028747135266249775724709369995")]
        [InlineData("2", "7.38905609893065022723042746043140059877635432611060")]
        [InlineData("-1", "0.36787944117144232159559838961884752045234826078750")]
        public void Exp(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Exp(new BigFloat(a)));
        }

        [Theory]
        [InlineData("0", "1")]
        [InlineData("1", "2.71828182845904523536028747135266249775724709369995")]
        [InlineData("2", "7.38905609893065022723042746043140059877635432611060")]
        [InlineData("-1", "0.36787944117144232159559838961884752045234826078750")]
        public void Exp_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.ExpInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("0", "0")]
        [InlineData("0.52359877559829887307710723054658381403286156656252", "0.5")]
        [InlineData("1.57079632679489661923132169163975144209858469968756", "1")]
        [InlineData("-1.57079632679489661923132169163975144209858469968756", "-1")]
        public void Sin(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Sin(new BigFloat(a)));
        }

        [Theory]
        [InlineData("0", "0")]
        [InlineData("0.52359877559829887307710723054658381403286156656252", "0.5")]
        [InlineData("1.57079632679489661923132169163975144209858469968756", "1")]
        [InlineData("-1.57079632679489661923132169163975144209858469968756", "-1")]
        public void Sin_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.SinInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("0", "1")]
        [InlineData("1.04719755119659774615421446109316762806572313312504", "0.5")]
        [InlineData("3.14159265358979323846264338327950288419716939937510", "-1")]
        public void Cos(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Cos(new BigFloat(a)));
        }

        [Theory]
        [InlineData("0", "1")]
        [InlineData("1.04719755119659774615421446109316762806572313312504", "0.5")]
        [InlineData("3.14159265358979323846264338327950288419716939937510", "-1")]
        public void Cos_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.CosInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("0", "0")]
        [InlineData("0.78539816339744830961566084581988372450846923428522", "1")]
        [InlineData("-0.78539816339744830961566084581988372450846923428522", "-1")]
        public void Tan(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Tan(new BigFloat(a)));
        }

        [Theory]
        [InlineData("0", "0")]
        [InlineData("0.78539816339744830961566084581988372450846923428522", "1")]
        [InlineData("-0.78539816339744830961566084581988372450846923428522", "-1")]
        public void Tan_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.TanInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("3.7", "3")]
        [InlineData("3.0", "3")]
        [InlineData("-3.2", "-4")]
        [InlineData("-3.0", "-3")]
        [InlineData("0.9", "0")]
        public void Floor(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Floor(new BigFloat(a)));
        }

        [Theory]
        [InlineData("3.7", "3")]
        [InlineData("3.0", "3")]
        [InlineData("-3.2", "-4")]
        [InlineData("-3.0", "-3")]
        [InlineData("0.9", "0")]
        public void Floor_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.FloorInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("3.2", "4")]
        [InlineData("3.0", "3")]
        [InlineData("-3.7", "-3")]
        [InlineData("-3.0", "-3")]
        [InlineData("0.1", "1")]
        public void Ceiling(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Ceiling(new BigFloat(a)));
        }

        [Theory]
        [InlineData("3.2", "4")]
        [InlineData("3.0", "3")]
        [InlineData("-3.7", "-3")]
        [InlineData("-3.0", "-3")]
        [InlineData("0.1", "1")]
        public void Ceiling_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.CeilingInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("3.9", "3")]
        [InlineData("-3.9", "-3")]
        [InlineData("3.0", "3")]
        [InlineData("0.5", "0")]
        [InlineData("-0.5", "0")]
        public void Truncate(string a, string expected)
        {
            AssertApproxEqual(new BigFloat(expected), BigFloat.Truncate(new BigFloat(a)));
        }

        [Theory]
        [InlineData("3.9", "3")]
        [InlineData("-3.9", "-3")]
        [InlineData("3.0", "3")]
        [InlineData("0.5", "0")]
        [InlineData("-0.5", "0")]
        public void Truncate_InPlace(string a, string expected)
        {
            BigFloat x = new BigFloat(a);
            x.TruncateInPlace();
            AssertApproxEqual(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("5", false, true, false)]
        [InlineData("-5", false, false, true)]
        [InlineData("0", true, false, false)]
        public void SignProperties(string a, bool expectedZero, bool expectedPositive, bool expectedNegative)
        {
            BigFloat x = new BigFloat(a);
            Assert.Equal(expectedZero, x.IsZero);
            Assert.Equal(expectedPositive, x.IsPositive);
            Assert.Equal(expectedNegative, x.IsNegative);
        }

        [Fact]
        public void Cast_ToFloatExp()
        {
            BigFloat a = new BigFloat("1e-10");
            FloatExp result = (FloatExp)a;

            Console.WriteLine(result);
        }

        [Theory]
        [InlineData("-100000000000", "-1e11")]
        [InlineData("0.00001", "1e-5")]
        [InlineData("12.213698163871286387126378612873678", "1.22136e1")]
        public void ToString_ScientificNotation(string a, string expected)
        {
            BigFloat x = new(a);
            Assert.Equal(expected, x.ToString());
        }
    }
}