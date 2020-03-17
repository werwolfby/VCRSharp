using System;
using System.Net;
using System.Net.Http;

namespace VCRSharp.IntegrationTests
{
    public interface IWithVcr
    {
        Cassette Cassette { get; set; }
        
        Func<CookieContainer, HttpMessageHandler> HttpMessageHandlerFunc { get; set; }
    }
}