using System;
using System.Collections.Generic;
using System.Globalization;
using Civic.Core.Audit.Configuration;
using Civic.Core.Configuration;
using Civic.Core.Data;

namespace Civic.Core.Audit.Providers
{
    public class SqlAuditProvider : IAuditProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

        public string LogChange(string module, string trackingID, string who, DateTime when, string clientMachine, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after)
        {
            var config = Configuration as AuditProviderElement;

            var log = new AuditLog
                {
                    TrackingUID = trackingID,
                    ModuleCode = module,
                    EntityCode = schema + "_" + entityCode,
                    EntityKeys = entityKeys,
                    RelatedEntityCode = relatedEntityCode,
                    RelatedEntityKeys = relatedEntityKeys,
                    Success = false,
                    Action = action,
                    ClientMachine = clientMachine,
                    Before = before,
                    After = after,
                    Created = when,
                    CreatedBy = who
                };

            using (var database = DatabaseFactory.CreateDatabase(module))
            {
                AuditData.AddAuditLog(log, database, config.UseLocalTime);
            }

            return log.TrackingUID;
        }

        public void MarkSuccessFul(string module, string trackingID, string enityKey)
        {
            using (var database = DatabaseFactory.CreateDatabase(module))
            {
                AuditData.MarkAuditLogSuccessFul(trackingID, enityKey, database);
            }
        }
    }
}
