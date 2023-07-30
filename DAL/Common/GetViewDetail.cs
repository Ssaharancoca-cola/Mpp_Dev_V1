using Model.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetViewDetail : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public  string GetViewName(int entityTypeId, string userName, out string viewName)
        {
            string outMsg = Constant.statusSuccess;            
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    var viewNameOutput = new OutputParameter<string>();

                     mPP_Context.Procedures.GET_VIEW_NAMEAsync(entityTypeId, userName.ToUpper(), viewNameOutput);
                    viewName = viewNameOutput.Value;
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                viewName = "";
                outMsg = ex.Message;
            }
            return outMsg;
        }
    }
}
