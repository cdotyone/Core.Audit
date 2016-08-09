using System;
using System.Collections.Generic;

namespace Civic.Core.Audit
{
    public interface IAuditLogger
    {
        string LogChange(string who, DateTime when, string clientMachine, string module, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after);

        void MarkSuccessFul(string module, string id, string enityKey);

        void MarkSuccessFul(string module, IEnumerable<string> id);
    }
}