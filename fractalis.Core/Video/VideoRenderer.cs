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
    public class VideoConfig
    {
        public double   Duration                { get; set; }
        public int      FPS                     { get; set; }
        public int      FrameCount 
        { 
            get
            {
                return (int)Math.Floor(Duration * FPS);
            }
        }
        public BigFixed ZoomStart               { get; set; }
        public BigFixed ZoomEnd                 { get; set; }
        public double   StopAnimationDuration   { get; set; }
    }

    public class VideoRenderer<TFractal> where TFractal : IFractal, new()
    {
        private readonly string RenderId = Guid.NewGuid().ToString();
        private string ImageSequencePath
        {
            get
            {
                return $"render-{RenderId}";
            }
        }

        private VideoConfig Config { get; set; }
        private VideoEncoder Encoder { get; set; }
        private FractalRenderer<TFractal> Renderer { get; set; }

        public VideoRenderer(FractalRenderer<TFractal> r)
        {
            Renderer = r;
        }

        public void Start()
        {
            CreateOutputDirectory();

            // Start rendering each frame one by one
            for (int i = 0; i < Config.FrameCount; i++)
            {
                Renderer.ZoomHigh = GetZoom(i);
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
            return Config.ZoomStart * BigFixed.Pow(Config.ZoomEnd / Config.ZoomStart, frameId / Config.FrameCount);
        }

        private void CreateOutputDirectory() => Directory.CreateDirectory($"render-{RenderId}");
        private void RemoveOutputDirectory() => Directory.Delete(ImageSequencePath, true);
    }
}
