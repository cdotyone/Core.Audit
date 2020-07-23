using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Core.Audit.Configuration;
using Core.Logging;
using Core.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Core.Audit
{
    public static class AuditManager
    {
        #region claims
        [Obsolete("Remove Schema Parameter")]

        public static string LogModify<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to, trackingID);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null as T, trackingID);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null as T, from, trackingID);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string schema, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null as T, trackingID);
        }

        #endregion claims

        #region claims - no schema

        public static string LogModify<T>(ClaimsPrincipal who, string module, string entityKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string entityKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, null, null, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string entityKeys, Type relatedType, string relatedKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string entityKeys, Type relatedType, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedType.Name, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null, from);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null);
        }

        public static string LogModify<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from, T to, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "MOD", from, to, trackingID);
        }

        public static string LogRemove<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "DEL", from, null, trackingID);
        }

        public static string LogAdd<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ADD", null, from, trackingID);
        }

        public static string LogAccess<T>(ClaimsPrincipal who, string module, string entityKeys, string relatedEntityCode, string relatedKeys, T from, string trackingID) where T : class
        {
            return LogChange(who, module, typeof(T).Name, entityKeys, relatedEntityCode, relatedKeys, "ACC", from, null, trackingID);
        }

        #endregion claims

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

            string jsonBeforeText = JsonConvert.SerializeObject(before, settings);
            string jsonAfterText = JsonConvert.SerializeObject(after, settings);

            var jsonBefore = JsonConvert.DeserializeObject<JObject>(jsonBeforeText);
            var jsonAfter = JsonConvert.DeserializeObject<JObject>(jsonAfterText);

            foreach (var kv in jsonBefore)
            {
                if (kv.Key.ToLowerInvariant() == "modified" || !jsonAfter.TryGetValue(kv.Key, out var secondValue)) continue;
                if (kv.Value.ToString() != secondValue.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        public static string LogChange<T>(ClaimsPrincipal who, string module, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, T before, T after, string trackingID = null)
        {
            string ouid = IdentityManager.GetClaimValue(who, StandardClaimTypes.ORGANIZATION_ID);
            string whoName = IdentityManager.GetUsername(who);
            string clientMachine = IdentityManager.GetClaimValue(who, StandardClaimTypes.CLIENT_IP);

            return LogChange(ouid, whoName, clientMachine, module, entityCode, entityKeys, relatedEntityCode, relatedEntityKeys, action, before, after, trackingID);
        }

        internal static string LogChange<T>(string ouid, string who, string clientMachine, string module, string entityCode, string entityKeys, string relatedEntityCode, string relatedEntityKeys, string action, T before, T after, string trackingID = null)
        {
            try
            {
                if (string.IsNullOrEmpty(trackingID)) trackingID = Guid.NewGuid().ToString().Replace("-", "");

                string jsonBeforeText = null;
                string jsonAfterText = null;
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
                    jsonBeforeText = JsonConvert.SerializeObject(before, settings);
                }
                if (after != null)
                {
                    jsonAfterText = JsonConvert.SerializeObject(after, settings);
                }

                if (before != null && after != null)
                {
                    var jsonBefore = JsonConvert.DeserializeObject<JObject>(jsonBeforeText);
                    var jsonAfter = JsonConvert.DeserializeObject<JObject>(jsonAfterText);

                    var difference = AuditManager.difference(jsonBefore, jsonAfter, ref when);

                    dictAfter = difference;
                    dictBefore = difference.Keys.ToDictionary(key => key, key => jsonBefore[key].ToString());
                }
                else
                {
                    if (before != null)
                    {
                        var jsonBefore = JsonConvert.DeserializeObject<JObject>(jsonBeforeText);
                        dictBefore = new Dictionary<string, string>();
                        foreach (var kv in jsonBefore)
                        {
                            dictBefore.Add(kv.Key, kv.Value.ToString());

                            var key = kv.Key.ToLowerInvariant();
                            if (key == "modified" || (key == "created" && !when.HasValue))
                            {
                                if (DateTime.TryParse(kv.Value.ToString(), out var date))
                                    when = date;
                            }
                        }
                    }
                    if (after != null)
                    {
                        var jsonAfter = JsonConvert.DeserializeObject<JObject>(jsonAfterText);

                        dictAfter = new Dictionary<string, string>();
                        foreach (var kv in jsonAfter)
                        {
                            dictAfter.Add(kv.Key, kv.Value.ToString());

                            var key = kv.Key.ToLowerInvariant();
                            if (key != "modified" && (key != "created" || when.HasValue)) continue;
                            if (DateTime.TryParse(kv.Value.ToString(), out var date))
                                when = date;
                        }
                    }
                }

                if (!when.HasValue || when.Value == DateTime.MinValue || when.Value == DateTime.MaxValue || (DateTime.UtcNow - when.Value).TotalSeconds > 5) when = DateTime.UtcNow;

                T entity = after == null ? before : after;

                var providers = AuditConfig.Current.GetProvidersByModule(module);
                foreach (var provider in providers)
                {
                    var when2 = when;

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (provider.Configuration is AuditProviderElement config && (when2.HasValue && when2.Value.Kind == DateTimeKind.Utc && config.UseLocalTime))
                        when2 = when.Value.ToLocalTime();

                    return provider.LogChange(module, trackingID, ouid, who, when2.Value, clientMachine, entityCode, entityKeys, relatedEntityCode, relatedEntityKeys, action, dictBefore, dictAfter, entity);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }

            return null;
        }

        public static Dictionary<string, string> Difference<T>(T before, T after)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            string jsonBeforeText = JsonConvert.SerializeObject(before, settings);
            string jsonAfterText = JsonConvert.SerializeObject(after, settings);

            var jsonBefore = JsonConvert.DeserializeObject<JObject>(jsonBeforeText);
            var jsonAfter = JsonConvert.DeserializeObject<JObject>(jsonAfterText);

            DateTime? when = new DateTime?();

            return difference(jsonBefore, jsonAfter, ref when);
        }

        private static Dictionary<string, string> difference(JObject before, JObject after, ref DateTime? when)
        {
            var difference = new Dictionary<string, string>();
            foreach (var kv in before)
            {
                if (!after.TryGetValue(kv.Key, out var secondValue)) continue;

                var key = kv.Key.ToLowerInvariant();
                if (key == "modified" || (key == "created" && !when.HasValue))
                {
                    if (DateTime.TryParse(secondValue.ToString(), out var date))
                        when = date;
                }

                if (kv.Value.ToString() != secondValue.ToString())
                {
                    difference.Add(kv.Key, secondValue.ToString());
                }
            }

            return difference;
        }

        public static void MarkSuccessFul<T>(ClaimsPrincipal who, string module, string trackingID, string entityKey, T entity)
        {
            string ouid = IdentityManager.GetClaimValue(who, StandardClaimTypes.ORGANIZATION_ID);
            string whoName = IdentityManager.GetUsername(who);

            try
            {
                var providers = AuditConfig.Current.GetProvidersByModule(module);
                foreach (var provider in providers)
                {
                    provider.MarkSuccessFul(module, trackingID, ouid, whoName, entityKey, entity);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex))
                    throw;
            }
        }

        public static void MarkSuccessFul(string module, string trackingID, string entityKey)
        {
            try
            {
                var providers = AuditConfig.Current.GetProvidersByModule(module);
                foreach (var provider in providers)
                {
                    provider.MarkSuccessFul(module, trackingID, null, null, entityKey, null as object);
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
