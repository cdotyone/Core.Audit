using Civic.Core.Configuration;

namespace Civic.Core.Audit
{
    public class AuditConfig : NamedConfigurationElement
    {
        protected const string USE_LOCAL_TIME = "useLocalTime";
        private static CivicSection _coreConfig;
        private static AuditConfig _current;
        private bool _useLocalTime;

        public AuditConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() {Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;

            _useLocalTime = !Attributes.ContainsKey(USE_LOCAL_TIME) || bool.Parse(Attributes[USE_LOCAL_TIME]);
        }

        /// <summary>
        /// The current configuration for the audit library
        /// </summary>
        public static AuditConfig Current
        {
            get
            {
                if (_current != null) return _current;

                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new AuditConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
        }

        /// <summary>
        /// The name of the configuration section.
        /// </summary>
        public static string SectionName
        {
            get { return "audit"; }
        }

        public bool UseLocalTime
        {
            get { return _useLocalTime; }
            set { _useLocalTime = value; Attributes[USE_LOCAL_TIME] = value.ToString(); }
        }
    }
}

