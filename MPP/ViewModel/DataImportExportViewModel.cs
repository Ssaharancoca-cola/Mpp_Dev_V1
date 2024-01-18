using CsvHelper;
using DAL;
using DAL.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using Model;
using System.Collections;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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

        public string LoadTableToFlatFile(List<Entity_Type_Attr_Detail> attributeList, string FileName, string TableName, string ColumnNames, string ExcludeColumnNames, string WhereClause,
           string GroupByClause, string OrderByClause, bool IsColumnheaderToBeAdded, string Delimiters, short LoadID, bool boolAppend)
        {
            string FormedQuery = string.Empty;
            string outMsg = Constant.statusSuccess;
            try
            {
                FormedQuery = FormSQLStringWithEnterprise(TableName, ColumnNames, ExcludeColumnNames, WhereClause, GroupByClause, OrderByClause, out outMsg);
                if (outMsg == Constant.statusSuccess)
                    outMsg = WriteToFlatFileWithEnterprise(attributeList, FileName, FormedQuery, Delimiters, IsColumnheaderToBeAdded, boolAppend);
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
            }
            return outMsg;
        }
        private string FormSQLStringWithEnterprise(string TableName, string ColumnNames, string ExcludeColumnNames, string WhereClause,
             string GroupByClause, string OrderByClause, out string outMsg)
        {
            #region Variable Declaration
            string FormedString = "";
            string ColumnListString = "";
            string WhereSyntax = "";
            string SelectSyntax = "";
            string FromSyntax = "";
            string GroupBySyntax = "";
            string OrderBySyntax = "";
            string Columns = "";
            WhereSyntax = " WHERE ";
            SelectSyntax = "SELECT ";
            FromSyntax = " FROM ";
            GroupBySyntax = " GROUP BY ";
            OrderBySyntax = " ORDER BY ";
            outMsg = Constant.statusSuccess;
            ArrayList arColumns = new ArrayList();
            #endregion Variable Declaration
            if (TableName.Length == 0)
                outMsg = "Table Name cannot be null. Table Name was not specified";
            try
            {
                if (ColumnNames.Length == 0)
                {
                    if (ExcludeColumnNames.Length == 0)
                        ColumnListString = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name ='" + TableName + "'";
                    else
                        ColumnListString = "SELECT COLUMN_NAME FROM SYSCOLUMNS WHERE TABLE_NAME = '" + TableName +
                                "' AND COLUMN_NAME NOT IN (" + ExcludeColumnNames + ")";
                    DataTable dtColumnList;
                    DataSet dsColumnList;
                    dsColumnList = new DataSet();
                    DataExportImport objdataexport = new DataExportImport();
                     Task.Run(async() =>
                    {
                        dsColumnList = await objdataexport.GetData(ColumnListString);
                    }).Wait();
                    dtColumnList = dsColumnList.Tables[0];
                    Columns = "";
                    int iColCounter = 0;
                    for (; iColCounter < dtColumnList.Rows.Count; iColCounter++)
                    {
                        string colName = (string)dtColumnList.Rows[iColCounter]["COLUMN_NAME"];
                        if (iColCounter < dtColumnList.Rows.Count - 1)
                            Columns += colName + ", ";
                        else
                            Columns += colName;
                    }
                }
                else
                    Columns = ColumnNames;

                if (WhereClause.Length == 0)
                    WhereSyntax = "";

                if (GroupByClause.Length == 0)
                    GroupBySyntax = "";

                if (OrderByClause.Length == 0)
                    OrderBySyntax = "";
                FormedString = SelectSyntax + Columns + FromSyntax + TableName + WhereSyntax + WhereClause +
                                 GroupBySyntax + GroupByClause + OrderBySyntax + OrderByClause;
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
            }
            return FormedString;
        }
        public string WriteToFlatFileWithEnterprise(List<Entity_Type_Attr_Detail> attributeList, string FileName, string FormedQueryString, string Delimiter, bool IsColumnheaderToBeAdded,
            bool boolAppend)
        {
            DataSet dsResult = new DataSet();
            string outMsg = Constant.statusSuccess;
            try
            {
                using (DataExportImport objdataexport = new DataExportImport())
                {
                   // dsResult = objdataexport.GetData(FormedQueryString);
                    Task.Run(async () =>
                    {
                        dsResult = await objdataexport.GetData(FormedQueryString);
                    }).Wait();
                }
                outMsg = WriteDataSetToFlatFile(attributeList, FileName, dsResult, Delimiter, IsColumnheaderToBeAdded, boolAppend);
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            finally
            {
                dsResult = null;
            }
            return outMsg;

        }
        private string WriteDataSetToFlatFile(List<Entity_Type_Attr_Detail> attributeList, string FileName, DataSet dsResult, string Delimiter,
            bool IsColumnheaderToBeAdded, bool boolAppend)
        {
            #region Variable Declaration
            int Counter;
            string TextToBeWritten = "";
            StreamWriter swCSVWriter = null;
            string outMsg = Constant.statusSuccess;
            StringBuilder TextData = new StringBuilder();
            #endregion Variable Declaration
            try
            {
                if (FileName.Length == 0)
                    return ("File Name cannot be null");
                if (IsColumnheaderToBeAdded)
                {
                    TextToBeWritten = "";
                    Counter = 0;
                    DataColumnCollection dataCols = dsResult.Tables[0].Columns;
                    for (Counter = 0; Counter < dataCols.Count; Counter++)
                    {
                        TextToBeWritten += dataCols[Counter].ColumnName;
                        if (Counter != dataCols.Count - 1)
                            TextToBeWritten += Delimiter;
                    }
                    TextData.AppendLine(TextToBeWritten);
                }
                TextToBeWritten = "";
                DataRowCollection dataRows = dsResult.Tables[0].Rows;
                foreach (DataRow dataRow in dataRows)
                {
                    Counter = 0;
                    TextToBeWritten = "";
                    for (Counter = 0; Counter < dsResult.Tables[0].Columns.Count; Counter++)
                    {
                        if (dataRow[Counter].ToString().Contains(Delimiter))
                        {
                            TextToBeWritten += "\"" + dataRow[Counter].ToString().Replace("\"", "\"\"") + "\"";
                        }
                        else
                        {
                            if (dsResult.Tables[0].Columns[Counter].DataType.ToString() == "System.DateTime")
                            {
                                if (dataRow[Counter].ToString() == "")
                                    TextToBeWritten += "";
                                else
                                    TextToBeWritten += ((DateTime)dataRow[Counter]).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                TextToBeWritten += dataRow[Counter];
                            }
                        }
                        if (Counter != dsResult.Tables[0].Columns.Count - 1)
                            TextToBeWritten += Delimiter;
                    }
                    TextData.AppendLine(TextToBeWritten);
                }

                if (FileName.Length > 0)
                {
                    // if (boolAppend == false)
                    {
                        swCSVWriter = File.CreateText(FileName);
                        swCSVWriter.Write(TextData);
                        swCSVWriter.Close();
                        swCSVWriter = null;
                    }
                    //  else
                    //   File.AppendAllText(FileName, TextData.ToString());

                }

                dsResult = null;
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return outMsg; ;
        }

        public string LoadFlatFileToTable(List<Entity_Type_Attr_Detail> attributeList, int entityTypeId, string filePath, string tableColumnNames, bool hasHeader, string strDelimiter,
       string userID, short loadID, string rejectFileName, string strTableColumnDataTypes, out int[] ArrayRowsCount, out int loadErrorCount,
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
            string strConnectFile = "";
            string strInputFile = "Input File";
            ArrayRowsCount = new int[2] { 0, 0 };
            string outMsg = Constant.statusSuccess;
            string extraColumnNames = string.Empty;
            string extraColumnValues = string.Empty;
            string colErrorMessage = "ERROR_MESSAGE";
            string colWarningMessage = "WARNING_MESSAGE";
            #endregion Variable Declaration
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
            foreach (var data in attributeList)
            {

                if (data.Isvisible == "N")
                {

                    extraColumnNames = "," + data.AttrName;
                    extraColumnValues = "NULL,";
                }
            }
            //if (tableName.Contains("MAPPING"))
            //{
            //    extraColumnNames = extraColumnNames + ",SOURCE_SYSTEM_NAME,LD_OID,SESSION_ID,TREAT_NULLS_AS_NULLS,USER_ID";
            //}
            //else
            //{
                extraColumnNames = extraColumnNames + ",SOURCE_SYSTEM_NAME,LD_OID,SESSION_ID,TREAT_NULLS_AS_NULLS,USER_ID";
            //}
            extraColumnValues = extraColumnValues + "'MPP_IMPORT',NEXT VALUE FOR MPP_CORE.SEQ_LD_OID,'" + sessionID + "',1,'" + g_UserID + "'";
            
            outMsg = CheckFileLength(filePath, tableName, sessionID, userID, loadID);
            if (outMsg != Constant.statusSuccess)
                return outMsg;
            if (tableName.Length == 0)
                outMsg = "Table Name cannot be null.";
            if (tableColumnNames.Length == 0 || hasHeader == false)
                outMsg = "Table Columns is not available.";
            try
            {
                DataTable dtTable = new DataTable();
                string folderPath = filePath.Substring(0, filePath.LastIndexOf('\\'));
                string fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                try
                {
                    using (StreamReader fileReader = new StreamReader(filePath))
                    using (CsvReader csvReader = new CsvReader(fileReader, CultureInfo.InvariantCulture))
                    {
                        // Reading the CSV file header to create the DataTable columns
                        csvReader.Read();
                        csvReader.ReadHeader();

                        foreach (string header in csvReader.HeaderRecord)
                        {
                            dtTable.Columns.Add(header);
                        }

                        // Reading the rest of the data and populate the DataTable
                        while (csvReader.Read())
                        {
                            DataRow row = dtTable.NewRow();
                            for (int i = 0; i < dtTable.Columns.Count; i++)
                            {
                                row[i] = csvReader.GetField(i);
                            }
                            dtTable.Rows.Add(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    using (LogError objLogError = new LogError())
                    {
                        objLogError.LogErrorInTextFile(ex);
                    }
                    outMsg = "File is Corrupted,Please upload valid file (If Error Message column exists, please load file with out Error Message column)";
                    return outMsg;
                }
                
                dtTable.TableName = strInputFile;
                ArrayRowsCount[0] = 0;
                if (dtTable.Columns.Contains(colErrorMessage))
                {
                    dtTable.Columns.Remove(colErrorMessage);

                }
                if (dtTable.Columns.Contains(colWarningMessage))
                {
                    dtTable.Columns.Remove(colWarningMessage);

                }
                if (!(dtTable.Columns.Contains(Constant.dateFromColumnName)))
                {
                    dtTable.Columns.Add(Constant.dateFromColumnName);

                }
                dtTable.Columns.Add(colErrorMessage);
                dtTable.Columns.Add(colWarningMessage);


                int colCount = dtTable.Columns.Count;
                bool isEmpty = false;

                foreach (DataColumn dc in dtTable.Columns)
                {
                    dc.ReadOnly = false;
                }
                string[] tableCols = tableColumnNames.Split(new char[] { ',' });
                string[] tableColDataTypes = strTableColumnDataTypes.Split(new char[] { ',' });
                foreach (DataRow dr in dtTable.Rows)
                {

                    int colCountEmpt = 0;
                    for (int i = 0; i < colCount; i++)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dr[i])))
                            break;
                        else
                        {
                            isEmpty = true;
                            colCountEmpt++;
                        }
                    }
                    if (!isEmpty && colCountEmpt != colCount)
                    {

                        for (int i = 0; i < colCount; i++)
                        {
                            if (dr[i] is string)
                                dr[i] = CleanseData(dr[i].ToString());
                        }
                        //Execute the query formed.
                        StringBuilder StrInsertQuery = null;
                        StrInsertQuery = new StringBuilder();
                        StrInsertQuery.Append("Insert into MPP_APP.");
                        StrInsertQuery.Append(tableName);
                        StrInsertQuery.Append("(");
                        StrInsertQuery.Append(tableColumnNames.Trim(',') + extraColumnNames);
                        StrInsertQuery.Append(")");
                        StrInsertQuery.Append(" Values ");
                        StrInsertQuery.Append("(");
                        for (int i = 0; i < tableCols.Length; i++)
                        {
                            string DataValue = dr[tableCols[i]].ToString();
                            if (DataValue == "")
                            {
                                if (tableColDataTypes[i] == "DATE")
                                    StrInsertQuery.Append("convert(date,getdate()),");
                                else
                                    StrInsertQuery.Append("NULL,");
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
                                    case "DT":
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
                            else
                                ArrayRowsCount[0]++;
                        }
                    }
                }
                DataSet dsErrorRows = new DataSet();
                DataView dvFileErrors = new DataView(dtTable);
                dvFileErrors.RowFilter = "ERROR_MESSAGE<>''";
                dsErrorRows.Tables.Add(dvFileErrors.ToTable());
                ArrayRowsCount[1] = dvFileErrors.Count;
                outMsg = WriteDataSetToFlatFile(attributeList, rejectFileName, dsErrorRows, strDelimiter, true, false);
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
                using (DataValidationUsingSP objDataValidationUsingSP = new DataValidationUsingSP())
                {
                    outMsg = objDataValidationUsingSP.ValidateData(attributeList, entityTypeId.ToString(), userID, bSupressWarning, rejectFileName, tableName,
                          tableColumnNames, sessionID.ToString(), out loadErrorCount, out hasLoadErrors, out download);
                    if (loadErrorCount == 0)
                    {
                        if (hasLoadErrors == true)
                        {
                            LoadTableToFlatFile(attributeList, rejectFileName, "MPP_APP." + tableName, tableColumnNames + ", ERROR_MESSAGE,WARNING_MESSAGE ", "",
                            " SESSION_ID = '" + sessionID + "' AND (ERROR_MESSAGE IS NOT NULL OR WARNING_MESSAGE IS NOT NULL) ", "", "",
                            true, ",", 1, true);
                            using (InsertAndDeleteInLandingTable objInsertAndDeleteInLandingTable = new InsertAndDeleteInLandingTable())
                            {
                                objInsertAndDeleteInLandingTable.DeleteDataFromLandingTableOnRowStatus(tableName, Convert.ToInt32(sessionID), out loadErrorCount);
                            }
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
                //if (tableName.Contains("MAPPING"))
                //{
                //    extraColumnNames = extraColumnNames + ",SOURCE_SYSTEM_NAME,LD_OID,SESSION_ID,TREAT_NULLS_AS_NULLS,USER_ID";
                //}
                //else
                //{
                    extraColumnNames = extraColumnNames + ",SOURCE_SYSTEM_NAME,LD_OID,SESSION_ID,TREAT_NULLS_AS_NULLS,USER_ID";
                //}

                extraColumnValues = extraColumnValues + "'MPP_IMPORT',NEXT VALUE FOR [MPP_CORE].SEQ_LD_OID," + sessionID + ",1,'" + g_UserID + "'";
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
                 outMsg = WriteDataSetToFlatFile(attributeList, rejectFileName, dsErrorRows, ",", true, false);
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
