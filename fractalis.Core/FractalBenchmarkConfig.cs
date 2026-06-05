namespace fractalis.Core
{
    public record FractalBenchmarkConfig
    {
        public required string   Label   { get; init; }
        public required int      Runs    { get; init; }
    }
}
