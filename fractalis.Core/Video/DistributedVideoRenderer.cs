using fractalis.Core.Distributed;
using fractalis.Core.Distributed.Clients;
using fractalis.Core.Distributed.Networking;
using fractalis.Core.Distributed.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    public class DistributedVideoRenderer(FractalRendererConfig rendererConfig, VideoConfig videoConfig) : VideoRendererBase(videoConfig)
    {
        private FrameListener? _listener;

        public void Initialize(int listenPort = 8060)
        {
            _listener = new FrameListener(listenPort, ImageSequencePath);
        }

        /// <summary>
        /// Starts rendering the video using the locally distributed render network.
        /// </summary>
        /// <param name="uri">URI of the network orchestrator.</param>
        /// <param name="config">Fractal renderer config to use.</param>
        public async Task Start(Uri orchestratorUri)
        {
            if (_listener is null)
            {
                throw new Exception("The video renderer is not yet initialized.");
            }

            CreateOutputDirectory();

            // Starting frame listener
            _ = _listener.Start();
            Console.WriteLine($"Frame listener running at {_listener.Uri}");

            // Connecting to the orchestrator
            InitiatorClient client = new InitiatorClient();
            await client.Connect(orchestratorUri, "Administrator");

            // Sending render request to the orchestrator
            await client.SendMessageToServerAsync(new VideoRenderRequest()
            {
                UploadUri = new Uri(_listener.Uri, "/frame"),
                VideoConfig = Config,
                FractalRendererConfig = rendererConfig,
            });

            // Wait until the job has been completed
            await client.Start();
            
            // Cleanup
            _listener.Stop();
            await client.Disconnect();
        }
    }
}
