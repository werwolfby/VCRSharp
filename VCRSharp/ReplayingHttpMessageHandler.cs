using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VCRSharp
{
    public class ReplayingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Cassette _cassette;
        private readonly IEqualityComparer<CassetteRecordRequest>? _comparer;

        public ReplayingHttpMessageHandler(Cassette cassette, IEqualityComparer<CassetteRecordRequest> comparer = null)
        {
            _cassette = cassette;
            _comparer = comparer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var newRecord = CassetteRecordRequest.NewFromRequest(request);
            if (request.Content != null)
            {
                newRecord.Body = await request.Content.ReadAsStringAsync();
                request.Content = new StringContent(newRecord.Body);
            }

            var record = _cassette.Find(newRecord, _comparer);
            if (record == null)
            {
                throw new ArgumentException("Can't find request in cassette", nameof(request));
            }

            var recordResponse = record.Response;
            var response = new HttpResponseMessage
            {
                Version = recordResponse.Version,
                StatusCode = (HttpStatusCode)recordResponse.StatusCode,
                ReasonPhrase = recordResponse.StatusMessage,
                RequestMessage = request,
                Content = new StringContent(recordResponse.Body)
            };
            foreach (string header in recordResponse.Headers)
            {
                if (!response.Headers.TryAddWithoutValidation(header, recordResponse.Headers.GetValues(header)))
                {
                    throw new ArgumentException($"Can't add {header} to response");
                }
            }

            await Task.Yield();

            return response;
        }
    }
}