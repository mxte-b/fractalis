using System.Text.Json.Serialization;

namespace fractalis.Core.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Represents a margin used for positioning of elements on an image.
    /// </summary>
    /// <param name="x">Horizontal offset in pixels.</param>
    /// <param name="y">Vertical offset in pixels.</param>
    [method: JsonConstructor]
    public readonly struct Margin(int x, int y)
    {
        [JsonInclude]
        public readonly int X = x;

        [JsonInclude]
        public readonly int Y = y;

        public static readonly Margin Zero = new(0, 0);
    }
}
