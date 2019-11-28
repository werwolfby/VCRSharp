﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;
using Version = System.Version;

namespace VCRSharp
{
    public class YamlCassetteStorage : ICassetteStorage
    {
        private readonly IValueSerializer _serializer;
        private readonly IValueDeserializer _deserializer;

        public YamlCassetteStorage()
        {
            var uriYamlTypeConverter = new UriYamlTypeConverter();
            var versionYamlTypeConverter = new VersionYamlTypeConverter();
            var nameValueCollectionYamlTypeConverter = new NameValueCollectionYamlTypeConverter();
            
            var serializerBuilder = new SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(versionYamlTypeConverter)
                .WithTypeConverter(nameValueCollectionYamlTypeConverter)
                .WithTypeConverter(uriYamlTypeConverter)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults);
            _serializer = serializerBuilder.BuildValueSerializer();

            var deserializerBuilder = new DeserializerBuilder()
                .WithTypeConverter(versionYamlTypeConverter)
                .WithTypeConverter(nameValueCollectionYamlTypeConverter)
                .WithTypeConverter(uriYamlTypeConverter);
            _deserializer = deserializerBuilder.BuildValueDeserializer();
        }

        public void Save(TextWriter textWriter, IEnumerable<CassetteRecord> records)
        {
            var emiter = new Emitter(textWriter);
            emiter.Emit(new StreamStart());
            foreach (var record in records)
            {
                emiter.Emit(new DocumentStart());
                _serializer.SerializeValue(emiter, record, record.GetType());
                emiter.Emit(new DocumentEnd(true));
            }
            emiter.Emit(new StreamEnd());
        }

        public IReadOnlyList<CassetteRecord> Load(TextReader textReader)
        {
            var records = new List<CassetteRecord>();
            var parser = new Parser(textReader);
            parser.Consume<StreamStart>();
            while (!(parser.Current is StreamEnd))
            {
                parser.Consume<DocumentStart>();
                var record = (CassetteRecord)(_deserializer.DeserializeValue(parser, typeof(CassetteRecord), new SerializerState(), _deserializer)
                    ?? throw new ArgumentException("Can't deserialize CassetteRecord"));
                records.Add(record);
                parser.Consume<DocumentEnd>();
            }
            parser.Consume<StreamEnd>();

            return records;
        }
        
        private class UriYamlTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
                => type == typeof(Uri);

            public object ReadYaml(IParser parser, Type type)
                => new Uri(parser.Consume<Scalar>().Value);

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var uri = (Uri)(value ?? throw new ArgumentNullException(nameof(value)));
                emitter.Emit(new Scalar(null, null,
                    uri.ToString(), ScalarStyle.Any, true,
                    false));
            }
        }
        
        private class VersionYamlTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
                => type == typeof(Version);

            public object ReadYaml(IParser parser, Type type)
                => new Version(parser.Consume<Scalar>().Value);

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var version = (Version) (value ?? throw new ArgumentNullException(nameof(value)));
                emitter.Emit(new Scalar(null, null, version.ToString(2), ScalarStyle.Any, true, false));
            }
        }
        
        private class NameValueCollectionYamlTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
                => type == typeof(NameValueCollection);

            public object ReadYaml(IParser parser, Type type)
            {
                var nameValueCollection = new NameValueCollection();
                parser.Consume<SequenceStart>();
                while (parser.TryConsume(out MappingStart _))
                {
                    var key = parser.Consume<Scalar>().Value;
                    if (parser.Current is Scalar)
                    {
                        var value = parser.Consume<Scalar>().Value;
                        nameValueCollection.Add(key, value);
                    }
                    else if (parser.Current is SequenceStart)
                    {
                        parser.Consume<SequenceStart>();
                        while (parser.Current is Scalar)
                        {
                            nameValueCollection.Add(key, parser.Consume<Scalar>().Value);
                        }
                        parser.Consume<SequenceEnd>();
                    }
                    else
                    {
                        throw new YamlException($"Expect Scalar or Sequence start, got: {parser.Current?.GetType().Name ?? "null"}");
                    }

                    parser.Consume<MappingEnd>();
                }

                parser.Consume<SequenceEnd>();
                
                return nameValueCollection;
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var nameValueCollection = (NameValueCollection)(value ?? throw new ArgumentNullException(nameof(value)));
                
                emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Block));
                for (var i = 0; i < nameValueCollection.Count; i++)
                {
                    var key = nameValueCollection.GetKey(i);
                    var values = nameValueCollection.GetValues(i) ?? Array.Empty<string>();
                    emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                    emitter.Emit(new Scalar(key));
                    if (values.Length == 1)
                    {
                        emitter.Emit(new Scalar(values[0]));
                    }
                    else
                    {
                        emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));
                        foreach (var singleValue in values)
                        {
                            emitter.Emit(new Scalar(singleValue));
                        }
                        emitter.Emit(new SequenceEnd());
                    }

                    emitter.Emit(new MappingEnd());
                }
                emitter.Emit(new SequenceEnd());
            }
        }
    }
}