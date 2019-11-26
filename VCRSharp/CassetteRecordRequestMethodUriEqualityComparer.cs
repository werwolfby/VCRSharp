using System.Collections.Generic;

namespace VCRSharp
{
    public class CassetteRecordRequestMethodUriEqualityComparer : IEqualityComparer<CassetteRecordRequest>
    {
        public bool Equals(CassetteRecordRequest? x, CassetteRecordRequest? y) => x?.Method == y?.Method && x?.Uri == y?.Uri;

        public int GetHashCode(CassetteRecordRequest obj) => obj.Uri.GetHashCode() * 17 + obj.Method.GetHashCode();
    }
}