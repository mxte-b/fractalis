using fractalis.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Video
{
    public class DistributedVideoRenderer(FractalRenderer renderer, VideoConfig config) : VideoRenderer(renderer, config)
    {
        private FrameListener? _listener;

        public void Initialize(int listenPort = 8060)
        {
            _listener = new FrameListener(listenPort);
        }

        /// <summary>
        /// Starts rendering the video using the locally distributed render network.
        /// </summary>
        /// <param name="uri">URI of the network orchestrator.</param>
        /// <param name="config">Fractal renderer config to use.</param>
        public async Task Start(Uri uri, FractalRendererConfig config)
        {
            if (_listener is null)
            {
                throw new Exception("The video renderer is not yet initialized.");
            }

            // Starting frame listener
            _ = _listener.Start();
            Console.WriteLine($"Frame listener running at {_listener.Url}");

            // Connecting to the orchestrator
            Client client = new Client(new InitiatorRuntime());
            await client.Connect(uri, "Administrator", ClientRole.Initiator);

            // Sending render request to the orchestrator
            await client.SendMessageToServerAsync(new VideoRenderRequest()
            {
                UploadUrl = _listener.Url,
                VideoConfig = Config,
                FractalRendererConfig = config
            });

            // Wait until the job has been completed
            await client.Start();
            
            // Cleanup
            _listener.Stop();
            await client.Disconnect();
        }
    }
}
