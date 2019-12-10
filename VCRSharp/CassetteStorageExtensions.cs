using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCRSharp
{
    public static class CassetteStorageExtensions
    {
        public static void Save(this ICassetteStorage storage, StreamWriter streamWriter, IEnumerable<CassetteRecord> records) => storage.Save(streamWriter, records);

        public static IReadOnlyList<CassetteRecord> Load(this ICassetteStorage storage, StreamReader streamReader) => storage.Load(streamReader);
        
        public static void Save(this ICassetteStorage storage, string path, IEnumerable<CassetteRecord> records)
        {
            var fileInfo = new FileInfo(path);
            
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            storage.Save(streamWriter, records);
        }

        public static IReadOnlyList<CassetteRecord> Load(this ICassetteStorage storage, string path)
        {
            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                return Array.Empty<CassetteRecord>();
            }
            
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            return storage.Load(streamReader);
        }
        
        public static void SaveCassette(this  ICassetteStorage storage, TextWriter textWriter, Cassette cassette) => storage.Save(textWriter, cassette.Records);
        
        public static void SaveCassette(this  ICassetteStorage storage, StreamWriter streamWriter, Cassette cassette) => storage.Save(streamWriter, cassette.Records);
        
        public static void SaveCassette(this  ICassetteStorage storage, string path, Cassette cassette) => storage.Save(path, cassette.Records);
        
        public static Cassette LoadCassette(this  ICassetteStorage storage, TextReader textReader) => new Cassette(storage.Load(textReader));
        
        public static Cassette LoadCassette(this  ICassetteStorage storage, StreamReader streamReader) => new Cassette(storage.Load(streamReader));
        
        public static Cassette LoadCassette(this  ICassetteStorage storage, string path) => new Cassette(storage.Load(path));
    }
}