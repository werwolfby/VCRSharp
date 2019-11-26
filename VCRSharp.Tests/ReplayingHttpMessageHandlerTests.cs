using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class ReplayingHttpMessageHandlerTests
    {
        [Test]
        public async Task SendAsync_GetRequest_Success()
        {
            var cassette = new Cassette(".");
            var record = new CassetteRecord
            {
                Request = new CassetteRecordRequest
                {
                    Method = HttpMethod.Get.Method,
                    Uri = new Uri("http://localhost:8080/test"),
                    Headers = new NameValueCollection
                    {
                        {"Cookie", "value=1"},
                    },
                    Body = null
                },
                Response = new CassetteRecordResponse
                {
                    Version = new Version(1, 1),
                    StatusCode = 200,
                    StatusMessage = "OK",
                    Headers = new NameValueCollection
                    {
                        {"Server", "Test"},
                    },
                    Body = @"{""a"": 1, ""b"": 2}",
                },
            };
            cassette.Add(record);
            
            var replayingHttpMessageHandler = new PublicReplayingHttpMessageHandler(cassette);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {"Cookie", "value=1"},
                },
                Version = new Version(1, 1),
                RequestUri = new Uri("http://localhost:8080/test"),
                Content = null,
            };
            var response = await replayingHttpMessageHandler.SendAsync(request, CancellationToken.None);
            
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Version, Is.EqualTo(record.Response.Version));
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode) record.Response.StatusCode));
            Assert.That(response.ReasonPhrase, Is.EqualTo(record.Response.StatusMessage));
            Assert.That(response.RequestMessage, Is.Not.Null);
            Assert.That(response.Headers.GetValues("Server"), Is.EqualTo(new[] {record.Response.Headers["Server"]}));
        }
        
        private class PublicReplayingHttpMessageHandler : ReplayingHttpMessageHandler
        {
            public PublicReplayingHttpMessageHandler(Cassette cassette) : base(cassette)
            {
            }
            
            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);
        }
    }
}