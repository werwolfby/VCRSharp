using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class CassetteStorageExtensionsTests
    {
        [Test]
        public void Load_StringOverloadExistingFile_Success()
        {
            const string yaml =
                "Request:\n" +
                "  Method: GET\n" +
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
            
            const string path = "test.cassette";
            File.WriteAllText(path, yaml, Encoding.UTF8);

            try
            {
                var cassetteStorageMock = new Mock<ICassetteStorage>();
                cassetteStorageMock.Setup(m => m.Load(It.IsAny<TextReader>())).Returns(Array.Empty<CassetteRecord>());

                var records = cassetteStorageMock.Object.Load(path);
                
                Assert.That(records, Has.Length.EqualTo(0));
                cassetteStorageMock.Verify(m => m.Load(It.IsAny<TextReader>()), Times.Once);
            }
            finally
            {
                File.Delete(path);
            }
        }
        
        [Test]
        public void Load_StringOverloadNotExistingFile_ReturnsEmptyWithoutDelegatingCall()
        {
            const string path = "test.cassette";

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            cassetteStorageMock.Setup(m => m.Load(It.IsAny<TextReader>())).Returns(Array.Empty<CassetteRecord>());

            var records = cassetteStorageMock.Object.Load(path);
                
            Assert.That(records, Has.Length.EqualTo(0));
            cassetteStorageMock.Verify(m => m.Load(It.IsAny<TextReader>()), Times.Never);
        }
        
        [Test]
        public void Save_StringOverload_Success()
        {
            const string path = "test.cassette";

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            IEnumerable<CassetteRecord> records = null;
            cassetteStorageMock
                .Setup(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()))
                .Callback((TextWriter _, IEnumerable<CassetteRecord> r) => records = r);

            try
            {
                cassetteStorageMock.Object.Save(path, new CassetteRecord[1]);

                Assert.That(records, Has.Length.EqualTo(1).And.All.Null);
                cassetteStorageMock.Verify(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()), Times.Once);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        
        [Test]
        public void Save_StringOverloadInDirectory_Success()
        {
            const string folder = "test";
            const string name = "test.cassette";
            var path = Path.Combine(folder, name);

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            IEnumerable<CassetteRecord> records = null;
            cassetteStorageMock
                .Setup(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()))
                .Callback((TextWriter _, IEnumerable<CassetteRecord> r) => records = r);

            try
            {
                cassetteStorageMock.Object.Save(path, new CassetteRecord[1]);

                Assert.That(records, Has.Length.EqualTo(1).And.All.Null);
                cassetteStorageMock.Verify(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()), Times.Once);
                
                Assert.That(Directory.Exists(folder), Is.True);
            }
            finally
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }
        
        [Test]
        public void SaveCassette_StringOverload_Success()
        {
            const string path = "test.cassette";

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            IEnumerable<CassetteRecord> records = null;
            cassetteStorageMock
                .Setup(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()))
                .Callback((TextWriter _, IEnumerable<CassetteRecord> r) => records = r);

            try
            {
                cassetteStorageMock.Object.SaveCassette(path, new Cassette(new CassetteRecord[1]));

                Assert.That(records, Has.Count.EqualTo(1).And.All.Null);
                cassetteStorageMock.Verify(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()), Times.Once);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        
        [Test]
        public void SaveCassette_StringOverloadInDirectory_Success()
        {
            const string folder = "test";
            const string name = "test.cassette";
            var path = Path.Combine(folder, name);

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            IEnumerable<CassetteRecord> records = null;
            cassetteStorageMock
                .Setup(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()))
                .Callback((TextWriter _, IEnumerable<CassetteRecord> r) => records = r);

            try
            {
                cassetteStorageMock.Object.SaveCassette(path, new Cassette(new CassetteRecord[1]));

                Assert.That(records, Has.Count.EqualTo(1).And.All.Null);
                cassetteStorageMock.Verify(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()), Times.Once);
                
                Assert.That(Directory.Exists(folder), Is.True);
            }
            finally
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }
        
        [Test]
        public void SaveCassette_TextWriter_Success()
        {
            var cassetteStorageMock = new Mock<ICassetteStorage>();
            IEnumerable<CassetteRecord> records = null;
            cassetteStorageMock
                .Setup(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()))
                .Callback((TextWriter _, IEnumerable<CassetteRecord> r) => records = r);

            cassetteStorageMock.Object.SaveCassette(new StreamWriter(new MemoryStream()), new Cassette(new CassetteRecord[1]));

            Assert.That(records, Has.Count.EqualTo(1).And.All.Null);
            cassetteStorageMock.Verify(m => m.Save(It.IsAny<TextWriter>(), It.IsAny<IEnumerable<CassetteRecord>>()), Times.Once);
        }
        
        [Test]
        public void LoadCassette_StringOverloadNotExistingFile_ReturnsEmptyWithoutDelegatingCall()
        {
            const string path = "test.cassette";

            var cassetteStorageMock = new Mock<ICassetteStorage>();
            cassetteStorageMock.Setup(m => m.Load(It.IsAny<TextReader>())).Returns(Array.Empty<CassetteRecord>());

            var cassette = cassetteStorageMock.Object.LoadCassette(path);
                
            Assert.That(cassette, Is.Not.Null.And.Property(nameof(Cassette.Records)).Count.EqualTo(0));
            cassetteStorageMock.Verify(m => m.Load(It.IsAny<TextReader>()), Times.Never);
        }
        
        [Test]
        public void LoadCassette_TextReader_ReturnsEmptyWithoutDelegatingCall()
        {
            var cassetteStorageMock = new Mock<ICassetteStorage>();
            cassetteStorageMock.Setup(m => m.Load(It.IsAny<TextReader>())).Returns(Array.Empty<CassetteRecord>());

            var cassette = cassetteStorageMock.Object.LoadCassette(new StreamReader(new MemoryStream()));
                
            Assert.That(cassette, Is.Not.Null.And.Property(nameof(Cassette.Records)).Count.EqualTo(0));
            cassetteStorageMock.Verify(m => m.Load(It.IsAny<TextReader>()), Times.Once);
        }
    }
}