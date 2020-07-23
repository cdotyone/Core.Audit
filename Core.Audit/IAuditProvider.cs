using System;
using System.Collections.Generic;
using Core.Configuration;

namespace Core.Audit
{
    public interface IAuditProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        INamedElement Configuration { get; set; }

        string LogChange<T>(string module, string trackingID, string ouid, string who, DateTime when, string clientMachine, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after, T entity);

        void MarkSuccessFul<T>(string module, string trackingID, string ouid, string who, string entityKey, T entity);
    }
}