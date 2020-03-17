using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;

namespace VCRSharp.IntegrationTests
{
    public class TestServerLoginTests : IWithVcr
    {
        private readonly Uri _baseAddress = new Uri("http://localhost:5000");

        public Cassette Cassette { get; set; }

        public Func<CookieContainer, HttpMessageHandler> HttpMessageHandlerFunc { get; set; }
        
        [Test]
        [UseVcr("cassette")]
        public async Task LoginAndInfo_AnyUserWithCookieContainer_Success()
        {
            using var _ = TestServerHelper.BuildAndStartHost(Cassette);
            
            var cookieContainer = new CookieContainer();

            using var httpClient = new HttpClient(HttpMessageHandlerFunc(cookieContainer))
            {
                BaseAddress = _baseAddress,
            };
            var query = QueryHelpers.AddQueryString("/api/users/login", new Dictionary<string, string>
            {
                {"username", "admin"},
                {"password", "password"},
            });
            var response = await httpClient.GetAsync(query);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            Assert.That(cookieContainer.GetCookies(_baseAddress)["value"]?.Value, Is.EqualTo("123"));

            response = await httpClient.GetAsync("/api/users/me/info");
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        
        [Test]
        [UseVcr("cassette")]
        public async Task LoginAndInfo_AnyUserWithoutCookies_UnauthorizedOnMeInfo()
        {
            using var _ = TestServerHelper.BuildAndStartHost(Cassette);

            var cookieContainer = new CookieContainer();
            using var httpClient = new HttpClient(HttpMessageHandlerFunc(cookieContainer))
            {
                BaseAddress = _baseAddress,
            };
            var query = QueryHelpers.AddQueryString("/api/users/login", new Dictionary<string, string>
            {
                {"username", "admin"},
                {"password", "password"},
            });
            var response = await httpClient.GetAsync(query);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var cookies = cookieContainer.GetCookies(_baseAddress);
            foreach (Cookie cookie in cookies)
            {
                cookie.Expired = true;
            }
            
            response = await httpClient.GetAsync("/api/users/me/info");
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}