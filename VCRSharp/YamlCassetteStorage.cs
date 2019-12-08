using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
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
            var cassetteBodyYamlTypeConverter = new CassetteBodyYamlTypeConverter();
            
            var serializerBuilder = new SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(versionYamlTypeConverter)
                .WithTypeConverter(nameValueCollectionYamlTypeConverter)
                .WithTypeConverter(uriYamlTypeConverter)
                .WithTypeConverter(cassetteBodyYamlTypeConverter)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults);
            _serializer = serializerBuilder.BuildValueSerializer();

            var deserializerBuilder = new DeserializerBuilder()
                .WithTypeConverter(versionYamlTypeConverter)
                .WithTypeConverter(nameValueCollectionYamlTypeConverter)
                .WithTypeConverter(uriYamlTypeConverter)
                .WithTypeConverter(cassetteBodyYamlTypeConverter)
                .WithNodeDeserializer(new ConstructorNodeDeserializer<CassetteRecord>(), r => r.OnTop())
                // We want to ignore nullable checks in lambda function
                #nullable disable
                .WithNodeDeserializer(new ConstructorNodeDeserializer<CassetteRecordRequest>(() => new CassetteRecordRequest(null, null, null, (CassetteBody)null)), r => r.OnTop())
                .WithNodeDeserializer(new ConstructorNodeDeserializer<CassetteRecordResponse>(() => new CassetteRecordResponse(null, 0, null, null, (CassetteBody)null, (CassetteRecordRequest)null)), r => r.OnTop());
                #nullable enable
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

        public static Cassette LoadCassette(TextReader textReader) => new YamlCassetteStorage().LoadCassette(textReader);

        public static Cassette LoadCassette(StreamReader streamReader) => new YamlCassetteStorage().LoadCassette(streamReader);

        public static Cassette LoadCassette(string path) => new YamlCassetteStorage().LoadCassette(path);

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
        
        private class ConstructorNodeDeserializer<T> : INodeDeserializer
        {
            private readonly ConstructorInfo? _constructorInfo;

            public ConstructorNodeDeserializer() : this((ConstructorInfo?)null)
            {
            }

            public ConstructorNodeDeserializer(Expression<Func<T>> expression)
            {
                _constructorInfo = ((NewExpression)expression.Body).Constructor;
            }

            public ConstructorNodeDeserializer(ConstructorInfo? constructorInfo)
            {
                _constructorInfo = constructorInfo;
            }

            public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
            {
                if (expectedType != typeof(T) || !parser.TryConsume(out MappingStart _))
                {
                    value = null;
                    return false;
                }

                var constructor = _constructorInfo ?? expectedType.GetConstructors().Single();
                var parameters = constructor.GetParameters();
                var expectedParameters = parameters
                    .ToDictionary(
                        p => p.Name,
                        p => (type: p.ParameterType, hasDefault: (p.Attributes & ParameterAttributes.HasDefault) != 0, value: (object?)null),
                        StringComparer.OrdinalIgnoreCase);
                var start = parser.Current?.Start ?? throw new YamlException("Parsing not started yet");
                while (!parser.TryConsume(out MappingEnd _))
                {
                    var scalar = parser.Consume<Scalar>();
                    if (!expectedParameters.TryGetValue(scalar.Value, out var expectedParameterDescription))
                    {
                        throw new YamlException(scalar.Start, scalar.End, $"{scalar.Value} parameter not found for type `{expectedType.Name}`");
                    }

                    if (expectedParameterDescription.value != null)
                    {
                        throw new YamlException(scalar.Start, scalar.End, $"{scalar.Value} parameter specified multiple times for type `{expectedType.Name}`");
                    }

                    var parameterValue = nestedObjectDeserializer(parser, expectedParameterDescription.type);
                    expectedParameters[scalar.Value] = (expectedParameterDescription.type, expectedParameterDescription.hasDefault, parameterValue);
                }

                var end = parser.Current?.End ?? throw new YamlException("Parsing not started yet");;
                if (expectedParameters.Any(kv => kv.Value.value == null && !kv.Value.hasDefault))
                {
                    var emptyParameters = expectedParameters.Where(p => p.Value.value == null && !p.Value.hasDefault).Select(p => p.Key);
                    throw new YamlException(start, end, $"Required parameters `{string.Join(", ", emptyParameters)}` is not specified");
                }

                var parameterValues = parameters.Select(p => expectedParameters.TryGetValue(p.Name, out var parameterValue) ? parameterValue.value : null).ToArray();
                value = constructor.Invoke(parameterValues);
                return true;
            }
        }
        
        private class CassetteBodyYamlTypeConverter : IYamlTypeConverter
        {
            private const string BinaryTag = "!binary";

            public bool Accepts(Type type)
                => typeof(CassetteBody).IsAssignableFrom(type);

            public object ReadYaml(IParser parser, Type type)
            {
                var scalar = parser.Consume<Scalar>();
                return scalar.Tag switch
                {
                    BinaryTag => new BytesCassetteBody(Convert.FromBase64String(scalar.Value)),
                    _ => new StringCassetteBody(scalar.Value)
                };
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var cassetteBody = (CassetteBody)(value ?? throw new ArgumentNullException(nameof(value)));
                switch (cassetteBody)
                {
                    case StringCassetteBody s:
                        emitter.Emit(new Scalar(null, null,
                            s.Value, ScalarStyle.Any, true,
                            false));
                        break;
                    case BytesCassetteBody b:
                        emitter.Emit(new Scalar(null, BinaryTag,
                            Convert.ToBase64String(b.Value, Base64FormattingOptions.InsertLineBreaks), 
                            ScalarStyle.Literal, true, false));
                        break;
                }
            }
        }
    }
}