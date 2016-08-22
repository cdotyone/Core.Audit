﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Civic.Core.Data;
using Newtonsoft.Json;

namespace Civic.Core.Audit
{
    public class AuditData
    {

        public static AuditLog GetAuditLog(Int32 id, IDBConnection database)
        {
            Debug.Assert(database != null);
            var AuditLogReturned = new AuditLog();

            using (var command = database.CreateStoredProcCommand("civic", "usp_AuditLogGet"))
            {
                command.AddInParameter("@id", id);
                command.ExecuteReader(dataReader =>
                    {
                        if (populateAuditLog(AuditLogReturned, dataReader))
                        {
                            AuditLogReturned.ID = id;
                        }
                        else AuditLogReturned = null;
                    });
            }

            return AuditLogReturned;
        }

        public static List<AuditLog> GetPagedAuditLog(int skip, ref int count, bool retCount, string filterBy, string orderBy, IDBConnection database)
        {
            Debug.Assert(database != null);
            var list = new List<AuditLog>();

            using (var command = database.CreateStoredProcCommand("civic", "usp_AuditLogGetFiltered"))
            {
                command.AddInParameter("@skip", skip);
                command.AddInParameter("@retcount", retCount);
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
            using (var command = database.CreateStoredProcCommand("civic", "usp_AuditLogAdd"))
            {
                buildAuditLogCommandParameters(auditLog, command, useLocalTime, true);
                command.ExecuteNonQuery();
                return
               auditLog.ID = Int32.Parse(
               command.GetOutParameter("@id").Value.ToString());
            }
        }

        private static void buildAuditLogCommandParameters(AuditLog auditLog, IDBCommand command, bool useLocalTime, bool addRecord)
        {
            if (addRecord) command.AddParameter("@id", ParameterDirection.InputOutput, auditLog.ID);
            else command.AddInParameter("@id", auditLog.ID);
            command.AddInParameter("@trackingUID", auditLog.TrackingUID);
            command.AddInParameter("@entitycode", auditLog.EntityCode);
            command.AddInParameter("@entitykeys", auditLog.EntityKeys);
            command.AddInParameter("@clientMachine", auditLog.ClientMachine);
            command.AddInParameter("@relatedentitycode", auditLog.RelatedEntityCode);
            command.AddInParameter("@relatedentitykeys", auditLog.RelatedEntityKeys);
            command.AddInParameter("@action", auditLog.Action);
            if(auditLog.Created.HasValue) command.AddInParameter("@created", auditLog.Created);   
            else command.AddInParameter("@created", useLocalTime ? DateTime.Now : DateTime.UtcNow);
            command.AddInParameter("@success", auditLog.Success ? "Y" : "N");
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
            
            string before = dataReader["Before"] != null && !string.IsNullOrEmpty(dataReader["Before"].ToString()) ? dataReader["Before"].ToString() : string.Empty;
            string after = dataReader["After"] != null && !string.IsNullOrEmpty(dataReader["After"].ToString()) ? dataReader["After"].ToString() : string.Empty;

            var format = new CultureInfo("en-US").DateTimeFormat;
            auditLog.Success = dataReader["Success"] != null && !(dataReader["Success"] is DBNull) && dataReader["Success"].ToString()=="Y";
            if (!(dataReader["Created"] is DBNull)) auditLog.Created = DateTime.Parse(dataReader["Created"].ToString(), format, DateTimeStyles.AssumeLocal);
            if (!(dataReader["Recorded"] is DBNull)) auditLog.Recorded = DateTime.Parse(dataReader["Recorded"].ToString(), format, DateTimeStyles.AssumeLocal);
            auditLog.CreatedBy = dataReader["CreatedBy"] != null && !string.IsNullOrEmpty(dataReader["CreatedBy"].ToString()) ? dataReader["CreatedBy"].ToString() : string.Empty;

            if (!string.IsNullOrEmpty(before)) auditLog.Before = JsonConvert.DeserializeObject<Dictionary<string,string>>(before);
            if (!string.IsNullOrEmpty(after)) auditLog.After = JsonConvert.DeserializeObject<Dictionary<string, string>>(after);

            return true;
        }

        public static void MarkAuditLogSuccessFul(string trackingUID, string enityKey, IDBConnection database)
        {
            Debug.Assert(database != null);
            using (var command = database.CreateStoredProcCommand("civic", "usp_AuditLogMarkSuccessfulAdd"))
            {
                command.AddInParameter("@trackingUID", trackingUID);
                command.AddInParameter("@enityKey", enityKey);
                command.ExecuteNonQuery();
            }
        }
    }
}
