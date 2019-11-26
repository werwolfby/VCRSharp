using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.Unicode;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;
using Version = System.Version;

namespace VCRSharp
{
    public class Cassette
    {
        private readonly List<CassetteRecord> _records = new List<CassetteRecord>();

        public IReadOnlyList<CassetteRecord> Records => _records;

        public void Add(CassetteRecord record) => _records.Add(record);
    }
}