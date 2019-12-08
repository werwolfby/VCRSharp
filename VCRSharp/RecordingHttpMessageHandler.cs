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
            var (requestContent, newRequestContent) = await CassetteBody.CreateCassetteBody(request.Content);

            if (newRequestContent != null)
            {
                request.Content = newRequestContent;
            }
            
            var response = await base.SendAsync(request, cancellationToken);
            
            var recordRequest = CassetteRecordRequest.NewFromRequest(request);
            
            // Host header is required by HTTP 1.1 spec, so we should add it if it is not provided
            if (recordRequest.Headers["Host"] == null)
            {
                recordRequest.Headers.Add("Host", request.RequestUri.IdnHost);
            }
            recordRequest.Body = requestContent;
            
            var recordResponse = CassetteRecordResponse.NewFromResponse(response);
            if (response.Content != null)
            {
                var (responseContent, newResponseContent) = await CassetteBody.CreateCassetteBody(response.Content);
                recordResponse.Body = responseContent;
                if (newResponseContent != null)
                {
                    response.Content = newResponseContent;
                }
            }
            
            var record = new CassetteRecord(recordRequest, recordResponse);
            _cassette.Add(record);

            return response;
        }
    }
}