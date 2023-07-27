using DAL.Common;
using Model;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class AddNewRecord : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public string SaveRecord(List<Entity_Type_Attr_Detail> attrList, Dictionary<string, string> attrValues, int entityTypeId, string userName, int bSupressWarning, string sourceSystemName, string languageCode)
        {
            string outMsg = Constant.statusSuccess;
            string inputTableName = string.Empty;
            int ldOid = 0;
            int sessionID = 0;
            try
            {
                using (GetSessionValue getSessionValue = new GetSessionValue())
                {
                    sessionID = getSessionValue.GetNextSessionValue(out outMsg);
                    if (outMsg != Constant.statusSuccess) return outMsg;
                }
                using(GetSequenceValue getSequenceValue = new GetSequenceValue())
                {
                    ldOid = getSequenceValue.GetNextSequanceValue("MPP_CORE.SEQ_LD_OID", out outMsg);
                    if (outMsg != Constant.statusSuccess) return outMsg;
                }
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    inputTableName = mPP_Context.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.InputTableName).FirstOrDefault();
                }
                //using (Insert)
                //if (outMsg != Constant.statusSuccess) return outMsg;
                using (DataValidationUsingSP dataValidationUsingSP = new DataValidationUsingSP())
                {
                   // outMsg = dataValidationUsingSP.ValidateData(sessionID.ToString(), entityTypeId.ToString(), userName, bSupressWarning, inputTableName);
                    if (outMsg != Constant.statusSuccess) return outMsg;
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
            return outMsg;
        }
    
    }
}
