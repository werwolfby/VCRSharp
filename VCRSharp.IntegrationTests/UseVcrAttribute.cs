using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            var withVcr = GetWithVcr(test);

            withVcr.Cassette = null;
            withVcr.HttpMessageHandler = null;
        }

        protected virtual IWithVcr GetWithVcr(ITest test)
        {
            var parent = test.Parent;
            while (parent != null && !(parent.Fixture is IWithVcr))
            {
                parent = parent.Parent;
            }
            
            if (parent == null)
            {
                throw new ArgumentException($"Test class have to implement {typeof(IWithVcr).Name} interface");
            }

            return (IWithVcr)parent.Fixture;
        }

        protected virtual TryReplayHttpMessageHandler CreateHttpMessageHandler(Cassette cassette)
            => new TryReplayHttpMessageHandler(cassette, new SocketsHttpHandler());

        protected virtual void BeforeTest(ITest test, YamlCassetteStorage cassetteStorage, string cassettePath)
        {
            var cassette = cassetteStorage.LoadCassette(cassettePath);
            var httpMessageHandler = CreateHttpMessageHandler(cassette);

            var withVcr = GetWithVcr(test);

            withVcr.Cassette = cassette;
            withVcr.HttpMessageHandler = httpMessageHandler;
        }

        protected virtual void AfterTest(ITest test, YamlCassetteStorage cassetteStorage, string cassettePath)
        {
            var withVcr = GetWithVcr(test);

            foreach (var record in withVcr.Cassette.Records)
            {
                FilterCassetteRecord(record);
            }

            cassetteStorage.SaveCassette(cassettePath, withVcr.Cassette);
        }

        protected virtual void FilterCassetteRecord(CassetteRecord record)
        {
        }

        public ActionTargets Targets { get; } = ActionTargets.Test;

        private (YamlCassetteStorage storage, string path) CreateStorageAndPath(string testName)
            => (new YamlCassetteStorage(), Path.Combine(_cassetteFolder, ReplaceFileNameInvalidChars(testName) + ".cassette"));

        private static string ReplaceFileNameInvalidChars(string fileName)
            => Path.GetInvalidFileNameChars()
                .Aggregate(new StringBuilder(fileName), (sb, c) => sb.Replace(c, '_'))
                .ToString();
    }
}