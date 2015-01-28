using System;
using System.Collections.Generic;
using System.Globalization;
using Civic.Core.Data;

namespace Civic.Core.Audit
{
    public class AuditLogger : IAuditLogger
    {
        public string LogChange(string who, DateTime when, string clientMachine, string module, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, Dictionary<string, string> before, Dictionary<string, string> after)
        {
            var log = new SystemEntityLog
                {
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
                AuditData.AddSystemEntityLog(log, database);
            }

            return log.ID.ToString(CultureInfo.InvariantCulture);
        }

        public void MarkSuccessFul(string module, string id)
        {
            using (var database = DatabaseFactory.CreateDatabase(module))
            {
                AuditData.MarkSystemEntityLogSuccessFul(new[] {id},database);
            }
        }

        public void MarkSuccessFul(string module, IEnumerable<string> ids)
        {
            using (var database = DatabaseFactory.CreateDatabase(module))
            {
                AuditData.MarkSystemEntityLogSuccessFul(ids, database);
            }
        }
    }
}
