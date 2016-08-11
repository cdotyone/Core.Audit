using System;
using System.Collections.Generic;
using Civic.Core.Configuration;

namespace Civic.Core.Audit
{
    public interface IAuditProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        INamedElement Configuration { get; set; }

        string LogChange(string module, string trackingID, string who, DateTime when, string clientMachine, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after);

        void MarkSuccessFul(string module, string trackingID, string enityKey);
    }
}