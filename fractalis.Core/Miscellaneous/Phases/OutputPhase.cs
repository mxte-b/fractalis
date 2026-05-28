namespace fractalis.Core.Miscellaneous.Phases
{
    public record OutputPhaseResult(Resolution Resolution, double ProcessorUsageLimit, bool OpenRenderedImage);

    public class OutputPhase(AppMode appMode, VideoMode? videoMode) : IPromptPhase<OutputPhaseResult>
    {
        public OutputPhaseResult Run()
        {
            Prompts.Section("Output");

            var resolution = Prompts.Selection(
                $"What [{ThemeColor.Accent}]resolution[/] should the renderer use?",
                Resolution.CommonResolutions,
                converter: x => x.Name).Resolution;

            var cpu = videoMode != VideoMode.Distributed ? Prompts.Selection(
                $"What should the [{ThemeColor.Accent}]CPU usage limit[/] be?",
                [1, 0.75, 0.5, 0.25],
                converter: x => $"{x:p0}") : 1;

            var open = appMode == AppMode.Image && Prompts.Confirm($"[{ThemeColor.Accent}]Open[/] rendered image automatically?", true);

            Prompts.Done();
            return new(resolution, cpu, open);
        }
    }
}
