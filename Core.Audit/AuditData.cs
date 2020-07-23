using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Core.Data;
using Core.Logging;
using Newtonsoft.Json;
using IDbCommand = Core.Data.IDbCommand;

namespace Core.Audit
{
    public class AuditData
    {

        public static AuditLog GetAuditLog(int id, IDBConnection database)
        {
            Debug.Assert(database != null);
            var auditLogReturned = new AuditLog();

            using (var command = database.CreateStoredProcCommand("core", "usp_AuditLogGet"))
            {
                command.AddInParameter("@id", id);
                command.ExecuteReader(dataReader =>
                    {
                        if (populateAuditLog(auditLogReturned, dataReader))
                        {
                            auditLogReturned.ID = id;
                        }
                        else auditLogReturned = null;
                    });
            }

            return auditLogReturned;
        }

        public static List<AuditLog> GetPagedAuditLog(int skip, ref int count, bool retCount, string filterBy, string orderBy, IDBConnection database)
        {
            Debug.Assert(database != null);
            var list = new List<AuditLog>();

            using (var command = database.CreateStoredProcCommand("core", "usp_AuditLogGetFiltered"))
            {
                command.AddInParameter("@skip", skip);
                command.AddInParameter("@retCount", retCount);
                if (!string.IsNullOrEmpty(filterBy)) command.AddInParameter("@filterBy", filterBy);
                command.AddInParameter("@orderBy", orderBy);
                command.AddParameter("@count", ParameterDirection.InputOutput, count);

                command.ExecuteReader(dataReader =>
                {
                    var item = new AuditLog();
                    while (populateAuditLog(item, dataReader))
                    {
                        list.Add(item);
                        item = new AuditLog();
                    }
                });

                if (retCount) count = int.Parse(command.GetOutParameter("@count").Value.ToString());
            }

            return list;
        }

        public static int AddAuditLog(AuditLog auditLog, IDBConnection database, bool useLocalTime)
        {
            Debug.Assert(database != null);
            using (var command = database.CreateStoredProcCommand("core", "usp_AuditLogAdd"))
            {
                buildAuditLogCommandParameters(auditLog, command, useLocalTime, true);
                command.ExecuteNonQuery();
                auditLog.ID = Int32.Parse(command.GetOutParameter("@id").Value.ToString());
                return auditLog.ID.Value;
            }
        }

        private static void buildAuditLogCommandParameters(AuditLog auditLog, IDbCommand command, bool useLocalTime, bool addRecord)
        {
            if (addRecord) command.AddParameter("@id", ParameterDirection.InputOutput, auditLog.ID);
            else command.AddInParameter("@id", auditLog.ID);
            command.AddInParameter("@trackingUID", auditLog.TrackingUID);
            command.AddInParameter("@entityCode", auditLog.EntityCode);
            command.AddInParameter("@entityKeys", auditLog.EntityKeys);
            command.AddInParameter("@clientMachine", auditLog.ClientMachine);
            command.AddInParameter("@relatedEntityCode", auditLog.RelatedEntityCode);
            command.AddInParameter("@relatedEntityKeys", auditLog.RelatedEntityKeys);
            command.AddInParameter("@action", auditLog.Action);
            if(auditLog.Created.HasValue) command.AddInParameter("@created", auditLog.Created);   
            else command.AddInParameter("@created", useLocalTime ? DateTime.Now : DateTime.UtcNow);
            command.AddInParameter("@success", auditLog.Success ? "Y" : "N");
            command.AddInParameter("@processed", auditLog.Processed);
            command.AddInParameter("@createdBy", auditLog.CreatedBy);

            if (auditLog.Before == null || auditLog.Before.Count == 0) command.AddInParameter("@before", null);
            else command.AddInParameter("@before", JsonConvert.SerializeObject(auditLog.Before));
            if (auditLog.After == null || auditLog.After.Count == 0) command.AddInParameter("@after", null);
            else command.AddInParameter("@after", JsonConvert.SerializeObject(auditLog.After));
        }

