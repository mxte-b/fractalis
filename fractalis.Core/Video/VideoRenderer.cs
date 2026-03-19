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
        public required BigFloat    ZoomStart               { get; init; }
        public required BigFloat    ZoomEnd                 { get; init; }
        public double               StartAnimationDuration  { get; init; } = 1;
        public double               StopAnimationDuration   { get; init; } = 1;
        public int                  StartFrame              { get; init; } = 0;

        public int                  StartAnimationFrames    => (int)Math.Round(StartAnimationDuration * FPS);
        public int                  StopAnimationFrames     => (int)Math.Round(StopAnimationDuration * FPS);
        public int                  FrameCount              => (int)Math.Floor(Duration * FPS);
        public int                  StopAnimationStartFrame => FrameCount - StopAnimationFrames;
    }

    public class VideoRenderer(FractalRenderer r, VideoConfig c)
    {
        private VideoConfig         Config      { get; set; } = c;
        private FractalRenderer     Renderer    { get; set; } = r;

        private readonly string     _renderId           = Guid.NewGuid().ToString();

        private string              ImageSequencePath   => $"render-{_renderId}";
        private double              Delta
        {
            get
            {
                return 3.0 * Config.FrameCount / (Config.FrameCount + 2 * Config.StopAnimationStartFrame - 2 * Config.StartAnimationFrames);
            }
        }

        private double              Gamma
        {
            get
            {
                return -2.0 * Config.StartAnimationFrames * Delta / 3;
            }
        }

        public void Start()
        {
            CreateOutputDirectory();

            // Start rendering each frame one by one
            for (int i = Config.StartFrame; i < Config.FrameCount; i++)
            {
                Renderer.Zoom = GetZoom(i);
                Image<Rgb24> image = Renderer.Render(false);

                image.Save(ImageSequencePath + $"/frame{(i+1).ToString().PadLeft(5, '0')}.png");
            }
        }

        public void Save(string fileName = "render")
        {
            VideoEncoder.MergeImageSequence($"render-{_renderId}", Config.FPS, fileName);
            RemoveOutputDirectory();
        }

        private static double NormalizedTime(double from, double length, double frame) => (frame - from) / length;
        private double TBase(double t) => t * Delta + Gamma;
        private double TStart(double t) 
        {
            double aStart = TBase(Config.StartAnimationFrames);
            double u = NormalizedTime(Config.StartAnimationFrames, Config.StartAnimationFrames, t);

            return aStart * Math.Pow(u + 1, 3);
        }
        private double TStop(double t)
        {
            double aStop = TBase(Config.StopAnimationStartFrame);
            double u = NormalizedTime(Config.StopAnimationStartFrame, Config.StopAnimationFrames, t);

            return aStop + (Config.FrameCount - aStop) * (1 - Math.Pow(1 - u, 3));
        }

        private double Time(double t)
        {
            if (t < Config.StartAnimationFrames)
            {
                return TStart(t);
            }
            else if (t < Config.StopAnimationStartFrame)
            {
                return TBase(t);
            }
            else return TStop(t);
        }

        private BigFloat GetZoom(int frameId) 
        {
            BigFloat zoom = (Config.ZoomEnd / Config.ZoomStart) ^ (Time(frameId) / Config.FrameCount);
            return Config.ZoomStart * zoom;
        }

        private void CreateOutputDirectory() => Directory.CreateDirectory($"render-{_renderId}");
        private void RemoveOutputDirectory() => Directory.Delete(ImageSequencePath, true);
    }
}
