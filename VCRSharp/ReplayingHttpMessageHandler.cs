﻿using System;
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
        private readonly CookieContainer? _cookieContainer;
        private readonly IEqualityComparer<CassetteRecordRequest>? _comparer;

        public ReplayingHttpMessageHandler(Cassette cassette, CookieContainer? cookieContainer = null, IEqualityComparer<CassetteRecordRequest>? comparer = null)
        {
            _cassette = cassette;
            _cookieContainer = cookieContainer;
            _comparer = comparer;
        }
        
        internal Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken) => SendAsync(request, cancellationToken);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var newRecord = await CassetteRecordRequest.CreateFromRequest(request, _cookieContainer);

            var record = _cassette.Find(newRecord, _comparer);
            if (record == null)
            {
                throw new ArgumentException("Can't find request in cassette", nameof(request));
            }

            var recordResponse = record.Response;
            var response = recordResponse.ToResponseMessage();
            
            // If there are no changes in request stored in cassette, then use original request
            response.RequestMessage ??= request;

            if (_cookieContainer != null)
            {
                CookieHelper.ProcessReceivedCookies(response, _cookieContainer);
            }

            // Simulate async processing
            await Task.Yield();

            return response;
        }
    }
}