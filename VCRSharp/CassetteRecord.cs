using System;
using System.Collections.Specialized;
using System.Net.Http;

namespace VCRSharp
{
    public class CassetteRecord
    {
        public CassetteRecordRequest Request { get; set; }
        
        public CassetteRecordResponse Response { get; set; }
    }

    public class CassetteRecordRequest
    {
        public string Method { get; set; }
        
        public Uri Uri { get; set; }
        
        public NameValueCollection Headers { get; set; }
        
        public string Body { get; set; }

        public static CassetteRecordRequest NewFromRequest(HttpRequestMessage request)
        {
            return new CassetteRecordRequest
            {
                Method = request.Method.Method,
                Uri = request.RequestUri,
                Headers = request.Headers.ToNameValueCollection(),
            };
        }
    }

    public class CassetteRecordResponse
    {
        public Version Version { get; set; }
        
        public int StatusCode { get; set; }
        
        public string StatusMessage { get; set; }
        
        public NameValueCollection Headers { get; set; }
        
        public string Body { get; set; }

        public static CassetteRecordResponse NewFromResponse(HttpResponseMessage response)
        {
            return new CassetteRecordResponse
            {
                StatusCode = (int) response.StatusCode,
                StatusMessage = response.ReasonPhrase,
                Version = response.Version,
                Headers = response.Headers.ToNameValueCollection(),
            };
        }
    }
}