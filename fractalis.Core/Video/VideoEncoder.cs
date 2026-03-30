using System.Diagnostics;

namespace fractalis.Core.Video
{
    public static class VideoEncoder
    {

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

        public static void MergeImageSequence(string path, int fps, string outputPath)
        {
            if (!IsFFmpegAvailable())
            {
                throw new InvalidOperationException("FFmpeg is not available in PATH or is not installed.");
            }

            string inputPattern = Path.Combine(path, "frame%05d.png");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -framerate {fps} -i \"{inputPattern}\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}.mp4\"",
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
