using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Video;

namespace fractalis.Core.Distributed.Runtimes
{
    public class WorkerRuntime(IClientContext context) : IRuntime
    {
        private readonly IClientContext                     _context    = context;
        private bool                                        _idle       = true;
        private List<RenderJob>                             _jobs       = new List<RenderJob>();
        private readonly Dictionary<Guid, VideoRenderer>    _renderers  = new();

        private async Task RequestAssignmentAsync() => await _context.SendMessageToServerAsync(new RenderAssignmentRequest());

        public async Task<MessageHandlingResult> HandleMessage(Message message)
        {
            switch (message)
            {
                case RenderJobListMessage jobListMessage:
                    _jobs = jobListMessage.Jobs;

                    // Request assignment if there are jobs
                    if (_jobs.Count > 0)
                    {
                        await RequestAssignmentAsync();
                    }
                    Console.WriteLine($"Currently available jobs: {jobListMessage.Jobs.Count}");
                    break;

                case RenderJobAnnouncementMessage announcementMessage:
                    _jobs.Add(announcementMessage.Job);

                    // Request assignment if idle
                    if (_idle)
                    {
                        await RequestAssignmentAsync();
                    }
                    Console.WriteLine("New job available");
                    break;

                case RenderAssignmentMessage assignmentMessage:
                    RenderAssignment assignment = assignmentMessage.Assignment;

                    _idle = false;

                    // Do the work
                    Console.WriteLine($"Assignment: JobId: {assignment.JobId}, from frame {assignment.StartFrameIndex} render {assignment.FrameCount} frames and upload to {assignmentMessage.UploadUri}");

                    _idle = true;
                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
