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
    public abstract class VideoRendererBase(VideoConfig config)
    {
        /// <summary>
        /// Unique ID for this render session (can be overridden in <paramref name="config"/>).
        /// </summary>
        protected readonly string   _renderId           = config.RenderIdOverride ?? Guid.NewGuid().ToString();

        /// <summary>
        /// Configuration of the video.
        /// </summary>
        protected VideoConfig       Config              { get; set; } = config;

        /// <summary>
        /// Path of the output directory for the frames.
        /// </summary>
        protected string            ImageSequencePath   => $"render-{_renderId}";

        /// <summary>
        /// Merges the rendered image sequence into an MP4 video file with ffmpeg, then removes the image sequence directory.
        /// </summary>
        /// <param name="fileName">The filename of the video.</param>
        public void Save(string fileName = "render")
        {
            VideoEncoder.MergeImageSequence($"render-{_renderId}", Config.FPS, fileName);
            RemoveOutputDirectory();
        }

        protected void CreateOutputDirectory() => Directory.CreateDirectory($"render-{_renderId}");
        private void RemoveOutputDirectory() => Directory.Delete(ImageSequencePath, true);
    }
}
