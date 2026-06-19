using fractalis.Core.Distributed.Contexts;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Renderers;
using fractalis.Core.Video;
using System.ComponentModel;

namespace fractalis.Core.Distributed.Runtimes
{
    public class WorkerRuntime(IClientContext context, double processorUsageLimit) : IRuntime
    {
        private readonly IClientContext                     _context    = context;
        private bool                                        _idle       = true;
        private List<RenderJob>                             _jobs       = [];
        private readonly Dictionary<Guid, VideoRenderer>    _renderers  = [];

        private async Task RequestAssignmentAsync() => await _context.SendMessageToServerAsync(new RenderAssignmentRequest());

        private VideoRenderer CreateRenderer(FractalRendererConfig rendererConfig, VideoConfig videoConfig)
        {
            return new VideoRenderer(new FractalRenderer(rendererConfig with
            {
                ProcessorUsageLimit = processorUsageLimit
            }), videoConfig);
        }

        public async Task<MessageHandlingResult> HandleMessage(Message message)
        {
            switch (message)
            {
                case RenderJobListMessage jobListMessage:
                    _jobs = jobListMessage.Jobs;
                    _renderers.Clear();

                    foreach (var job in _jobs)
                    {
                        _renderers.Add(job.Id, CreateRenderer(job.FractalRendererConfig, job.VideoConfig));
                    }
                    
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
                    _renderers.Add(announcedJob.Id, CreateRenderer(announcedJob.FractalRendererConfig, announcedJob.VideoConfig));

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

                    //Console.WriteLine($"Assignment: Id: {assignment.Id} JobId: {assignment.JobId}, from frame {assignment.StartFrameIndex} render {assignment.FrameCount} frames and upload to {assignedJob.UploadUri}");
                    Console.WriteLine($"New assignment: {assignment.Id} - {assignment.StartFrameIndex}");

                    // Render images
                    _renderers.TryGetValue(assignment.JobId, out VideoRenderer? renderer);
                    if (renderer is null)
                    {
                        Console.WriteLine("Renderer not found");
                        _idle = true;
                        break;
                    }

                    var uploads = new List<Task>();

                    renderer.RenderSegment(assignment.StartFrameIndex, assignment.FrameCount, (frameIndex, bytes) =>
                    {
                        uploads.Add(
                            HttpHelper.PostAsync(
                                assignedJob.UploadUri.ToString(),
                                new RenderedImageMessage
                                {
                                    FrameIndex = frameIndex,
                                    Bytes = bytes
                                })
                        );
                    });

                    await Task.WhenAll(uploads);

                    // Report back to the orchestrator
                    _ = _context.SendMessageToServerAsync(new RenderAssignmentStatusMessage()
                    {
                        AssignmentId = assignment.Id,
                        Status = RenderStatus.Finished
                    });

                    // Request new assignment
                    _idle = true;
                    await RequestAssignmentAsync();
                    break;

                case RenderJobStatusMessage jobStatusMessage:
                    if (jobStatusMessage.Status == RenderStatus.Finished)
                    {
                        RenderJob? job = _jobs.FirstOrDefault(x => x.Id == jobStatusMessage.JobId);
                        if (job is null) break;

                        _jobs.Remove(job);
                        _renderers.Remove(job.Id);
                    }
                    break;

                case NoAssignmentMessage:
                    Console.WriteLine("No more assignments are available right now.");

                    if (_jobs.Count > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        await RequestAssignmentAsync();
                    }

                    break;
            }

            return MessageHandlingResult.Continue;
        }
    }
}
