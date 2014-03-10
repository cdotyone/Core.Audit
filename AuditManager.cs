using System;
using System.Collections.Generic;
using System.Linq;
using Civic.Core.Logging;
using Newtonsoft.Json;

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
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, null, null, "ADD", from, null);
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
            return LogChange(who, clientMachine, schema, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ADD", from, null);
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

                if(before!=null) jsonBefore = JsonConvert.SerializeObject(before);
                if(after!=null) jsonAfter = JsonConvert.SerializeObject(after);

                if (before != null && after != null)
                {
                    dictBefore = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonBefore);
                    dictAfter = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAfter);

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
                    if (before != null) dictBefore = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonBefore);
                    if (after != null) dictAfter = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAfter);
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
