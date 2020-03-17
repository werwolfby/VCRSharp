using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class CassetteBodyTests
    {
        [Test]
        public async Task CreateCassetteBody_StringContent_ReturnStringCassetteBody()
        {
            var content = new StringContent("{}");

            var (cassetteBody, newContent) = await CassetteBody.CreateCassetteBody(content);
            
            Assert.That(cassetteBody, Is.TypeOf<StringCassetteBody>());
            Assert.That(newContent, Is.Null);
        }
        
        [Test]
        public async Task CreateCassetteBody_BytesContent_ReturnBytesCassetteBody()
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes("{}"));

            var (cassetteBody, newContent) = await CassetteBody.CreateCassetteBody(content);
            
            Assert.That(cassetteBody, Is.TypeOf<BytesCassetteBody>());
            Assert.That(newContent, Is.Null);
        }
        
        [Test]
        public async Task CreateCassetteBody_StreamContent_ReturnBytesCassetteBodyAndNewContent()
        {
            var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
            content.Headers.TryAddWithoutValidation("Accept-Language", "sm-SM");

            var (cassetteBody, newContent) = await CassetteBody.CreateCassetteBody(content);
            
            Assert.That(cassetteBody, Is.TypeOf<BytesCassetteBody>());
            Assert.That(newContent, Is.TypeOf<ByteArrayContent>());
        }
        
        [TestCase("text message", "text/plain", "utf-8", typeof(StringCassetteBody), typeof(StringContent))]
        [TestCase("немного русского", "text/plain", "utf-8", typeof(StringCassetteBody), typeof(StringContent))]
        [TestCase("{\"some\": \"json\"}", "application/json", "utf-8", typeof(StringCassetteBody), typeof(StringContent))]
        [TestCase("c29tZSB0ZXh0", "application/octet+stream", null, typeof(BytesCassetteBody), typeof(ByteArrayContent))]
        [TestCase("text message", null, null, typeof(BytesCassetteBody), typeof(ByteArrayContent))]
        public async Task CreateCassetteBody_CustomHttpContent_UseBaseTypes(string message, string mediaType, string charSet, Type cassetteBodyType, Type newContentType)
        {
            var mediaTypeHeaderValue =
                mediaType != null
                    ? new MediaTypeHeaderValue(mediaType)
                    {
                        CharSet = charSet
                    }
                    : null;
            var content = new CustomHttpContent(message)
            {
                Headers =
                {
                    ContentType = mediaTypeHeaderValue
                }
            };

            var (cassetteBody, newContent) = await CassetteBody.CreateCassetteBody(content);

            Assert.That(cassetteBody, Is.TypeOf(cassetteBodyType));
            Assert.That(newContent, Is.TypeOf(newContentType));
        }
        
        private class CustomHttpContent : HttpContent
        {
            private readonly string _message;

            public CustomHttpContent(string message) => _message = message;

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                var charset = Headers.ContentType?.CharSet ?? "ascii";
                var encoding = Encoding.GetEncoding(charset);
                await using var streamWriter = new StreamWriter(stream, encoding, leaveOpen: true);
                await streamWriter.WriteAsync(_message);
            }

            protected override bool TryComputeLength(out long length)
            {
                var charset = Headers.ContentType?.CharSet ?? "ascii";
                var encoding = Encoding.GetEncoding(charset);
                length = encoding.GetEncoder().GetByteCount((ReadOnlySpan<char>) _message, true);
                return true;
            }
        }
    }
}