using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fractalis.Core.Distributed.Networking
{
    public static class HttpHelper
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Sends a POST request with a serialized <see cref="Message"/> payload.
        /// </summary>
        /// <param name="url">Target endpoint URL.</param>
        /// <param name="data">Message to serialize and send.</param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">Thrown if the response indicates failure.</exception>
        public static async Task<string> PostAsync(string url, Message data)
        {
            string json = JsonSerializer.Serialize(data);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
