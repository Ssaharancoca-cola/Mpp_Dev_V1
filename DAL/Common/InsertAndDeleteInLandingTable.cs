using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class InsertAndDeleteInLandingTable : IDisposable
    {
        void IDisposable.Dispose()
        {
           
        }
        public string DeleteDataFromLandingTable(string tableName, int sessionId)
        {
            string outMsg = Constant.statusSuccess;
            string deleteQuery = "DELETE FROM MPP_APP." + tableName +" WHERE SESSION_ID = " + "'" + sessionId + "'";
            int noOfRowDeleted;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    noOfRowDeleted = mPP_Context.Database.ExecuteSqlRaw(deleteQuery);
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
        public string GetErrorOrWarning(string tableName, string errorOrWarning, string sessionID)
        {
            string dbCommand = null;
            string outPut = "Could not read the Error/Warning from Staging Table.";
            try
            {
                if (errorOrWarning == "WARNING")
                    dbCommand = "SELECT 'WARNING: ' ||WARNING_MESSAGE AS WARNING_MESSAGE FROM MPP_APP." + tableName + "WHERE SESSION_ID =" + "'" + sessionID + "'";
                else
                    dbCommand = "SELECT 'ERROR: ' ||ERROR_MESSAGE AS ERROR_MESSAGE FROM MPP_APP." + tableName + "WHERE SESSION_ID =" + "'" + sessionID + "'";
                using(MPP_Context mPP_Context = new MPP_Context())
                {
                    mPP_Context.Database.ExecuteSqlRaw(dbCommand);
                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                //bool rethrow = true;
                //rethrow = Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionPolicy.HandleException(ex, "Database policy");
                //if (rethrow)
                //{
                //    throw;
                //}
            }
            return outPut;
        }
    }
}
