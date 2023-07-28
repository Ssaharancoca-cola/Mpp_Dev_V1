using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetSessionValue : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public int GetNextSessionValue(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            int sessionID = 0;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context("MPPAppContext"))
                {
                    // sessionID = mPP_Context.Database.SqlQueryRaw<int>("SELECT MPP_APP.SEQ_SESSION.NEXTVAL AS SESSION_ID FROM DUAL").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return sessionID;
        }
    }
}
