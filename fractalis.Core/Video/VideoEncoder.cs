using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    public class VideoEncoder
    {
        private double Duration { get; set; }
        private int FPS { get; set; }
        public string InputDirectory { get; set; }
        public string OutputPath { get; set; }

        public VideoEncoder() 
        {
            if (!IsFFmpegAvailable())
            {
                throw new InvalidOperationException("FFmpeg is not installed on this machine.");
            }
        }

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

        public void MergeImageSequence()
        {
            // Merge image sequence into a video with FFmpeg
        }
    }
}
