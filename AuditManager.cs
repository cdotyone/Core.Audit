﻿using System;
using System.Collections.Generic;
using System.Linq;
using Civic.Core.Logging;
using Newtonsoft.Json;
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
                    jsonBefore = safeEncodeComplexProperties(JsonConvert.SerializeObject(before, settings));
                }
                if (after != null && !(after is Dictionary<string, string>))
                {
                    jsonAfter = safeEncodeComplexProperties(JsonConvert.SerializeObject(after, settings));
                }

                if (before != null && after != null)
                {
                    if (!(before is Dictionary<string, string>))
                        dictBefore = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonBefore);
                    else
                        dictBefore = before as Dictionary<string, string>;

                    if (!(after is Dictionary<string, string>))
                        dictAfter = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAfter);
                    else
                        dictAfter = after as Dictionary<string, string>;

                    var difference = new Dictionary<string, string>();
                    foreach (var kv in dictBefore)
                    {
                        string secondValue;
                        if (dictAfter.TryGetValue(kv.Key, out secondValue))
                        {
                            if (kv.Value != secondValue)
                            {
                                difference.Add(kv.Key, secondValue);
                            }
                        }
                    }
                    dictAfter = difference;

                    dictBefore = dictAfter.Keys.ToDictionary(key => key, key => dictBefore[key]);
                }
                else
                {
                    if (before != null)
                    {
                        if (!(before is Dictionary<string, string>))
                            dictBefore = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonBefore);
                        else
                            dictBefore = before as Dictionary<string, string>;

                        dictBefore = (from kv in dictBefore where kv.Value != null select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
                    }
                    if (after != null)
                    {
                        if (!(after is Dictionary<string, string>))
                            dictAfter = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAfter);
                        else
                            dictAfter = after as Dictionary<string, string>;

                        dictAfter = (from kv in dictAfter where kv.Value != null select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
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

        private static string safeEncodeComplexProperties(string inp)
        {
            string result = inp.Trim(new[] {'{','}'});

            int pos = 0,pos2;
            while (pos < result.Length && result.Substring(pos).Contains("{"))
            {
                pos += result.IndexOf('{', pos);
                pos2 = result.IndexOf('}', pos);

                result = result.Substring(0, pos) +
                         result.Substring(pos, pos2 - pos + 1).Replace("\"", "\\\"").Replace("{", "\"{").Replace("}", "}\"") +
                         result.Substring(pos2 + 1);
                           
                pos = pos2;
            }

            return "{" + result + "}";
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
