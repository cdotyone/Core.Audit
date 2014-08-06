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

        public static string LogChange(string who, string clientMachine, string schema, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, object before, object after)
        {
            try
            {
                var logger = Current;
                string jsonBefore = null;
                string jsonAfter = null;
                var dictBefore = new Dictionary<string, string>();
                var dictAfter = new Dictionary<string, string>();

                var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };

                if (before != null && !(before is Dictionary<string, string>))
                {
                    jsonBefore = JsonConvert.SerializeObject(before, settings);
                }
                if (after != null && !(after is Dictionary<string, string>))
                {
                    jsonAfter = JsonConvert.SerializeObject(after, settings);
                }

                if (before != null && after != null)
                {
                    JObject jbefore, jafter;

                    if (!(before is Dictionary<string, string>))
                        jbefore = JsonConvert.DeserializeObject<JObject>(jsonBefore);
                    else
                        jbefore = new JObject();

                    if (!(after is Dictionary<string, string>))
                        jafter = JsonConvert.DeserializeObject<JObject>(jsonAfter);
                    else
                        jafter = new JObject();

                    var difference = new Dictionary<string, string>();
                    foreach (var kv in jbefore)
                    {
                        JToken secondValue;
                        if (jafter.TryGetValue(kv.Key, out secondValue))
                        {
                            if (kv.Value != secondValue)
                            {
                                difference.Add(kv.Key, secondValue.ToString());
                            }
                        }
                    }

                    dictAfter = difference;
                    dictBefore = difference.Keys.ToDictionary(key => key, key => jbefore[key].ToString());
                }
                else
                {
                    if (before != null)
                    {
                        JObject jbefore;
                        if (!(before is Dictionary<string, string>))
                            jbefore = JsonConvert.DeserializeObject<JObject>(jsonBefore);
                        else
                            jbefore = new JObject();


                        dictBefore = new Dictionary<string, string>();
                        foreach (var kv in jbefore)
                        {
                            dictBefore.Add(kv.Key,kv.Value.ToString());
                        } 
                    }
                    if (after != null)
                    {
                        JObject jafter;
                        if (!(after is Dictionary<string, string>))
                            jafter = JsonConvert.DeserializeObject<JObject>(jsonAfter);
                        else
                            jafter = new JObject();

                        dictAfter = new Dictionary<string, string>();
                        foreach (var kv in jafter)
                        {
                            dictAfter.Add(kv.Key, kv.Value.ToString());
                        } 
                    }
                }

                return logger.LogChange(who, clientMachine, schema, entityCode, entityKeys, relatedEntityCode, relatedEntityKeys, action, dictBefore, dictAfter);
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
