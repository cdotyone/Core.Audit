using Civic.Core.Configuration;

namespace Civic.Core.Audit
{
    public class AuditConfig : NamedConfigurationElement
    {
        protected const string USE_LOCAL_TIME = "useLocalTime";

        public AuditConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() {Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;
        }

        /// <summary>
        /// The current configuration for the audit library
        /// </summary>
        public static AuditConfig Current
        {
            get
            {
                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new AuditConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
        }

        private static CivicSection _coreConfig;
        private static AuditConfig _current;


        /// <summary>
        /// The name of the configuration section.
        /// </summary>
        public static string SectionName
        {
            get { return "audit"; }
        }

        public bool UseLocalTime
        {
            get
            {
                return !Attributes.ContainsKey(USE_LOCAL_TIME) || bool.Parse(Attributes[USE_LOCAL_TIME]);
            }
            set
            {
                Attributes[USE_LOCAL_TIME] = value.ToString();
            }
        }
    }
}

