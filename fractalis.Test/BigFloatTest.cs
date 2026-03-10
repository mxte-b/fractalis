using fractalis.Core.Numbers;
using System;
using Xunit;

namespace fractalis.Test
{
    public class BigFloatTest
    {
        [Theory]
        [InlineData("1.5", "1.5")]
        [InlineData("-1.5", "-1.5")]
        [InlineData("0", "0")]
        [InlineData("1073741824", "1073741824")]
        [InlineData("3.1415", "3.1415")]
        public void InitializesCorrectly(string s, string expected)
        {
            BigFloat a = new BigFloat(s);
            Assert.Equal(new BigFloat(expected), a);
        }

        [Theory]
        [InlineData("1.4", "2.43", "3.83")]
        [InlineData("5", "5", "10")]
        [InlineData("-2", "3", "1")]
        [InlineData("0.1", "0.2", "0.3")]
        [InlineData("-1.5", "-2.5", "-4")]
        public void Addition(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            Assert.Equal(new BigFloat(expected), x1 + x2);
        }

        [Theory]
        [InlineData("1.4", "2.43", "3.83")]
        [InlineData("5", "5", "10")]
        [InlineData("-2", "3", "1")]
        [InlineData("0.1", "0.2", "0.3")]
        [InlineData("-1.5", "-2.5", "-4")]
        public void Addition_InPlace(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            x1.AddInPlace(x2);

            Assert.Equal(new BigFloat(expected), x1);
        }

        [Theory]
        [InlineData("1.4", "2.43", "-1.03")]
        [InlineData("10", "5", "5")]
        [InlineData("5", "10", "-5")]
        [InlineData("0.5", "0.25", "0.25")]
        [InlineData("-3", "-2", "-1")]
        public void Subtraction(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            Assert.Equal(new BigFloat(expected), x1 - x2);
        }

        [Theory]
        [InlineData("1.4", "2.43", "-1.03")]
        [InlineData("10", "5", "5")]
        [InlineData("5", "10", "-5")]
        [InlineData("0.5", "0.25", "0.25")]
        [InlineData("-3", "-2", "-1")]
        public void Subtraction_InPlace(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            x1.SubtractInPlace(x2);

            Assert.Equal(new BigFloat(expected), x1);
        }

        [Theory]
        [InlineData("1.5", "2", "3")]
        [InlineData("2", "3", "6")]
        [InlineData("-2", "3", "-6")]
        [InlineData("0.5", "0.5", "0.25")]
        [InlineData("-1.5", "-2", "3")]
        public void Multiplication(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            Assert.Equal(new BigFloat(expected), x1 * x2);
        }

        [Theory]
        [InlineData("1.5", "2", "3")]
        [InlineData("2", "3", "6")]
        [InlineData("-2", "3", "-6")]
        [InlineData("0.5", "0.5", "0.25")]
        [InlineData("-1.5", "-2", "3")]
        public void Multiplication_InPlace(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            x1.MultiplyInPlace(x2);

            Assert.Equal(new BigFloat(expected), x1);
        }

        [Theory]
        [InlineData("1", "2", "0.5")]
        [InlineData("10", "2", "5")]
        [InlineData("-6", "3", "-2")]
        [InlineData("0.5", "0.5", "1")]
        [InlineData("-9", "-3", "3")]
        public void Division(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            Assert.Equal(new BigFloat(expected), x1 / x2);
        }

        [Theory]
        [InlineData("1", "2", "0.5")]
        [InlineData("10", "2", "5")]
        [InlineData("-6", "3", "-2")]
        [InlineData("0.5", "0.5", "1")]
        [InlineData("-9", "-3", "3")]
        public void Division_InPlace(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            x1.DivideInPlace(x2);

            Assert.Equal(new BigFloat(expected), x1);
        }

        [Theory]
        [InlineData("2", 3, "8")]
        [InlineData("5", 0, "1")]
        [InlineData("10", 2, "100")]
        [InlineData("1.5", 2, "2.25")]
        [InlineData("-2", 3, "-8")]
        public void Power(string a, int exponent, string expected)
        {
            BigFloat x = new BigFloat(a);

            Assert.Equal(new BigFloat(expected), x ^ exponent);
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
            BigFloat e = new BigFloat(exponent);

            x.PowerInPlace(e);

            Assert.Equal(new BigFloat(expected), x);
        }

        [Theory]
        [InlineData("2", "100", "1267650600228229401496703205376")]
        public void Power_BigFloatExponent_Precision(string a, string exponent, string expected)
        {
            BigFloat x = new BigFloat(a);
            BigFloat e = new BigFloat(exponent);

            Assert.Equal(new BigFloat(expected), x ^ e);
        }

        [Theory]
        [InlineData("10", "400", "1e400")]
        [InlineData("2", "100", "1267650600228229401496703205376")]
        public void Power_BigFloatExponent_Precision_InPlace(string a, string exponent, string expected)
        {
            BigFloat x = new BigFloat(a);
            BigFloat e = new BigFloat(exponent);

            x.PowerInPlace(e);

            Assert.Equal(new BigFloat(expected), x);
        }
    }
}