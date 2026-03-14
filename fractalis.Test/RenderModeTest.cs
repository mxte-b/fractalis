using fractalis.Core;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;

namespace fractalis.Test
{
    public class RenderModeTest
    {
        private static FractalRenderer CreateRenderer(BigFloat zoom, int width, int height)
        {
            FractalRendererConfig rendererConfig = new FractalRendererConfig()
            {
                Fractal = new Mandelbrot(),
                Iterations = 150,
                Width = width,
                Height = height,
                Zoom = zoom,
                Center = new BigComplex(0, 0),
            };

            return new FractalRenderer(rendererConfig);
        }

        [Theory]
        [InlineData(1d, 800, 600, RenderMode.Default)]
        [InlineData(1e20, 800, 600, RenderMode.HighPrecision)]
        public void RenderMode_SwitchesCorrectly(double zoom, int w, int h, RenderMode expected)
        {
            var renderer = CreateRenderer((BigFloat)zoom, w, h);
            Assert.Equal(expected, renderer.RenderMode);
        }

        [Fact]
        public void RenderMode_SwitchesCorrectly_Extreme()
        {
            var renderer = CreateRenderer(new BigFloat("1e330"), 800, 600);
            Assert.Equal(RenderMode.HighPrecisionWithFloatExp, renderer.RenderMode);
        }
    }
}