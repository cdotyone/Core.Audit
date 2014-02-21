using System;
using System.Collections.Generic;

namespace Civic.Core.Audit
{
    public class AuditLogger : IAuditLogger
    {
        public string LogChange(string who, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, string before, string after)
        {

            var log = new SystemEntityLog
                {
                    EntityCode = schema + "_" + entityCode,
                    EntityKeys = entityKeys,
                    RelatedEntityCode = relatedEntityCode,
                    RelatedEntityKeys = relatedEntityKeys,
                    Success = false,
                    Action = action,
                    Before = before,
                    After = after,
                    Created = DateTime.UtcNow,
                    CreatedBy = who
                };

            AuditData.AddSystemEntityLog(log);

            return log.ID.ToString();
        }

        public void MarkSuccessFul(string id)
        {
            AuditData.MarkSystemEntityLogSuccessFul(new[] { id });
        }

        public void MarkSuccessFul(IEnumerable<string> ids)
        {
            AuditData.MarkSystemEntityLogSuccessFul(ids);
        }
    }
}
