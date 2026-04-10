using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed
{
    public class WorkerRuntime : IRuntime
    {
        private List<RenderJob> Jobs = new List<RenderJob>();

        public Task<MessageHandlingResult> HandleMessage(Message message)
        {
            switch (message)
            {
                case RenderJobListMessage jobListMessage:
                    Jobs = jobListMessage.Jobs;

                    Console.WriteLine($"Currently available jobs: {jobListMessage.Jobs.Count}");
                    break;

                case RenderJobAnnouncementMessage announcementMessage:
                    Jobs.Add(announcementMessage.Job);

                    Console.WriteLine("New job available");
                    break;

                case RenderJobAssignmentMessage assignmentMessage:
                    Guid jobId = assignmentMessage.RenderJobId;

                    Console.WriteLine($"Assignment: JobId: {jobId}, from frame {assignmentMessage.StartFrameIndex} render {assignmentMessage.FrameCount} frames and upload to {assignmentMessage.UploadUri}");
                    break;
            }

            return Task.FromResult(MessageHandlingResult.Continue);
        }
    }
}
