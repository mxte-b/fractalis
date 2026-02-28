using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fractalis.Core;

namespace fractalis.Test
{
    public class FloatExpTest
    {
        [Fact]
        public void Normalization()
        {
            FloatExp x = new FloatExp(4d, 2);

            Assert.Equal(1, x.Mantissa);
            Assert.Equal(4, x.Exponent);
        }

        [Fact]
        public void Multiplication()
        {
            FloatExp a = new FloatExp(2, 0);
            FloatExp b = new FloatExp(3, 1);

            FloatExp result = a * b;

            Assert.Equal(1.5, result.Mantissa);
            Assert.Equal(3, result.Exponent);
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
            FloatExp a = new FloatExp(1, 4);
            FloatExp b = new FloatExp(1, 2);

            FloatExp result = a + b;

            Assert.Equal(1.25, result.Mantissa);
            Assert.Equal(4, result.Exponent);
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
            FloatExp a = new FloatExp(1, 4);
            FloatExp b = new FloatExp(1, 2);

            FloatExp result = a - b;

            Assert.Equal(1.5, result.Mantissa);
            Assert.Equal(3, result.Exponent);
        }

        [Fact]
        public void StringConversion()
        {
            FloatExp x = new FloatExp(4d, 2);

            Assert.Equal("1.00 * 2^4", x.ToString());
        }
    }
}
