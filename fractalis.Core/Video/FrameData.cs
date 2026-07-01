namespace fractalis.Core.Video
{
    /// <summary>
    /// Represents data for a single image frame of a rendered video.
    /// This record type is used for video recovery.
    /// </summary>
    public sealed record FrameData(string FullPath)
    {
        /// <summary>The full path to the frame.</summary>
        public string FullPath { get; } = FullPath;

        /// <summary>The extension of the frame.</summary>
        public string Extension => Path.GetExtension(FullPath);

        /// <summary>The filename of the frame, without the extension.</summary>
        public string FileName => Path.GetFileNameWithoutExtension(FullPath);

        /// <summary>The zero-based index of the frame.</summary>
        public int Index => int.Parse(FileName.Replace("frame", ""));
    }
}
