using System;
using System.Collections.Generic;
using System.Linq;
using Civic.Core.Audit.Configuration;
using Civic.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Civic.Core.Audit
{
    public static class AuditManager
    {
        public static string LogModify<T>(string who, string clientMachine, string module, string schema, string entityKeys, T from, T to)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, null, null, "MOD", from, to);
        }

        public static string LogRemove<T>(string who, string clientMachine, string module, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, null, null, "DEL", from, null);
        }

        public static string LogAdd<T>(string who, string clientMachine, string module, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, null, null, "ADD", null, from);
        }

        public static string LogAccess<T>(string who, string clientMachine, string module, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, null, null, "ACC", from, null);
        }

        public static string LogModify<T>(string who, string clientMachine, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from, T to)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(string who, string clientMachine, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(string who, string clientMachine, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(string who, string clientMachine, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to, string trackingID)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to, trackingID);
        }

        public static string LogRemove<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null, trackingID);
        }

        public static string LogAdd<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null, from, trackingID);
        }

        public static string LogAccess<T>(string who, string clientMachine, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID)
        {
            return LogChange(who, clientMachine, module, schema, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null, trackingID);
        }

        public static bool HasChanged(object before, object after)
        {
            if ((before == null && after != null) || (before != null && after == null)) return true;
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (after == null && before == null) return false;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            string jsonBefore = JsonConvert.SerializeObject(before, settings);
            string jsonAfter = JsonConvert.SerializeObject(after, settings);

            var jbefore = JsonConvert.DeserializeObject<JObject>(jsonBefore);
            var jafter = JsonConvert.DeserializeObject<JObject>(jsonAfter);

            foreach (var kv in jbefore)
            {
                JToken secondValue;
                if (kv.Key.ToLowerInvariant() == "modified" || !jafter.TryGetValue(kv.Key, out secondValue)) continue;
                if (kv.Value.ToString() != secondValue.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        public static string LogChange(string who, string clientMachine, string module, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, object before, object after, string trackingID = null)
        {
            try
            {
                if (string.IsNullOrEmpty(trackingID)) trackingID = Guid.NewGuid().ToString();

                string jsonBefore = null;
                string jsonAfter = null;
                var dictBefore = new Dictionary<string, string>();
                var dictAfter = new Dictionary<string, string>();
                var when = new DateTime?();

                var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };

                if (before != null)
                {
                    jsonBefore = JsonConvert.SerializeObject(before, settings);
                }
                if (after != null)
                {
                    jsonAfter = JsonConvert.SerializeObject(after, settings);
                }

                if (before != null && after != null)
                {
                    var jbefore = JsonConvert.DeserializeObject<JObject>(jsonBefore);
                    var jafter = JsonConvert.DeserializeObject<JObject>(jsonAfter);

                    var difference = new Dictionary<string, string>();
                    foreach (var kv in jbefore)
                    {
                        JToken secondValue;
                        if (!jafter.TryGetValue(kv.Key, out secondValue)) continue;

                        var key = kv.Key.ToLowerInvariant();
                        if (key == "modified" || (key=="created" && !when.HasValue))
                        {
                            DateTime tdate;
                            if (DateTime.TryParse(secondValue.ToString(), out tdate))
                                when = tdate;
                        }

                        if (kv.Value.ToString() != secondValue.ToString())
                        {
                            difference.Add(kv.Key, secondValue.ToString());
                        }
                    }

                    dictAfter = difference;
                    dictBefore = difference.Keys.ToDictionary(key => key, key => jbefore[key].ToString());
                }
                else
                {
                    if (before != null)
                    {
                        var jbefore = JsonConvert.DeserializeObject<JObject>(jsonBefore);
                        dictBefore = new Dictionary<string, string>();
                        foreach (var kv in jbefore)
                        {
                            dictBefore.Add(kv.Key,kv.Value.ToString());

                            var key = kv.Key.ToLowerInvariant();
                            if (key == "modified" || (key == "created" && !when.HasValue))
                            {
                                DateTime tdate;
                                if (DateTime.TryParse(kv.Value.ToString(), out tdate))
                                    when = tdate;
                            }
                        } 
                    }
                    if (after != null)
                    {
                        var jafter = JsonConvert.DeserializeObject<JObject>(jsonAfter);

                        dictAfter = new Dictionary<string, string>();
                        foreach (var kv in jafter)
                        {
                            dictAfter.Add(kv.Key, kv.Value.ToString());

                            var key = kv.Key.ToLowerInvariant();
                            if (key == "modified" || (key == "created" && !when.HasValue))
                            {
                                DateTime tdate;
                                if (DateTime.TryParse(kv.Value.ToString(), out tdate))
                                    when = tdate;
                            }
                        } 
                    }
                }

                if (!when.HasValue || when.Value == DateTime.MinValue || when.Value == DateTime.MaxValue || (DateTime.UtcNow-when.Value).TotalSeconds>5) when = DateTime.UtcNow;

                var providers = AuditConfig.Current.GetProvidersByModule(module);
                foreach (var provider in providers)
                {
                    var pwhen = when;
                    var config = provider.Configuration as AuditProviderElement;

                    if (config != null && (pwhen.HasValue && pwhen.Value.Kind == DateTimeKind.Utc && config.UseLocalTime))
                        pwhen = when.Value.ToLocalTime();

                    return provider.LogChange( module, trackingID, who, pwhen.Value, clientMachine, schema, entityCode, entityKeys, relatedEntityCode, relatedEntityKeys, action, dictBefore, dictAfter);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }

            return null;
        }

        public static void MarkSuccessFul(string module, string trackingID, string entityKey)
        {
            try
            {
                var providers = AuditConfig.Current.GetProvidersByModule(module);
                foreach (var provider in providers)
                {
                    provider.MarkSuccessFul(module, trackingID, entityKey);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }
        }

        public static void MarkSuccessFul(string module, string trackingID)
        {
            MarkSuccessFul(module, trackingID, null);
        }
    }
}
