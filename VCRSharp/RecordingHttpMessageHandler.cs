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
            var recordRequest = CassetteRecordRequest.NewFromRequest(request);
            if (request.Content != null)
            {
                recordRequest.Body = await request.Content.ReadAsStringAsync();
                request.Content = new StringContent(recordRequest.Body);
            }

            var response = await base.SendAsync(request, cancellationToken);
            
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