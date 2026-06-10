using fractalis.Core.Compositor.Layers.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
	/// <summary>
	/// Represents an effect layer that converts image data into ASCII art.
	/// </summary>
	public class AsciiArtLayer : CompositeLayer
    {
        #region JSON-exposed parameters
        public float Scale => _scale;
        #endregion

        private readonly Memory<Vector4> _atlas;
        private readonly int _atlasWidth;
        private readonly int _cellsPerRow;
        private readonly float _scale;

        private static readonly int _stepCount = 68;
        private static readonly int _cellSize = 30;
        private static readonly Vector4 _luma = new(0.2126f, 0.7152f, 0.0722f, 0.0f);

        /// <summary>
        /// Initializes a new instance of the ASCII art layer.
        /// </summary>
        /// <param name="scale">The relative scaling of the ASCII characters.</param>
        /// <exception cref="Exception">When the font atlas cannot be loaded.</exception>
        public AsciiArtLayer(float scale = 1)
        {
            _scale = scale;

            // Loading the font atlas
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "fractalis.Core.Resources.fontatlas.png";

            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? 
                throw new Exception("Couldn't load font atlas.");

            using Image<Rgba32> image = Image.Load<Rgba32>(stream);

            _atlas = new Vector4[image.Width * image.Height].AsMemory();
            _atlasWidth = image.Width;
            _cellsPerRow = _atlasWidth / _cellSize;

            var buffer = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(buffer);

            ColorUtility.ToLinearSpace(buffer, _atlas);
        }

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            int sizeScaled = (int)(_cellSize * _scale);

            int cellsX = width / sizeScaled;
            int cellsY = height / sizeScaled;

            // We map through each region where a character can fit
            Parallel.For(0, cellsX * cellsY, cidx =>
            {
                int cellX = cidx % cellsX;
                int cellY = cidx / cellsX;

                float u = cellX / (float)cellsX;
                float v = cellY / (float)cellsY;

                Vector4 avg = Vector4.Zero;
                int sizeSquared = sizeScaled * sizeScaled;
                for (int i = 0; i < sizeSquared; i++)
                {
                    int dx = i % sizeScaled;
                    int dy = i / sizeScaled;

                    int pixelX = cellX * sizeScaled + dx;
                    int pixelY = cellY * sizeScaled + dy;

                    avg += src.Span[pixelY * width + pixelX];
                }
                avg /= sizeSquared;

                float luma = Vector4.Dot(avg, _luma);
                int charIdx = Math.Clamp((int)(2 * luma * _stepCount), 0, _stepCount - 1);

                int charOffsetX = (charIdx % _cellsPerRow) * _cellSize;
                int charOffsetY = (charIdx / _cellsPerRow) * _cellSize;

                // Blit character to dst buffer
                for (int i = 0; i < sizeSquared; i++)
                {
                    int dx = i % sizeScaled;
                    int dy = i / sizeScaled;

                    // Get UV coordinates in character-space
                    float au = (float)dx / sizeScaled * _cellSize;
                    float av = (float)dy / sizeScaled * _cellSize;

                    var atlasColor = SampleAtlasBilinear(charOffsetX, charOffsetY, au, av);
                    var color = atlasColor * avg * 5 + avg * 0.5f;

                    int pixelX = cellX * sizeScaled + dx;
                    int pixelY = cellY * sizeScaled + dy;
                    dst.Span[pixelY * width + pixelX] = color;
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector4 SampleAtlasBilinear(int offsetX, int offsetY, float u, float v)
        {
            int x0 = (int)u;
            int y0 = (int)v;

            int x1 = Math.Min(x0 + 1, _cellSize - 1);
            int y1 = Math.Min(y0 + 1, _cellSize - 1);

            float tx = u - x0;
            float ty = v - y0;

            var a = _atlas.Span[(y0 + offsetY) * _atlasWidth + (x0 + offsetX)];
            var b = _atlas.Span[(y0 + offsetY) * _atlasWidth + (x1 + offsetX)];
            var c = _atlas.Span[(y1 + offsetY) * _atlasWidth + (x0 + offsetX)];
            var d = _atlas.Span[(y1 + offsetY) * _atlasWidth + (x1 + offsetX)];

            return Vector4.Lerp(
                Vector4.Lerp(a, b, tx),
                Vector4.Lerp(c, d, tx),
                ty
            );
        }
    }
}
