using DAL.Common;
using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UpdateRecord : IDisposable
    {
        void IDisposable.Dispose()
        {

        }

        public DataSet GetDataSet(string selectCommand, out string outMsg)
        {
            var result = new DataSet();
            outMsg = Constant.statusSuccess;
            MPP_Context mPP_Context = new MPP_Context();

            using var command = mPP_Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = selectCommand;

            try
            {
                mPP_Context.Database.OpenConnection();
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
                mPP_Context.Database.CloseConnection();
            }

            return result;
        }

        public DataSet GetSelectedRecords(string OIDCode, bool isRowIdExists, int entityTypeId, string userName, out string columnData,
                   out List<string> rowData, out string outMsg, out List<Dictionary<string, string>> dataList)
        {
            #region varaibleDeclaration
            string[] tableName = null;
            DataSet ds = new DataSet();
            outMsg = Constant.statusSuccess;
            string viewName = string.Empty;
            string fieldList = string.Empty;
            string searchQuery = string.Empty;
            string OIDColumnName = string.Empty;
            columnData = string.Empty;
            rowData = new List<string>();
            List<EntityTypeAttr> attrNameList = new List<EntityTypeAttr>();
            List<EntityTypeData> objEntityTypeData = new List<EntityTypeData>();
            dataList = new List<Dictionary<string, string>>();
            #endregion varaibleDeclaration
            using (GetViewDetail objviewdetail = new GetViewDetail())
            {
                outMsg = objviewdetail.GetFieldList(entityTypeId, out fieldList, out attrNameList);
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(fieldList) || attrNameList.Count() == 0)
                    return ds;

                if (!isRowIdExists)
                {
                    outMsg = objviewdetail.GetViewName(entityTypeId, userName.ToUpper(), out viewName);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(viewName))
                        return ds;
                    else
                    {
                        viewName = viewName.Replace("FL_", "FNL_");

                        if (viewName.Length > 100)
                        {
                            string[] splitViewName = viewName.Split(' ');
                            int count = 0;
                            for (int i = 0; i < splitViewName.Length; i++)
                            {
                                if (splitViewName[i].Contains("MPP_APP"))
                                    count++;
                                if (count == 2)
                                {
                                    tableName = splitViewName[i].Split('.');
                                    break;
                                }
                            }
                        }
                        else
                        {
                            tableName = viewName.Split('.');
                        }
                    }



                    outMsg = objviewdetail.GetOIDColumnNameByEntityId(entityTypeId, out OIDColumnName);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(OIDColumnName))
                        return ds;

                    outMsg = GetSearchQuery(fieldList, viewName, OIDCode, OIDColumnName, attrNameList, out searchQuery);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(searchQuery))
                        return ds;
                }
                else
                {
                    string tablename = objviewdetail.GetTableName(entityTypeId, out outMsg);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(tablename))
                        return ds;

                    outMsg = GetSearchQuery(fieldList, tablename, OIDCode, attrNameList, out searchQuery);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(searchQuery))
                        return ds;
                }
                 ds = GetDataSet(searchQuery, out outMsg);
                
                if (outMsg != Constant.statusSuccess)
                    return ds;
                outMsg = GetDataFromDataSet(ds, entityTypeId, OIDColumnName, out columnData, out rowData, out dataList);
            }
            return ds;
        }
        public string GetDataFromDataSet(DataSet dsResult, int entityTypeId, string OIDColumnName, out string columnData, out List<string> rowData,
          out List<Dictionary<string, string>> dataList)
        {
            DataColumnCollection dataCols = dsResult.Tables[0].Columns;
            StringBuilder strCoulmnDetail = new StringBuilder();
            rowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            string outMsg;
            try
            {
                outMsg = Constant.statusSuccess;
                for (int i = 0; i < dataCols.Count; i++)
                {

                    string colName = string.Empty;
                    string dscolName = dataCols[i].ColumnName.ToString();
                    using (MPP_Context objMDMContext = new MPP_Context())
                    {
                        colName = objMDMContext.EntityTypeAttr.Where(x => x.AttrName == dscolName)
                            .Select(x => x.AttrDisplayName).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(colName))
                        strCoulmnDetail.Append(colName + ",");
                    if (dscolName == Constant.dateFromColumnName)
                        strCoulmnDetail.Append("Effective Date" + ",");
                    if (dscolName == OIDColumnName)
                        strCoulmnDetail.Append("OID" + ",");
                    if (dscolName == "CURRENT_EDIT_LEVEL")
                        strCoulmnDetail.Append("Current Edit Level");

                }
                DataRowCollection dataRows = dsResult.Tables[0].Rows;
                foreach (DataRow dataRow in dataRows)
                {
                    StringBuilder strRowDetail = new StringBuilder();
                    Dictionary<string, string> listRowData = new Dictionary<string, string>();
                    // List<Dictionary<string,string>> listRowData = new List<Dictionary<string, string>>();
                    for (int i = 0; i < dsResult.Tables[0].Columns.Count; i++)
                    {
                        string coldatatype = dataCols[i].DataType.FullName.ToString();
                        if (coldatatype == Constant.datedatatype)
                        {
                            string dtcol = string.Empty;
                            if (dataRow[i].ToString() != "")
                            {
                                dtcol = Convert.ToDateTime(dataRow[i].ToString()).ToShortDateString();
                            }

                            listRowData.Add(dataCols[i].ColumnName.ToString(), dtcol);
                        }
                        else
                            listRowData.Add(dataCols[i].ColumnName.ToString(), dataRow[i].ToString());
                        strRowDetail.Append(dataRow[i].ToString() + ",");
                        // listRowData.Add(listRowData);

                    }
                    dataList.Add(listRowData);
                    rowData.Add(strRowDetail.ToString().Trim(','));
                    strRowDetail.AppendLine();
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
            columnData = Convert.ToString(strCoulmnDetail).Trim(',');
            return outMsg;
        }

        public string GetSearchQuery(string fieldList, string tableName, string OIDCode, string OIDColumnName, List<EntityTypeAttr> attrNameList, out string searchQuery)
        {
            searchQuery = "";
            string outMsg = Constant.statusSuccess;
            try
            {
                fieldList = fieldList + "," + "DATE_FROM" + "," + OIDColumnName + "," + "CURRENT_EDIT_LEVEL";
                OIDCode = OIDCode.Substring(0, OIDCode.Length - 1);
                string strWhereClause = " where " + OIDColumnName + " in ( " + OIDCode + " ) ";
                StringBuilder strQuery = new StringBuilder();
                string strSelectClause = fieldList == "*" ? "t.*" : fieldList;
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
                strQuery.Append("SELECT " + strSelectClause + " FROM ");
                if (tableName.Contains("31-DEC-2049"))
                {
                    strQuery.Append(" ( " + tableName + " ) ");
                }
                else
                {
                    strQuery.Append(" " + tableName + " ");
                }
                strQuery.Append(" t ");
                strQuery.Append(strWhereClause);
                searchQuery = strQuery.ToString();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return outMsg;

        }
        public string GetSearchQuery(string fieldList, string tableName, string OIDCode, List<EntityTypeAttr> attrNameList, out string searchQuery)
        {
            string outMsg = Constant.statusSuccess;
            string strAsSelectClause = string.Empty;
            searchQuery = "";
            try
            {
                fieldList = fieldList + "," + "DATE_FROM" + "," + "INPUT_ROW_ID";
                OIDCode = OIDCode.Substring(0, OIDCode.Length - 1);
                string strWhereClause = " where INPUT_ROW_ID in ( " + OIDCode + " ) ";
                StringBuilder strQuery = new StringBuilder();
                string strSelectClause = (fieldList == "*" ? "t.*" : fieldList);
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
                strQuery.Append("SELECT " + strSelectClause + " FROM ");
                
                strQuery.Append("MPP_APP." + tableName);
                strQuery.Append(" t ");
                strQuery.Append(strWhereClause);
                searchQuery = strQuery.ToString();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return outMsg;

        }
        public string UpdateSelectedRecords(List<Dictionary<string, string>> listattrValues, string actionType, int entityTypeid,
           string userName, int bSuppressWarning, out bool download, out bool hasLoadErrors, out int sessionID,
          out List<Dictionary<string, string>> dataList, string strInputRowIds)
        {
            dataList = new List<Dictionary<string, string>>();
            List<EntityTypeAttr> attrList = new List<EntityTypeAttr>();
            string[] InputRowIdList = strInputRowIds.Split(',');
            string outMsg = Constant.statusSuccess;
            StringBuilder Query = new StringBuilder();
            string inputTablename = string.Empty;
            int noOfRowInserted = 0;
            sessionID = 0;
            download = true;
            hasLoadErrors = false;
            string fieldList;
            //Query.Append("begin ");
            using (GetSessionValue objGetSessionValue = new GetSessionValue())
            {
                sessionID = objGetSessionValue.GetNextSessionValue(out outMsg);
            }
            if (sessionID == 0 || outMsg != Constant.statusSuccess)
                return outMsg;
            using (MPP_Context objMDMContext = new MPP_Context())
            {
                inputTablename = objMDMContext.EntityType.Where(x => x.Id == entityTypeid).Select(x => x.InputTableName).FirstOrDefault();
            }

            using (GetViewDetail objGetViewDetail = new GetViewDetail())
            {
                outMsg = objGetViewDetail.GetFieldListVisible(entityTypeid, out fieldList, out attrList);
                if (outMsg != Constant.statusSuccess) return outMsg;
            }
            if (actionType == Constant.ActionTypeWhileUpdateFromWorkFlow)
            {
                foreach (string InputRowId in InputRowIdList)
                {
                    using (InsertAndDeleteInLandingTable objInsertAndDaleteInLandingTable = new InsertAndDeleteInLandingTable())
                    {
                        outMsg = objInsertAndDaleteInLandingTable.InsertIntoHistoryTable(inputTablename, InputRowId);
                        if (outMsg != Constant.statusSuccess) return outMsg;
                        outMsg = objInsertAndDaleteInLandingTable.DeleteDataFromLandingTable(inputTablename, InputRowId);
                        if (outMsg != Constant.statusSuccess) return outMsg;

                    }
                }

                UpdateDataInToLandingTable(listattrValues, entityTypeid, attrList, inputTablename, sessionID, out noOfRowInserted);
                if (noOfRowInserted == 0 || outMsg != Constant.statusSuccess)
                    return outMsg;
                //foreach (var _CATEGORY in _MassUpdateList)
                //{
                //    SessionID = _CATEGORY.SESSION_ID;
                //    UserName = _CATEGORY.USER_NAME;
                //    bSuppressWarning = _CATEGORY.SUPRESS_WARNING;
                //    String subQuery = String.Format("UPDATE MPP_APP.LD_CATEGORY SET CTGRY_CD = " + (_CATEGORY.CTGRY_CD == null ? "NULL" : "'" + _CATEGORY.CTGRY_CD + "'") + ",CTGRY_DSC = " + (_CATEGORY.CTGRY_DSC == null ? "NULL" : "'" + _CATEGORY.CTGRY_DSC + "'") + ",SHORT_DSC = " + (_CATEGORY.SHORT_DSC == null ? "NULL" : "'" + _CATEGORY.SHORT_DSC + "'") + ",DUMMY_FLAG = " + (_CATEGORY.DUMMY_FLAG == null ? "NULL" : "'" + _CATEGORY.DUMMY_FLAG + "'") + ",SORT_ORDER = " + _CATEGORY.SORT_ORDER + ",ACTIVE_FLAG = " + (_CATEGORY.ACTIVE_FLAG == null ? "NULL" : "'" + _CATEGORY.ACTIVE_FLAG + "'") + ",DATE_FROM = " + (_CATEGORY.DATE_FROM == null ? "NULL" : "to_date('" + ((DateTime)_CATEGORY.DATE_FROM).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')") + ", SESSION_ID = '" + _CATEGORY.SESSION_ID + "',ROW_STATUS = '1' WHERE INPUT_ROW_ID = '" + _CATEGORY.INPUT_ROW_ID + "' ");
                //    Query.Append(subQuery);
                //    Query.Append(";");
                //}
            }
            else if (actionType == Constant.ActionTypeWhileUpdateFromSearch)
            {

                using (InsertAndDeleteInLandingTable objInsertAndDaleteInLandingTable = new InsertAndDeleteInLandingTable())
                {
                    outMsg = objInsertAndDaleteInLandingTable.DeleteDataFromLandingTable(inputTablename, sessionID);
                }
                if (outMsg != Constant.statusSuccess) return outMsg;

                InsertDataIntoLandingTable(listattrValues, entityTypeid, attrList, inputTablename, sessionID, out noOfRowInserted);
                if (noOfRowInserted == 0 || outMsg != Constant.statusSuccess)
                    return outMsg;
            }
            outMsg = ValidateData(sessionID, entityTypeid, userName, bSuppressWarning, inputTablename);
            if (outMsg != Constant.statusSuccess)
            {
                hasLoadErrors = true;
                DataSet ds = new DataSet();
                ds = GetDataForDownloadingExcel(sessionID, entityTypeid, inputTablename, out dataList, out outMsg);
                //outMsg = InsertDataIntoLandingTableFromHistTable(inputTablename, sessionID);
                //outMsg = DeleteFromHistoryTable(inputTablename, sessionID);
                //outMsg = DeleteDataFromLandingTableOnRowStatus(inputTablename, sessionID);
            }
            else
            {
                download = false;
            }
            return outMsg;
        }
        private string UpdateDataInToLandingTable(List<Dictionary<string, string>> listattrValues, int entityTypeid,
           List<EntityTypeAttr> attrList, string inputTableName, int sessionID, out int noOfRowInserted)
        {
            StringBuilder finalQuery = new StringBuilder();
            string outMsg = Constant.statusSuccess;
            string whereClauseData = string.Empty;
            string sourceSystemName = "MPP UI";
            int treat_nulls_as_nulls = 0;
            noOfRowInserted = 0;
            int ldOid = 0;
            string inputRowId = string.Empty;
            try
            {
                //finalQuery.Append("begin ");
                foreach (var dictionary in listattrValues)
                {
                    StringBuilder updateQuery = new StringBuilder();
                    StringBuilder whereClause = new StringBuilder();
                    string whereData = string.Empty;
                    StringBuilder selectColumn = new StringBuilder();
                    foreach (var data in dictionary)
                    {
                        whereClauseData = data.Value == null ? "NULL" : data.Value;
                        string attrData = Convert.ToString(attrList.Find(x => x.AttrName == data.Key.ToString()));
                        if (!string.IsNullOrEmpty(attrData))
                        {
                            selectColumn.Append(data.Key + " , ");
                            string attrDataType = attrList.Find(x => x.AttrName == data.Key.ToString()).AttrDataType;
                            switch (attrDataType)
                            {
                                case "VC":
                                case "SUPPLIED_CODE":
                                case "PARENT_CODE":
                                    whereClause.Append(data.Key + " = " + " '" + whereClauseData + "' , ");
                                    break;
                                case "N":
                                    if (string.IsNullOrEmpty(whereClauseData.ToString()))
                                        whereClause.Append(data.Key + " = " + " '' , ");
                                    else
                                        whereClause.Append(data.Key + " = " + "" + whereClauseData + ", ");

                                    break;
                            }
                        }
                        else
                        {
                            if (data.Key == "CURRENT_EDIT_LEVEL")
                            {
                                selectColumn.Append(data.Key + " , ");
                                whereClause.Append(data.Key + " = " + " " + whereClauseData + " , ");
                            }
                            else if (data.Key == "DATE_FROM")
                            {
                                //whereClauseData = data.Value == null ? "NULL" : "to_date('" + (DateTime.Parse(data.Value)).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')";
                                whereClauseData = data.Value == null ? "CONVERT(DATE, GETDATE())" : "CONVERT(DATE, '" + (DateTime.Parse(data.Value)).ToString("MM/dd/yyyy") + "', 101)";
                                selectColumn.Append(data.Key + " , ");
                                whereClause.Append(data.Key + " = " + " " + whereClauseData + " , ");
                            }

                        }
                        if (data.Key == "INPUT_ROW_ID")
                            inputRowId = data.Value;

                    }
                    whereClauseData = "SESSION_ID = '" + sessionID + "',ROW_STATUS = '1' WHERE INPUT_ROW_ID = '" + inputRowId + "' ";
                    whereClause.Append(whereClauseData);
                    updateQuery.Append(" UPDATE ");
                    updateQuery.Append("MPP_APP." + inputTableName + " SET ");
                    updateQuery.Append(whereClause);
                    updateQuery.Append(";");
                    finalQuery.Append(updateQuery);
                    //"UPDATE MPP_APP.LD_CATEGORY SET CTGRY_CD = " + (_CATEGORY.CTGRY_CD == null ? "NULL" : "'" + _CATEGORY.CTGRY_CD + "'") + 
                    //    ",CTGRY_DSC = " + (_CATEGORY.CTGRY_DSC == null ? "NULL" : "'" + _CATEGORY.CTGRY_DSC + "'") + ",
                    //    SHORT_DSC = " + (_CATEGORY.SHORT_DSC == null ? "NULL" : "'" + _CATEGORY.SHORT_DSC + "'") + ",
                    //DUMMY_FLAG = " + (_CATEGORY.DUMMY_FLAG == null ? "NULL" : "'" + _CATEGORY.DUMMY_FLAG + "'") + ",
                    //SORT_ORDER = " + _CATEGORY.SORT_ORDER + ",
                    //ACTIVE_FLAG = " + (_CATEGORY.ACTIVE_FLAG == null ? "NULL" : "'" + _CATEGORY.ACTIVE_FLAG + "'") + ",
                    //DATE_FROM = " + (_CATEGORY.DATE_FROM == null ? "NULL" : "to_date('" + ((DateTime)_CATEGORY.DATE_FROM).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')") + ", 
                    //SESSION_ID = '" + _CATEGORY.SESSION_ID + "',ROW_STATUS = '1' WHERE INPUT_ROW_ID = '" + _CATEGORY.INPUT_ROW_ID + "' ");

                }
                //finalQuery.Append(" Commit; ");
                //finalQuery.Append(" End;");
                using (MPP_Context objmdmContext = new MPP_Context())
                {
                    noOfRowInserted = objmdmContext.Database.ExecuteSqlRaw(finalQuery.ToString());
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
            return outMsg;
        }
        private string InsertDataIntoLandingTable(List<Dictionary<string, string>> listattrValues, int entityTypeid,
            List<EntityTypeAttr> attrList, string inputTableName, int sessionID, out int noOfRowInserted)
        {
            StringBuilder finalQuery = new StringBuilder();
            string outMsg = Constant.statusSuccess;
            string whereClauseData = string.Empty;
            string sourceSystemName = "MPP UI";
            int treat_nulls_as_nulls = 0;
            noOfRowInserted = 0;
            int ldOid = 0;
            try
            {

               // finalQuery.Append("begin ");
                foreach (var dictionary in listattrValues)
                {
                    using (GetSequenceValue objGetSequenceValue = new GetSequenceValue())
                    {
                        ldOid = objGetSequenceValue.GetNextSequanceValue("MPP_CORE.SEQ_LD_OID", out outMsg);
                        if (outMsg != Constant.statusSuccess) return outMsg;
                    }
                    StringBuilder insertQuery = new StringBuilder();
                    StringBuilder whereClause = new StringBuilder();
                    StringBuilder selectColumn = new StringBuilder();
                    foreach (var data in dictionary)
                    {
                        whereClauseData = data.Value == null ? "NULL" : data.Value;
                        string attrData = Convert.ToString(attrList.Find(x => x.AttrName == data.Key.ToString()));
                        if (!string.IsNullOrEmpty(attrData))
                        {
                            selectColumn.Append(data.Key + " , ");
                            string attrDataType = attrList.Find(x => x.AttrName == data.Key.ToString()).AttrDataType;
                            switch (attrDataType)
                            {
                                case "VC":
                                case "SUPPLIED_CODE":
                                case "PARENT_CODE":
                                    whereClause.Append(" '" + whereClauseData + "' , ");
                                    break;
                                case "N":
                                    if (string.IsNullOrEmpty(whereClauseData.ToString()))
                                        whereClause.Append("'" + whereClauseData + "', ");

                                    else
                                        whereClause.Append("" + whereClauseData + ", ");
                                    break;
                                case "DT":
                                    whereClause.Append("" + whereClauseData + ", ");
                                    break;
                            }
                        }
                        else
                        {
                            if (data.Key == "CURRENT_EDIT_LEVEL")
                            {
                                selectColumn.Append(data.Key + " , ");
                                whereClause.Append(" " + whereClauseData + " , ");
                            }
                            else if (data.Key == "DATE_FROM")
                            {
                                //whereClauseData = data.Value == null ? "TRUNC(SYSDATE)" : "to_date('" + (DateTime.Parse(data.Value)).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')";
                                whereClauseData = data.Value == null ? "CONVERT(DATE, GETDATE())" : "CONVERT(DATE, '" + (DateTime.Parse(data.Value)).ToString("MM/dd/yyyy") + "', 101)";
                                selectColumn.Append(data.Key + " , ");
                                whereClause.Append(" " + whereClauseData + " , ");
                            }

                        }

                    }
                    //if (inputTableName.Contains("MAPPING"))
                    //{
                    //    selectColumn.Append("SESSION_ID ,SOURCE_SYSTEM_NAME, TREAT_NULLS_AS_NULLS,LD_OID ");
                    //}
                    //else
                    //{
                        selectColumn.Append("SESSION_ID ,SOURCE_SYSTEM_NAME, TREAT_NULLS_AS_NULLS,LD_OID ");
                    //}
                    whereClause.Append("" + sessionID + ",'" + sourceSystemName + "'," + treat_nulls_as_nulls + "," + ldOid + "");
                    insertQuery.Append(" INSERT INTO ");
                    insertQuery.Append("MPP_APP." + inputTableName + "(");
                    insertQuery.Append(selectColumn);
                    insertQuery.Append(" ) VALUES (");
                    insertQuery.Append(whereClause);
                    insertQuery.Append(" ) ");
                    insertQuery.Append(";");
                    finalQuery.Append(insertQuery);
                }
                //finalQuery.Append(" Commit; ");
                //finalQuery.Append(" End;");
                using (MPP_Context objmdmContext = new MPP_Context())
                {
                    noOfRowInserted = objmdmContext.Database.ExecuteSqlRaw(finalQuery.ToString());
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
            return outMsg;
        }
        private string ValidateData(int sessionId, int entityTypeId, string userID, Nullable<decimal> suppressWarning, string inputTablename)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    mPP_Context.Database.SetCommandTimeout(360);
                    mPP_Context.Procedures.MPP_LOAD_CHKAsync(sessionId.ToString(), entityTypeId.ToString(), userID.ToUpper(), suppressWarning).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message.ToString();
                
                //if (!string.IsNullOrEmpty(ex.Message.ToString()))
                //{
                //    try
                //    {
                //        using (InsertAndDeleteInLandingTable objInsertAndDaleteInLandingTable = new InsertAndDeleteInLandingTable())
                //        {
                //            objInsertAndDaleteInLandingTable.DeleteDataFromLandingTable(inputTablename, Convert.ToInt32(sessionId));

                //            //if (ex.Message.ToString().Contains("WARNING:"))
                //            //{
                //            //    throw new Exception(objInsertAndDaleteInLandingTable.GetErrorOrWarning(inputTablename, "WARNING", sessionId.ToString()));
                //            //}
                //            //else if (ex.Message.ToString().Contains("ERROR:"))
                //            //{
                //            //    throw new Exception(objInsertAndDaleteInLandingTable.GetErrorOrWarning(inputTablename, "ERROR", sessionId.ToString()));
                //            //}
                //            //else
                //            //{
                //            //    throw;
                //            //}
                //        }
                //    }
                //    catch (Exception ex1)
                //    {
                //        using (LogError objLogError = new LogError())
                //        {
                //            objLogError.LogErrorInTextFile(ex);
                //        }
                //        //throw ex1;
                //    }
                //}
                if (!string.IsNullOrEmpty(ex.Message.ToString()))
                {
                    outMsg = ex.Message.ToString();

                }
                else
                {
                    using (InsertAndDeleteInLandingTable objInsertAndDaleteInLandingTable = new InsertAndDeleteInLandingTable())
                    {
                        objInsertAndDaleteInLandingTable.DeleteDataFromLandingTable(inputTablename, Convert.ToInt32(sessionId));
                    }
                    if (string.IsNullOrEmpty(ex.Message.ToString()))
                    {
                        outMsg = ex.Message.ToString();
                    }
                    //if (ex.Message.ToString().Contains("diagnostic text: "))
                    //{
                    //    int startPos = ex.InnerException.Message.ToString().IndexOf("diagnostic text: ") + "diagnostic text: ".Length;
                    //    int noChars = 100;
                    //    if (ex.InnerException.Message.ToString().Contains("SQLSTATE="))
                    //        noChars = ex.InnerException.Message.ToString().IndexOf("SQLSTATE=") - startPos;
                    //    outMsg = ex.InnerException.Message.ToString().Substring(startPos, noChars);
                    //}
                    //else
                    //    outMsg = ex.InnerException.Message.ToString();
                    //if (ex.InnerException.ToString().Contains("WARNING:"))
                    //{
                    //    outMsg = ex.InnerException.Message.ToString();
                    //}
                    //else if (ex.InnerException.ToString().Contains("Unique"))
                    //{
                    //    outMsg = ex.InnerException.Message.ToString().Substring(ex.InnerException.Message.ToString().IndexOf("Unique"), ex.InnerException.Message.ToString().IndexOf(",") - ex.InnerException.Message.ToString().IndexOf("Unique"));
                    //}
                    //else if (ex.InnerException.ToString().ToUpper().Contains("THIS RECORD IS MODIFIED BY"))
                    //{

                    //    outMsg = "This record is being modified by another user. Please try again later.";
                    //}
                    //else if (ex.InnerException.ToString().ToUpper().Contains("DOES NOT EXIST IN THE MASTER"))
                    //{

                    //    outMsg = ex.InnerException.Message.ToString().Substring(ex.InnerException.Message.ToString().IndexOf("~"), ex.InnerException.Message.ToString().LastIndexOf("~") - ex.InnerException.Message.ToString().IndexOf("~") + 1);
                    //}
                    //else if (ex.InnerException.ToString().ToUpper().Contains("CODE ALREADY EXISTS"))
                    //{

                    //    outMsg = "Code already exists.";
                    //}
                    //else if (ex.InnerException.ToString().ToUpper().Contains("IT IS AN INACTIVE ENTITY"))
                    //{

                    //    outMsg = "InActive entity.";
                    //}
                    else
                            {
                        outMsg = DateTime.Now + ":Please Contact MPP Support Team";
                    }
                }
            }
            return outMsg;
        }
        private DataSet GetDataForDownloadingExcel(int sessionID, int entityTypeid, string inputTablename, out List<Dictionary<string, string>> dataList, out string outMsg)
        {
            #region variableDeclaration
            string columnData = string.Empty;
            List<string> rowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            List<EntityTypeData> listEntityTypeData = new List<EntityTypeData>();
            List<EntityTypeAttr> attrList = new List<EntityTypeAttr>();
            StringBuilder strWhereClause = new StringBuilder();
            StringBuilder columnName = new StringBuilder();
            StringBuilder strQuery = new StringBuilder();
            outMsg = Constant.statusSuccess;
            DataSet ds = new DataSet();
            string fieldList;
            #endregion variableDeclaration
            try
            {
                using (GetViewDetail objGetViewDetail = new GetViewDetail())
                {
                    outMsg = objGetViewDetail.GetFieldList(entityTypeid, out fieldList, out attrList);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(fieldList) || attrList.Count() == 0)
                        return ds;
                    columnName.Append(fieldList + ",");
                    columnName.Append(Constant.dateFromColumnName + ",");
                    columnName.Append("ERROR_MESSAGE");
                }

                strWhereClause.Append(" WHERE ROW_STATUS = 1 AND SESSION_ID = '" + sessionID + "'");
                strQuery.Append("Select " + columnName + " from  ");
                strQuery.Append("MPP_APP." + inputTablename );
                strQuery.Append(strWhereClause);
                ds = GetDataSet(strQuery.ToString(), out outMsg);
                if (outMsg != Constant.statusSuccess)
                    return ds;
                outMsg = GetDataFromDataSet(ds, entityTypeid, "", out columnData, out rowData, out dataList);

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
        private string InsertDataIntoLandingTableFromHistTable(string inputTablename, int sessionID)
        {
            StringBuilder strInsertQuery = new StringBuilder();
            string outMsg = Constant.statusSuccess;
            int noOfRowInserted = 0;
            strInsertQuery.Append(" INSERT INTO MPP_APP." + inputTablename);
            strInsertQuery.Append(" SELECT * FROM MPP_APP." + inputTablename.Replace("LD", "HIST") + " WHERE LD_OID IN ");
            strInsertQuery.Append("( SELECT " + Constant.ldOID + " FROM MPP_APP." + inputTablename);
            strInsertQuery.Append(" WHERE " + Constant.rowStatus + " = 1 AND " + Constant.sessionID + "='" + sessionID + "')");
            try
            {
                using (MPP_Context objMdmContext = new MPP_Context())
                {
                    noOfRowInserted = objMdmContext.Database.ExecuteSqlRaw(strInsertQuery.ToString());
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
            return outMsg;
        }

        private string DeleteFromHistoryTable(string inputTableName, int sessionID)
        {
            StringBuilder strDeleteQuery = new StringBuilder();
            string outMsg = Constant.statusSuccess;
            int noOfRowDeleted = 0;
            strDeleteQuery.Append(" DELETE FROM " + inputTableName.Replace("LD", "HIST"));
            strDeleteQuery.Append(" WHERE " + Constant.ldOID + " IN ");
            strDeleteQuery.Append("( SELECT " + Constant.ldOID + " FROM " + inputTableName);
            strDeleteQuery.Append(" WHERE " + Constant.rowStatus + " = 1 AND " + Constant.sessionID + "='" + sessionID + "')");
            try
            {
                using (MPP_Context objMdmContext = new MPP_Context())
                {
                    noOfRowDeleted = objMdmContext.Database.ExecuteSqlRaw(strDeleteQuery.ToString());
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
            return outMsg;

        }

        private string DeleteDataFromLandingTableOnRowStatus(string tableName, int sessionId)
        {
            string outMsg = Constant.statusSuccess;
            string deleteQuery = "DELETE FROM MPP_APP." + tableName + " WHERE " + Constant.rowStatus + " = 1 AND " + Constant.sessionID + "  =  " + "'" + sessionId + "'";
            int noOfRowDeleted;
            try
            {
                using (MPP_Context objmdmContext = new MPP_Context())
                {
                    noOfRowDeleted = objmdmContext.Database.ExecuteSqlRaw(deleteQuery);
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
            return outMsg;
        }

    }
}
