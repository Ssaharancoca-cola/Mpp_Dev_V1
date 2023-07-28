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
        public string GetViewName(int entityTypeId, string userName, out string viewName)
        {
            string outMsg = Constant.statusSuccess;            
            try
            {
                using (MPP_ContextProcedures mPP_Context = new MPP_ContextProcedures())
                {
                    var viewnameParameter = new ObjectParameter("view_name", typeof(string));
                     mPP_Context.GetViewName(entityTypeId, userName.ToUpper(), viewnameParameter, viewName = viewnameParameter.Value.ToString());
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
