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

        //public string ValidateData(string sessionId, string entityTypeId, string userID, decimal? suppressWarning, string inputtableName)
        //{
        //    string outMsg = Constant.statusSuccess;
        //    try
        //    {
        //        using(MPP_ContextProcedures mPP_Context = new MPP_ContextProcedures())
        //        {
        //            mPP_Context.MPP_Load_Check(sessionId, entityTypeId, userID.ToUpper(), suppressWarning);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        outMsg = ex.Message;
        //        using (LogError logError = new LogError())
        //        {
        //            logError.LogErrorInTextFile(ex);
        //        }
        //        using
        //    }
        //}
    }
}
