using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
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

    public class CassetteRecordRequest : IEquatable<CassetteRecordRequest>
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

        public HttpRequestMessage ToRequestMessage()
        {
            var request = new HttpRequestMessage(new HttpMethod(Method), Uri)
            {
                Content = Body?.CreateContent()
            };

            foreach (string? header in Headers)
            {
                var values = Headers.GetValues(header);
                if (!request.Headers.TryAddWithoutValidation(header, values) &&
                    request.Content?.Headers.TryAddWithoutValidation(header, values) != true)
                {
                    throw new ArgumentException($"Can't add {header} to request");
                }
            }

            return request;
        }

        public bool Equals(CassetteRecordRequest? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Method != other.Method || !Uri.Equals(other.Uri) || !Equals(Body, other.Body)) return false;

            if (Headers.Count != other.Headers.Count) return false;
            foreach (string? header in Headers)
            {
                var values = Headers.GetValues(header);
                var otherValues = other.Headers.GetValues(header);

                if (values == null || otherValues == null)
                {
                    return false;
                }

                if (!values.SequenceEqual(otherValues))
                {
                    return false;
                }
            }
            
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CassetteRecordRequest) obj);
        }

        public override int GetHashCode() => Method.GetHashCode() * 17 + Uri.GetHashCode();

        public static bool operator ==(CassetteRecordRequest? left, CassetteRecordRequest? right) => Equals(left, right);

        public static bool operator !=(CassetteRecordRequest? left, CassetteRecordRequest? right) => !Equals(left, right);

        public static async Task<CassetteRecordRequest> CreateFromRequest(HttpRequestMessage request)
        {
            var record = new CassetteRecordRequest(request.Method.Method, request.RequestUri,
                request.ToNameValueCollection());

            // Host header is required by HTTP 1.1 spec, so we should add it if it is not provided
            if (record.Headers["Host"] == null)
            {
                record.Headers.Add("Host", request.RequestUri.IdnHost);
            }

            var (body, newContent) = await CassetteBody.CreateCassetteBody(request.Content);
            record.Body = body;
            if (newContent != null)
            {
                request.Content = newContent;
            }

            return record;
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

        public CassetteRecordResponse(Version version, int statusCode, string statusMessage, NameValueCollection headers, CassetteBody? body = null, CassetteRecordRequest? request = null)
        {
            Version = version;
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            Headers = headers;
            Body = body;
            Request = request;
        }

        public Version Version { get; }
        
        public int StatusCode { get; }
        
        public string StatusMessage { get; }
        
        public NameValueCollection Headers { get; }

        public CassetteBody? Body { get; set; }
        
        public CassetteRecordRequest? Request { get; set; }

        public HttpResponseMessage ToResponseMessage()
        {
            var response = new HttpResponseMessage((HttpStatusCode)StatusCode)
            {
                RequestMessage = Request?.ToRequestMessage(),
                Content = Body?.CreateContent()
            };

            foreach (string? header in Headers)
            {
                var values = Headers.GetValues(header);
                if (!response.Headers.TryAddWithoutValidation(header, values) &&
                    response.Content?.Headers.TryAddWithoutValidation(header, values) != true)
                {
                    throw new ArgumentException($"Can't add {header} to response");
                }
            }

            return response;
        }

        public static async Task<CassetteRecordResponse> CreateFromResponse(HttpResponseMessage response, CassetteRecordRequest recordRequest)
        {
            var record = new CassetteRecordResponse(response.Version, (int) response.StatusCode, response.ReasonPhrase, response.ToNameValueCollection())
            {
                Request = await CassetteRecordRequest.CreateFromRequest(response.RequestMessage)
            };

            // In case of Redirect inner innerHandler can create a new request, that actually did redirect
            // External users can expect this RequestMessage for check redirecting URI for example
            // innerHandler can change request, so we should create a new CassetteRecordRequest
            // from response and compare it with the original value 
            if (record.Request == recordRequest)
            {
                record.Request = null;
            }

            var (body, newContent) = await CassetteBody.CreateCassetteBody(response.Content);
            record.Body = body;
            if (newContent != null)
            {
                response.Content = newContent;
            }

            return record;
        }
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
                {
                    var bytesCassetteBody = await BytesCassetteBody.FromContentAsync(c);
                    return (bytesCassetteBody, bytesCassetteBody.CreateContentWithHeaders(c.Headers));
                }
                case { Headers: var headers } c when IsTextContent(c):
                {
                    var stringContent = await c.ReadAsStringAsync();
                    var stringCassetteBody = new StringCassetteBody(stringContent);
                    return (stringCassetteBody, stringCassetteBody.CreateContentWithHeaders(headers));
                }
                case { Headers: var headers } c:
                {
                    var bytesContent = await c.ReadAsByteArrayAsync();
                    var bytesCassetteBody = new BytesCassetteBody(bytesContent);
                    return (bytesCassetteBody, bytesCassetteBody.CreateContentWithHeaders(headers));
                }
                case null:
                    return (null, null);
            }
        }

        private static bool IsTextContent(HttpContent httpContent)
        {
            if (httpContent.Headers.ContentType == null)
            {
                return false;
            }

            if (httpContent.Headers.ContentType.MediaType.StartsWith("plain/") ||
                httpContent.Headers.ContentType.MediaType.StartsWith("text/"))
            {
                return true;
            }

            if (httpContent.Headers.ContentType.MediaType == "application/json")
            {
                return true;
            }

            return false;
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