using System.Net;
using System.Net.Sockets;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace fractalis.Core.Distributed.Networking
{
    /// <summary>
    /// Represents a request that contains a rendered frame's data.
    /// </summary>
    internal record FrameUploadRequest
    {
        /// <summary>
        /// The unique identifier of the rendered frame.
        /// </summary>
        public required int     FrameId     { get; init; }

        /// <summary>
        /// The raw byte array of the frame.
        /// </summary>
        public required byte[]  Bytes       { get; init; }
    }

    public class FrameListener : IDisposable
    {
        private readonly Webserver _listener;
        public Uri Uri { get; private set; }

        public FrameListener(int port)
        {
            Uri = new Uri($"http://{GetLocalIPAddress()}:{port}/");

            // Creating the webserver
            WebserverSettings settings = new WebserverSettings(GetLocalIPAddress(), port);
            _listener = new Webserver(settings, DefaultRoute);

            // Endpoints
            _listener.Post<FrameUploadRequest>("/jobs/{jobId}", PostFrame);
        }

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
        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send("Not found");
        }

        private static Task<object?> PostFrame(ApiRequest req)
        {
            Guid jobId              = req.Parameters.GetGuid("jobId");
            FrameUploadRequest body = req.GetData<FrameUploadRequest>();

            Console.WriteLine($"Got data for job {jobId}, frame {body.FrameId}.");
            return Task.FromResult<object?>(null);
        }
        #endregion

        public async Task Start()   => await _listener.StartAsync();

        public void Stop()          => _listener.Stop();

        public void Dispose()       => _listener.Dispose();
    }
}