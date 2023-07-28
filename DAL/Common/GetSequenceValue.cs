using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetSequenceValue : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        //To get sequence next value 

        public int GetNextSequanceValue(string sequencename, out string outMsg)
        {
            outMsg = Constant.statusSuccess; 
            int nextVal = 0;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                   // nextVal = mPP_Context.Database.SqlQuery<int>("select" + sequencename + ".NEXTVAL from dual").FirstOrDefault();
                    return nextVal;
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
            return nextVal;
        }
        //Get Max Id for Entity Type
        public int GetMaxIdForEntityType()
        {
            using (MPP_Context mPP_Context = new MPP_Context())
            {
                var nextVal = mPP_Context.Database.SqlQueryRaw<int>("select max(ID) from Mpp_Core.entity_type").FirstOrDefault();
                return nextVal;
            }
        }
    }
}
