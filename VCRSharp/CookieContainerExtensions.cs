using System;
using System.Net;

namespace VCRSharp
{
    public static class CookieContainerExtensions
    {
        public static CookieContainer Clone(this CookieContainer cookieContainer, Uri uri)
        {
            var cloneCookieContainer = new CookieContainer();
            
            var cloneCookieCollection = cookieContainer.GetCookies(uri);
            foreach (Cookie cookie in cloneCookieCollection)
            {
                cloneCookieContainer.Add(new Uri(uri, cookie.Path), cookie);
            }
            
            return cloneCookieContainer;
        }
    }
}