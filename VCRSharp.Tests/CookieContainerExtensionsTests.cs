using System;
using System.Net;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class CookieContainerExtensionsTests
    {
        [Test]
        public void Clone_EmptyCookieContainer_ReturnsEmptyCookieContainer()
        {
            var cookieContainer = new CookieContainer();
            var clonedCookieContainer = cookieContainer.Clone(new Uri("http://localhost"));
            
            Assert.That(clonedCookieContainer, Has.Count.EqualTo(0));
        }
        
        [Test]
        public void Clone_SingleCookieForLocalHost_ReturnsCookieContainerWithSingleCookieForLocalHost()
        {
            var localhostUri = new Uri("http://localhost");

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(localhostUri, new Cookie("value", "123"));
            var clonedCookieContainer = cookieContainer.Clone(localhostUri);
            
            Assert.That(clonedCookieContainer, Has.Count.EqualTo(1));
            Assert.That(clonedCookieContainer.GetCookies(localhostUri)["value"]?.Value, Is.EqualTo("123"));
        }
        
        [Test]
        public void Clone_CookieForLocalHostAndGoogle_ReturnsCookieContainerWithSingleCookieForLocalHost()
        {
            var localhostUri = new Uri("http://localhost");
            var googleUri = new Uri("http://google.com");

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(localhostUri, new Cookie("value", "123"));
            cookieContainer.Add(googleUri, new Cookie("value", "321"));
            var clonedCookieContainer = cookieContainer.Clone(localhostUri);
            
            Assert.That(clonedCookieContainer, Has.Count.EqualTo(1));
            Assert.That(clonedCookieContainer.GetCookies(localhostUri)["value"]?.Value, Is.EqualTo("123"));
        }
        
        [Test]
        public void Clone_TwoCookieForLocalHostWithPath_ReturnsCookieContainerWithSingleCookieForLocalHost()
        {
            var localhostUri = new Uri("http://localhost");
            var localhostApiUsersUri = new Uri(localhostUri, "api/users");

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(localhostApiUsersUri, new Cookie("value", "123"));
            cookieContainer.Add(localhostUri, new Cookie("value", "321"));
            var clonedCookieContainer = cookieContainer.Clone(localhostApiUsersUri);
            
            Assert.That(clonedCookieContainer, Has.Count.EqualTo(2));
            Assert.That(clonedCookieContainer.GetCookies(localhostUri)["value"]?.Value, Is.EqualTo("321"));
            Assert.That(clonedCookieContainer.GetCookies(localhostApiUsersUri)["value"]?.Value, Is.EqualTo("123"));
        }
    }
}