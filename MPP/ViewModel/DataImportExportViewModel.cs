using DAL.Common;
using Model;
using System.Data;
using System.Text;

namespace MPP.ViewModel
{
    public class DataImportExportViewModel : IDisposable
    {
        void IDisposable.Dispose()
        {
           
        }
        string g_UserID = string.Empty;
        short g_LoadID;
        public string GetViewName(int entityTypeId, string userName, out string viewName)
        {
            string outMsg = Constant.statusSuccess;
            viewName = string.Empty;
            try
            {
                using (GetViewDetail getViewDetail = new GetViewDetail())
                {
                    viewName = string.Empty;
                    outMsg = getViewDetail.GetViewName(entityTypeId, userName.ToUpper(), out viewName);
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
            }
            return outMsg;
        }

        public string LoadExcelToTable2(List<Entity_Type_Attr_Detail> attributeList, int entityTypeId, string filePath, string workSheetName,
                  string tableColumnNames, bool hasHeader, string startCell, string endCell, bool cleanseFileFlag, string userID, short loadID,
                  string rejectFileName, string strTableColumnDataTypes, out int[] ArrayRowsCount, out int loadErrorCount,
                  out bool hasLoadErrors, out bool download)
        {
            #region Variable Declaration
            int sessionID;
            download = true;
            string tableName;
            g_LoadID = loadID;
            g_UserID = userID;
            loadErrorCount = 0;
            hasLoadErrors = false;
            int bSupressWarning = 0;
            string strInputFile = "Input File";
            ArrayRowsCount = new int[2] { 0, 0 };
            string outMsg = Constant.statusSuccess;
            string extraColumnNames = string.Empty;
            string extraColumnValues = string.Empty;
            string colErrorMessage = "ERROR_MESSAGE";
            string colWarningMessage = "WARNING_MESSAGE";
            ETLExcelReader excelReader = new ETLExcelReader();
            #endregion Variable Declaration
            try
            {
                using (GetSessionValue objGetSessionValue = new GetSessionValue())
                {
                    sessionID = objGetSessionValue.GetNextSessionValue(out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return outMsg;
                }
                using (GetViewDetail objviewdetail = new GetViewDetail())
                {
                    tableName = objviewdetail.GetTableName(entityTypeId, out outMsg);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(tableName))
                        return outMsg;

                }
                extraColumnNames = ",SOURCE_SYSTEM_NAME,LD_OID,SESSION_ID,TREAT_NULLS_AS_NULLS,USER_ID";
                extraColumnValues = "'MPP_IMPORT',MPP_CORE.SEQ_LD_OID.NEXTVAL,'" + sessionID + "',1,'" + g_UserID + "'";
                outMsg = CheckFileLength(filePath, tableName, sessionID, userID, loadID);
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
                //Set the properties of the ExcelReader
                excelReader.FileName = filePath;
                excelReader.HasHeader = hasHeader;
                excelReader.SheetName = workSheetName;
                if (startCell.Length > 0 && endCell.Length > 0)
                    excelReader.SheetRange = startCell + ":" + endCell;
                if (hasHeader == false && tableColumnNames.Length == 0)
                    return "File Path cannot be null";
                if (tableName.Length == 0)
                    return "Table Name cannot be null";

                DataSet dsResult = excelReader.ReadDataSet(strInputFile, tableColumnNames);
                dsResult.Tables[0].TableName = strInputFile;
                ArrayRowsCount[0] = dsResult.Tables[0].Rows.Count;
                excelReader.CloseFileConnection();
                int colCountCheck = dsResult.Tables[strInputFile].Columns.Count;
                if (dsResult.Tables[strInputFile].Columns.Contains(colErrorMessage))
                {
                    dsResult.Tables[strInputFile].Columns.Remove(colErrorMessage);

                }
                if (dsResult.Tables[strInputFile].Columns.Contains(colWarningMessage))
                {
                    dsResult.Tables[strInputFile].Columns.Remove(colWarningMessage);

                }
                dsResult.Tables[strInputFile].Columns.Add(colErrorMessage);
                dsResult.Tables[strInputFile].Columns.Add(colWarningMessage);

                if (cleanseFileFlag)    //If the data has to be cleared
                {
                    int colCount = dsResult.Tables[0].Columns.Count;
                    foreach (DataRow dr in dsResult.Tables[0].Rows)
                    {
                        for (int i = 0; i < colCount; i++)
                        {
                            if (dr[i] is string)
                                dr[i] = CleanseData(dr[i].ToString());
                        }
                    }
                }
                string[] tableCols = tableColumnNames.Split(new char[] { ',' });
                string[] tableColDataTypes = strTableColumnDataTypes.Split(new char[] { ',' });
                foreach (DataRow dr in dsResult.Tables[0].Rows)
                {
                    StringBuilder StrInsertQuery = null;
                    StrInsertQuery = new StringBuilder();
                    StrInsertQuery.Append("Insert into ");
                    StrInsertQuery.Append(tableName);
                    StrInsertQuery.Append("(");
                    StrInsertQuery.Append(tableColumnNames + extraColumnNames);
                    StrInsertQuery.Append(")");
                    StrInsertQuery.Append(" Values ");
                    StrInsertQuery.Append("(");
                    for (int i = 0; i <= tableCols.GetUpperBound(0); i++)
                    {
                        string DataValue = dr[tableCols[i]].ToString();
                        if (DataValue == "")
                        {
                            if (tableColDataTypes[i] == "DATE")
                            {
                                StrInsertQuery.Append("TRUNC(SYSDATE),");
                            }
                            else
                            {
                                StrInsertQuery.Append("NULL,");
                            }
                        }
                        else
                        {
                            switch (tableColDataTypes[i])
                            {
                                case "NUMERIC":
                                case "DECIMAL":
                                    StrInsertQuery.Append(DataValue);
                                    StrInsertQuery.Append(",");
                                    break;
                                case "VARCHAR":
                                    StrInsertQuery.Append("'");
                                    StrInsertQuery.Append(DataValue);
                                    StrInsertQuery.Append("',");
                                    break;
                                case "DATE":
                                    DateTime dt = DateTime.Parse(DataValue);
                                    DataValue = String.Format("{0:r}", dt);
                                    DataValue = DataValue.Trim().Substring(5, 11);
                                    DataValue = DataValue.Replace(" ", "-");
                                    StrInsertQuery.Append("'");
                                    StrInsertQuery.Append(DataValue);
                                    StrInsertQuery.Append("',");
                                    break;
                                default:
                                    StrInsertQuery.Append("'");
                                    StrInsertQuery.Append(DataValue);
                                    StrInsertQuery.Append("',");
                                    break;

                            }
                        }
                    }
                    StrInsertQuery.Append(extraColumnValues + ")");
                    using (DataExportImport objDataExportImport = new DataExportImport())
                    {
                        outMsg = objDataExportImport.InsertInLandingTable(StrInsertQuery.ToString());
                        if (outMsg != Constant.statusSuccess)
                            dr[colErrorMessage] = outMsg;
                    }
                }

                //Save the Error File

                DataSet dsErrorRows = new DataSet();
                DataView dvFileErrors = new DataView(dsResult.Tables[0]);
                dvFileErrors.RowFilter = "ERROR_MESSAGE<>''";
                dsErrorRows.Tables.Add(dvFileErrors.ToTable());
                ArrayRowsCount[1] = dvFileErrors.Count;
               // outMsg = WriteDataSetToFlatFile(attributeList, rejectFileName, dsErrorRows, ",", true, false);
                if (outMsg != Constant.statusSuccess)
                    return "Cannot write Error data to Reject file";
                if (excelReader != null)
                {
                    excelReader.Dispose();
                    excelReader = null;
                }
                using (DataValidationUsingSP objDataValidationUsingSP = new DataValidationUsingSP())
                {
                    outMsg = objDataValidationUsingSP.ValidateData(attributeList, entityTypeId.ToString(), userID, bSupressWarning, rejectFileName, tableName,
                          tableColumnNames, sessionID.ToString(), out loadErrorCount, out hasLoadErrors, out download);
                    if (hasLoadErrors)
                    {
                        LoadTableToFlatFile(attributeList, rejectFileName, "MDM_APP." + tableName, tableColumnNames + ", ERROR_MESSAGE,WARNING_MESSAGE ", "",
                        " SESSION_ID = '" + sessionID + "' AND (ERROR_MESSAGE IS NOT NULL OR WARNING_MESSAGE IS NOT NULL) ", "", "", true, ",", 1, true);
                        using (InsertAndDeleteInLandingTable objInsertAndDeleteInLandingTable = new InsertAndDeleteInLandingTable())
                        {
                            objInsertAndDeleteInLandingTable.DeleteDataFromLandingTableOnRowStatus(tableName, Convert.ToInt32(sessionID), out loadErrorCount);
                        }
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
            return outMsg;
        }

        private string CheckFileLength(string filePath, string tableName, int sessionID, string userID, short loadID)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                if (filePath.Length == 0)
                {
                    outMsg = "File Path cannot be null.";
                }
                else if (!File.Exists(filePath))
                {
                    outMsg = "File does not exist at the specified path.";

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
        private string CleanseData(string strData)
        {
            string strCleansedData;
            strCleansedData = RemoveSpecialCharacters(strData.Trim());
            strCleansedData = strCleansedData.Replace("'", "''");
            return strCleansedData;
        }
        private static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                    (c == '.') || (c == '_') || (c == '/') || (c == '\\') || (c == '*') || (c == ',') ||
                    (c == '-') || (c == ';') || (c == ':') || (c == '(') || (c == ')') || (c == ' ') || (c == '#') ||
                    (c == '@') || (c == '\'') || (c == '-') || (c == '&') || (c == '~') || (c == '!') || (c == '%') ||
                    (c == '^') || (c == ']') || (c == '[') || (c == '`') || (c == '"') || (c == '{') || (c == '}') ||
                    (c == '|') || (c == '?') || (c == '+') || (c == '=') || (c == '$') || (c == ' '))
                {
                    sb.Append(c);

                }
            }
            return sb.ToString();

        }
    }
}
