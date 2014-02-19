using System;
using System.Collections.Generic;
using System.Data;
using Civic.Core.Data;

namespace Civic.Core.Audit
{
    public class AuditData
    {

        public static SystemEntityLog GetSystemEntityLog(Int32 id, string[] fillProperties = null)
        {
            var systemEntityLogReturned = new SystemEntityLog();

            var database = DatabaseFactory.CreateDatabase("Civic");
            using (var command = database.CreateStoredProcCommand("dbo", "usp_SystemEntityLogGet"))
            {
                command.AddInParameter("@id", id);

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    if (populateSystemEntityLog(systemEntityLogReturned, dataReader))
                    {
                        systemEntityLogReturned.ID = id;
                    }
                    else return null;
                }
            }

            return systemEntityLogReturned;
        }

        public static List<SystemEntityLog> GetPagedSystemEntityLog(int skip, ref int count, bool retCount, string filterBy, string orderBy, string[] fillProperties = null)
        {
            var list = new List<SystemEntityLog>();

            var database = DatabaseFactory.CreateDatabase("Civic");
            using (var command = database.CreateStoredProcCommand("dbo", string.IsNullOrEmpty(filterBy) ? "usp_SystemEntityLogGetPaged" : "usp_SystemEntityLogGetFiltered"))
            {
                command.AddInParameter("@skip", skip);
                command.AddInParameter("@retcount", retCount);
                if (!string.IsNullOrEmpty(filterBy)) command.AddInParameter("@filterBy", filterBy);
                command.AddInParameter("@orderBy", orderBy);
                command.AddParameter("@count", ParameterDirection.InputOutput, count);

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    var item = new SystemEntityLog();
                    while (populateSystemEntityLog(item, dataReader))
                    {
                        list.Add(item);
                        item = new SystemEntityLog();
                    }
                }

                if (retCount) count = int.Parse(command.GetOutParameter("@count").Value.ToString());
            }

            return list;
        }

        public static int AddSystemEntityLog(SystemEntityLog systemEntityLog)
        {
            var database = DatabaseFactory.CreateDatabase("Civic");
            using (var command = database.CreateStoredProcCommand("dbo", "usp_SystemEntityLogAdd"))
            {
                buildSystemEntityLogCommandParameters(systemEntityLog, command, true);
                command.ExecuteNonQuery();
                return
               systemEntityLog.ID = Int32.Parse(
               command.GetOutParameter("@id").Value.ToString());
            }
        }

        private static void buildSystemEntityLogCommandParameters(SystemEntityLog systemEntityLog, IDBCommand command, bool addRecord)
        {
            if (addRecord) command.AddParameter("@id", ParameterDirection.InputOutput, systemEntityLog.ID);
            else command.AddInParameter("@id", systemEntityLog.ID);
            command.AddInParameter("@entitycode", systemEntityLog.EntityCode);
            command.AddInParameter("@entitykeys", systemEntityLog.EntityKeys);
            command.AddInParameter("@relatedentitycode", systemEntityLog.RelatedEntityCode);
            command.AddInParameter("@relatedentitykeys", systemEntityLog.RelatedEntityKeys);
            command.AddInParameter("@action", systemEntityLog.Action);
            command.AddInParameter("@before", systemEntityLog.Before);
            command.AddInParameter("@after", systemEntityLog.After);
            command.AddInParameter("@success", systemEntityLog.Success);
            command.AddInParameter("@createdBy", systemEntityLog.CreatedBy);

        }

        private static bool populateSystemEntityLog(SystemEntityLog systemEntityLog, IDataReader dataReader)
        {
            if (dataReader == null || !dataReader.Read()) return false;

            systemEntityLog.ID = dataReader["ID"] != null && !(dataReader["ID"] is DBNull) ? Int32.Parse(dataReader["ID"].ToString()) : 0;
            systemEntityLog.EntityCode = dataReader["EntityCode"] != null && !string.IsNullOrEmpty(dataReader["EntityCode"].ToString()) ? dataReader["EntityCode"].ToString() : string.Empty;
            systemEntityLog.EntityKeys = dataReader["EntityKeys"] != null && !string.IsNullOrEmpty(dataReader["EntityKeys"].ToString()) ? dataReader["EntityKeys"].ToString() : string.Empty;
            systemEntityLog.RelatedEntityCode = dataReader["RelatedEntityCode"] != null && !string.IsNullOrEmpty(dataReader["RelatedEntityCode"].ToString()) ? dataReader["RelatedEntityCode"].ToString() : string.Empty;
            systemEntityLog.RelatedEntityKeys = dataReader["RelatedEntityKeys"] != null && !string.IsNullOrEmpty(dataReader["RelatedEntityKeys"].ToString()) ? dataReader["RelatedEntityKeys"].ToString() : string.Empty;
            systemEntityLog.Action = dataReader["Action"] != null && !string.IsNullOrEmpty(dataReader["Action"].ToString()) ? dataReader["Action"].ToString() : string.Empty;
            systemEntityLog.Before = dataReader["Before"] != null && !string.IsNullOrEmpty(dataReader["Before"].ToString()) ? dataReader["Before"].ToString() : string.Empty;
            systemEntityLog.After = dataReader["After"] != null && !string.IsNullOrEmpty(dataReader["After"].ToString()) ? dataReader["After"].ToString() : string.Empty;
            systemEntityLog.Success = dataReader["Success"] != null && !(dataReader["Success"] is DBNull) && Boolean.Parse(dataReader["Success"].ToString());
            if (!(dataReader["Created"] is DBNull)) systemEntityLog.Created = DateTime.Parse(dataReader["Created"].ToString());
            systemEntityLog.EntityCode = dataReader["CreatedBy"] != null && !string.IsNullOrEmpty(dataReader["CreatedBy"].ToString()) ? dataReader["CreatedBy"].ToString() : string.Empty;

            return true;
        }

        public static void MarkSystemEntityLogSuccessFul(IEnumerable<string> ids)
        {
            var database = DatabaseFactory.CreateDatabase("Civic");
            using (var command = database.CreateStoredProcCommand("dbo", "usp_SystemEntityLogMarkSuccessful"))
            {
                command.AddInParameter("@ids", string.Join(",", ids));
                command.ExecuteNonQuery();
            }
        }
    }
}

