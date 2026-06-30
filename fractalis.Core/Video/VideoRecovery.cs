using fractalis.Core.Miscellaneous;
using SixLabors.ImageSharp;
using System.Text.Json;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Provides functionalities to save recovery configs and recover videos.
    /// </summary>
    internal static class VideoRecovery
    {
        private const int BATCH_SIZE = 5;
        private const string FRAME_PREFIX = "frame";

        #region Public methods
        /// <summary>
        /// Saves the recovery config to the specified image sequence folder.
        /// </summary>
        /// <param name="config">The recovery config associated with the video.</param>
        /// <param name="imageSequencePath">The path to the image sequence of the video.</param>
        public static void Save(VideoRecoveryConfig config, string imageSequencePath)
        {
            var outputPath = $"{imageSequencePath}/recovery.json";
            if (File.Exists(outputPath)) return;

            var serliazed = JsonSerializer.Serialize(config, FractalisJsonOptions.Default);
            File.WriteAllText(outputPath, serliazed);
        }

        /// <summary>
        /// Recovers a video given a recovery config path.
        /// </summary>
        /// <param name="recoveryConfigPath">The path of the recovery config file.</param>
        /// <returns>The parsed video config and distributed renderer config when necessary.</returns>
        public static AppSettings Recover(string recoveryConfigPath)
        {
            var folderPath = Path.GetDirectoryName(recoveryConfigPath)
                ?? throw new Exception("The recovery config is not in an image sequence folder.");

            var recoveryConfig = ParseRecoveryConfig(recoveryConfigPath);

            return recoveryConfig.VideoMode switch
            {
                VideoMode.Local => RecoverLocal(folderPath, recoveryConfig),
                VideoMode.Distributed => RecoverDistributed(folderPath, recoveryConfig),
                _ => throw new Exception("Unknown video mode.")
            };
        }
        #endregion

        #region Private helpers
        /// <summary>
        /// Parses a <see cref="VideoRecoveryConfig"/> from a given file path.
        /// </summary>
        /// <param name="path">The path of the recovery config.</param>
        /// <returns>The parsed config.</returns>
        /// <exception cref="JsonException">If the config couldn't be parsed.</exception>
        private static VideoRecoveryConfig ParseRecoveryConfig(string path) =>
            JsonSerializer.Deserialize<VideoRecoveryConfig>(File.ReadAllText(path), FractalisJsonOptions.Default)
            ?? throw new JsonException("Invalid VideoRecoveryConfig JSON.");

        /// <summary>
        /// Scans the specified directory for already rendered frames and returns their indicies.
        /// </summary>
        /// <param name="directoryPath">The path to the frame directory.</param>
        /// <param name="videoMode">The video mode for the video used to validate images.</param>
        /// <returns>The array of indices already rendered.</returns>
        private static HashSet<FrameData> ScanFrameDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath)
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(FRAME_PREFIX))
                .Select(f => new FrameData(f))
                .ToHashSet();
        }

        /// <summary>
        /// Validates a set of frames that were rendered locally, ensuring that they are not corrupted.
        /// </summary>
        /// <param name="frames">The set of frames already rendered.</param>
        private static void ValidateFramesLocal(HashSet<FrameData> frames)
        {
            // For locally rendered videos, the only frame that can be corrupted is the last one
            // (with the largest index), since rendering is sequential.
            var last = frames.MaxBy(f => f.Index);
            if (last is null) return;

            try
            {
                Image.Identify(last.FullPath);
            }
            catch
            {
                Console.WriteLine("Last image was corrupted, skipping it.");
                frames.Remove(last);
            }
        }

        /// <summary>
        /// Validates a set of frames that were rendered distributed, ensuring that they are not corrupted.
        /// </summary>
        /// <param name="frames">The set of frames already rendered.</param>
        private static void ValidateFramesDistributed(HashSet<FrameData> frames)
        {
            var ordered = frames.OrderBy(f => f.Index).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var frame = ordered[i];

                // Corruption check needs to be performed only on
                // batch boundaries or at the end of an incomplete batch. 
                if (
                    frame.Index % BATCH_SIZE == 0 ||
                    (i + 1 < ordered.Count && ordered[i + 1].Index - frame.Index != 1)
                )
                {
                    try
                    {
                        Console.WriteLine($"Checking {frame.Index}");
                        Image.Identify(frame.FullPath);
                    }
                    catch
                    {
                        Console.WriteLine($"Found corrupted frame with index {frame.Index}");
                        frames.Remove(frame);
                    }
                }
            }
        }

        /// <summary>
        /// Recovers a locally rendered video using the saved recovery config.
        /// </summary>
        /// <param name="folderPath">
        /// The path to the folder containing the rendered frames. 
        /// Must be in the same folder as the frames of the video.
        /// </param>
        /// <param name="config">The recovery configuration.</param>
        /// <returns>The recovered app settings for the video rendering.</returns>
        private static AppSettings RecoverLocal(string folderPath, VideoRecoveryConfig config)
        {
            HashSet<FrameData> renderedFrames = ScanFrameDirectory(folderPath);
            ValidateFramesLocal(renderedFrames);

            return new()
            {
                Mode = AppMode.Video,
                FractalRendererConfig = config.FractalRendererConfig,
                VideoMode = VideoMode.Local,
                VideoConfig = config.VideoConfig with
                {
                    RenderIdOverride = config.RenderId,
                    StartFrame = renderedFrames.Count
                },
                OutputPath = config.OutputPath
            };
        }

        /// <summary>
        /// Recovers a distributed video render using the saved recovery config.
        /// </summary>
        /// <param name="folderPath">
        /// The path to the folder containing the rendered frames. 
        /// Must be in the same folder as the frames of the video.
        /// </param>
        /// <param name="config">
        /// The recovery configuration.
        /// </param>
        /// <returns>The recovered app settings for the video rendering.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private static AppSettings RecoverDistributed(string folderPath, VideoRecoveryConfig config)
        {
            HashSet<FrameData> renderedFrames = ScanFrameDirectory(folderPath);

            ValidateFramesDistributed(renderedFrames);

            List<FrameRange> validated = FrameRange.FromIndicies(renderedFrames.Select(f => f.Index));
            List<FrameRange> missing = FrameRange.Invert(validated, 1, config.VideoConfig.FrameCount);

            Console.WriteLine("Missing ranges: ");
            Console.WriteLine(String.Join("\n", missing));

            return new()
            {
                Mode = AppMode.Video,
                FractalRendererConfig = config.FractalRendererConfig,
                VideoMode = VideoMode.Distributed,
                VideoConfig = config.VideoConfig with
                {
                    RenderIdOverride = config.RenderId,
                },
                DistributedRendererSettings = config.DistributedRendererConfig is not null ? 
                    config.DistributedRendererConfig with { FramesToRender = missing }
                : null,
                OutputPath = config.OutputPath
            };
        }
        #endregion
    }
}
