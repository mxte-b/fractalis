using fractalis.Core;
using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;

namespace fractalis.Test
{
    public class RenderModeTest
    {
        private static MandelbrotRenderer CreateRenderer(BigFixed zoom, int width, int height)
        {
            return new MandelbrotRenderer(150, width, height, zoom, new BigComplex(0, 0));
        }

        [Theory]
        [InlineData(1d, 800, 600, RenderMode.Default)]
        [InlineData(1e20, 800, 600, RenderMode.HighPrecision)]
        [InlineData(1e300, 800, 600, RenderMode.HighPrecisionWithFloatExp)]
        public void RenderMode_SwitchesCorrectly(double zoom, int w, int h, RenderMode expected)
        {
            var renderer = CreateRenderer(zoom, w, h);
            Assert.Equal(expected, renderer.RenderMode);
        }
    }
}