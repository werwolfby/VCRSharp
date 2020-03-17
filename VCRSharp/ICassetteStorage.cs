using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VCRSharp
{
    public interface ICassetteStorage
    {
        void Save(TextWriter textWriter, IEnumerable<CassetteRecord> records);
        
        IReadOnlyList<CassetteRecord> Load(TextReader textReader);
    }
}