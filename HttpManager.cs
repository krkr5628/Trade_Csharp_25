using System;
using System.Net.Http;

namespace WindowsFormsApp1
{
    public static class HttpManager
    {
        private static readonly Lazy<HttpClient> lazyClient = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();
            // Set a reasonable timeout for API calls
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        });

        public static HttpClient Client => lazyClient.Value;
    }
}
