using fractalis.Core.Miscellaneous;
using System.Text.Json;

namespace fractalis.Core.Video
{
    public abstract class VideoRendererBase(VideoConfig config)
    {
        private VideoRecoveryConfig? _recoveryConfig;

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
        protected string ImageSequencePath => $"render-{_renderId}";

        /// <summary>
        /// The recovery config associated with this video. It will be saved into the output directory
        /// when not null.
        /// </summary>
        public VideoRecoveryConfig? RecoveryConfig
        {
            get => _recoveryConfig;
            set => _recoveryConfig = value is not null ? value with
            {
                RenderId = _renderId,
            } : null;
        }

        /// <summary>
        /// Merges the rendered image sequence into an MP4 video file with ffmpeg, then removes the image sequence directory.
        /// </summary>
        /// <param name="fileName">The output path of the video.</param>
        public void Save(string fileName = "render.mp4")
        {
            Prompts.Info("Merging image sequence");
            VideoEncoder.MergeImageSequence($"render-{_renderId}", Config.FPS, fileName);
            RemoveOutputDirectory();
            Prompts.Done();
        }

        protected void CreateOutputDirectory() => Directory.CreateDirectory($"render-{_renderId}");
        private void RemoveOutputDirectory() => Directory.Delete(ImageSequencePath, true);
    }
}
