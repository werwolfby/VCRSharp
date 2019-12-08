using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        public CassetteRecordRequest(string method, Uri uri, NameValueCollection headers, string body) : this(method, uri, headers, new StringCassetteBody(body))
        {
        }

        public CassetteRecordRequest(string method, Uri uri, NameValueCollection headers, CassetteBody? body = null)
        {
            Method = method;
            Uri = uri;
            Headers = headers;
            Body = body;
        }

        public string Method { get; }
        
        public Uri Uri { get; }
        
        public NameValueCollection Headers { get; }
        
        public CassetteBody? Body { get; set; }

        public static CassetteRecordRequest NewFromRequest(HttpRequestMessage request)
        {
            return new CassetteRecordRequest(request.Method.Method, request.RequestUri,
                request.ToNameValueCollection());
        }
    }

    public class CassetteRecordResponse
    {
        public CassetteRecordResponse(Version version, int statusCode, string statusMessage, NameValueCollection headers, string body) : this(version, statusCode, statusMessage, headers, new StringCassetteBody(body))
        {
        }

        public CassetteRecordResponse(Version version, int statusCode, string statusMessage, NameValueCollection headers, byte[] body) : this(version, statusCode, statusMessage, headers, new BytesCassetteBody(body))
        {
        }

        public CassetteRecordResponse(Version version, int statusCode, string statusMessage, NameValueCollection headers, CassetteBody? body = null)
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
        
        public CassetteBody? Body { get; set; }

        public static CassetteRecordResponse NewFromResponse(HttpResponseMessage response)
            => new CassetteRecordResponse(response.Version, (int) response.StatusCode, response.ReasonPhrase, response.ToNameValueCollection());
    }

    public abstract class CassetteBody
    {
        public abstract HttpContent CreateContent();

        public HttpContent CreateContentWithHeaders(HttpContentHeaders contentHeaders)
        {
            var headers = contentHeaders.ToNameValueCollection();
            var newContent = CreateContent();

            foreach (string? header in headers)
            {
                var values = headers.GetValues(header);

                // This method will never return false, cause we get headers from HttpContentHeaders
                // I can't create any unit tests that will cause return false
                newContent.Headers.TryAddWithoutValidation(header, values);
            }
            
            return newContent;
        }
        
        public static async Task<(CassetteBody? cassetteBody, HttpContent? content)> CreateCassetteBody(HttpContent content)
        {
            switch (content)
            {
                case StringContent c:
                    return (await StringCassetteBody.FromContentAsync(c), null);
                case ByteArrayContent c:
                    return (await BytesCassetteBody.FromContentAsync(c), null);
                case StreamContent c:
                    var bytesCassetteBody = await BytesCassetteBody.FromContentAsync(c);
                    return (bytesCassetteBody, bytesCassetteBody.CreateContentWithHeaders(content.Headers));
                case null:
                    return (null, null);
                default:
                    throw new ArgumentException($"Unsupported HttpContent type: {content.GetType()}", nameof(content));
            }
        }
    }

    public class StringCassetteBody : CassetteBody, IEquatable<StringCassetteBody>
    {
        public StringCassetteBody(string value)
        {
            Value = value;
        }

        public string Value { get; }
        
        public override HttpContent CreateContent() => new StringContent(Value);

        public static async Task<StringCassetteBody> FromContentAsync(StringContent content) => new StringCassetteBody(await content.ReadAsStringAsync());

        public bool Equals(StringCassetteBody? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringCassetteBody) obj);
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(StringCassetteBody? left, StringCassetteBody? right) => Equals(left, right);

        public static bool operator !=(StringCassetteBody? left, StringCassetteBody? right) => !Equals(left, right);

        public override string ToString() => Value;
    }

    public class BytesCassetteBody : CassetteBody, IEquatable<BytesCassetteBody>
    {
        public BytesCassetteBody(byte[] value)
        {
            Value = value;
        }

        public byte[] Value { get; }

        public override HttpContent CreateContent() => new ByteArrayContent(Value);

        public static async Task<BytesCassetteBody> FromContentAsync(ByteArrayContent content) => new BytesCassetteBody(await content.ReadAsByteArrayAsync());

        public static async Task<BytesCassetteBody> FromContentAsync(StreamContent content) => new BytesCassetteBody(await content.ReadAsByteArrayAsync());

        public bool Equals(BytesCassetteBody? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BytesCassetteBody) obj);
        }

        public override int GetHashCode() => new BigInteger(Value).GetHashCode();

        public static bool operator ==(BytesCassetteBody? left, BytesCassetteBody? right) => Equals(left, right);

        public static bool operator !=(BytesCassetteBody? left, BytesCassetteBody? right) => !Equals(left, right);
    }
}