namespace fractalis.Core.Miscellaneous.Phases
{
    public record OutputPhaseResult(Resolution Resolution);

    public class OutputPhase : IPromptPhase<OutputPhaseResult>
    {
        public OutputPhaseResult Run()
        {
            Prompts.Section("Output", 3);

            var resolution = Prompts.Selection(
                $"What [{ThemeColor.Accent}]resolution[/] should the renderer use?",
                Resolution.CommonResolutions,
                converter: x => x.Name).Resolution;

            Prompts.Done();
            return new(resolution);
        }
    }
}
