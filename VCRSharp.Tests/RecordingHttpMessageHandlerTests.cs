using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class RecordingHttpMessageHandlerTests
    {
        [Test]
        public async Task SendAsync_GetRequest_Success()
        {
            var cassette = new Cassette();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:8080"),
            };

            const string body = @"{""a"":1, ""b"": 2}";
            var handler = new PublicRecordingHttpMessageHandler(new MockHttpRequestHandler(new StringContent(body, Encoding.UTF8, "application/json")), cassette);
            await handler.SendAsync(request, CancellationToken.None);
            
            Assert.That(cassette.Records, Has.Count.EqualTo(1));
            Assert.That(cassette.Records[0].Request.Method, Is.EqualTo(request.Method.Method));
            Assert.That(cassette.Records[0].Request.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(cassette.Records[0].Request.Headers, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(cassette.Records[0].Request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(cassette.Records[0].Request.Body, Is.Null);
            
            Assert.That(cassette.Records[0].Response.Version, Is.EqualTo(new Version(1, 1)));
            Assert.That(cassette.Records[0].Response.StatusCode, Is.EqualTo(200));
            Assert.That(cassette.Records[0].Response.Headers, Has.Count.EqualTo(2));
            Assert.That(cassette.Records[0].Response.Headers["Server"], Is.EqualTo("Test-Server"));
            Assert.That(cassette.Records[0].Response.Headers["Content-Type"], Contains.Substring("application/json").And.Contains("charset=utf-8"));
            Assert.That(cassette.Records[0].Response.Body, Is.EqualTo(body));
        }
        
        [Test]
        public async Task SendAsync_PostRequest_Success()
        {
            var cassette = new Cassette();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:8080"),
                Content = new StringContent("{}"),
            };

            const string body = @"{""a"":1, ""b"": 2}";
            var handler = new PublicRecordingHttpMessageHandler(new MockHttpRequestHandler(new StringContent(body)), cassette);
            await handler.SendAsync(request, CancellationToken.None);
            
            Assert.That(cassette.Records, Has.Count.EqualTo(1));
            Assert.That(cassette.Records[0].Request.Method, Is.EqualTo(request.Method.Method));
            Assert.That(cassette.Records[0].Request.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(cassette.Records[0].Request.Headers, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(cassette.Records[0].Request.Headers["Content-Type"], Contains.Substring("text/plain").And.Contains("charset=utf-8"));
            Assert.That(cassette.Records[0].Request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(cassette.Records[0].Request.Body, Is.EqualTo("{}"));
            
            Assert.That(cassette.Records[0].Response.Version, Is.EqualTo(new Version(1, 1)));
            Assert.That(cassette.Records[0].Response.StatusCode, Is.EqualTo(200));
            Assert.That(cassette.Records[0].Response.Headers, Has.Count.EqualTo(2));
            Assert.That(cassette.Records[0].Response.Headers["Server"], Is.EqualTo("Test-Server"));
            Assert.That(cassette.Records[0].Response.Headers["Content-Type"], Contains.Substring("text/plain").And.Contains("charset=utf-8"));
            Assert.That(cassette.Records[0].Response.Body, Is.EqualTo(body));
        }
        
        [Test]
        public async Task SendAsync_PostJsonContentRequest_Success()
        {
            var cassette = new Cassette();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:8080"),
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };

            const string body = @"{""a"":1, ""b"": 2}";
            var handler = new PublicRecordingHttpMessageHandler(new MockHttpRequestHandler(new StringContent(body, Encoding.UTF8, "application/json")), cassette);
            await handler.SendAsync(request, CancellationToken.None);
            
            Assert.That(cassette.Records, Has.Count.EqualTo(1));
            Assert.That(cassette.Records[0].Request.Method, Is.EqualTo(request.Method.Method));
            Assert.That(cassette.Records[0].Request.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(cassette.Records[0].Request.Headers, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(cassette.Records[0].Request.Headers["Content-Type"], Contains.Substring("application/json").And.Contains("charset=utf-8"));
            Assert.That(cassette.Records[0].Request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(cassette.Records[0].Request.Body, Is.EqualTo("{}"));
            
            Assert.That(cassette.Records[0].Response.Version, Is.EqualTo(new Version(1, 1)));
            Assert.That(cassette.Records[0].Response.StatusCode, Is.EqualTo(200));
            Assert.That(cassette.Records[0].Response.Headers, Has.Count.EqualTo(2));
            Assert.That(cassette.Records[0].Response.Headers["Server"], Is.EqualTo("Test-Server"));
            Assert.That(cassette.Records[0].Response.Headers["Content-Type"], Contains.Substring("application/json").And.Contains("charset=utf-8"));
            Assert.That(cassette.Records[0].Response.Body, Is.EqualTo(body));
        }
        
        private class PublicRecordingHttpMessageHandler : RecordingHttpMessageHandler
        {
            public PublicRecordingHttpMessageHandler(HttpMessageHandler innerHandler, Cassette cassette) : base(innerHandler, cassette)
            {
            }
            
            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);
        }
        
        private class MockHttpRequestHandler : HttpMessageHandler
        {
            private readonly HttpContent _content;

            public MockHttpRequestHandler(HttpContent content)
            {
                _content = content;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await Task.Yield();
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
                    Content = _content,
                };
            }
        }
    }
}