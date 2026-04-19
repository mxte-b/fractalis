namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Sent by the orchestrator to a worker client, describing a unit of rendering work to execute.
    /// </summary>
    public class RenderAssignment
    {
        private int             _pending        = 1;

        /// <summary>
        /// Attempts to claim this assignment for dispatch. Returns false if already claimed.
        /// </summary>
        public bool             TryClaim()      => Interlocked.CompareExchange(ref _pending, 0, 1) == 1;

        /// <summary>
        /// Attempts to cancel this assignment if it has been claimed.
        /// </summary>
        /// <returns><see langword="true"/> if the assignment was successfully reverted to pending; otherwise <see langword="false"/>.</returns>
        public bool             TryCancel()     => Interlocked.CompareExchange(ref _pending, 1, 0) == 0;

        /// <summary>
        /// Whether this assignment is still pending and has not been assigned.
        /// </summary>
        public bool             IsPending       => Volatile.Read(ref _pending) == 1;

        /// <summary>
        /// Unique identifier for this assignment.
        /// </summary>
        public Guid             Id              { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The unique identifier of the assigned render job.
        /// </summary>
        public required Guid    JobId           { get; init; }

        /// <summary>
        /// Index of the first frame to render.
        /// </summary>
        public required int     StartFrameIndex { get; init; }

        /// <summary>
        /// Number of frames to render
        /// </summary>
        public required int     FrameCount      { get; init; }
    }
}
