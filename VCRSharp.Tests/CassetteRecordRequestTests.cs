using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class CassetteRecordRequestTests
    {
        [Test]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void Equals_Null_False()
        {
            var cassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection());
            var differentCassetteRecordRequest = (CassetteRecordRequest) null;
            
            Assert.That(cassetteRecordRequest.Equals(differentCassetteRecordRequest), Is.False);
            Assert.That(cassetteRecordRequest.Equals((object)differentCassetteRecordRequest), Is.False);
            Assert.That(cassetteRecordRequest == differentCassetteRecordRequest, Is.False);
            Assert.That(cassetteRecordRequest != differentCassetteRecordRequest, Is.True);
        }
        
        [Test]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void Equals_String_False()
        {
            var cassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection());

            Assert.That(cassetteRecordRequest.Equals("String"), Is.False);
        }
        
        [Test]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void Equals_TheSameObject_True()
        {
            var cassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection());
            var differentCassetteRecordRequest = cassetteRecordRequest;
            
            Assert.That(cassetteRecordRequest.Equals(differentCassetteRecordRequest), Is.True);
            Assert.That(cassetteRecordRequest.Equals((object)differentCassetteRecordRequest), Is.True);
            Assert.That(cassetteRecordRequest == differentCassetteRecordRequest, Is.True);
            Assert.That(cassetteRecordRequest != differentCassetteRecordRequest, Is.False);
        }
        
        [Test]
        public void Equals_DifferentObjectWithSameValueAndSameBody_True()
        {
            var cassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), new StringCassetteBody("{}"));
            var differentCassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), new StringCassetteBody("{}"));
            
            Assert.That(cassetteRecordRequest.Equals(differentCassetteRecordRequest), Is.True);
            Assert.That(cassetteRecordRequest.Equals((object)differentCassetteRecordRequest), Is.True);
            Assert.That(cassetteRecordRequest == differentCassetteRecordRequest, Is.True);
            Assert.That(cassetteRecordRequest != differentCassetteRecordRequest, Is.False);
        }
        
        [Test]
        public void Equals_DifferentObjectWithDifferentValueAndBody([Values]bool method, [Values]bool uri, [Values]bool headers, [Values]bool body)
        {
            var cassetteRecordRequest = new CassetteRecordRequest(
                HttpMethod.Get.Method,
                new Uri("http://localhost:5000/api/users/1"),
                new NameValueCollection
                {
                    {"Some-Header", "value"},
                },
                new StringCassetteBody("{}"));
            var differentCassetteRecordRequest = new CassetteRecordRequest(
                method ? HttpMethod.Get.Method : HttpMethod.Post.Method,
                uri ? new Uri("http://localhost:5000/api/users/1") : new Uri("http://localhost:5000/api/users/2"),
                new NameValueCollection
                {
                    {"Some-Header", headers ? "value" : "other"},
                },
                new StringCassetteBody(body ? "{}" : "{\"a\": 1}"));

            var equal = method && uri && headers && body;
            
            Assert.That(cassetteRecordRequest.Equals(differentCassetteRecordRequest), Is.EqualTo(equal));
            Assert.That(cassetteRecordRequest.Equals((object)differentCassetteRecordRequest), Is.EqualTo(equal));
            Assert.That(cassetteRecordRequest == differentCassetteRecordRequest, Is.EqualTo(equal));
            Assert.That(cassetteRecordRequest != differentCassetteRecordRequest, Is.EqualTo(!equal));
        }
        
        [Test]
        public void Equals_DifferentObjectWithDifferentHeader_False([Values(1, 2, 3, 4)]int headers)
        {
            var cassetteRecordRequest = new CassetteRecordRequest(
                HttpMethod.Get.Method,
                new Uri("http://localhost:5000/api/users/1"),
                new NameValueCollection
                {
                    {"Some-Header", "value"},
                },
                new StringCassetteBody("{}"));
            var differentCassetteRecordRequest = new CassetteRecordRequest(
                HttpMethod.Get.Method,
                new Uri("http://localhost:5000/api/users/1"),
                new NameValueCollection(),
                new StringCassetteBody("{}"));

            switch (headers)
            {
                case 1: // Different Value
                    differentCassetteRecordRequest.Headers.Add("Some-Header", "other");
                    break;
                case 2: // Different Headers Count
                    differentCassetteRecordRequest.Headers.Add("Some-Header", "value");
                    differentCassetteRecordRequest.Headers.Add("Some-Other-Header", "other");
                    break;
                case 3: // Different Header Value Count
                    differentCassetteRecordRequest.Headers.Add("Some-Header", "value");
                    differentCassetteRecordRequest.Headers.Add("Some-Header", "other");
                    break;
                case 4: // Different Headers
                    differentCassetteRecordRequest.Headers.Add("Some-Other-Header", "other");
                    break;
            };
            
            Assert.That(cassetteRecordRequest.Equals(differentCassetteRecordRequest), Is.False);
            Assert.That(cassetteRecordRequest.Equals((object)differentCassetteRecordRequest), Is.False);
            Assert.That(cassetteRecordRequest == differentCassetteRecordRequest, Is.False);
            Assert.That(cassetteRecordRequest != differentCassetteRecordRequest, Is.True);
        }
        
        [Test]
        public void GetHashCode_DifferentObjectWithSameMethodAndUri_EqualHashCodes()
        {
            var cassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), new StringCassetteBody("{}"));
            var differentCassetteRecordRequest = new CassetteRecordRequest(HttpMethod.Get.Method, new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), new StringCassetteBody("{}"));
            
            Assert.That(differentCassetteRecordRequest.GetHashCode(), Is.EqualTo(cassetteRecordRequest.GetHashCode()));
        }

        [Test]
        public async Task CreateFromRequest_RequestWithoutBody()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:5000/api/users/1"));

            var record = await CassetteRecordRequest.CreateFromRequest(request, null);
            
            Assert.That(record.Method, Is.EqualTo(request.Method.Method));
            Assert.That(record.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(record.Body, Is.Null);
            
            // Host header should be added always
            Assert.That(record.Headers, Has.Count.EqualTo(1));
            Assert.That(record.Headers["Host"], Is.EqualTo(request.RequestUri.IdnHost));
        }

        [Test]
        public async Task CreateFromRequest_RequestWithStringBody()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:5000/api/users/1"))
            {
                Content = new StringContent("text", Encoding.UTF8, "text/plain")
            };

            var record = await CassetteRecordRequest.CreateFromRequest(request, null);
            
            Assert.That(record.Method, Is.EqualTo(request.Method.Method));
            Assert.That(record.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(record.Body, Is.EqualTo(new StringCassetteBody("text")));
            
            // Host header should be added always
            Assert.That(record.Headers, Has.Count.EqualTo(2));
            Assert.That(record.Headers["Host"], Is.EqualTo(request.RequestUri.IdnHost));
            Assert.That(record.Headers["Content-Type"], Contains.Substring("text/plain").And.Contains("utf-8"));
        }

        [Test]
        public async Task CreateFromRequest_RequestWithStreamBody()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:5000/api/users/1"))
            {
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("text")))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("text/plain")
                        {
                            CharSet = "utf-8"
                        }
                    }
                }
            };

            var record = await CassetteRecordRequest.CreateFromRequest(request, null);
            
            Assert.That(record.Method, Is.EqualTo(request.Method.Method));
            Assert.That(record.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(record.Body, Is.EqualTo(new BytesCassetteBody(Encoding.UTF8.GetBytes("text"))));
            
            // Host header should be added always
            Assert.That(record.Headers, Has.Count.EqualTo(2));
            Assert.That(record.Headers["Host"], Is.EqualTo(request.RequestUri.IdnHost));
            Assert.That(record.Headers["Content-Type"], Contains.Substring("text/plain").And.Contains("utf-8"));
        }

        [Test]
        public async Task CreateFromRequest_RequestWithoutBodyWithHostHeader_HeaderShouldNotBeAdded()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:5000/api/users/1"))
            {
                Headers =
                {
                    Host = "some-host"
                }
            };

            var record = await CassetteRecordRequest.CreateFromRequest(request, null);
            
            Assert.That(record.Method, Is.EqualTo(request.Method.Method));
            Assert.That(record.Uri, Is.EqualTo(request.RequestUri));
            Assert.That(record.Body, Is.Null);
            
            // Host header should be added always
            Assert.That(record.Headers, Has.Count.EqualTo(1));
            Assert.That(record.Headers["Host"], Is.EqualTo(request.Headers.Host));
        }

        [Test]
        public void ToRequestMessage_RecordWithoutBody()
        {
            var record = new CassetteRecordRequest("GET", new Uri("http://localhost:5000/api/users/1"), new NameValueCollection());

            var message = record.ToRequestMessage();
            
            Assert.That(message.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(message.RequestUri, Is.EqualTo(record.Uri));
            Assert.That(message.Headers.Count(), Is.EqualTo(0));
            Assert.That(message.Content, Is.Null);
        }

        [Test]
        public async Task ToRequestMessage_RecordWithBody()
        { 
            var record = new CassetteRecordRequest("GET", new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), "text");
            record.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var message = record.ToRequestMessage();
            
            Assert.That(message.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(message.RequestUri, Is.EqualTo(record.Uri));
            Assert.That(message.Headers.Count(), Is.EqualTo(0));
            Assert.That(message.Content, Is.TypeOf<StringContent>());
            Assert.That(await message.Content.ReadAsStringAsync(), Is.EqualTo("text"));
            Assert.That(message.Content.Headers.ContentType.MediaType, Is.EqualTo("text/plain"));
            Assert.That(message.Content.Headers.ContentType.CharSet, Is.EqualTo("utf-8"));
        }

        [Test]
        public void ToRequestMessage_RecordWithWrongHeader_ThrowsArgumentException()
        { 
            var record = new CassetteRecordRequest("GET", new Uri("http://localhost:5000/api/users/1"), new NameValueCollection(), "text");
            record.Headers.Add("Does-It-A-Valid-Header?", "No");

            Assert.Throws<ArgumentException>(() => record.ToRequestMessage());
        }
    }
}