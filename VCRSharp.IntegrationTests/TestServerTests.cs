using System;
using System.IO;
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

        [Test]
        public async Task Load_GetUser_SuccessReadFromFile()
        {
            const string cassettePath = "cassette/Load_GetUser_SuccessReadFromFile.yml";
            if (!File.Exists(cassettePath))
            {
                using var host = BuildAndStartHost();
                var httpClientHandler = new HttpClientHandler();
                var innerHandler = new StubHttpRequestHandler(httpClientHandler);
                var cassette = new Cassette();
                var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

                using var httpClient = new HttpClient(tryReplayHandler) {BaseAddress = _baseAddress};
                await httpClient.GetStringAsync("/api/users/1");
                
                var yamlStorage = new YamlCassetteStorage();
                yamlStorage.Save(cassettePath, cassette.Records);
            }

            {
                var cassette = YamlCassetteStorage.LoadCassette(cassettePath);
                
                var replayingHandler = new ReplayingHttpMessageHandler(cassette);
                
                using var httpClient = new HttpClient(replayingHandler) {BaseAddress = _baseAddress};
                var user = await httpClient.GetStringAsync("/api/users/1");

                var actual = JObject.Parse(user);
                var expected = JObject.Parse("{\"id\": 1, \"name\": \"User 1\"}");
            
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [Test]
        public async Task LoadMultiple_GetUser_SuccessReadFromFile()
        {
            const string cassettePath = "cassette/LoadMultiple_GetUser_SuccessReadFromFile.yml";
            if (!File.Exists(cassettePath))
            {
                using var host = BuildAndStartHost();
                var httpClientHandler = new HttpClientHandler();
                var innerHandler = new StubHttpRequestHandler(httpClientHandler);
                var cassette = new Cassette();
                var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

                using var httpClient = new HttpClient(tryReplayHandler) {BaseAddress = _baseAddress};
                await httpClient.GetStringAsync("/api/users/4");
                await httpClient.GetStringAsync("/api/users/5");
                
                var yamlStorage = new YamlCassetteStorage();
                yamlStorage.Save(cassettePath, cassette.Records);
            }

            {
                var cassette = YamlCassetteStorage.LoadCassette(cassettePath);
                
                var replayingHandler = new ReplayingHttpMessageHandler(cassette);
                
                using var httpClient = new HttpClient(replayingHandler) {BaseAddress = _baseAddress};
                var user4 = await httpClient.GetStringAsync("/api/users/4");
                var user5 = await httpClient.GetStringAsync("/api/users/5");

                var actual4 = JObject.Parse(user4);
                var expected4 = JObject.Parse("{\"id\": 4, \"name\": \"User 4\"}");
            
                Assert.That(actual4, Is.EqualTo(expected4));

                var actual5 = JObject.Parse(user5);
                var expected5 = JObject.Parse("{\"id\": 5, \"name\": \"User 5\"}");
            
                Assert.That(actual5, Is.EqualTo(expected5));
            }
        }

        [Test]
        public async Task Load_GetUserRedirect_SuccessReadFromFile()
        {
            const string cassettePath = "cassette/Load_GetUserRedirect_SuccessReadFromFile.yml";
            if (!File.Exists(cassettePath))
            {
                using var host = BuildAndStartHost();
                var httpClientHandler = new HttpClientHandler();
                var innerHandler = new StubHttpRequestHandler(httpClientHandler);
                var cassette = new Cassette();
                var tryReplayHandler = new TryReplayHttpMessageHandler(cassette, innerHandler);

                using var httpClient = new HttpClient(tryReplayHandler) {BaseAddress = _baseAddress};
                await httpClient.GetStringAsync("/api/get_users/3");
                
                var yamlStorage = new YamlCassetteStorage();
                yamlStorage.Save(cassettePath, cassette.Records);
            }

            {
                var cassette = YamlCassetteStorage.LoadCassette(cassettePath);
                
                var replayingHandler = new ReplayingHttpMessageHandler(cassette);
                
                using var httpClient = new HttpClient(replayingHandler) {BaseAddress = _baseAddress};
                var user = await httpClient.GetStringAsync("/api/get_users/3");

                var actual = JObject.Parse(user);
                var expected = JObject.Parse("{\"id\": 3, \"name\": \"User 3\"}");
            
                Assert.That(actual, Is.EqualTo(expected));
            }
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