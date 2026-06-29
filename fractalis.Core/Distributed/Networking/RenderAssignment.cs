namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Sent by the orchestrator to a worker client, describing a unit of rendering work to execute.
    /// </summary>
    public class RenderAssignment
    {
        private long                    _claimedAt      = 0;

        private static readonly long    _leaseDuration  = TimeSpan.FromMinutes(5).Ticks;

        /// <summary>
        /// Unique identifier for this assignment.
        /// </summary>
        public Guid                     Id              { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The unique identifier of the assigned render job.
        /// </summary>
        public required Guid            JobId           { get; init; }

        /// <summary>
        /// Index of the first frame to render.
        /// </summary>
        public required int             StartFrameIndex { get; init; }

        /// <summary>
        /// Number of frames to render.
        /// </summary>
        public required int             FrameCount      { get; init; }

        /// <summary>
        /// Whether this assignment's lease has expired.
        /// </summary>
        public bool                     IsExpired
        {
            get
            {
                long t = Interlocked.Read(ref _claimedAt);
                return t != 0 && DateTime.UtcNow.Ticks - t > _leaseDuration;
            }
        }

        /// <summary>
        /// Whether this assignment is still pending and has not been claimed.
        /// </summary>
        public bool                     IsPending       => Interlocked.Read(ref _claimedAt) == 0;

        /// <summary>
        /// Attempts to claim this assignment for dispatch.
        /// </summary>
        /// <param name="authToken">The issued authentication token if claim succeeded, otherwise <see cref="Guid.Empty"/>.</param>
        /// <returns><see langword="true"/> if the assignment was successfully claimed.</returns>
        public bool TryClaim()
        {
            long now = DateTime.UtcNow.Ticks;
            long claimed = Interlocked.Read(ref _claimedAt);

            if (claimed == 0 || now - claimed > _leaseDuration)
            {
                long previous = Interlocked.CompareExchange(ref _claimedAt, now, claimed);
                return previous == claimed;
            }

            return false;
        }

        /// <summary>
        /// Attempts to cancel this assignment if it has been claimed.
        /// </summary>
        /// <returns><see langword="true"/> if the assignment was successfully reverted to pending.</returns>
        public bool TryCancel()
        {
            long current = Interlocked.Read(ref _claimedAt);
            if (current == 0) return false;

            return Interlocked.CompareExchange(ref _claimedAt, 0, current) == current;
        }
    }
}