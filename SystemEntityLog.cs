using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Civic.Core.Audit
{
    [DataContract(Name = "systemEntityLog")]
    public class SystemEntityLog
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }

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
        public Dictionary<string,string> Before { get; set; }

        [DataMember(Name = "after")]
        public Dictionary<string, string> After { get; set; }

        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "created")]
        public System.DateTime? Created { get; set; }

        [DataMember(Name = "recorded")]
        public System.DateTime Recorded { get; set; }

        [DataMember(Name = "createdBy")]
        public string CreatedBy { get; set; }

        [DataMember(Name = "clientMachine")]
        public string ClientMachine { get; set; }
    }
}

