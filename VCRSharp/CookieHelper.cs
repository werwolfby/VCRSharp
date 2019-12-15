// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Origin: https://github.com/dotnet/runtime/blob/8425f3af5835f0cbdd5ba8e2c07dc4f6f3aa245b/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/CookieHelper.cs

using System.Linq;
using System.Net;
using System.Net.Http;

namespace VCRSharp
{
    internal static class CookieHelper
    {
        public static void ProcessReceivedCookies(HttpResponseMessage response, CookieContainer cookieContainer)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var valuesArray = values as string[] ?? values.ToArray();

                var requestUri = response.RequestMessage.RequestUri;
                foreach (var value in valuesArray)
                {
                    cookieContainer.SetCookies(requestUri, value);
                }
            }
        }
    }
}