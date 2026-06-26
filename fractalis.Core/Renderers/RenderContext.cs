using fractalis.Core.Numbers;

namespace fractalis.Core.Renderers
{
    public sealed record RenderContext
    {
        public required BigFloat Zoom { get; init; }
    }
}
