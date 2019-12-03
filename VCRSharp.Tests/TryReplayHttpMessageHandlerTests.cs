using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class TryReplayHttpMessageHandlerTests
    {
        [Test]
        public async Task SendAsync_ExistsInCassette_DoesNotCallInnerHttpHandler()
        {
            var cassette = new Cassette();
            var record = new CassetteRecord(
                new CassetteRecordRequest(
                    HttpMethod.Get.Method,
                    new Uri("http://localhost:8080/test"),
                    new NameValueCollection
                    {
                        {"Cookie", "value=1"},
                    }),
                new CassetteRecordResponse(
                    new Version(1, 1),
                    200,
                    "OK",
                    new NameValueCollection
                    {
                        {"Server", "Test"},
                    },
                    @"{""a"": 1, ""b"": 2}"));
            cassette.Add(record);
            
            var mockHttpHandler = new MockHttpRequestHandler();
            var tryReplayMessageHandler = new PublicTryReplayHttpMessageHandler(cassette, mockHttpHandler);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:8080/test"),
            };
            var response = await tryReplayMessageHandler.SendAsync(request);
            
            Assert.That(mockHttpHandler.Invoked, Is.False);
            
            Assert.That(response.Version, Is.EqualTo(record.Response.Version));
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode) record.Response.StatusCode));
            Assert.That(response.ReasonPhrase, Is.EqualTo(record.Response.StatusMessage));
            Assert.That(response.RequestMessage, Is.Not.Null);
            Assert.That(response.Headers.GetValues("Server"), Is.EqualTo(new[] {record.Response.Headers["Server"]}));
        }

        [Test]
        public async Task SendAsync_DoesNotExistInCassette_CallsInnerHttpHandler()
        {
            var cassette = new Cassette();
            
            var mockHttpHandler = new MockHttpRequestHandler();
            var tryReplayMessageHandler = new PublicTryReplayHttpMessageHandler(cassette, mockHttpHandler);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:8080/test"),
            };
            var response = await tryReplayMessageHandler.SendAsync(request);
            
            Assert.That(mockHttpHandler.Invoked, Is.True);
            
            Assert.That(response.Version, Is.EqualTo(new Version(1, 1)));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.ReasonPhrase, Is.EqualTo("OK"));
            Assert.That(response.RequestMessage, Is.Not.Null);
            Assert.That(response.Headers.GetValues("Server"), Is.EqualTo(new[] {"Test-Server"}));
        }
        
        private class PublicTryReplayHttpMessageHandler : TryReplayHttpMessageHandler
        {
            public PublicTryReplayHttpMessageHandler(Cassette cassette, HttpMessageHandler innerHandler, IEqualityComparer<CassetteRecordRequest> comparer = null) : base(cassette, innerHandler, comparer)
            {
            }

            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) => base.SendAsync(request, cancellationToken);
        }
        
        private class MockHttpRequestHandler : HttpMessageHandler
        {
            public bool Invoked { get; private set; }
            
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await Task.Yield();
                Invoked = true;
                return new HttpResponseMessage
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "OK",
                    Version = new Version(1, 1),
                    Headers =
                    {
                        {"Server", "Test-Server"}, 
                    },
                    Content = new StringContent(@"{""a"":1, ""b"": 2}"),
                };
            }
        }
    }
}