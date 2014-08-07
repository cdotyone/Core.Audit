using System;
using System.Collections.Generic;
using System.Linq;
using Civic.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Civic.Core.Audit
{
    public static class AuditManager
    {
        private static IAuditLogger _current;

        public static IAuditLogger Current
        { 
            get { return _current ?? (_current = new AuditLogger()); }
        }

        public static string LogModify<T>(string who, string clientMachine, string schema, string entityKeys, T from, T to)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, null, null, "MOD", from, to);
        }

        public static string LogRemove<T>(string who, string clientMachine, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, null, null, "DEL", from, null);
        }

        public static string LogAdd<T>(string who, string clientMachine, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, null, null, "ADD", null, from);
        }

        public static string LogAccess<T>(string who, string clientMachine, string schema, string entityKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, null, null, "ACC", from, null);
        }

        public static string LogModify<T>(string who, string clientMachine, string schema, string entityKeys, Type relatedType, string relatedKeys, T from, T to)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(string who, string clientMachine, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(string who, string clientMachine, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(string who, string clientMachine, string schema, string entityKeys, Type relatedType, string relatedKeys, T from)
        {
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ACC", from, null);
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

        public static string LogChange(string who, string clientMachine, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, object before, object after)
        {
            try
            {
                var logger = Current;
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

                if (!when.HasValue) when = DateTime.UtcNow;

                return logger.LogChange(who, when.Value, clientMachine, schema, entityCode, entityKeys, relatedEntityCode, relatedEntityKeys, action, dictBefore, dictAfter);
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }

            return null;
        }

        public static void MarkSuccessFul(string id)
        {
            try
            {
                var logger = Current;
                logger.MarkSuccessFul(id);
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }
        }

        public static void MarkSuccessFul(IEnumerable<string> ids)
        {
            try
            {
                var logger = Current;
                logger.MarkSuccessFul(ids);
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }
        }
    }
}
