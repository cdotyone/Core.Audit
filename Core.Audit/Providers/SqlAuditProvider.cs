using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Audit.Configuration;
using Core.Configuration;
using Core.Data;

namespace Core.Audit.Providers
{
    public class SqlAuditProvider : IAuditProvider
    {
        /// <inheritdoc />
        public INamedElement Configuration { get; set; }

        /// <inheritdoc />
        public string LogChange<T>(string module, string trackingID, string ouid, string who, DateTime when, string clientMachine, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after, T entity)
        {
            var config = Configuration as AuditProviderElement;

            var log = new AuditLog
            {
                UID = string.Empty.InsureUID(),
                OUID = ouid,
                TrackingUID = trackingID,
                ModuleCode = module,
                EntityCode = module + "_" + entityCode,
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

            var connection = config != null && config.Attributes.ContainsKey(Constants.CONFIG_DEFAULTPROVIDER) ? config.Attributes[Constants.CONFIG_DEFAULTPROVIDER] : module;

            if (config != null && config.Children.ContainsKey(module))
                connection = config.Children[module].Attributes["to"];

            using (var database = DatabaseFactory.CreateDatabase(connection))
            {
                AuditData.AddAuditLog(log, database, config != null && config.UseLocalTime);
            }

            return log.TrackingUID;
        }

        /// <inheritdoc />
        public void MarkSuccessFul<T>(string module, string trackingID, string ouid, string who, string entityKeys, T entity)
        {
            var config = Configuration as AuditProviderElement;

            var connection = config != null && config.Attributes.ContainsKey(Constants.CONFIG_DEFAULTPROVIDER) ? config.Attributes[Constants.CONFIG_DEFAULTPROVIDER] : module;

            if (config != null && config.Children.ContainsKey(module))
                connection = config.Children[module].Attributes["to"];

            //Thread to increase endpoint performance.
            Task.Run(() => {
                using (var database = DatabaseFactory.CreateDatabase(connection))
                {
                    AuditData.MarkAuditLogSuccessFul(trackingID, entityKeys, database);
                }
            });
        }
    }
}
