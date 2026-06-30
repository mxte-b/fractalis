namespace fractalis.Core.Video
{
    public record DistributedRendererConfig
    {
        public required Uri                     OrchestratorUri         { get; init; }
        public int                              FrameListenerPort       { get; init; } = 8060;
        public List<FrameRange>?                FramesToRender          { get; init; } = null;
    }
}
