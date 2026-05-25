namespace fractalis.Core.Miscellaneous.Phases
{
    public record AppearancePhaseResult(PalettePreset Palette);

    public class AppearancePhase : IPromptPhase<AppearancePhaseResult>
    {
        public AppearancePhaseResult Run()
        {
            Prompts.Section("Appearance", 4);

            var palette = Prompts.Selection(
                $"What [{ThemeColor.Accent}]color palette[/] should the renderer use?",
                Enum.GetValues<PalettePreset>());

            Prompts.Done();
            return new(palette);
        }
    }
}