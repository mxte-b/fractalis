using fractalis.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Test
{
    public class BigFixedTest
    {
        [Fact]
        public void Cast_FloatExp_NoFractionalDigits()
        {
            BigFixed a = new BigFixed("2.5e1");

            FloatExp result = (FloatExp)a;

            Assert.Equal(2.5, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Cast_FloatExp_NoFractionalDigits_Regular()
        {
            BigFixed a = new BigFixed("0.0123");

            FloatExp result = (FloatExp)a;

            Assert.Equal(1.23, result.Mantissa);
            Assert.Equal(-2, result.Exponent);
        }

        [Fact]
        public void Cast_FloatExp_WithFractionalDigits()
        {
            BigFixed a = new BigFixed("2005e-2");

            FloatExp result = (FloatExp)a;

            Console.WriteLine(result);

            Assert.Equal(2.005, result.Mantissa);
            Assert.Equal(1, result.Exponent);
        }

        [Fact]
        public void Cast_FloatExp_Extreme()
        {
            BigFixed a = new BigFixed("25e-370");

            FloatExp result = (FloatExp)a;

            Console.WriteLine(result);

            Assert.Equal(2.5, result.Mantissa);
            Assert.Equal(-369, result.Exponent);
        }
    }
}
