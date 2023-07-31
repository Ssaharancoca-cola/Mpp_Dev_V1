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

    public class EntityTypeData
    {
        public string type1 { get ; set;}
        public string type2 { get; set;}    
        public string type3 { get; set;}            
        public string type4 { get; set;}
        public string type5 { get; set;}
        public string type6 { get; set;}
        public string type7 { get; set;}
        public string type8 { get; set;}
        public string type9 { get; set;}
        public string type10 { get; set;}
        public string type11 { get; set;}
        public string type12 { get; set;}
        public string type13 { get; set;}
        public string type14 { get; set;}
        public string type15 { get; set;}
        public string type16 { get; set;}
        public string type17 { get; set;}
        public string type18 { get; set;}
        public string type19 { get; set;}
        public string type20 { get; set;}
        public int type21 { get; set;}
        public int type22 { get; set;}
        public int type23 { get; set;}
        public int type24 { get; set;}
        public int type25 { get; set;}
        public int type26 { get; set;}
        public int type27 { get; set;}
        public int type28 { get; set;}
        public int type29 { get; set;}
        public int type30 { get; set;}
        public int type31 { get; set;}
        public int type32 { get; set;}
        public int type33 { get; set;}
        public int type34 { get; set;}
        public int type35 { get; set;}
        public int type36 { get; set;}
        public int type37 { get; set;}
        public int type38 { get; set;}
        public int type39 { get; set;}
        public int type40 { get; set;}
        public DateTime type41 { get; set;}
        public DateTime type42 { get; set;}
        public DateTime type43 { get; set;}
        public DateTime type44 { get; set;}
        public DateTime type45 { get; set;}
        public DateTime type46 { get; set;}
        public DateTime type47 { get; set;}
        public DateTime type48 { get; set;}
        public DateTime type49 { get; set;}
        public DateTime type50 { get; set;}

    }
}
