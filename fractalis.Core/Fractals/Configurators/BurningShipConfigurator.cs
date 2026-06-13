namespace fractalis.Core.Fractals.Configurators;

public class BurningShipConfigurator : IFractalConfigurator
{
    public FractalType TargetType => FractalType.BurningShip;

    public FractalParameters Configure() => new NoParameters();
}