using fractalis.Core.Compositor.Layers.Color;
using fractalis.Core.Numbers;
using fractalis.Core.Renderers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
    public class ZoomValueLayer : CompositeLayer, IContextAwareLayer
    {
        [JsonIgnore]
        public BigFloat Zoom { get; set; } = BigFloat.One;

        #region JSON-exposed parameters
        public float Scale => _scale;
        public Alignment Position => _position;
        public float BackgroundOpacity => _backgroundOpacity;
        #endregion

        private readonly Memory<Vector4> _atlas;
        private readonly int[] _atlasChars = ['0','1','2','3','4','5','6','7','8','9','.','e','Z','o','m',':',' ', '-'];
        private readonly int _atlasWidth;
        private readonly int _atlasHeight;
        private readonly float _scale;
        private readonly float _backgroundOpacity;
        private readonly Alignment _position;

        private const int CELL_WIDTH = 24;
        private const int CELL_HEIGHT = 64;
        private const int OVERLAY_PADDING_X = 20;
        private const int OVERLAY_PADDING_Y = 5;

        /// <summary>
        /// Initializes a new instance of the zoom value layer.
        /// </summary>
        /// <param name="scale">The relative scaling of the characters.</param>
        /// <param name="backgroundOpacity">The opacity of the background of the overlay.</param>
        /// <param name="position">The position of the overlay on the image.</param>
        /// <exception cref="Exception">When the font atlas cannot be loaded.</exception>
        public ZoomValueLayer(float scale = 1, float backgroundOpacity = 0.5f, Alignment position = Alignment.TopLeft)
        {
            _scale = scale;
            _position = position;
            _backgroundOpacity = backgroundOpacity;

            // Loading the font atlas
            var resourceName = "fractalis.Core.Resources.fontatlas_zoom.png";
            using var stream = ResourceManager.ReadEmbeddedResourceStream(resourceName);
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);

            _atlas = new Vector4[image.Width * image.Height].AsMemory();
            _atlasHeight = image.Height;
            _atlasWidth = image.Width;

            var buffer = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(buffer);

            ColorUtility.ToLinearSpace(buffer, _atlas);
        }

        public void SetContext(RenderContext ctx) => Zoom = ctx.Zoom;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            string zoomString = $"Zoom: {Zoom}";

            int scaledCharWidth  = (int)(CELL_WIDTH * Scale);
            int scaledCharHeight = (int)(CELL_HEIGHT * Scale);

            int textWidth = zoomString.Length * scaledCharWidth;
            int overlayWidth = textWidth + 2 * OVERLAY_PADDING_X;
            int overlayHeight = scaledCharHeight + 2 * OVERLAY_PADDING_Y;

            (int overlayOffsetX, int overlayOffsetY) = Position switch
            {
                Alignment.TopLeft => (0, 0),
                Alignment.TopRight => (width - overlayWidth, 0),
                Alignment.BottomLeft => (0, height - overlayHeight),
                Alignment.BottomRight => (
                    width - overlayWidth,
                    height - overlayHeight
                ),
                _ => (0, 0)
            };

            (int textOffsetX, int textOffsetY) = Position switch
            {
                Alignment.TopLeft => (OVERLAY_PADDING_X, OVERLAY_PADDING_Y),
                Alignment.TopRight => (width - overlayWidth + OVERLAY_PADDING_X, OVERLAY_PADDING_Y),
                Alignment.BottomLeft => (OVERLAY_PADDING_X, height - overlayHeight + OVERLAY_PADDING_Y),
                Alignment.BottomRight => (
                    width - overlayWidth + OVERLAY_PADDING_X,
                    height - overlayHeight + OVERLAY_PADDING_Y
                ),
                _ => (OVERLAY_PADDING_X, OVERLAY_PADDING_Y)
            };

            Parallel.For(0, src.Length, idx =>
            {
                int x = idx % width;
                int y = idx / width;

                if (
                     x >= textOffsetX && x < textOffsetX + textWidth &&
                     y >= textOffsetY && y < textOffsetY + scaledCharHeight
                )
                {
                    // UV coordinates in character-space
                    float au = ((x - textOffsetX) % scaledCharWidth) / (float)scaledCharWidth;
                    float av = (y - textOffsetY) / (float)scaledCharHeight;

                    // Pixel coordinates in atlas
                    float charX = au * CELL_WIDTH;
                    float charY = av * (CELL_HEIGHT - 1);

                    // Get current character index
                    int charIdx = (x - textOffsetX) / scaledCharWidth;
                    if (charIdx >= zoomString.Length)
                    {
                        dst.Span[idx] = src.Span[idx] * BackgroundOpacity;
                    }

                    char currentChar = zoomString[charIdx];
                    int charOffset = Array.IndexOf(_atlasChars, currentChar);

                    var atlasColor = SampleAtlasBilinear(charOffset * CELL_WIDTH, 0, charX, charY);
                    var srcPixel = src.Span[idx] * BackgroundOpacity;

                    dst.Span[idx] = Vector4.Lerp(srcPixel, atlasColor, atlasColor.X);
                }
                else if (
                     x >= overlayOffsetX && x < overlayOffsetX + overlayWidth &&
                     y >= overlayOffsetY && y < overlayOffsetY + overlayHeight
                )
                {
                    dst.Span[idx] = src.Span[idx] * BackgroundOpacity;
                }
                else
                {
                    dst.Span[idx] = src.Span[idx];
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector4 SampleAtlasBilinear(int offsetX, int offsetY, float u, float v)
        {
            int x0 = (int)u;
            int y0 = (int)v;

            int x1 = Math.Min(x0 + 1, CELL_WIDTH - 1);
            int y1 = Math.Min(y0 + 1, CELL_HEIGHT - 1);

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
