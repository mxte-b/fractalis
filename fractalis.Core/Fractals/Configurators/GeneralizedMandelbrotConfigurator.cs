using fractalis.Core.Miscellaneous;

namespace fractalis.Core.Fractals.Configurators;

public class GeneralizedMandelbrotConfigurator : IFractalConfigurator
{
    public FractalType TargetType => FractalType.GeneralizedMandelbrot;
    
    public FractalParameters Configure()
    {
        var exponent = Prompts.Text<double>($"[{ThemeColor.Accent}]Exponent[/] of the fractal?");
    
        return new GeneralizedMandelbrotParameters
        {
            Exponent = exponent
        };
    }
}