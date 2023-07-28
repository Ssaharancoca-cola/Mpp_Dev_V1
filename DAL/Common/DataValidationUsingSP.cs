using Model.Models;
using System;
using System.Collections.Generic;
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
                using (MPP_ContextProcedures mPP_Context = new MPP_ContextProcedures())
                {
                    mPP_Context.MPP_LOAD_CHKAsync(sessionId, entityTypeId, userID.ToUpper(), suppressWarning);
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
                if (ex.InnerException.ToString().Contains("75001"))
                {
                    try
                    {
                        using (InsertAndDeleteInLandingTable insertAndDelete = new InsertAndDeleteInLandingTable())
                        {
                            if (ex.InnerException.ToString().Contains("WARNING:"))
                            {
                                throw new Exception(insertAndDelete.GetErrorOrWarning(inputtableName, "WARNING", sessionId));
                            }
                            else if (ex.InnerException.ToString().Contains("ERROR:"))
                            {
                                throw new Exception(insertAndDelete.GetErrorOrWarning(inputtableName, "ERROR", sessionId));
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        using (LogError objLogError = new LogError())
                        {
                            objLogError.LogErrorInTextFile(ex);
                        }
                        throw ex1 as Exception;
                    }
                }
                if (ex.InnerException.ToString().Contains("ORA-20001") || ex.InnerException.ToString().Contains("ORA-20002"))
                {
                    string message = ex.InnerException.Message.ToString();
                    outMsg = message.Substring(message.IndexOf("ERROR:", 0) + 7, message.IndexOf("\n") - 18);
                }
                else
                {
                    if (ex.InnerException.ToString().Contains("diagnostic text: "))
                    {
                        int startPros = ex.InnerException.ToString().IndexOf("diagnostic text: ") + "diagnostic text: ".Length;
                        int noChars = 100;
                        if (ex.InnerException.ToString().Contains("SQLSTATE="))
                            noChars = ex.InnerException.ToString().IndexOf("SQLSTATE") - startPros;
                        outMsg = ex.InnerException.ToString().Substring(startPros, noChars);
                    }
                    else
                        outMsg = ex.InnerException.ToString();
                    if (ex.InnerException.ToString().Contains("WARNING:"))
                    {

                    }
                    else if (ex.InnerException.ToString().Contains("Unique"))
                    {
                        outMsg = ex.InnerException.ToString().Substring(ex.InnerException.Message.IndexOf("Unique"), ex.InnerException.Message.ToString().IndexOf(",") - ex.InnerException.Message.ToString().IndexOf("Unique"));
                    }
                    else if (ex.InnerException.ToString().ToUpper().Contains("THIS RECORD IS MODIFIED BY"))
                    {
                        outMsg = "This record is modified by another user. Please try again later.";
                    }
                    else if (ex.InnerException.ToString().ToUpper().Contains("DOES NOT EXIST IN THE MASTER"))
                    {
                        outMsg = ex.InnerException.ToString().Substring(ex.InnerException.Message.IndexOf("~"), ex.InnerException.Message.ToString().LastIndexOf("~") - ex.InnerException.Message.ToString().IndexOf("~") + 1);

                    }
                    else if (ex.InnerException.ToString().ToUpper().Contains("CODE ALREADY EXISTS"))
                    {
                        outMsg = "Code already exists.";
                    }
                    else if (ex.InnerException.ToString().ToUpper().Contains("IT IS AN INACTIVE ENTITY"))
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
    }
}
