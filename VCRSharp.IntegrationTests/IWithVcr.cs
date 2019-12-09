using System.Net.Http;

namespace VCRSharp.IntegrationTests
{
    public interface IWithVcr
    {
        Cassette Cassette { get; set; }
        
        HttpMessageHandler HttpMessageHandler { get; set; }
    }
}