using fractalis.Core.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Test
{
    public class ScaledComplexTest
    {
        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 0, 4)]
        [InlineData(-3, 2, 13)]
        public void MagnitudeSquared(double r, double i, double expected)
        {
            ScaledComplex a = new ScaledComplex(r, i);

            Assert.Equal(expected, a.MagnitudeSquared);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 1, 1.41421356237)]
        [InlineData(2, 0, 2)]
        [InlineData(0, 2, 2)]
        [InlineData(-3, 4, 5)]
        [InlineData(3, -4, 5)]
        [InlineData(-3, -4, 5)]
        [InlineData(5, 12, 13)]
        [InlineData(1e6, 0, 1e6)]
        [InlineData(1e6, 1e6, 1414213.56237)]
        public void Magnitude(double r, double i, double expected)
        {
            ScaledComplex a = new ScaledComplex(r, i);

            Assert.Equal(expected, a.Magnitude, 1e-5);
        }

        [Theory]
        [InlineData(1.4, 1, 6, 0, 7.4, 1)]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(-3, 4, 3, -4, 0, 0)]
        [InlineData(2.5, -1.5, -1.5, 2.5, 1.0, 1.0)]
        [InlineData(10, 20, -5, -15, 5, 5)]
        [InlineData(-7.2, 3.3, 2.2, -3.3, -5.0, 0.0)]
        public void Addition(
            double r1, double i1,
            double r2, double i2,
            double expectedR, double expectedI
        )
        {
            var a = new ScaledComplex(r1, i1);
            var b = new ScaledComplex(r2, i2);

            var result = a + b;

            Assert.Equal(expectedR, result.Real, 10);
            Assert.Equal(expectedI, result.Imaginary, 10);
        }

        [Theory]
        [InlineData(1.4, 1, 6, 0, -4.6, 1)]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(-3, 4, 3, -4, -6, 8)]
        [InlineData(2.5, -1.5, -1.5, 2.5, 4.0, -4.0)]
        [InlineData(10, 20, -5, -15, 15, 35)]
        [InlineData(-7.2, 3.3, 2.2, -3.3, -9.4, 6.6)]
        public void Subtraction(
            double r1, double i1,
            double r2, double i2,
            double expectedR, double expectedI
        )
        {
            var a = new ScaledComplex(r1, i1);
            var b = new ScaledComplex(r2, i2);

            var result = a - b;

            Assert.Equal(expectedR, result.Real, 10);
            Assert.Equal(expectedI, result.Imaginary, 10);
        }

        [Theory]
        [InlineData(1, 1, 2, 0, 2, 2)]
        [InlineData(0, 0, 5, 7, 0, 0)]
        [InlineData(3, 4, 1, 2, -5, 10)]
        [InlineData(-2, 3, 4, -1, -5, 14)]
        [InlineData(2.5, -1.5, -1.5, 2.5, 0.0, 8.5)]
        [InlineData(5, 0, 0, 3, 0, 15)]
        public void Multiplication(
            double r1, double i1,
            double r2, double i2,
            double expectedR, double expectedI
        )
        {
            var a = new ScaledComplex(r1, i1);
            var b = new ScaledComplex(r2, i2);

            var result = a * b;

            Assert.Equal(expectedR, result.Real, 10);
            Assert.Equal(expectedI, result.Imaginary, 10);
        }

        [Theory]
        [InlineData(1, 1, 2, 2, 2)]
        [InlineData(3, -4, -1, -3, 4)]
        [InlineData(0, 5, 2, 0, 10)]
        [InlineData(-2, 3, 0.5, -1, 1.5)]
        [InlineData(2.5, -1.5, -2, -5, 3)]
        [InlineData(0, 0, 100, 0, 0)]
        public void Multiplication_WithDouble(
            double r, double i, double scalar,
            double expectedR, double expectedI
        )
        {
            var z = new ScaledComplex(r, i);

            var result = z * scalar;

            Assert.Equal(expectedR, result.Real, 10);
            Assert.Equal(expectedI, result.Imaginary, 10);
        }

        [Theory]
        [InlineData(1.0, 1.0, "1 + 1i")]
        [InlineData(0.0, 0.0, "0 + 0i")]
        [InlineData(-3.5, 2.1, "-3.5 + 2.1i")]
        [InlineData(2.5, -1.5, "2.5 - 1.5i")]
        [InlineData(-7.2, -3.3, "-7.2 - 3.3i")]
        [InlineData(0, -5, "0 - 5i")]
        [InlineData(0, 7, "0 + 7i")]
        public void StringConversion(double r, double i, string expected)
        {
            var z = new ScaledComplex(r, i);

            string result = z.ToString();

            Assert.Equal(expected, result);
        }
    }
}
