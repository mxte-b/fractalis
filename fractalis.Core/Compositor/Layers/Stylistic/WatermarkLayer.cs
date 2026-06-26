using fractalis.Core.Compositor.Layers.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Configuration options for watermark rendering.
    /// </summary>
    public sealed record WatermarkOptions
    {
        /// <summary>The position of the watermark on the image.</summary>
        public Alignment Position { get; init; } = Alignment.BottomRight;

        /// <summary>The opacity of the watermark.</summary>
        public float Opacity { get; init; } = 1f;

        /// <summary>The scale factor applied to the watermark.</summary>
        public float Scale { get; init; } = 1f;

        /// <summary>The margin offset applied from the selected position.</summary>
        public Margin Margin { get; init; } = Margin.Zero;
    }

    /// <summary>
    /// Represents an effect layer that applies a watermark overlay.
    /// </summary>
    public class WatermarkLayer : CompositeLayer
    {
        #region JSON-exposed parameters
        public string ImagePath => _imagePath;
        public WatermarkOptions Options => _options;
        #endregion

        private readonly string _imagePath;
        private readonly WatermarkOptions _options;
        private readonly int _wmWidth, _wmHeight;
        private readonly Memory<Vector4> _watermark;

        /// <summary>
        /// Initializes a new instance of the watermark layer.
        /// </summary>
        /// <param name="imagePath">
        /// The path to the watermark. When a "resource:" prefix is present,
        /// it will be regarded and loaded as an embedded resource.
        /// The default Fractalis watermark paths can be accessed through <see cref="Watermarks"/>.
        /// </param>
        /// <param name="options">Watermark rendering options.</param>
        public WatermarkLayer(
            string imagePath, 
            WatermarkOptions? options = null
        )
        {
            _imagePath = imagePath;
            _options = options ?? new WatermarkOptions();

            // Loading watermark and converting to linear color space
            using Image<Rgba32> image = LoadImage(imagePath);

            _watermark = new Vector4[image.Width * image.Height].AsMemory();
            _wmWidth = image.Width;
            _wmHeight = image.Height;

            var buffer = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(buffer);

            ColorUtility.ToLinearSpace(buffer, _watermark);
        }

        private static Image<Rgba32> LoadImage(string path)
        {
            if (path.StartsWith("resource:", StringComparison.Ordinal))
            {
                string resourceName = path["resource:".Length..];
                using var stream = ResourceManager.ReadEmbeddedResourceStream(resourceName);

                return Image.Load<Rgba32>(stream);
            }

            return Image.Load<Rgba32>(path);
        }

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            int wmScaledWidth = (int)(_wmWidth * _options.Scale);
            int wmScaledHeight = (int)(_wmHeight * _options.Scale);

            int mx = _options.Margin.X;
            int my = _options.Margin.Y;

            (int wmOffsetX, int wmOffsetY) = _options.Position switch
            {
                Alignment.TopLeft => (mx, my),
                Alignment.TopRight => (width - wmScaledWidth - mx, my),
                Alignment.BottomLeft => (mx, height - wmScaledHeight - my),
                Alignment.BottomRight => (
                    width - wmScaledWidth - mx, 
                    height - wmScaledHeight - my
                ),
                _ => (mx, my)
            };

            Parallel.For(0, src.Length, idx =>
            {
                int x = idx % width;
                int y = idx / width;

                if (
                    x >= wmOffsetX && x < wmOffsetX + wmScaledWidth && 
                    y >= wmOffsetY && y < wmOffsetY + wmScaledHeight
                )
                {
                    // UV coordinates of the scaled watermark
                    float u = (x - wmOffsetX) / (float)wmScaledWidth;
                    float v = (y - wmOffsetY) / (float)wmScaledHeight;

                    // Scale UV to watermark size
                    float sx = u * _wmWidth;
                    float sy = v * _wmHeight;

                    // Left neighbors
                    int x0 = (int)sx;
                    int y0 = (int)sy;

                    // Right neighbors
                    int x1 = Math.Min(x0 + 1, _wmWidth - 1);
                    int y1 = Math.Min(y0 + 1, _wmHeight - 1);

                    // Lerp value towards right neighbors
                    float tx = sx - x0;
                    float ty = sy - y0;

                    // Neighbor pixels
                    var a = _watermark.Span[y0 * _wmWidth + x0];
                    var b = _watermark.Span[y0 * _wmWidth + x1];
                    var c = _watermark.Span[y1 * _wmWidth + x0];
                    var d = _watermark.Span[y1 * _wmWidth + x1];

                    // Lerp between neighbors - horizontal, then vertical
                    var wm = Vector4.Lerp(
                        Vector4.Lerp(a, b, tx),
                        Vector4.Lerp(c, d, tx),
                        ty
                    );
                    var pixel = src.Span[idx];
                    dst.Span[idx] = Vector4.Lerp(pixel, wm, wm.W * _options.Opacity);
                }
                else
                {
                    dst.Span[idx] = src.Span[idx];
                }
            });
        }
    }
}
