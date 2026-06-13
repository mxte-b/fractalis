namespace fractalis.Core.Fractals
{
    /// <summary>Represents the supported fractal algorithms.</summary>
    public enum FractalType
    {
        /// <summary>The Mandelbrot set fractal.</summary>
        Mandelbrot,

        /// <summary>The Julia set fractal.</summary>
        Julia,
        
        /// <summary>The Burning Ship fractal. </summary>
        BurningShip,
        
        /// <summary>The generalized Mandelbrot set fractal.</summary>
        GeneralizedMandelbrot,
        
        /// <summary>The generalized Julia set fractal.</summary>
        GeneralizedJulia,
    }
}