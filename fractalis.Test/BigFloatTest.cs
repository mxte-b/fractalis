using fractalis.Core.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Test
{
    public class BigFloatTest
    {
        [Theory]
        [InlineData("1.5", 1.5)]
        [InlineData("-1.5", -1.5)]
        [InlineData("0", 0)]
        [InlineData("1073741824", 1073741824.0)]
        public void InitializesCorrectly(string s, double expected)
        {
            BigFloat a = new BigFloat(s);
            Assert.Equal((BigFloat)expected, a);
        }

        [Theory]
        [InlineData("1.4", "2.43", "3.83")]
        public void Addition(string a, string b, string expected)
        {
            BigFloat x1 = new BigFloat(a);
            BigFloat x2 = new BigFloat(b);

            Assert.Equal(new BigFloat(expected), x1 + x2);
        }

        [Fact]
        public void Addition_InPlace()
        {
            BigFloat a = new BigFloat("1.4");
            BigFloat b = new BigFloat("2.43");

            a.AddInPlace(b);
            Assert.Equal(new BigFloat("3.83"), a);
        }

        [Fact]
        public void Subtraction()
        {
            BigFloat a = new BigFloat("1.4");
            BigFloat b = new BigFloat("2.43");

            Assert.Equal(new BigFloat("-1.03"), a - b);
        }

        [Fact]
        public void Subtraction_InPlace()
        {
            BigFloat a = new BigFloat("1.4");
            BigFloat b = new BigFloat("2.43");

            a.SubtractInPlace(b);
            Assert.Equal(new BigFloat("-1.03"), a);
        }
    }
}
