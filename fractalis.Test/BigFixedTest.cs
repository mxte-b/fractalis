using fractalis.Core.Numbers;
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

            Assert.Equal(1.5625, result.Mantissa, 6);
            Assert.Equal(4, result.Exponent);
        }

        [Fact]
        public void Cast_FloatExp_NoFractionalDigits_Regular()
        {
            BigFixed a = new BigFixed("0.0123");

            FloatExp result = (FloatExp)a;

            Assert.Equal(1.5744, result.Mantissa, 4);
            Assert.Equal(-7, result.Exponent);
        }

        [Fact]
        public void Cast_FloatExp_Negative()
        {
            BigFixed a = new BigFixed("-1.05");

            FloatExp result = (FloatExp)a;

            Assert.Equal(-1.05, (double)result, 2);
        }

        [Fact]
        public void Cast_FloatExp_WithFractionalDigits()
        {
            BigFixed a = new BigFixed("2005e-2");

            FloatExp result = (FloatExp)a;

            Assert.Equal(1.253125, result.Mantissa, 6);
            Assert.Equal(4, result.Exponent);
        }
    }
}
