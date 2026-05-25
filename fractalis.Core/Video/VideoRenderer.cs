using fractalis.Core.Distributed;
using fractalis.Core.Numbers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Responsible for generating a sequence of fractal images and assembling them into a video.
    /// </summary>
    /// <param name="renderer">The fractal renderer to use to generate the frames with.</param>
    /// <param name="config">Configuration of the video.</param>
    public class VideoRenderer(FractalRenderer renderer, VideoConfig config) : VideoRendererBase(config)
    {
        /// <summary>
        /// The fractal renderer to use to generate the frames with.
        /// </summary>
        private FractalRenderer Renderer { get; set; } = renderer;

        /// <summary>
        /// Delta controls linear scaling of time across the animation. 
        /// Ensures start/stop easing transitions align correctly with total duration.
        /// </summary>
        private double              Delta
        {
            get
            {
                double pStop = Config.StopAnimation.Exponent;
                double pStartAdjusted = 1 / Config.StartAnimation.Exponent - 1;

                return pStop * Config.FrameCount / 
                    (
                        Config.FrameCount + 
                        (pStop - 1) * Config.StopAnimationStartFrame +
                        pStop * pStartAdjusted * Config.StartAnimationFrames
                    );
            }
        }

        /// <summary>
        /// Gamma shifts the time curve to ensure continuity between easing phases.
        /// </summary>
        private double              Gamma
        {
            get
            {
                return (1 / Config.StartAnimation.Exponent - 1) * Config.StartAnimationFrames * Delta;
            }
        }

        /// <summary>
        /// Starts rendering the video locally, saving frames to the output directory.
        /// </summary>
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

        /// <summary>
        /// Renders a segment of the video, invoking a callback for each frame with its PNG bytes.
        /// </summary>
        /// <param name="startFrame">Absolute frame index to begin rendering from.</param>
        /// <param name="frameCount">Number of frames to render.</param>
        /// <param name="onFrame">Async callback receiving the absolute frame index and PNG bytes.</param>
        public void RenderSegment(int startFrame, int frameCount, Action<int, byte[]> onFrame)
        {
            int endFrame = Math.Min(startFrame + frameCount, Config.FrameCount);

            for (int i = startFrame; i < endFrame; i++)
            {
                Renderer.Zoom = GetZoom(i);

                using Image<Rgb24> image = Renderer.Render(false);
                using MemoryStream ms = new();

                image.SaveAsPng(ms);
                onFrame(i, ms.ToArray());
            }
        }

        /// <summary>
        /// Normalizes a frame index into a [0,1] range over a given segment.
        /// </summary>
        /// <param name="from">Start of the segment.</param>
        /// <param name="length">Length of the segment.</param>
        /// <param name="frame">Current frame index</param>
        private static double NormalizedTime(double from, double length, double frame) => (frame - from) / length;

        /// <summary>
        /// Base linear time mapping with correction applied.
        /// </summary>
        /// <param name="t">Current frame index</param>
        private double TBase(double t) => t * Delta + Gamma;

        /// <summary>
        /// Time mapping for the start easing phase.
        /// </summary>
        /// <param name="t">Current frame index</param>
        private double TStart(double t) 
        {
            double aStart = TBase(Config.StartAnimationFrames);
            double u = NormalizedTime(Config.StartAnimationFrames, Config.StartAnimationFrames, t);

            return aStart * Math.Pow(u + 1, Config.StartAnimation.Exponent);
        }

        /// <summary>
        /// Time mapping for the stop easing phase.
        /// </summary>
        /// <param name="t">Current frame index</param>
        private double TStop(double t)
        {
            double aStop = TBase(Config.StopAnimationStartFrame);
            double u = NormalizedTime(Config.StopAnimationStartFrame, Config.StopAnimationFrames, t);

            return aStop + (Config.FrameCount - aStop) * (1 - Math.Pow(1 - u, Config.StopAnimation.Exponent));
        }

        /// <summary>
        /// Returns corrected timeline position for a given frame. 
        /// Handles start easing, linear section, and stop easing.
        /// </summary>
        /// <param name="t">Current frame index</param>
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

        /// <summary>
        /// Computes zoom level for a specific frame using exponential interpolation.
        /// </summary>
        /// <param name="frameId">Current frame index.</param>
        private BigFloat GetZoom(int frameId) 
        {
            BigFloat zoom = (Config.ZoomEnd / Config.ZoomStart) ^ (Time(frameId) / Config.FrameCount);
            return Config.ZoomStart * zoom;
        }
    }
}
