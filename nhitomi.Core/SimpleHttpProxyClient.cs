using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace nhitomi.Core
{
    public class SimpleHttpProxyClient : IHttpProxyClient
    {
        public HttpClient Client { get; }
        public ProxyList Proxies => null;

        public SimpleHttpProxyClient(HttpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public ProxyInfo GetNextProxy() => null;

        public Task<HttpResponseMessage> GetAsync(
            string requestUrl,
            bool allowCache = false,
            CancellationToken cancellationToken = default) =>
            Client.GetAsync(requestUrl, cancellationToken);
    }
}