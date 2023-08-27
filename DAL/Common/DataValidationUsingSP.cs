using Model;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class DataValidationUsingSP : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        string g_UserID = string.Empty;
        short g_LoadID;

        public string ValidateData(string sessionId, string entityTypeId, string userID, decimal? suppressWarning, string inputtableName)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    mPP_Context.Procedures.MPP_LOAD_CHKAsync(sessionId, entityTypeId, userID.ToUpper(), suppressWarning).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                using (InsertAndDeleteInLandingTable insertAndDeleteInLandingTable = new InsertAndDeleteInLandingTable())
                {
                    insertAndDeleteInLandingTable.DeleteDataFromLandingTable(inputtableName, Convert.ToInt32(sessionId));
                }
                if (ex.Message != null)
                {
                    outMsg = ex.Message;                   
                }
                if (ex.Message.ToString().Contains("ORA-20001") || ex.Message.ToString().Contains("ORA-20002"))
                {
                    string message = ex.Message.ToString();
                    outMsg = message.Substring(message.IndexOf("ERROR:", 0) + 7, message.IndexOf("\n") - 18);
                }
                else
                {
                    if (ex.Message.ToString().Contains("diagnostic text: "))
                    {
                        int startPros = ex.Message.ToString().IndexOf("diagnostic text: ") + "diagnostic text: ".Length;
                        int noChars = 100;
                        if (ex.Message.ToString().Contains("SQLSTATE="))
                            noChars = ex.Message.ToString().IndexOf("SQLSTATE") - startPros;
                        outMsg = ex.Message.ToString().Substring(startPros, noChars);
                    }
                    else
                        outMsg = ex.Message.ToString();
                    if (ex.Message.ToString().Contains("WARNING:"))
                    {

                    }
                    else if (ex.Message != null)
                    {
                        outMsg = ex.Message;
                    }
                    else if (ex.Message.ToString().Contains("Unique"))
                    {
                        outMsg = ex.Message.ToString().Substring(ex.Message.IndexOf("Unique"), ex.Message.ToString().IndexOf(",") - ex.Message.ToString().IndexOf("Unique"));
                    }
                    else if (ex.Message.ToString().ToUpper().Contains("THIS RECORD IS MODIFIED BY"))
                    {
                        outMsg = "This record is modified by another user. Please try again later.";
                    }
                    else if (ex.Message.ToString().ToUpper().Contains("DOES NOT EXIST IN THE MASTER"))
                    {
                        outMsg = ex.Message.ToString().Substring(ex.Message.IndexOf("~"), ex.Message.ToString().LastIndexOf("~") - ex.Message.ToString().IndexOf("~") + 1);

                    }
                    else if (ex.Message.ToString().ToUpper().Contains("CODE ALREADY EXISTS"))
                    {
                        outMsg = "Code already exists.";
                    }
                    else if (ex.Message.ToString().ToUpper().Contains("IT IS AN INACTIVE ENTITY"))
                    {
                        outMsg = "InActive entity.";
                    }
                    else
                    {
                        outMsg = DateTime.Now + ":Please Contact MPP Support Team";
                    }
                }
            }
            return outMsg;
        }


        public string ValidateData(List<Entity_Type_Attr_Detail> attributeList, string entityTypeId, string userID,
         Nullable<decimal> suppressWarning, string strRejectFilePath, string tableName, string strExport, string strSessionId, out int loadErrorCount,
         out bool hasLoadErrors, out bool download)
        {
            string outMsg = Constant.statusSuccess;
            hasLoadErrors = false;
            download = true;
            loadErrorCount = 0;
            try
            {
                using (MPP_Context objMdmContext = new MPP_Context())
                {
                    objMdmContext.Procedures.MPP_LOAD_CHKAsync(strSessionId, entityTypeId, userID.ToUpper(), suppressWarning).GetAwaiter().GetResult(); ;
                }
            }
            catch (Exception ex)

            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                if (ex.InnerException.ToString().Contains("ORA-20001") || ex.InnerException.ToString().Contains("ORA-20002"))
                {
                    hasLoadErrors = true;
                    string message = ex.InnerException.Message.ToString();
                    outMsg = message.Substring(message.IndexOf("ERROR:", 0) + 7, message.IndexOf("\n") - 18);
                }
                else
                {
                    if (entityTypeId != "88" && entityTypeId != "91")
                    {
                        string FormedQuery = "MPP_CORE.SP_GETMAXDATE_DIMENSION";
                        int Status = WriteToFlatFileWithProcedure(strRejectFilePath, FormedQuery, ",", true, false, Convert.ToInt16(entityTypeId), strSessionId);
                        hasLoadErrors = true;
                        using (InsertAndDeleteInLandingTable objInsertAndDeleteInLandingTable = new InsertAndDeleteInLandingTable())
                        {
                            objInsertAndDeleteInLandingTable.DeleteDataFromLandingTableOnRowStatus(tableName, Convert.ToInt32(strSessionId), out loadErrorCount);
                        }
                    }
                }
            }
            return outMsg;
        }


        public int WriteToFlatFileWithProcedure(string FileName,
 string FormedQueryString, string Delimiter, bool IsColumnheaderToBeAdded, bool boolAppend, int entityTypeId, string sessionId)
        {

            DataSet dsResult = new DataSet();
            int intDebugCounter;

            intDebugCounter = 5;

            try
            {


                using (GetDataSetValue objGetDataSetValue = new GetDataSetValue())
                {
                    dsResult = objGetDataSetValue.GetDataSetWithParameter(FormedQueryString, entityTypeId, sessionId);
                }

            }
            catch (Exception ex)
            {
                throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                            "Error : Unable to execute the query. ", ex,
                            "Query formed = " + FormedQueryString + ";", g_UserID, g_LoadID, 3);
            }

            try
            {
                dsResult.Tables[0].Columns.Remove("LD_OID");
                if (WriteDataSetToFlatFile(FileName, dsResult, Delimiter, IsColumnheaderToBeAdded, boolAppend) != 1)
                    throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                "Error : Cannot finish writing to Flatfile from Dataset. ", null,
                                "Query formed = " + FormedQueryString + ";", g_UserID, g_LoadID, 3);
            }
            catch (ETLLoaderExceptionnew ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                "Error : while trying to write dataset to file. ", ex,
                                "Query formed = " + FormedQueryString + ";", g_UserID, g_LoadID, 3);
            }

            //cleanup
            try
            {
                intDebugCounter = 1;
                dsResult = null;
            }
            catch (Exception ex)
            {
                throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                            "Error : while trying to release resources. ", ex,
                                            "", g_UserID, g_LoadID, 3);
            }
            return intDebugCounter;
        }
        internal int WriteDataSetToFlatFile(string FileName, DataSet dsResult, string Delimiter, bool IsColumnheaderToBeAdded, bool boolAppend)
        {
            StreamWriter swCSVWriter = null;
            string TextToBeWritten = "";
            StringBuilder TextData = new StringBuilder();
            int Counter;
            int intDebugCounter = 5;

            if (FileName.Length == 0)
            {
                throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                        "Error : File Name cannot be null.", null, "",
                        g_UserID, g_LoadID, 3);
            }

            if (IsColumnheaderToBeAdded)
            {
                intDebugCounter = 30;
                TextToBeWritten = "";
                Counter = 0;
                DataColumnCollection dataCols = dsResult.Tables[0].Columns;
                for (Counter = 0; Counter < dataCols.Count; Counter++)
                {

                    TextToBeWritten += dataCols[Counter].ColumnName;
                    if (Counter != dataCols.Count - 1)
                        TextToBeWritten += Delimiter;
                }
                intDebugCounter = 35;
                try
                {
                    intDebugCounter = 40;
                    TextData.AppendLine(TextToBeWritten);
                }
                catch (Exception ex)
                {
                    throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                        "Error : Unable to write into file. ", ex,
                                        "Data to be written = " + TextToBeWritten + ";", g_UserID, g_LoadID, 3);
                }

            }
            intDebugCounter = 50;

            TextToBeWritten = "";
            intDebugCounter = 60;

            // To write data into the stream.
            DataRowCollection dataRows = dsResult.Tables[0].Rows;
            foreach (DataRow dataRow in dataRows)
            {
                intDebugCounter = 65;
                Counter = 0;
                TextToBeWritten = "";
                for (Counter = 0; Counter < dsResult.Tables[0].Columns.Count; Counter++)
                {
                    //if (dataRow[Counter].ToString().Contains(Delimiter))
                    //{


                    //    TextToBeWritten += @"""" + dataRow[Counter] + @"""";

                    //}
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

                try
                {
                    intDebugCounter = 70;
                    TextData.AppendLine(TextToBeWritten);
                }
                catch (Exception ex)
                {
                    throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                        "Error : Unable to write into file. ", ex,
                                        "Data to be written = " + TextToBeWritten + ";", g_UserID, g_LoadID, 3);
                }
                intDebugCounter = 75;
            }

            if (FileName.Length > 0)
            {
                if (boolAppend == false)
                {
                    try
                    {
                        intDebugCounter = 15;
                        swCSVWriter = File.CreateText(FileName);
                    }
                    catch (Exception ex)
                    {
                        throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                            "Error : Unable to open file. ", ex, "Flat File Name = " + FileName + ";",
                            g_UserID, g_LoadID, 3);
                    }
                    try
                    {
                        swCSVWriter.Write(TextData);
                        swCSVWriter.Close();
                        swCSVWriter = null;
                    }
                    catch (Exception ex)
                    {
                        throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                "Error : Unable to write to file. ", ex, "Flat File Name = " + FileName + ";",
                                g_UserID, g_LoadID, 3);
                    }
                }
                else
                {
                    try
                    {
                        File.AppendAllText(FileName, TextData.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new ETLLoaderExceptionnew("WriteToFlatFile", intDebugCounter,
                                "Error : Unable to write to file. ", ex, "Flat File Name = " + FileName + ";",
                                g_UserID, g_LoadID, 3);
                    }
                }
            }
            try
            {
                intDebugCounter = 1;
                dsResult = null;
            }
            catch (Exception ex)
            {
                throw new ETLLoaderExceptionnew("WriteDataSetToFlatFile", intDebugCounter,
                                            "Error : while trying to release resources. ", ex,
                                            "", g_UserID, g_LoadID, 3);
            }
            return intDebugCounter;
        }


    }
}
