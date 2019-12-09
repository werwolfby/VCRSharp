using System;
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

            BeforeTest(test, cassetteStorage, cassettePath);
        }

        public void AfterTest(ITest test)
        {
            var (cassetteStorage, cassettePath) = CreateStorageAndPath(test.FullName);

            AfterTest(test, cassetteStorage, cassettePath);
        }

        protected virtual void BeforeTest(ITest test, YamlCassetteStorage cassetteStorage, string cassettePath)
        {
            var cassette = cassetteStorage.LoadCassette(cassettePath);
            var httpMessageHandler = CreateHttpMessageHandler(cassette);

            var withVcr = GetWithVcr(test);

            withVcr.Cassette = cassette;
            withVcr.HttpMessageHandler = httpMessageHandler;
        }

        protected virtual IWithVcr GetWithVcr(ITest test)
        {
            var withVcr = test.Parent.Fixture as IWithVcr;
            if (withVcr == null)
            {
                throw new ArgumentException($"Test class have to implement {typeof(IWithVcr).Name} interface");
            }
            
            return withVcr;
        }

        protected virtual TryReplayHttpMessageHandler CreateHttpMessageHandler(Cassette cassette)
            => new TryReplayHttpMessageHandler(cassette, new SocketsHttpHandler());

        protected virtual void AfterTest(ITest test, YamlCassetteStorage cassetteStorage, string cassettePath)
        {
            var withVcr = GetWithVcr(test);

            cassetteStorage.SaveCassette(cassettePath, withVcr.Cassette);
        }

        public ActionTargets Targets { get; } = ActionTargets.Test;

        private (YamlCassetteStorage storage, string path) CreateStorageAndPath(string testName) 
            => (new YamlCassetteStorage(), Path.Combine(_cassetteFolder, testName + ".cassette"));
    }
}