        private static bool populateAuditLog(AuditLog auditLog, IDataReader dataReader)
        {
            if (dataReader == null || !dataReader.Read()) return false;

            auditLog.ID = dataReader["ID"] != null && !(dataReader["ID"] is DBNull) ? Int32.Parse(dataReader["ID"].ToString()) : 0;
            auditLog.TrackingUID = dataReader["TrackingUID"] != null && !string.IsNullOrEmpty(dataReader["TrackingUID"].ToString()) ? dataReader["TrackingUID"].ToString() : string.Empty;
            auditLog.EntityCode = dataReader["EntityCode"] != null && !string.IsNullOrEmpty(dataReader["EntityCode"].ToString()) ? dataReader["EntityCode"].ToString() : string.Empty;
            auditLog.EntityKeys = dataReader["EntityKeys"] != null && !string.IsNullOrEmpty(dataReader["EntityKeys"].ToString()) ? dataReader["EntityKeys"].ToString() : string.Empty;
            auditLog.RelatedEntityCode = dataReader["RelatedEntityCode"] != null && !string.IsNullOrEmpty(dataReader["RelatedEntityCode"].ToString()) ? dataReader["RelatedEntityCode"].ToString() : string.Empty;
            auditLog.RelatedEntityKeys = dataReader["RelatedEntityKeys"] != null && !string.IsNullOrEmpty(dataReader["RelatedEntityKeys"].ToString()) ? dataReader["RelatedEntityKeys"].ToString() : string.Empty;
            auditLog.Action = dataReader["Action"] != null && !string.IsNullOrEmpty(dataReader["Action"].ToString()) ? dataReader["Action"].ToString() : string.Empty;
            auditLog.ClientMachine = dataReader["ClientMachine"] != null && !string.IsNullOrEmpty(dataReader["ClientMachine"].ToString()) ? dataReader["ClientMachine"].ToString() : string.Empty;
            
            var format = new CultureInfo("en-US").DateTimeFormat;
            auditLog.Success = dataReader["Success"] != null && !(dataReader["Success"] is DBNull) && dataReader["Success"].ToString()=="Y";
            if (!(dataReader["Created"] is DBNull)) auditLog.Created = DateTime.Parse(dataReader["Created"].ToString(), format, DateTimeStyles.AssumeLocal);
            if (!(dataReader["Recorded"] is DBNull)) auditLog.Recorded = DateTime.Parse(dataReader["Recorded"].ToString(), format, DateTimeStyles.AssumeLocal);
            auditLog.CreatedBy = dataReader["CreatedBy"] != null && !string.IsNullOrEmpty(dataReader["CreatedBy"].ToString()) ? dataReader["CreatedBy"].ToString() : string.Empty;

            try
            {
                if (dataReader["Before"] != null)
                    auditLog.Before = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataReader["Before"].ToString());
            }
            catch
            {
                Logger.LogError(LoggingBoundaries.DataLayer, "Audit.EntityCode={0} , Audit.EntityCode={1} : Failed to parse before json: {2}", auditLog.EntityCode, auditLog.EntityKeys, dataReader["Before"].ToString());
            }
            try
            {
                if (dataReader["After"] != null)
                    auditLog.After = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataReader["After"].ToString());
            }
            catch
            {
                Logger.LogError(LoggingBoundaries.DataLayer, "Audit.EntityCode={0} , Audit.EntityCode={1} : Failed to parse after json: {2}", auditLog.EntityCode, auditLog.EntityKeys, dataReader["After"].ToString());
            }

            return true;
        }

        public static void MarkAuditLogSuccessFul(string trackingUID, string entityKey, IDBConnection database)
        {
            Debug.Assert(database != null);
            using (var command = database.CreateStoredProcCommand("core", "usp_AuditLogMarkSuccessfulAdd"))
            {
                command.AddInParameter("@trackingUID", trackingUID);
                command.AddInParameter("@entityKey", entityKey);
                command.ExecuteNonQuery();
            }
        }
    }
}

