using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VCRSharp.IntegrationTests
{
    internal class StubHttpRequestHandler : DelegatingHandler
    {
        public StubHttpRequestHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public bool Invoked { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Invoked = true;
            return base.SendAsync(request, cancellationToken);
        }
    }
}