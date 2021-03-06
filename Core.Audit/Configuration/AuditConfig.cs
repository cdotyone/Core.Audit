﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Core.Audit.Providers;
using Core.Configuration;

namespace Core.Audit.Configuration
{
    public class AuditConfig : NamedConfigurationElement
    {
        protected const string USE_LOCAL_TIME = "useLocalTime";
        protected const string USE_OUID = "useOUID";

        private static CoreSection _coreConfig;
        private static AuditConfig _current;
        private string _default;
        private bool _useLocalTime;
        private bool _useOUID;

        public AuditConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() {Name = SectionName};
            Children = element.Children;
            Attributes = element.Attributes;
            //Name = element.Name;
            _useLocalTime = Attributes.ContainsKey(USE_LOCAL_TIME) && bool.Parse(Attributes[USE_LOCAL_TIME]);
            _useOUID = Attributes.ContainsKey(USE_OUID) && bool.Parse(Attributes[USE_OUID]);
        }

        /// <summary>
        /// The current configuration for the audit library
        /// </summary>
        public static AuditConfig Current
        {
            get
            {
                if (_current != null) return _current;

                if (_coreConfig == null) _coreConfig = CoreSection.Current;
                _current = new AuditConfig(_coreConfig.Children.ContainsKey(SectionName)
                    ? _coreConfig.Children[SectionName]
                    : null);
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
            set
            {
                _default = value;
                Attributes[Constants.CONFIG_PROP_DEFAULTPROVIDER] = value;
            }
        }

        public bool UseLocalTime
        {
            get { return _useLocalTime; }
            set
            {
                _useLocalTime = value;
                Attributes[USE_LOCAL_TIME] = value.ToString();
            }
        }

        public bool UseOUID
        {
            get { return _useOUID; }
            set
            {
                _useOUID = value;
                Attributes[USE_OUID] = value.ToString();
            }
        }

        private static object providerLock = new object();

        /// <summary>
        /// Gets the collection cache providers
        /// </summary>
        public ConcurrentDictionary<string, AuditProviderElement> Providers
        {
            get
            {
                /// Made thread safe.
                lock (providerLock)
                {
                    if (_providers == null)
                    {
                        if (Children.Count == 0)
                            Children["SqlAuditProvider"] = new AuditProviderElement(new SqlAuditProvider());

                        _providers = new ConcurrentDictionary<string, AuditProviderElement>();
                        foreach (var element in Children)
                            _providers[element.Key.ToLowerInvariant()] = new AuditProviderElement(element.Value, this);
                    }
                }

                return _providers;
            }
        }

        private ConcurrentDictionary<string, AuditProviderElement> _providers;

        /// <summary>
        /// Gets a list of providers that should receive audit messages
        /// </summary>
        /// <param name="moduleName">The name of the module that is to receive the audit messages</param>
        /// <returns>the list of providers</returns>
        public List<IAuditProvider> GetProvidersByModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
                throw new ArgumentException("argument is null or empty", nameof(moduleName));

            moduleName = moduleName.ToUpperInvariant();

            lock (_moduleMap)
            {
                if (_moduleMap.ContainsKey(moduleName)) return _moduleMap[moduleName];
            }


            var list = new List<IAuditProvider>();
            var providers = Providers;

            foreach (var provider in providers.Values)
            {
                if (provider.Modules.Count == 0) list.Add(provider.Provider);
                else if (provider.Modules.Contains(moduleName)) list.Add(provider.Provider);
            }

            lock (_moduleMap)
            {
                if (list.Count > 0) _moduleMap[moduleName] = list;
            }

            return list;
        }

        private readonly Dictionary<string, List<IAuditProvider>> _moduleMap = new Dictionary<string, List<IAuditProvider>>();
    }
}

