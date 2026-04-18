using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Video;
using System.Text.Json;

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
                    Console.WriteLine($"Currently available jobs: {jobListMessage.Jobs.Count}");

                    // Request assignment if there are jobs
                    if (_jobs.Count > 0)
                    {
                        await RequestAssignmentAsync();
                    }
                    break;

                case RenderJobAnnouncementMessage announcementMessage:
                    Console.WriteLine("New job available");
                    RenderJob announcedJob = announcementMessage.Job;

                    _jobs.Add(announcedJob);
                    _renderers.Add(announcedJob.Id, new VideoRenderer(new FractalRenderer(announcedJob.FractalRendererConfig), announcedJob.VideoConfig));

                    // Request assignment if idle
                    if (_idle)
                    {
                        await RequestAssignmentAsync();
                    }
                    break;

                case RenderAssignmentMessage assignmentMessage:
                    RenderAssignment assignment = assignmentMessage.Assignment;
                    RenderJob? assignedJob = _jobs.FirstOrDefault(x => x.Id == assignment.JobId);
                    if (assignedJob is null)
                    {
                        Console.WriteLine("No matching job found.");
                        break;
                    }

                    _idle = false;

                    Console.WriteLine($"Assignment: Id: {assignment.Id} JobId: {assignment.JobId}, from frame {assignment.StartFrameIndex} render {assignment.FrameCount} frames and upload to {assignedJob.UploadUri}");

                    _renderers.TryGetValue(assignment.JobId, out VideoRenderer? renderer);
                    if (renderer is null)
                    {
                        Console.WriteLine("Renderer not found");
                        _idle = true;
                        break;
                    }

                    renderer.RenderSegment(assignment.StartFrameIndex, assignment.FrameCount, (frameIndex, bytes) =>
                    {
                        _ = HttpHelper.PostAsync(assignedJob.UploadUri.ToString(), new RenderedImageMessage()
                            {
                                FrameIndex = frameIndex,
                                Bytes = bytes
                            }
                        );
                    });

                    _idle = true;
                    await RequestAssignmentAsync();
                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
