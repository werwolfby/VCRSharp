using System.Net.Http;
using System.Text;
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
            string? requestContent = null; 
            if (request.Content != null)
            {
                var charSet = request.Content.Headers.ContentType.CharSet;
                requestContent = await request.Content.ReadAsStringAsync();
                request.Content = new StringContent(requestContent, Encoding.GetEncoding(charSet), request.Content.Headers.ContentType.MediaType);
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
                recordResponse.Body = await response.Content.ReadAsStringAsync();
                response.Content = new StringContent(recordResponse.Body);
            }
            
            var record = new CassetteRecord(recordRequest, recordResponse);
            _cassette.Add(record);

            return response;
        }
    }
}