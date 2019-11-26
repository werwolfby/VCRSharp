using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class CassetteRecordRequestMethodUriEqualityComparerTests
    {
        [Test]
        public void Equal_SameMethodAndUri_Equal()
        {
            var record1 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header1", "Foo"},
                },
            };
            var record2 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header2", "Bar"},
                },
            };

            var comparer = new CassetteRecordRequestMethodUriEqualityComparer();
            Assert.That(comparer.Equals(record1, record2));
        }
        
        [Test]
        public void GetHashCode_SameMethodAndUri_Equal()
        {
            var record1 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header1", "Foo"},
                },
            };
            var record2 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header2", "Bar"},
                },
            };

            var comparer = new CassetteRecordRequestMethodUriEqualityComparer();
            Assert.That(comparer.GetHashCode(record1), Is.EqualTo(comparer.GetHashCode(record2)));
        }
        
        [Test]
        public void UseInHashSet_True_Success()
        {
            var record1 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header1", "Foo"},
                },
            };
            var record2 = new CassetteRecordRequest
            {
                Method = HttpMethod.Get.Method,
                Uri = new Uri("http://localhost:8080/test"),
                Headers = new NameValueCollection
                {
                    {"Header2", "Bar"},
                },
            };

            var comparer = new CassetteRecordRequestMethodUriEqualityComparer();
            var hashSet = new HashSet<CassetteRecordRequest>(comparer);
            
            Assert.That(hashSet.Add(record1), Is.True);
            Assert.That(hashSet.Add(record2), Is.False);
        }
    }
}