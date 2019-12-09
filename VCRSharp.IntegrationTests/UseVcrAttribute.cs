using System.IO;
using System.Net.Http;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace VCRSharp.IntegrationTests
{
    public class UseVcrAttribute : NUnitAttribute, ITestAction
    {
        private readonly string _cassetteFolder;

        public UseVcrAttribute(string cassetteFolder)
        {
            _cassetteFolder = cassetteFolder;
        }

        public void BeforeTest(ITest test)
        {
            var (cassetteStorage, cassettePath) = CreateStorageAndPath(test.FullName);
            var cassette = cassetteStorage.LoadCassette(cassettePath);
            var httpMessageHandler = new TryReplayHttpMessageHandler(cassette, new SocketsHttpHandler());

            var withVcr = (IWithVcr) test.Parent.Fixture;

            withVcr.Cassette = cassette;
            withVcr.HttpMessageHandler = httpMessageHandler;
        }

        public void AfterTest(ITest test)
        {
            var (cassetteStorage, cassettePath) = CreateStorageAndPath(test.FullName);

            var withVcr = (IWithVcr) test.Parent.Fixture;
            
            cassetteStorage.SaveCassette(cassettePath, withVcr.Cassette);
        }

        public ActionTargets Targets { get; } = ActionTargets.Test;

        private (YamlCassetteStorage storage, string path) CreateStorageAndPath(string testName) 
            => (new YamlCassetteStorage(), Path.Combine(_cassetteFolder, testName + ".cassette"));
    }
}