namespace fractalis.Core.Miscellaneous.Phases
{
    public record AppearancePhaseResult(PalettePreset Palette, AntiAliasing AntiAliasing);

    public class AppearancePhase : IPromptPhase<AppearancePhaseResult>
    {
        public AppearancePhaseResult Run()
        {
            Prompts.Section("Appearance");

            var antiAliasing = Prompts.Selection(
                $"What level of [{ThemeColor.Accent}]anti-aliasing[/] should the renderer use?",
                Enum.GetValues<AntiAliasing>());

            var palette = Prompts.Selection(
                $"What [{ThemeColor.Accent}]color palette[/] should the renderer use?",
                Enum.GetValues<PalettePreset>());

            Prompts.Done();
            return new(palette, antiAliasing);
        }
    }
}