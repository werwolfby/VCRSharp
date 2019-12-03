using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace VCRSharp
{
    public static class Extensions
    {
        public static NameValueCollection ToNameValueCollection(this HttpHeaders headers)
        {
            var result = new NameValueCollection();
            headers.AddHeadersToNameValueCollection(result);
            return result;
        }

        public static NameValueCollection ToNameValueCollection(this HttpRequestMessage request)
        {
            var result = new NameValueCollection();
            request.Headers.AddHeadersToNameValueCollection(result);
            request.Content?.Headers.AddHeadersToNameValueCollection(result);
            return result;
        }

        public static NameValueCollection ToNameValueCollection(this HttpResponseMessage response)
        {
            var result = new NameValueCollection();
            response.Headers.AddHeadersToNameValueCollection(result);
            response.Content?.Headers.AddHeadersToNameValueCollection(result);
            return result;
        }

        public static void AddHeadersToNameValueCollection(this HttpHeaders headers, NameValueCollection result)
        {
            foreach (var (key, values) in headers)
            {
                foreach (var value in values)
                {
                    result.Add(key, value);
                }
            }
        }
    }
}