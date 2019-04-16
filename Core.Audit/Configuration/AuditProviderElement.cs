using System.Collections.Generic;
using Stack.Core.Configuration;
using Stack.Core.Configuration.Framework;

namespace Stack.Core.Audit.Configuration
{
	public class AuditProviderElement : NamedConfigurationElement
    {
        protected const string USE_LOCAL_TIME = "useLocalTime";
        private IAuditProvider _provider;
        private bool _useLocalTime;

        /// <summary>
        /// The "assembly" name given of the provider.
        /// 
        /// In the form: assembly="Core.Configuration, Version=1.0.0.0, Culture=neutral"
        /// </summary>
        public string Assembly
	    {
	        get { return _assembly; }
	        set
	        {
	            _assembly = value;
	            Attributes[Constants.ASSEMBLY] = value;
	        }
	    }

	    private string _assembly;

	    /// <summary>
	    /// The "type" name of the provider.
	    /// 
	    /// In the form of type="Core.Audit.Providers.WebCacheProvider"
	    /// </summary>
	    public string Type
	    {
	        get { return _typeName; }
	        set
	        {
	            _typeName = value;
	            Attributes[Constants.TYPE] = value;
	        }
	    }
	    private string _typeName;

		/// <summary>
		/// Trys to dynamically create the provider and then returns the provider.
		/// </summary>
		public IAuditProvider Provider
		{
			get {
                if(_provider!=null) return _provider;

			    _provider = (IAuditProvider) DynamicInstance.CreateInstance(Assembly, Type);
			    _provider.Configuration = this;

                return _provider;
			}
		}

        /// <summary>
        /// A comma seperated list of modules this provider writes for
        /// No value causes provider to write for all modules
        /// </summary>
        public List<string> Modules
        {
            get
            {
                return _modules;
            }
            set
            {
                _modules = value;
                Attributes[Constants.TYPE] = string.Join(",",value).ToUpperInvariant();
            }
        }
        private List<string> _modules;

        public bool UseLocalTime
        {
            get { return _useLocalTime; }
            set { _useLocalTime = value; Attributes[USE_LOCAL_TIME] = value.ToString(); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AuditProviderElement(INamedElement config, AuditConfig auditConfig)
		{
		    Attributes = config.Attributes;
		    Children = config.Children;

		    _assembly = Attributes[Constants.ASSEMBLY];
            _typeName = Attributes[Constants.TYPE];
            _modules = Attributes.ContainsKey(Constants.MODULES) ? new List<string>(Attributes[Constants.MODULES].ToUpperInvariant().Split(',')) : new List<string>();
            _useLocalTime = Attributes.ContainsKey(USE_LOCAL_TIME) ? bool.Parse(Attributes[USE_LOCAL_TIME]) : auditConfig.UseLocalTime;
        }

        /// <summary>
        /// Creates a AuditProviderElement from a IAuditProvider
        /// </summary>
        /// <param name="provider">the provider to create the configuration entry from</param>
        public AuditProviderElement(IAuditProvider provider)
        {
            _provider = provider;
            if (Name.EndsWith("Provider")) Name = Name.Substring(0, Name.Length - 8);
            Assembly = provider.GetType().Assembly.FullName;
            Type = provider.GetType().FullName;
            _useLocalTime = true;
        }

        /// <summary>
        /// Creates a AuditProviderElement from a IAuditProvider
        /// </summary>
        /// <param name="provider">the provider to create the configuration entry from</param>
        /// <param name="config">the configuration for the provider</param>
        public AuditProviderElement(IAuditProvider provider, INamedElement config)
		{
            Attributes = config.Attributes;
            Children = config.Children;

            _provider = provider;
            if (Name.EndsWith("Provider")) Name = Name.Substring(0, Name.Length - 8);

            Assembly = provider.GetType().Assembly.FullName;
            Type = provider.GetType().FullName;
            _useLocalTime = !Attributes.ContainsKey(USE_LOCAL_TIME) || bool.Parse(Attributes[USE_LOCAL_TIME]);
        }
    }
}