using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VCRSharp
{
    public class TryReplayHttpMessageHandler : HttpMessageHandler
    {
        private readonly ReplayingHttpMessageHandler _replayingHttpMessageHandler;
        private readonly RecordingHttpMessageHandler _recordingHttpMessageHandler;

        public TryReplayHttpMessageHandler(Cassette cassette, HttpMessageHandler innerHandler, IEqualityComparer<CassetteRecordRequest>? comparer = null)
        {
            _replayingHttpMessageHandler = new ReplayingHttpMessageHandler(cassette, comparer);
            _recordingHttpMessageHandler = new RecordingHttpMessageHandler(innerHandler, cassette);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await _replayingHttpMessageHandler.SendAsyncInternal(request, cancellationToken);
            }
            catch (ArgumentException e) when(e.ParamName == nameof(request))
            {
                return await _recordingHttpMessageHandler.SendAsyncInternal(request, cancellationToken);
            }
        }
    }
}