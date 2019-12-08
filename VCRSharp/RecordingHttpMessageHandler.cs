using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VCRSharp
{
    public class RecordingHttpMessageHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;

        public RecordingHttpMessageHandler(HttpMessageHandler innerHandler, Cassette cassette) : base(innerHandler)
        {
            _cassette = cassette;
        }
        
        internal Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken) => SendAsync(request, cancellationToken);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var recordRequest = await CassetteRecordRequest.CreateFromRequest(request);

            var response = await base.SendAsync(request, cancellationToken);

            var recordResponse = await CassetteRecordResponse.CreateFromResponse(response, recordRequest);
            
            var record = new CassetteRecord(recordRequest, recordResponse);
            _cassette.Add(record);

            return response;
        }
    }
}