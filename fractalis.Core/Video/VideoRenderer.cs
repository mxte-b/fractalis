using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    public record VideoConfig()
    {
        public required double      Duration                { get; init; }
        public int                  FPS                     { get; init; } = 30;
        public required BigFixed    ZoomStart               { get; init; }
        public required BigFixed    ZoomEnd                 { get; init; }
        public double               StartAnimationDuration  { get; init; } = 0;
        public double               StopAnimationDuration   { get; init; } = 0;

        public int                  FrameCount              => (int)Math.Floor(Duration * FPS);
    }

    public class VideoRenderer
    {
        private VideoConfig         Config              { get; set; }
        private VideoEncoder        Encoder             { get; set; }
        private FractalRenderer     Renderer            { get; set; }
        private readonly string     RenderId            = Guid.NewGuid().ToString();
        private string              ImageSequencePath   => $"render-{RenderId}";

        public VideoRenderer(FractalRenderer r, VideoConfig c)
        {
            Renderer = r;
            Config = c;
        }

        public void Start()
        {
            CreateOutputDirectory();

            // Start rendering each frame one by one
            for (int i = 0; i < Config.FrameCount; i++)
            {
                Renderer.Zoom = GetZoom(i);
                Console.WriteLine(Renderer.Zoom);
                Image<Rgb24> image = Renderer.Render();

                image.Save(ImageSequencePath + $"/frame-{i+1}.png");
            }
        }

        public void Save()
        {
            // Save video with encoder
            RemoveOutputDirectory();
        }

        private BigFixed GetZoom(int frameId) 
        {
            BigFixed zoom = Config.ZoomStart * BigFixed.Pow(Config.ZoomEnd / Config.ZoomStart, frameId / Config.FrameCount);
            Console.WriteLine("\n\n");
            Console.WriteLine(Config.ZoomStart);
            Console.WriteLine(Config.ZoomEnd);

            Console.WriteLine(Config.ZoomEnd / Config.ZoomStart);
            return zoom;
        }

        private void CreateOutputDirectory() => Directory.CreateDirectory($"render-{RenderId}");
        private void RemoveOutputDirectory() => Directory.Delete(ImageSequencePath, true);
    }
}
