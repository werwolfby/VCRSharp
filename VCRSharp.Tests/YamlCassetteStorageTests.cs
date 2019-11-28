using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using NUnit.Framework;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using Version=System.Version;

namespace VCRSharp.Tests
{
    public class YamlCassetteStorageTests
    {
        [Test]
        public void Save_SingleRecord_Success()
        {
            const string path = "cassette/WriteTest1.yml";
            var record = new CassetteRecord(
                new CassetteRecordRequest(
                    HttpMethod.Get.Method,
                    new Uri("http://localhost:8080/test"),
                    new NameValueCollection
                    {
                        {"Content-Type", "text"},
                        {"Cookie", "value=1"},
                        {"Cookie", "value=2"},
                    }),
                new CassetteRecordResponse(
                    new Version(1, 1), 
                    200,
                    "OK",
                    new NameValueCollection
                    {
                        {"Content-Type", "application/json"}
                    },
                     @"{""a"": 1, ""b"": 2}"));
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            yamlCassetteStorage.Save(path, new[] {record});

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(File.ReadAllText(path)));
            
            Assert.That(yamlStream.Documents, Has.Count.EqualTo(1));
            
            var document = yamlStream.Documents[0];
            Assert.That(document.RootNode, Is.TypeOf<YamlMappingNode>());
            var node = (YamlMappingNode) document.RootNode;
            
            Assert.That(node.Children, Has.Count.EqualTo(2));
            Assert.That(node.Children[new YamlScalarNode("Request")], Is.TypeOf<YamlMappingNode>());

            var requestNode = (YamlMappingNode)node.Children[new YamlScalarNode("Request")];
            Assert.That(requestNode.Children[new YamlScalarNode("Method")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Request.Method));
            Assert.That(requestNode.Children[new YamlScalarNode("Uri")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Request.Uri.ToString()));
            
