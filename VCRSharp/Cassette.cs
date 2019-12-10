using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
        private readonly IEqualityComparer<CassetteRecordRequest> _defaultComparer;

        public Cassette() : this(new CassetteRecordRequestMethodUriEqualityComparer())
        {
        }

        public Cassette(IEnumerable<CassetteRecord> records) : this(records, new CassetteRecordRequestMethodUriEqualityComparer())
        {
        }

        public Cassette(IEqualityComparer<CassetteRecordRequest> defaultComparer) => _defaultComparer = defaultComparer ?? throw new ArgumentNullException(nameof(defaultComparer));

        public Cassette(IEnumerable<CassetteRecord> records, IEqualityComparer<CassetteRecordRequest> defaultComparer)
        {
            _defaultComparer = defaultComparer ?? throw new ArgumentNullException(nameof(defaultComparer));
            _records.AddRange(records);
        }

        public IReadOnlyList<CassetteRecord> Records => _records;

        public void Add(CassetteRecord record) => _records.Add(record);

        public CassetteRecord? Find(CassetteRecordRequest request,
            IEqualityComparer<CassetteRecordRequest>? comparer = null)
        {
            comparer ??= _defaultComparer;
            return _records.FirstOrDefault(r => comparer.Equals(r.Request, request));
        }
    }
}