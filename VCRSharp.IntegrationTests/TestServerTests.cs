using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using VCRSharp.TestServer;

namespace VCRSharp.IntegrationTests
{
    public class TestServerTests
    {
        private readonly Uri _baseAddress = new Uri("http://localhost:5000");

        [Test]
        public async Task GetUsersApi_InvokedOnFirstCallFromCassetteOnSecondCall_Success()
        {
            using var host = BuildAndStartHost();

            var httpClientHandler = new HttpClientHandler();
            var innerHandler = new StubHttpRequestHandler(httpClientHandler);
            var cassette = new Cassette();
            var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

            using var httpClient = new HttpClient(tryReplayHandler) {BaseAddress = _baseAddress};
            await httpClient.GetStringAsync("/api/users/1");
            
            Assert.That(innerHandler.Invoked, Is.True);
            Assert.That(cassette.Records, Has.Count.EqualTo(1));

            innerHandler.Invoked = false;
            
            var result =  await httpClient.GetStringAsync("/api/users/1");
            Assert.That(innerHandler.Invoked, Is.False);
            
            var actual = JObject.Parse(result);
            var expected = JObject.Parse("{\"id\": 1, \"name\": \"User 1\"}");
            
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetRedirectUsersApi_InvokedOnFirstCallFromCassetteOnSecondCall_Success()
        {
            using var host = BuildAndStartHost();

            var httpClientHandler = new HttpClientHandler();
            var innerHandler = new StubHttpRequestHandler(httpClientHandler);
            var cassette = new Cassette();
            var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

            using var httpClient = new HttpClient(tryReplayHandler) {BaseAddress = _baseAddress};
            var response = await httpClient.GetAsync("/api/get_users/2");
            
            Assert.That(innerHandler.Invoked, Is.True);
            Assert.That(cassette.Records, Has.Count.EqualTo(1));
            Assert.That(response.RequestMessage.RequestUri, Is.EqualTo(new Uri(_baseAddress, "/api/users/2")));

            innerHandler.Invoked = false;
            
            response =  await httpClient.GetAsync("/api/get_users/2");
            Assert.That(innerHandler.Invoked, Is.False);
            Assert.That(response.RequestMessage?.RequestUri, Is.EqualTo(new Uri(_baseAddress, "/api/users/2")));
            
            var actual = JObject.Parse(await response.Content.ReadAsStringAsync());
            var expected = JObject.Parse("{\"id\": 2, \"name\": \"User 2\"}");
            
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IHost BuildAndStartHost()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureWebHost(builder =>
                {
                    builder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureEndpointDefaults(listenOptions => { });
                    });
                })
                .Build();
            
            host.Start();

            return host;
        }
    }
}