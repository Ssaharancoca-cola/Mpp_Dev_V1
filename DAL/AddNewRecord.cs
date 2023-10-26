using DAL.Common;
using Microsoft.EntityFrameworkCore;
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
                using (GetSequenceValue getSequenceValue = new GetSequenceValue())
                {
                    ldOid = getSequenceValue.GetNextSequanceValue("MPP_CORE.SEQ_LD_OID", out outMsg);
                    if (outMsg != Constant.statusSuccess) return outMsg;
                }
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    inputTableName = mPP_Context.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.InputTableName).FirstOrDefault();
                }
                using (InsertAndDeleteInLandingTable insertAndDelete = new InsertAndDeleteInLandingTable())
                {
                    outMsg = insertAndDelete.DeleteDataFromLandingTable(inputTableName, sessionID);
                }
                if (outMsg != Constant.statusSuccess) return outMsg;

                outMsg = InsertAndDeleteInLandingTable(attrList, attrValues, sourceSystemName, sessionID, ldOid, inputTableName);
                if(outMsg != Constant.statusSuccess)
                    return outMsg;
                using (DataValidationUsingSP dataValidationUsingSP = new DataValidationUsingSP())
                {
                    outMsg = dataValidationUsingSP.ValidateData(sessionID.ToString(), entityTypeId.ToString(), userName, bSupressWarning, inputTableName);
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
        private string InsertAndDeleteInLandingTable(List<Entity_Type_Attr_Detail> attrList, Dictionary<string, string> attrValues, string sourceSystemName, int sessionID, int ldOid, string inputTablename)
        {
            string outMsg = Constant.statusSuccess;
            StringBuilder insertQuery = new StringBuilder();
            StringBuilder whereClause = new StringBuilder();
            StringBuilder selectColumn = new StringBuilder();
            int noOfRowInserted = 0;
            string whereClauseData = string.Empty;
            try
            {
                foreach (var data in attrValues)
                {
                    whereClauseData = data.Value ?? "NULL";
                    string attrData = Convert.ToString(attrList.Find(x => x.AttrName == data.Key.ToString()));
                    if (!string.IsNullOrEmpty(attrData))
                    {
                        selectColumn.Append(data.Key + " , ");
                        string attrDataType = attrList.Find(x => x.AttrName == data.Key.ToString()).AttrDataType;
                        switch (attrDataType)
                        {
                            case "VC":
                            case "SUPPLIED_CODE":
                            case "PARENT_CODE":
                                whereClause.Append(" '" + whereClauseData + "' , ");
                                break;
                            case "N":
                                if (whereClauseData == "")
                                {
                                    whereClause.Append("" + whereClauseData + "NULL, ");
                                    break;
                                }
                                else
                                {
                                    whereClause.Append("" + whereClauseData + ", ");
                                    break;
                                }
                            case "DT":
                                whereClause.Append("" + whereClauseData + ", ");
                                break;
                        }
                    }
                    else
                    {
                        if (data.Key == "CURRENT_EDIT_LEVEL" || data.Key == "DATE_FROM")
                        {
                            selectColumn.Append(data.Key + " , ");
                            whereClause.Append(" " + whereClauseData + " , ");
                        }
                    }
                }
                int treat_nulls_as_nulls = 1;
                if (inputTablename.Contains("MAPPING"))
                {
                    selectColumn.Append("SESSION_ID ,SOURCE_SYSTEM_NAME, TREAT_NULLS_AS_NULLS,LD_OID ");
                }
                else
                {
                    selectColumn.Append("SESSION_ID ,SOURCE_SYSTEM_CODE, TREAT_NULLS_AS_NULLS,LD_OID ");
                }
                whereClause.Append("" + sessionID + ",'" + sourceSystemName + "'," + treat_nulls_as_nulls + "," + ldOid + "");
                insertQuery.Append(" INSERT INTO ");
                insertQuery.Append("MPP_APP." + inputTablename + "(");
                insertQuery.Append(selectColumn);
                insertQuery.Append(" ) VALUES (");
                insertQuery.Append(whereClause);
                insertQuery.Append(" ) ");
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    noOfRowInserted = mPP_Context.Database.ExecuteSqlRaw(insertQuery.ToString());
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
    }
}
