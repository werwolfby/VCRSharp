﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace VCRSharp.IntegrationTests
{
    public class RequestApiTests
    {
        [Test]
        public async Task ReqResIn_GetUser_SuccessfullyStoredForFirstCallAndReplayedOnSecond()
        {
            const string uri = "https://reqres.in/api/users/2";
            var httpClientHandler = new HttpClientHandler();
            var innerHandler = new StubHttpRequestHandler(httpClientHandler);
            var cassette = new Cassette();
            var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

            using var httpClient = new HttpClient(tryReplayHandler);
            var response = await httpClient.GetAsync(uri);
            
            Assert.That(innerHandler.Invoked, Is.True);
            await AssertResponse(response);
            Assert.That(cassette.Records, Has.Count.EqualTo(1));
            innerHandler.Invoked = false;

            var date = response.Headers.Date;
            response = await httpClient.GetAsync(uri);
            await AssertResponse(response);
            Assert.That(innerHandler.Invoked, Is.False);
            Assert.That(response.Headers.Date, Is.EqualTo(date));

            static async Task AssertResponse(HttpResponseMessage response)
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.Date, Is.Not.Null.And.EqualTo(DateTimeOffset.Now).Within(Debugger.IsAttached ? 60 * 5 : 5).Seconds);
                var actualJson = JObject.Parse(await response.Content.ReadAsStringAsync());
                var expectedJson = JObject.Parse("{\"data\":{\"id\":2,\"email\":\"janet.weaver@reqres.in\",\"first_name\":\"Janet\",\"last_name\":\"Weaver\",\"avatar\":\"https://s3.amazonaws.com/uifaces/faces/twitter/josephstein/128.jpg\"}}");
                Assert.That(actualJson, Is.EqualTo(expectedJson));
            }
        }
    }
}