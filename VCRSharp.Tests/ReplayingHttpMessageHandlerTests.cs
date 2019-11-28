using System;
using System.Collections.Specialized;
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
        
        [Test]
        public async Task SendAsync_PostRequest_Success()
        {
            var cassette = new Cassette();
            var record = new CassetteRecord(
                new CassetteRecordRequest(
                    HttpMethod.Post.Method,
                    new Uri("http://localhost:8080/test"),
                    new NameValueCollection
                    {
                        {"Cookie", "value=1"},
                    },
                    "{}"),
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
            
            var replayingHttpMessageHandler = new PublicReplayingHttpMessageHandler(cassette);

            var request = new HttpRequestMessage
            {
                Method =HttpMethod.Post,
                Headers =
                {
                    {"Cookie", "value=1"},
                },
                Version = new Version(1, 1),
                RequestUri = new Uri("http://localhost:8080/test"),
                Content = new StringContent("{}"),
            };
            var response = await replayingHttpMessageHandler.SendAsync(request, CancellationToken.None);
            
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Version, Is.EqualTo(record.Response.Version));
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode) record.Response.StatusCode));
            Assert.That(response.ReasonPhrase, Is.EqualTo(record.Response.StatusMessage));
            Assert.That(response.RequestMessage, Is.Not.Null);
            Assert.That(response.Headers.GetValues("Server"), Is.EqualTo(new[] {record.Response.Headers["Server"]}));
        }
        
        [Test]
        public void SendAsync_NotFoundRequest_ThrowsArgumentException()
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
            
            var replayingHttpMessageHandler = new PublicReplayingHttpMessageHandler(cassette);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {"Cookie", "value=1"},
                },
                Version = new Version(1, 1),
                RequestUri = new Uri("http://localhost:8080/test1"),
                Content = null,
            };
            var argumentException = Assert.ThrowsAsync<ArgumentException>(() => replayingHttpMessageHandler.SendAsync(request, CancellationToken.None));
            
            Assert.That(argumentException.ParamName, Is.EqualTo("request"));
        }
        
        [Test]
        public void SendAsync_WrongResponseHeader_ThrowsArgumentException()
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
                        {"Content-Type", "application/json"},
                    },
                    @"{""a"": 1, ""b"": 2}"));
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
            var argumentException = Assert.ThrowsAsync<ArgumentException>(() => replayingHttpMessageHandler.SendAsync(request, CancellationToken.None));
            
            Assert.That(argumentException.ParamName, Is.Null);
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