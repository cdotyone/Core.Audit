using System;
using System.Collections.Generic;
using Stack.Core.Audit.Configuration;
using Stack.Core.Configuration;
using Stack.Core.Data;

namespace Stack.Core.Audit.Providers
{
    public class SqlAuditProvider : IAuditProvider
    {
        /// <inheritdoc />
        public INamedElement Configuration { get; set; }

        /// <inheritdoc />
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

            var connection = config!=null && config.Attributes.ContainsKey(Constants.CONFIG_DEFAULTPROVIDER) ? config.Attributes[Constants.CONFIG_DEFAULTPROVIDER] : module;

            if (config != null && config.Children.ContainsKey(module))
                connection = config.Children[module].Attributes["to"];

            using (var database = DatabaseFactory.CreateDatabase(connection))
            {
                AuditData.AddAuditLog(log, database, config!=null && config.UseLocalTime);
            }

            return log.TrackingUID;
        }

        /// <inheritdoc />
        public void MarkSuccessFul(string module, string trackingID, string entityKey)
        {
            using (var database = DatabaseFactory.CreateDatabase(module))
            {
                AuditData.MarkAuditLogSuccessFul(trackingID, entityKey, database);
            }
        }
    }
}
