using System;
using System.Collections.Specialized;
using System.Net.Http;

namespace VCRSharp
{
    public class CassetteRecord
    {
        public CassetteRecord(CassetteRecordRequest request, CassetteRecordResponse response)
        {
            Request = request;
            Response = response;
        }

        public CassetteRecordRequest Request { get; }
        
        public CassetteRecordResponse Response { get; }
    }

    public class CassetteRecordRequest
    {
        public CassetteRecordRequest(string method, Uri uri, NameValueCollection headers, string? body = null)
        {
            Method = method;
            Uri = uri;
            Headers = headers;
            Body = body;
        }

        public string Method { get; }
        
        public Uri Uri { get; }
        
        public NameValueCollection Headers { get; }
        
        public string? Body { get; set; }

        public static CassetteRecordRequest NewFromRequest(HttpRequestMessage request)
        {
            return new CassetteRecordRequest(request.Method.Method, request.RequestUri,
                request.Headers.ToNameValueCollection());
        }
    }

    public class CassetteRecordResponse
    {
        public CassetteRecordResponse(Version version, int statusCode, string statusMessage, NameValueCollection headers, string? body = null)
        {
            Version = version;
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            Headers = headers;
            Body = body;
        }

        public Version Version { get; }
        
        public int StatusCode { get; }
        
        public string StatusMessage { get; }
        
        public NameValueCollection Headers { get; }
        
        public string? Body { get; set; }

        public static CassetteRecordResponse NewFromResponse(HttpResponseMessage response)
            => new CassetteRecordResponse(response.Version, (int) response.StatusCode, response.ReasonPhrase, response.Headers.ToNameValueCollection());
    }
}