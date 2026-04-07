using fractalis.Core.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Configuration for generating a fractal zoom video.
    /// </summary>
    public record VideoConfig()
    {
        /// <summary>
        /// Total duration of the video in seconds.
        /// </summary>
        public required double      Duration            { get; init; }

        /// <summary>
        /// Starting zoom level.
        /// </summary>
        [JsonConverter(typeof(BigFloatJsonConverter))]
        public required BigFloat    ZoomStart           { get; init; }

        /// <summary>
        /// Ending zoom level.
        /// </summary>
        [JsonConverter(typeof(BigFloatJsonConverter))]
        public required BigFloat    ZoomEnd             { get; init; }

        /// <summary>
        /// Frames per second.
        /// </summary>
        public int                  FPS                 { get; init; } = 30;

        /// <summary>
        /// Easing settings for the start of the animation.
        /// </summary>
        public AnimationSettings    StartAnimation      { get; init; } = new AnimationSettings();

        /// <summary>
        /// Easing settings for the end of the animation.
        /// </summary>
        public AnimationSettings    StopAnimation       { get; init; } = new AnimationSettings();

        /// <summary>
        /// Frame index to start rendering from (can be used to resume rendering).
        /// </summary>
        public int                  StartFrame          { get; init; }

        /// <summary>
        /// Overrides the identifier assigned to the render (used for resume functionality).
        /// </summary>
        public string?              RenderIdOverride    { get; init; } = null;

        /// <summary>
        /// Frame count of the start phase.
        /// </summary>
        public int                  StartAnimationFrames => (int)Math.Round(StartAnimation.Duration * FPS);

        /// <summary>
        /// Frame count of the stop phase.
        /// </summary>
        public int                  StopAnimationFrames => (int)Math.Round(StopAnimation.Duration * FPS);

        /// <summary>
        /// Total frame count of the video.
        /// </summary>
        public int                  FrameCount => (int)Math.Floor(Duration * FPS);

        /// <summary>
        /// Start frame index of the stop phase.
        /// </summary>
        public int                  StopAnimationStartFrame => FrameCount - StopAnimationFrames;
    }
}
