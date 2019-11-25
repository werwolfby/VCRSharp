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

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var record = new CassetteRecord();
            
            record.Request = CassetteRecordRequest.NewFromRequest(request);
            if (request.Content != null)
            {
                record.Request.Body = await request.Content.ReadAsStringAsync();
                request.Content = new StringContent(record.Request.Body);
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            record.Response = CassetteRecordResponse.NewFromResponse(response);
            if (response.Content != null)
            {
                record.Response.Body = await response.Content.ReadAsStringAsync();
                response.Content = new StringContent(record.Response.Body);
            }
            
            _cassette.Add(record);

            return response;
        }
    }
}