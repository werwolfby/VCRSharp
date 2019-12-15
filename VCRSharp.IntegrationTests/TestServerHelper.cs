using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using VCRSharp.TestServer;

namespace VCRSharp.IntegrationTests
{
    public static class TestServerHelper
    {
        public static IHost BuildAndStartHost()
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

        public static IHost BuildAndStartHost(Cassette cassette) => 
            cassette.Records.Count == 0 ? TestServerHelper.BuildAndStartHost() : null;
    }
}