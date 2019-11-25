using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace VCRSharp
{
    public static class Extensions
    {
        public static NameValueCollection ToNameValueCollection(this HttpHeaders headers)
        {
            var result = new NameValueCollection();
            foreach (var (key, values) in headers)
            {
                foreach (var value in values)
                {
                    result.Add(key, value);
                }
            }

            return result;
        }
    }
}