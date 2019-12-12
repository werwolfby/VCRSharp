using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using YamlDotNet.Core.Tokens;

namespace VCRSharp.Tests
{
    public class CassetteTests
    {
        [Test]
        public void Ctor_Empty_NoRecords()
        {
            var cassette = new Cassette();
            
            Assert.That(cassette.Records, Is.Empty);
        }

        [Test]
        public void Ctor_Records_NoRecords([Values(1, 5)]int count)
        {
            var records = Enumerable
                .Range(0, count)
                .Select(i => new CassetteRecord(
                    new CassetteRecordRequest("GET", new Uri("http://localhost:8080/test"), new NameValueCollection()),
                    new CassetteRecordResponse(new Version(1, 1), 204, "No Content", new NameValueCollection())));
            
            var cassette = new Cassette(records);
            
            Assert.That(cassette.Records, Has.Count.EqualTo(count));
        }

        [Test]
        public void Find_MethodEqualityComparerWithRecords_NoRecords()
        {
            var records = new[]
            {
                new CassetteRecord(
                    new CassetteRecordRequest("GET", new Uri("http://localhost:8080/test"), new NameValueCollection()),
                    new CassetteRecordResponse(new Version(1, 1), 204, "No Content", new NameValueCollection())),
                new CassetteRecord(
                    new CassetteRecordRequest("POST", new Uri("http://localhost:8080/test"), new NameValueCollection()),
                    new CassetteRecordResponse(new Version(1, 1), 204, "No Content", new NameValueCollection()))
            };

            var cassette = new Cassette(records, new MethodEqualityComparer());
            
            Assert.That(cassette.Records, Has.Count.EqualTo(2));
            Assert.That(cassette.Find(new CassetteRecordRequest("GET", new Uri("http://localhost:8080/test123"), new NameValueCollection())), Is.EqualTo(records[0]));
            Assert.That(cassette.Find(new CassetteRecordRequest("POST", new Uri("http://localhost:8080/test123"), new NameValueCollection())), Is.EqualTo(records[1]));
        }

        [Test]
        public void AddAndFind_MethodEqualityComparerWithAddedRecords_NoRecords()
        {
            var record0 = new CassetteRecord(
                new CassetteRecordRequest("GET", new Uri("http://localhost:8080/test"), new NameValueCollection()),
                new CassetteRecordResponse(new Version(1, 1), 204, "No Content", new NameValueCollection()));
            var record1 = new CassetteRecord(
                new CassetteRecordRequest("POST", new Uri("http://localhost:8080/test"), new NameValueCollection()),
                new CassetteRecordResponse(new Version(1, 1), 204, "No Content", new NameValueCollection()));

            var cassette = new Cassette(new MethodEqualityComparer());
            cassette.Add(record0);
            cassette.Add(record1);

            Assert.That(cassette.Records, Has.Count.EqualTo(2));
            Assert.That(cassette.Find(new CassetteRecordRequest("GET", new Uri("http://localhost:8080/test123"), new NameValueCollection())), Is.EqualTo(record0));
            Assert.That(cassette.Find(new CassetteRecordRequest("POST", new Uri("http://localhost:8080/test123"), new NameValueCollection())), Is.EqualTo(record1));
        }
        
        private class MethodEqualityComparer : IEqualityComparer<CassetteRecordRequest>
        {
            public bool Equals(CassetteRecordRequest x, CassetteRecordRequest y) => x?.Method == y?.Method;

            public int GetHashCode(CassetteRecordRequest obj) => obj.Method.GetHashCode();
        }
    }
}