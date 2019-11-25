using System;
using System.Collections.Specialized;

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
    }

    public class CassetteRecordResponse
    {
        public Version Version { get; set; }
        
        public int StatusCode { get; set; }
        
        public string StatusMessage { get; set; }
        
        public NameValueCollection Headers { get; set; }
        
        public string Body { get; set; }
    }
}