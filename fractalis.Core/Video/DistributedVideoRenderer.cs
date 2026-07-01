using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using fractalis.Core.Renderers;
using Spectre.Console;

namespace fractalis.Core.Video
{
    public class DistributedVideoRenderer(
        FractalRendererConfig rendererConfig, 
        VideoConfig videoConfig, 
        DistributedRendererConfig distributedConfig
    ) : VideoRendererBase(videoConfig)
    {
        private FrameListener? _listener;

        /// <summary>
        /// Starts rendering the video using the locally distributed render network.
        /// </summary>
        /// <param name="config">The configuration for </param>
        public async Task Start()
        {
            // If the render ranges are defined but empty, it means all frames have been previously render.
            // In this case, there is nothing to do.
            if (distributedConfig.FramesToRender is not null && distributedConfig.FramesToRender.Count == 0) return;

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
                var missingFrames = distributedConfig.FramesToRender?.Sum(r => r.Count) ?? Config.FrameCount;
                task.Value = Config.FrameCount - missingFrames;

                // Connecting to the orchestrator
                InitiatorClient client = new();
                await client.Connect(distributedConfig.OrchestratorUri, "Administrator");

                var allFramesReceived = new TaskCompletionSource();
                var remainingFrames = missingFrames;

                // Starting frame listener
                _listener = new FrameListener(distributedConfig.FrameListenerPort, ImageSequencePath, () => {
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
                    FramesToRender = distributedConfig.FramesToRender
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
