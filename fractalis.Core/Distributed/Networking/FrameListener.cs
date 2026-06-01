using fractalis.Core.Distributed.Networking.Messages;
using System.Net;
using System.Net.Sockets;
using WatsonWebserver;
using WatsonWebserver.Core;
using WatsonWebserver.Core.Health;

namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Listens for incoming rendered frames over HTTP and saves them to disk.
    /// </summary>
    public class FrameListener : IDisposable
    {
        private readonly Webserver _listener;
        private readonly string _imageSequencePath;
        private readonly Action? _reportFrameReceived;

        /// <summary>
        /// Base URI where the listener is accessible.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameListener"/> class.
        /// </summary>
        /// <param name="port">Port to listen on.</param>
        /// <param name="imageSequencePath">Directory where received frames will be saved.</param>
        public FrameListener(int port, string imageSequencePath, Action? reportFrameReceived = null )
        {
            Uri = new Uri($"http://{GetLocalIPAddress()}:{port}/");
            _imageSequencePath = imageSequencePath;

            // Creating the webserver
            WebserverSettings settings = new WebserverSettings(GetLocalIPAddress(), port);
            _listener = new Webserver(settings, DefaultRoute);

            // Endpoints
            _listener.UseHealthCheck();
            _listener.Post<RenderedImageMessage>("/frame", PostFrame);
            _reportFrameReceived = reportFrameReceived;
        }

        /// <summary>
        /// Retrieves the local IPv4 address of the machine.
        /// </summary>
        /// <returns>The first available IPv4 address.</returns>
        /// <exception cref="Exception">Thrown if no IPv4 address is found.</exception>
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        #region Endpoints

        /// <summary>
        /// Default route handler returning 404 for unknown endpoints.
        /// </summary>
        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send("Not found");
        }

        /// <summary>
        /// Receives a rendered frame and writes it to disk.
        /// </summary>
        /// <param name="req">Incoming request containing frame data.</param>
        /// <returns>A completed task.</returns>
        private Task<object?> PostFrame(ApiRequest req)
        {
            RenderedImageMessage body = req.GetData<RenderedImageMessage>();

            string path = $"{_imageSequencePath}/frame{(body.FrameIndex + 1).ToString().PadLeft(5, '0')}.png";

            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, body.Bytes);
                _reportFrameReceived?.Invoke();
            }

            return Task.FromResult<object?>(null);
        }

        #endregion

        /// <summary>
        /// Starts the listener.
        /// </summary>
        public async Task Start() => await _listener.StartAsync();

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public void Stop() => _listener.Stop();

        /// <inheritdoc/>
        public void Dispose() => _listener.Dispose();
    }
}