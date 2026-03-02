using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fractalis.Core.Numbers;

namespace fractalis.Test
{
    public class FloatExpTest
    {
        [Fact]
        public void Normalization()
        {
            FloatExp x = new FloatExp(14, 2);

            Assert.Equal(1.4, x.Mantissa);
            Assert.Equal(3, x.Exponent);
        }

        [Fact]
        public void Multiplication()
        {
            FloatExp a = new FloatExp(2, 0);
            FloatExp b = new FloatExp(3, 1);

            FloatExp result = a * b;

            Assert.Equal(6, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Division()
        {
            FloatExp a = new FloatExp(1, 4);
            FloatExp b = new FloatExp(1, 3);

            FloatExp result = a / b;

            Assert.Equal(1, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Addition()
        {
            FloatExp a = new FloatExp(1.5, 1);
            FloatExp b = new FloatExp(1, 1);

            FloatExp result = a + b;

            Assert.Equal(2.5, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Negation()
        {
            FloatExp a = new FloatExp(1, 2);

            FloatExp result = -a;

            Assert.Equal(-1, result.Mantissa);
            Assert.Equal(2, result.Exponent);
        }

        [Fact]
        public void Subtraction()
        {
            FloatExp a = new FloatExp(1, 1);
            FloatExp b = new FloatExp(1, 0);

            FloatExp result = a - b;

            Assert.Equal(9, result.Mantissa);
            Assert.Equal(0, result.Exponent);
        }

        [Fact]
        public void Sqrt_OddExponent()
        {
            FloatExp a = new FloatExp(2.5, 1);

            FloatExp result = a.Sqrt();

            Assert.Equal(5, result.Mantissa);
            Assert.Equal(0, result.Exponent);
        }

        [Fact]
        public void Sqrt_EvenExponent()
        {
            FloatExp a = new FloatExp(1.44, 2);

            FloatExp result = a.Sqrt();

            Assert.Equal(1.2, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Sqrt_Zero()
        {
            FloatExp a = FloatExp.Zero;

            FloatExp result = a.Sqrt();

            Assert.Equal(0, result.Mantissa);
            Assert.Equal(0, result.Exponent);
        }

        [Fact]
        public void Sqrt_Negative_Throws()
        {
            FloatExp a = new FloatExp(-1, 4);

            Assert.Throws<InvalidOperationException>(() => a.Sqrt());
        }

        [Fact]
        public void StringConversion()
        {
            FloatExp x = new FloatExp(4, 2);

            Assert.Equal("4.00e2", x.ToString());
        }
    }
}
