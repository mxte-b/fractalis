using System.Diagnostics;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Responsible for handling video encoding using FFmpeg.
    /// </summary>
    public static class VideoEncoder
    {
        /// <summary>
        /// Checks if FFmpeg is available on the machine's PATH.
        /// </summary>
        /// <returns><see langword="true"/> if available, <see langword="false"/> otherwise.</returns>
        public static bool IsFFmpegAvailable()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = "-version";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Merges the image sequence at <paramref name="path"/> using FFmpeg, and saves the video into <paramref name="outputPath"/>
        /// </summary>
        /// <param name="path">The image sequence path.</param>
        /// <param name="fps">The desired video framerate.</param>
        /// <param name="outputPath">The output path of the video.</param>
        /// <exception cref="InvalidOperationException">If FFmpeg is not available via PATH.</exception>
        /// <exception cref="Exception">If an exception occurs while encoding.</exception>
        public static void MergeImageSequence(string path, int fps, string outputPath)
        {
            if (!IsFFmpegAvailable())
            {
                throw new InvalidOperationException("FFmpeg is not available in PATH or is not installed.");
            }

            string inputPattern = Path.Combine(path, "frame%05d.png");

            ProcessStartInfo startInfo = new()
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -framerate {fps} -i \"{inputPattern}\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo) ?? throw new Exception("Couldn't start encoding process.");

            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed:\n{error}");
            }
                
        }
    }
}
