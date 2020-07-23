using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Core.Audit
{
    [DataContract(Name = "auditLog")]
    public class AuditLog
    {
        [DataMember(Name = "uid")]
        public string UID { get; set; }

        [DataMember(Name = "ouid")]
        public string OUID { get; set; }

        [DataMember(Name = "id")]
        [JsonIgnore]
        public int? ID { get; set; }

        [DataMember(Name = "moduleCode")]
        public string ModuleCode { get; set; }

        [DataMember(Name = "trackingUID")]
        public string TrackingUID { get; set; }

        [DataMember(Name = "entityCode")]
        public string EntityCode { get; set; }

        [DataMember(Name = "entityKeys")]
        public string EntityKeys { get; set; }

        [DataMember(Name = "relatedEntityCode")]
        public string RelatedEntityCode { get; set; }

        [DataMember(Name = "relatedEntityKeys")]
        public string RelatedEntityKeys { get; set; }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "before")]
        public Dictionary<string, string> Before { get; set; }

        [DataMember(Name = "after")]
        public Dictionary<string, string> After { get; set; }

        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "created")]
        public System.DateTime? Created { get; set; }

        [DataMember(Name = "processed")]
        public System.DateTime? Processed { get; set; }

        [DataMember(Name = "recorded")]
        public System.DateTime Recorded { get; set; }

        [DataMember(Name = "createdBy")]
        public string CreatedBy { get; set; }

        [DataMember(Name = "clientMachine")]
        public string ClientMachine { get; set; }
    }
}