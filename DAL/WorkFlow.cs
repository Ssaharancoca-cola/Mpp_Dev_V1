using DAL.Common;
using Model.Models;
using Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class WorkFlow : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public DataSet GetSubmittedRecords(string selectCommand, out string outMsg)
        {
            var result = new DataSet();
            outMsg = Constant.statusSuccess;
            using (var context = new MPP_Context())
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = selectCommand;
                try
                {
                    context.Database.OpenConnection();
                    var dataTable = new DataTable();

                    using var reader = command.ExecuteReader();
                    do
                    {
                        dataTable.Load((IDataReader)reader);
                        result.Tables.Add(dataTable);
                    } while (!reader.IsClosed);
                }
                catch (Exception ex)
                {
                    using LogError objLogError = new LogError();
                    objLogError.LogErrorInTextFile(ex);

                    outMsg = ex.Message;
                }
                finally
                {
                    context.Database.CloseConnection();
                }
                return result;
            }
            }
        public string UpdateStatus(string InputRowIds, string UserId, string Status, string Comments, int entityTypeid)
        {
            int SessionID = 0;
            int result = 0;
            string outMsg = Constant.statusSuccess;
            DbTransaction transaction = null;
            // string SessionID = string.Empty;
            string tableName = string.Empty;
            string errorMessage = "";
            StringBuilder finalQuery = new StringBuilder();
            try
            {
                using (GetSessionValue objGetSessionValue = new GetSessionValue())
                {
                    SessionID = objGetSessionValue.GetNextSessionValue(out outMsg);
                }
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    tableName = objMPP_Context.EntityType.Where(x => x.Id == entityTypeid).Select(x => x.InputTableName).FirstOrDefault();
                }

                if (Status.ToUpper() == Constant.ABANDON)
                {
                    string command = "UPDATE MPP_APP." + tableName + " SET ROW_STATUS = 5, SESSION_ID = " + SessionID + ", COMMENTS = CASE WHEN INPUT_ROW_ID IN (" + InputRowIds + ") THEN '" + Comments + "' ELSE COMMENTS END WHERE LD_OID IN(SELECT LD_OID FROM MPP_APP." + tableName + " WHERE INPUT_ROW_ID IN(" + InputRowIds + ")) AND User_Id = '" + UserId + "'";
                    finalQuery.Append(command + ";");                  
                }
                else if (Status.ToUpper() == Constant.DELETE)
                {
                    string command = "UPDATE MPP_APP." + tableName + " SET ROW_STATUS = 7, SESSION_ID = " + SessionID + " WHERE LD_OID IN(SELECT LD_OID FROM MPP_APP." + tableName + " WHERE INPUT_ROW_ID IN(" + InputRowIds + ")) AND User_Id = '" + UserId + "'";
                    finalQuery.Append(command);

                }
                else if (Status.ToUpper() == Constant.REJECT)
                {
                    string command = "UPDATE MPP_APP." + tableName + " SET ROW_STATUS = 4, SESSION_ID = " + SessionID + ", COMMENTS = CASE WHEN INPUT_ROW_ID IN (" + InputRowIds + ") THEN '" + Comments + "' ELSE COMMENTS END, APPROVER_STATUS = CASE WHEN INPUT_ROW_ID IN (" + InputRowIds + ") THEN 'REJECTED' ELSE APPROVER_STATUS END WHERE LD_OID IN(SELECT LD_OID FROM MPP_APP." + tableName + " WHERE INPUT_ROW_ID IN(" + InputRowIds + "))";
                    finalQuery.Append(command + ";");
                   
                }
                else if (Status.ToUpper() == Constant.APPROVE)
                {
                    string command = "UPDATE MPP_APP." + tableName + " SET SESSION_ID = " + SessionID + ", COMMENTS = CASE WHEN INPUT_ROW_ID IN(" + InputRowIds + ") THEN '" + Comments + "' ELSE COMMENTS END, APPROVER_STATUS = CASE WHEN INPUT_ROW_ID IN(" + InputRowIds + ") THEN 'APPROVED' ELSE APPROVER_STATUS END WHERE LD_OID IN(SELECT LD_OID FROM MPP_APP." + tableName + " WHERE INPUT_ROW_ID IN(" + InputRowIds + "))";
                    finalQuery.Append(command);
                }
                if (Status.ToUpper() == Constant.REJECT || Status.ToUpper() == Constant.ABANDON)
                {
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(finalQuery.ToString());
                    }
                }
                if (Status.ToUpper() == Constant.APPROVE || Status.ToUpper() == Constant.DELETE)
                {
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        using (var dbContextTransaction = objMPP_Context.Database.BeginTransaction())
                        {
                            try
                            {
                                int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(finalQuery.ToString());
                                //Insertion to Temp table to resolve LD table stuck issue
                                String[] RowIds = InputRowIds.Split(',');
                                foreach (string Item in RowIds)
                                {
                                    string query = "INSERT INTO MPP_APP.INPUTROWID_LDSTUCK(SESSION_ID, INPUTROW_ID, ENTITY_TYPE_ID) VALUES('" + SessionID + "', '" + Item + "','" + entityTypeid + "')";
                                    int noOfRowInserted = objMPP_Context.Database.ExecuteSqlRaw(query);
                                }
                                //var viewnameParameter = new ObjectParameter("resultval", typeof(string));
                                OutputParameter<int?> resultval = new OutputParameter<int?>();
                                OutputParameter<int> returnValue = new OutputParameter<int>();
                                objMPP_Context.Procedures.GET_MPP_WORKFLOW_SAVEAsync(SessionID.ToString(), entityTypeid, UserId, Status, resultval, returnValue);
                                result = Convert.ToInt32(returnValue.Value);

                                if (result == 0)
                                {
                                    objMPP_Context.SaveChanges();
                                    dbContextTransaction.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                using (LogError objLogError = new LogError())
                                {
                                    objLogError.LogErrorInTextFile(ex);
                                }
                                dbContextTransaction.Rollback();
                                outMsg = ex.Message;
                            }
                        }
                    }
                }
                if (Status.ToUpper() == Constant.APPROVE)
                    DeleteRecords(Status, Convert.ToString(SessionID), entityTypeid, out outMsg);
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
                errorMessage = ex.Message;
                transaction.Rollback();

            }
            return outMsg;
        }
        public string GetPKCode(int entityTypeId, out string PK_CODES)
        {
            string outMsg = Constant.statusSuccess;
            PK_CODE Dm = new PK_CODE();
            try
            {
                string query = "SELECT STUFF(( SELECT ',' + attr_name FROM MPP_CORE.ENTITY_TYPE_ATTR WHERE IS_PART_OF_CODE = 1 AND ENTITY_TYPE_ID = '" + entityTypeId + "' FOR XML PATH('')), 1, 1, '') AS PK_CODES";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    Dm= objMPP_Context.Set<PK_CODE>().FromSqlRaw(query).FirstOrDefault();
                }
                PK_CODES = Dm.PK_CODES;
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                PK_CODES = string.Empty;
                outMsg = ex.Message;
            }
            return outMsg;
        }
        public DataSet GetExistingRecordList(int entityTypeId, string colName, string approverId, string status, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string pkCode = string.Empty;
            string tableName = string.Empty;
            string viewName = string.Empty;
            DataSet ds = new DataSet();
            try
            {
                GetPKCode(entityTypeId, out pkCode);
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(pkCode))
                    return ds;
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    tableName = objMPP_Context.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.InputTableName).FirstOrDefault();
                }
                using (GetViewDetail objviewdetail = new GetViewDetail())
                {
                    outMsg = objviewdetail.GetViewName(entityTypeId, approverId.ToUpper(), out viewName);
                }
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(viewName))
                    return ds;
                string query = "";
                if (viewName.Contains("31-DEC-2049"))
                {
                     query = "SELECT " + colName + " FROM  (" + viewName + ") AS Q  WHERE (Q." + pkCode + ") IN (SELECT " + pkCode + " FROM MPP_APP." + tableName + " WHERE APPROVER_ID ='" + approverId + "' AND APPROVER_STATUS='" + status + "' AND ROW_STATUS =3)";
                }
                else
                {
                     query = "SELECT " + colName + " FROM  " + viewName + "  WHERE (" + pkCode + ") IN (SELECT " + pkCode + " FROM MPP_APP." + tableName + " WHERE APPROVER_ID ='" + approverId + "' AND APPROVER_STATUS='" + status + "' AND ROW_STATUS =3)";
                }
                ds = GetSubmittedRecords(query, out outMsg);
                if (outMsg != Constant.statusSuccess)
                    return ds;

            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return ds;
        }

        public void DeleteRecords(string Status, string SessionId, int entityTypeId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            try
            {
                if (Status.ToUpper() == "APPROVE" || Status.ToUpper() == "DELETE")
                {
                    string deleteQuery = "DELETE FROM MPP_APP.INPUTROWID_LDSTUCK WHERE SESSION_ID='" + SessionId + "' AND ENTITY_TYPE_ID='" + entityTypeId + "'";
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        int noOfRowDeleted = objMPP_Context.Database.ExecuteSqlRaw(deleteQuery);
                    }
                }

            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }

        }

    }
}





