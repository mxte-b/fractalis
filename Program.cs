using MandelbrotArbitraryPrecision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using ExtendedNumerics;

namespace fractalis
{
    internal class Program
    {
        static void Main(string[] args)
        {

            int w = 80;
            int h = 60;

            BigComplex center = new BigComplex(
                "-1.99977406013629035931268075596025004757104162338563840071485085742910123359845919282483641902157962595757183187999601753961068972458895812548344927013729496367830949",
                "-0.00000000329004032147943505349697867592668059678529465058784100883260469278535494529910563526811966311503252341715256643353534576212479229924708980210635830602189543"
            );


            BigFixed zoom = 1e75;

            int iterations = 10000;

            MandelbrotRenderer renderer = new MandelbrotRenderer(iterations, w, h, zoom, center);

            Image<Rgb24> image = renderer.Render();
            image.Save("render.png");

            Process.Start(new ProcessStartInfo("render.png") { UseShellExecute = true });
        }
    }
}
