using System.Collections.Generic;

namespace Civic.Core.Audit
{
    public interface IAuditLogger
    {
        string LogChange(string who, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, string before, string after);

        void MarkSuccessFul(string id);

        void MarkSuccessFul(IEnumerable<string> id);
    }
}