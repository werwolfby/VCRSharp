using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace VCRSharp
{
    public class RecordingHttpMessageHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;
        private readonly CookieContainer? _cookieContainer;

        public RecordingHttpMessageHandler(HttpMessageHandler innerHandler, Cassette cassette) : base(innerHandler)
        {
            var cookieContainerProperty = innerHandler.GetType().GetRuntimeProperty(nameof(HttpClientHandler.CookieContainer));
            if (cookieContainerProperty?.PropertyType == typeof(CookieContainer))
            {
                _cookieContainer = (CookieContainer?) cookieContainerProperty.GetValue(innerHandler);
            }
            
            _cassette = cassette;
        }
        
        internal Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken) => SendAsync(request, cancellationToken);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var recordRequest = await CassetteRecordRequest.CreateFromRequest(request, _cookieContainer);
            
            // After sending request _cookieContainer can be extended and recordRequest will definitely change as well
            // cause Cookie header will be added, that's why for compare we should clone CookieContainer
            var cloneCookieContainer = _cookieContainer?.Clone(request.RequestUri);

            var response = await base.SendAsync(request, cancellationToken);

            var recordResponse = await CassetteRecordResponse.CreateFromResponse(response, recordRequest, cloneCookieContainer);
            
            var record = new CassetteRecord(recordRequest, recordResponse);
            _cassette.Add(record);

            return response;
        }
    }
}