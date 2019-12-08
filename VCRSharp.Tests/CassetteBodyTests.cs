using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
        
        [Test]
        public void CreateCassetteBody_MockHttpContent_ThrowsException()
        {
            var content = new MultipartContent();

            var argumentException = Assert.ThrowsAsync<ArgumentException>(() => CassetteBody.CreateCassetteBody(content));
            
            Assert.That(argumentException.ParamName, Is.EqualTo(nameof(content)));
        }
    }
}