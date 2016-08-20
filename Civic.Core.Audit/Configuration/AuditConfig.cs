using System.Collections.Generic;
using Civic.Core.Audit.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Audit.Configuration
{
    public class AuditConfig : NamedConfigurationElement
    {
        protected const string DEFAULT_CONNECTION_STRING = "default";
        protected const string USE_LOCAL_TIME = "useLocalTime";
        private static CivicSection _coreConfig;
        private static AuditConfig _current;
        private string _default;

        public AuditConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() {Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;

            if (Attributes.ContainsKey(DEFAULT_CONNECTION_STRING)) _default = Attributes[DEFAULT_CONNECTION_STRING];
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

        /// <summary>
        /// Gets or sets the typename for the skin for the header and footer
        /// </summary>
        public string DefaultProvider
        {
            get { return _default; }
            set { _default = value; Attributes[Constants.CONFIG_PROP_DEFAULTPROVIDER] = value; }
        }

        /// <summary>
        /// Gets the collection cache providers
        /// </summary>
        public Dictionary<string, AuditProviderElement> Providers
        {
            get
            {
                if (_providers != null) return _providers;
                if (Children.Count == 0)
                {
                    Children.Add("SqlCacheProvider",
                        new AuditProviderElement(new SqlAuditProvider(),
                            new NamedConfigurationElement()
                            {
                                Attributes = new Dictionary<string, string> { { "connectionStringName", "CIVIC" } }
                            }));
                }

                _providers = new Dictionary<string, AuditProviderElement>();
                foreach (var element in Children)
                {
                    _providers[element.Key.ToLowerInvariant()] = new AuditProviderElement(element.Value);
                }

                return _providers;
            }
        }
        private Dictionary<string, AuditProviderElement> _providers;

        /// <summary>
        /// Gets a list of providers that should recieve audit messages
        /// </summary>
        /// <param name="moduleName">The name of the module that is to recieve the audit messages</param>
        /// <returns>the lsit of providers</returns>
        public List<IAuditProvider> GetProvidersByModule(string moduleName)
        {
            moduleName = moduleName.ToUpperInvariant();
            if (_moduleMap.ContainsKey(moduleName)) return _moduleMap[moduleName];

            var list = new List<IAuditProvider>();
            var providers = Providers;

            foreach (var provider in providers.Values)
            {
                if (provider.Modules.Count == 0) list.Add(provider.Provider);
                else if(provider.Modules.Contains(moduleName)) list.Add(provider.Provider);
            }

            if (list.Count > 0) _moduleMap[moduleName] = list;

            return list;
        }
        private readonly Dictionary<string, List<IAuditProvider>> _moduleMap = new Dictionary<string, List<IAuditProvider>>();
    }
}

