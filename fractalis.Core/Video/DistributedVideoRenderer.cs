using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Renderers;
using Spectre.Console;

namespace fractalis.Core.Video
{
    public class DistributedVideoRenderer(FractalRendererConfig rendererConfig, VideoConfig videoConfig) : VideoRendererBase(videoConfig)
    {
        private FrameListener? _listener;

        /// <summary>
        /// Starts rendering the video using the locally distributed render network.
        /// </summary>
        /// <param name="uri">URI of the network orchestrator.</param>
        /// <param name="listenPort">The listen port of the frame listener.</param>
        public async Task Start(Uri orchestratorUri, int listenPort = 8060)
        {
            await AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            ])
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"<#> Receiving frames", maxValue: Config.FrameCount);
                // Connecting to the orchestrator
                InitiatorClient client = new();
                await client.Connect(orchestratorUri, "Administrator");

                var allFramesReceived = new TaskCompletionSource();
                var remainingFrames = Config.FrameCount;

                // Starting frame listener
                _listener = new FrameListener(listenPort, ImageSequencePath, () => {
                    lock (task)
                    {
                        task.Increment(1);
                        if (--remainingFrames == 0)
                        {
                            allFramesReceived.TrySetResult();
                        }
                    }
                });
                _ = _listener.Start();

                CreateOutputDirectory();

                if (RecoveryConfig is not null) VideoRecovery.Save(RecoveryConfig, ImageSequencePath);

                // Sending render request to the orchestrator
                await client.SendMessageToServerAsync(new VideoRenderRequest()
                {
                    UploadUri = new Uri(_listener.Uri, "/frame"),
                    VideoConfig = Config,
                    FractalRendererConfig = rendererConfig,
                });

                // Wait until the job has been completed and all frames have arrived
                await Task.WhenAll(
                    client.Start(),
                    allFramesReceived.Task
                    );

                // Cleanup
                _listener.Stop();
                await client.Disconnect();

                task.StopTask();
            });
        }
    }
}
