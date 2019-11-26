using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCRSharp
{
    public interface ICassetteStorage
    {
        void Save(TextWriter textWriter, IEnumerable<CassetteRecord> records);
        
        IReadOnlyList<CassetteRecord> Load(TextReader textReader);

        public void Save(StreamWriter streamWriter, IEnumerable<CassetteRecord> records) => Save((TextWriter)streamWriter, records);

        public IReadOnlyList<CassetteRecord> Load(StreamReader streamReader) => Load((TextReader) streamReader);
        
        public void Save(string path, IEnumerable<CassetteRecord> records)
        {
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
            Save(streamWriter, records);
        }

        public IReadOnlyList<CassetteRecord> Load(string path)
        {
            using var streamReader = new StreamReader(path, Encoding.UTF8);
            return Load(streamReader);
        }
    }
}