            Assert.That(requestNode.Children[new YamlScalarNode("Headers")], Is.TypeOf<YamlSequenceNode>());
            var requestHeaders = (YamlSequenceNode) requestNode.Children[new YamlScalarNode("Headers")];
            Assert.That(requestHeaders.Children, Has.Count.EqualTo(2));
            Assert.That(requestHeaders[0], Is.TypeOf<YamlMappingNode>());
            Assert.That(((YamlMappingNode)requestHeaders[0]).Children[new YamlScalarNode("Content-Type")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("text"));
            Assert.That(requestHeaders[1], Is.TypeOf<YamlMappingNode>());
            Assert.That(((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")], Is.TypeOf<YamlSequenceNode>());
            Assert.That(((YamlSequenceNode)((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")])[0], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("value=1"));
            Assert.That(((YamlSequenceNode)((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")])[1], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("value=2"));

            Assert.That(node.Children[new YamlScalarNode("Response")], Is.TypeOf<YamlMappingNode>());

            var responseNode = (YamlMappingNode) node.Children[new YamlScalarNode("Response")];
            Assert.That(responseNode.Children[new YamlScalarNode("Version")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.Version.ToString()));
            Assert.That(responseNode.Children[new YamlScalarNode("StatusCode")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.StatusCode.ToString()));
            Assert.That(responseNode.Children[new YamlScalarNode("StatusMessage")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.StatusMessage));
            Assert.That(responseNode.Children[new YamlScalarNode("Body")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.Body));

            var responseHeaders = (YamlSequenceNode) responseNode.Children[new YamlScalarNode("Headers")];
            Assert.That(responseHeaders.Children, Has.Count.EqualTo(1));
            Assert.That(responseHeaders[0], Is.TypeOf<YamlMappingNode>());
            Assert.That(((YamlMappingNode)responseHeaders[0]).Children[new YamlScalarNode("Content-Type")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("application/json"));
        }
        
        [Test]
        public void Save_TwoRecord_Success()
        {
            var path = "cassette/WriteTest2.yml";
            var record = new CassetteRecord(
                new CassetteRecordRequest(
                    HttpMethod.Get.Method,
                    new Uri("http://localhost:8080/test"),
                    new NameValueCollection
                    {
                        {"Content-Type", "text"},
                        {"Cookie", "value=1"},
                        {"Cookie", "value=2"},
                    }),
                new CassetteRecordResponse(
                    new Version(1, 1),
                    200,
                    "OK",
                    new NameValueCollection
                    {
                        {"Content-Type", "application/json"}
                    },
                     @"{""a"": 1, ""b"": 2}"));
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            yamlCassetteStorage.Save(path, new[] {record, record});

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(File.ReadAllText(path)));
            
            Assert.That(yamlStream.Documents, Has.Count.EqualTo(2));

            AssertDocument(yamlStream.Documents[0], record);
            AssertDocument(yamlStream.Documents[1], record);
            
            static void AssertDocument(YamlDocument document, CassetteRecord record)
            {
                Assert.That(document.RootNode, Is.TypeOf<YamlMappingNode>());
                var node = (YamlMappingNode) document.RootNode;
                
                Assert.That(node.Children, Has.Count.EqualTo(2));
                Assert.That(node.Children[new YamlScalarNode("Request")], Is.TypeOf<YamlMappingNode>());

                var requestNode = (YamlMappingNode)node.Children[new YamlScalarNode("Request")];
                Assert.That(requestNode.Children[new YamlScalarNode("Method")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Request.Method));
                Assert.That(requestNode.Children[new YamlScalarNode("Uri")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Request.Uri.ToString()));
                
                Assert.That(requestNode.Children[new YamlScalarNode("Headers")], Is.TypeOf<YamlSequenceNode>());
                var requestHeaders = (YamlSequenceNode) requestNode.Children[new YamlScalarNode("Headers")];
                Assert.That(requestHeaders.Children, Has.Count.EqualTo(2));
                Assert.That(requestHeaders[0], Is.TypeOf<YamlMappingNode>());
                Assert.That(((YamlMappingNode)requestHeaders[0]).Children[new YamlScalarNode("Content-Type")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("text"));
                Assert.That(requestHeaders[1], Is.TypeOf<YamlMappingNode>());
                Assert.That(((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")], Is.TypeOf<YamlSequenceNode>());
                Assert.That(((YamlSequenceNode)((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")])[0], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("value=1"));
                Assert.That(((YamlSequenceNode)((YamlMappingNode)requestHeaders[1]).Children[new YamlScalarNode("Cookie")])[1], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("value=2"));

                Assert.That(node.Children[new YamlScalarNode("Response")], Is.TypeOf<YamlMappingNode>());

                var responseNode = (YamlMappingNode) node.Children[new YamlScalarNode("Response")];
                Assert.That(responseNode.Children[new YamlScalarNode("Version")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.Version.ToString()));
                Assert.That(responseNode.Children[new YamlScalarNode("StatusCode")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.StatusCode.ToString()));
                Assert.That(responseNode.Children[new YamlScalarNode("StatusMessage")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.StatusMessage));
                Assert.That(responseNode.Children[new YamlScalarNode("Body")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo(record.Response.Body));

                var responseHeaders = (YamlSequenceNode) responseNode.Children[new YamlScalarNode("Headers")];
                Assert.That(responseHeaders.Children, Has.Count.EqualTo(1));
                Assert.That(responseHeaders[0], Is.TypeOf<YamlMappingNode>());
                Assert.That(((YamlMappingNode)responseHeaders[0]).Children[new YamlScalarNode("Content-Type")], Is.TypeOf<YamlScalarNode>().And.Property(nameof(YamlScalarNode.Value)).EqualTo("application/json"));
            }
        }

        [Test]
        public void Load_SingleRecord_Success()
        {
            ICassetteStorage cassette = new YamlCassetteStorage();
            var records = cassette.Load("cassette/Test1.yml");
            
            Assert.That(records, Has.Count.EqualTo(1));
            
            var record = records[0];
            Assert.That(record.Request.Method, Is.EqualTo(HttpMethod.Get.Method));
            Assert.That(record.Request.Uri, Is.EqualTo(new Uri("http://localhost:8080/test")));
            Assert.That(record.Request.Body, Is.Null);
            Assert.That(record.Request.Headers, Has.Count.EqualTo(2));
            Assert.That(record.Request.Headers.GetKey(0), Is.EqualTo("Content-Type"));
            Assert.That(record.Request.Headers.GetValues(0), Is.EqualTo(new[] {"text"}));
            Assert.That(record.Request.Headers.GetKey(1), Is.EqualTo("Cookie"));
            Assert.That(record.Request.Headers.GetValues(1), Is.EqualTo(new[] {"value=1", "value=2"}));
            
            Assert.That(record.Response.Version, Is.EqualTo(new Version(1, 1)));
            Assert.That(record.Response.StatusCode, Is.EqualTo(200));
            Assert.That(record.Response.StatusMessage, Is.EqualTo("OK"));
            Assert.That(record.Response.Body, Is.EqualTo(@"{""a"": 1, ""b"": 2}"));
            Assert.That(record.Response.Headers, Has.Count.EqualTo(1));
            Assert.That(record.Response.Headers["Content-Type"], Is.EqualTo("application/json"));
        }

        [Test]
        public void Load_TwoRecord_Success()
        {
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            var records = yamlCassetteStorage.Load("cassette/Test2.yml");
            
            Assert.That(records, Has.Count.EqualTo(2));

            AssetRecord(records[0]);
            AssetRecord(records[1]);

            static void AssetRecord(CassetteRecord record)
            {
                Assert.That(record.Request.Method, Is.EqualTo(HttpMethod.Get.Method));
                Assert.That(record.Request.Uri, Is.EqualTo(new Uri("http://localhost:8080/test")));
                Assert.That(record.Request.Body, Is.Null);
                Assert.That(record.Request.Headers, Has.Count.EqualTo(2));
                Assert.That(record.Request.Headers.GetKey(0), Is.EqualTo("Content-Type"));
                Assert.That(record.Request.Headers.GetValues(0), Is.EqualTo(new[] {"text"}));
                Assert.That(record.Request.Headers.GetKey(1), Is.EqualTo("Cookie"));
                Assert.That(record.Request.Headers.GetValues(1), Is.EqualTo(new[] {"value=1", "value=2"}));

                Assert.That(record.Response.Version, Is.EqualTo(new Version(1, 1)));
                Assert.That(record.Response.StatusCode, Is.EqualTo(200));
                Assert.That(record.Response.StatusMessage, Is.EqualTo("OK"));
                Assert.That(record.Response.Body, Is.EqualTo(@"{""a"": 1, ""b"": 2}"));
                Assert.That(record.Response.Headers, Has.Count.EqualTo(1));
                Assert.That(record.Response.Headers["Content-Type"], Is.EqualTo("application/json"));
            }
        }

        [Test]
        public void Load_WrongHeadersFormat_ThrowsException()
        {
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            Assert.Throws<YamlException>(() => yamlCassetteStorage.Load("cassette/Test3.yml"));
        }

        [Test]
        public void Load_NotExistingAttribute_ThrowsException()
        {
            const string yaml =
                "Request:\n" +
                "  Method: GET\n" +
                "  Uri: http://localhost:8080/test\n" +
                "  WrongParameter: 123\n" +
                "  Headers:\n" +
                "  - Content-Type: text\n" +
                "  - Cookie: [value=1, value=2]\n" +
                "Response:\n" +
                "  Version: 1.1\n" +
                "  StatusCode: 200\n" +
                "  StatusMessage: OK\n" +
                "  Headers:\n" +
                "  - Content-Type: application/json\n" +
                "  Body: '{\"a\": 1, \"b\": 2}'";
            
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            var yamlException = Assert.Throws<YamlException>(() => yamlCassetteStorage.Load(new StringReader(yaml)));
            Assert.That(yamlException.Message, Contains.Substring("WrongParameter"));
        }

        [Test]
        public void Load_MissRequiredMethodAttribute_ThrowsException()
        {
            const string yaml =
                "Request:\n" +
                "  Method: GET\n" +
                "  Headers:\n" +
                "  - Content-Type: text\n" +
                "  - Cookie: [value=1, value=2]\n" +
                "Response:\n" +
                "  Version: 1.1\n" +
                "  StatusCode: 200\n" +
                "  StatusMessage: OK\n" +
                "  Headers:\n" +
                "  - Content-Type: application/json\n" +
                "  Body: '{\"a\": 1, \"b\": 2}'";
            
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            var yamlException = Assert.Throws<YamlException>(() => yamlCassetteStorage.Load(new StringReader(yaml)));
            Assert.That(yamlException.Message, Contains.Substring("uri"));
        }

        [Test]
        public void Load_UriAttributeSpecifiedTwice_ThrowsException()
        {
            const string yaml =
                "Request:\n" +
                "  Method: GET\n" +
                "  Uri: http://localhost:8080/test\n" +
                "  Uri: http://localhost:8080/test\n" +
                "  Headers:\n" +
                "  - Content-Type: text\n" +
                "  - Cookie: [value=1, value=2]\n" +
                "Response:\n" +
                "  Version: 1.1\n" +
                "  StatusCode: 200\n" +
                "  StatusMessage: OK\n" +
                "  Headers:\n" +
                "  - Content-Type: application/json\n" +
                "  Body: '{\"a\": 1, \"b\": 2}'";
            
            ICassetteStorage yamlCassetteStorage = new YamlCassetteStorage();
            var yamlException = Assert.Throws<YamlException>(() => yamlCassetteStorage.Load(new StringReader(yaml)));
            Assert.That(yamlException.Message, Contains.Substring("Uri"));
        }
    }
}