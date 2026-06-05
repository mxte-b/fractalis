namespace fractalis.Core.Miscellaneous
{
    public record ClientSettings
    {
        public required string  DisplayName         { get; init; }
        public required Uri     OrchestratorUri     { get; init; }
        public required double  ProcessorUsageLimit { get; init; }
    }
}
