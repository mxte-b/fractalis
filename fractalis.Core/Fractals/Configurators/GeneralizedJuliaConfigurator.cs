using fractalis.Core.Miscellaneous;
using fractalis.Core.Numbers;

namespace fractalis.Core.Fractals.Configurators;

public class GeneralizedJuliaConfigurator : IFractalConfigurator
{
    public FractalType TargetType => FractalType.GeneralizedJulia;
    public FractalParameters Configure()
    {
        var real = Prompts.Text<BigFloat>(
            $"[{ThemeColor.Accent}]Real part[/] of the Julia constant?",
            new(0)
        );

        var imaginary = Prompts.Text<BigFloat>(
            $"[{ThemeColor.Accent}]Imaginary part[/] of the Julia constant?",
            new(0)
        );
        
        var exponent = Prompts.Text<double>($"[{ThemeColor.Accent}]Exponent[/] of the fractal?");

        return new GeneralizedJuliaParameters()
        {
            Constant = new BigComplex(real, imaginary),
            Exponent = exponent
        };
    }
}