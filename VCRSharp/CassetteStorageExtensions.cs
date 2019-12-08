using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCRSharp
{
    public static class CassetteStorageExtensions
    {
        public static void Save(this ICassetteStorage storage, StreamWriter streamWriter, IEnumerable<CassetteRecord> records) => storage.Save((TextWriter)streamWriter, records);

        public static IReadOnlyList<CassetteRecord> Load(this ICassetteStorage storage, StreamReader streamReader) => storage.Load((TextReader) streamReader);
        
        public static void Save(this ICassetteStorage storage, string path, IEnumerable<CassetteRecord> records)
        {
            var fileInfo = new FileInfo(path);
            
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            storage.Save(streamWriter, records);
        }

        public static IReadOnlyList<CassetteRecord> Load(this ICassetteStorage storage, string path)
        {
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            return storage.Load(streamReader);
        }
    }
}