using DAL.Common;
using Model.Models;
using System.Data;
using System.Text;

namespace DAL
{
    public class HistoryData : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public DataSet GetHistoryDetails(int OIDCode, int entityTypeId, string userName, out string outMsg)
        {
            string viewName = string.Empty;
            string[] histViewname = null;
            outMsg = Constant.statusSuccess;
            string fieldList = string.Empty;
            string searchQuery = string.Empty;
            DataSet dsResult = new DataSet();
            List<EntityTypeAttr> attrNameList = new List<EntityTypeAttr>();
            List<EntityTypeData> listEntityTypeData = new List<EntityTypeData>();
            using (GetViewDetail objviewdetail = new GetViewDetail())
            {
                outMsg = objviewdetail.GetFieldList(entityTypeId, out fieldList, out attrNameList);
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(fieldList) || attrNameList.Count() == 0)
                    return dsResult;

                outMsg = objviewdetail.GetViewName(entityTypeId, userName.ToUpper(), out viewName);
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(viewName))
                    return dsResult;
                if (!string.IsNullOrEmpty(viewName))
                {
                    //histViewname = viewName.Split('.');
                    outMsg = GetSearchQuery(fieldList, viewName.Replace("MPP_APP.FL", "MPP_APP.FHL").ToString(), OIDCode, attrNameList, out searchQuery);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(searchQuery))
                        return dsResult;
                }
            }
            using (GetDataSetValue objGetDataSetValue = new GetDataSetValue())
            {
                dsResult = objGetDataSetValue.GetDataSet(searchQuery, out outMsg);
            }
            return dsResult;            

        }
        public string GetSearchQuery(string fieldList, string tableName, int OIDCode, List<EntityTypeAttr> attrNameList, out string searchQuery)
        {
            searchQuery = "";
            string outMsg = Constant.statusSuccess;
            string strAsSelectClause = string.Empty;
            try
            {
                fieldList = fieldList + "," + "DATE_FROM";
                string strWhereClause = " where " + Constant.historyViewEntityOIDColumnName + " in ( " + OIDCode + " ) ";
                StringBuilder strQuery = new StringBuilder();
                string strSelectClause = (fieldList == "*" ? "t.*" : fieldList);
                string orderby = " order by " + Constant.dateFromColumnName;
                
                if (outMsg != Constant.statusSuccess)
                    return outMsg;

                //strQuery.Append(" Select " + strSelectClause + " from ");
                //strQuery.Append( tableName);
                //strQuery.Append("  t");

                strQuery.Append(" Select " + strSelectClause + " from ");
                //strQuery.Append( tableName);
                if (tableName.Contains("31-DEC-2049"))
                {
                    strQuery.Append(" ( " + tableName + " ) ");
                }
                else
                {
                    strQuery.Append(" " + tableName + " ");
               }
                strQuery.Append("  t");
                strQuery.Append(strWhereClause);
                strQuery.Append(orderby);
                searchQuery = strQuery.ToString();
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
        public DataSet GetHistoryDataForWorkFLow(string tableName, string userName, int ldOID, out string outMsg)
        {
            string selectQuery = "Select U1.USER_NAME, L2.ROLE_NAME AS USER_LEVEL,U2.USER_NAME AS APPROVER_NAME,L1.ROLE_NAME AS APPROVER_LEVEL,LD.APPROVER_STATUS,LD.COMMENTS , LD.USER_ID ,LD.APPROVER_ID from MPP_APP." + tableName + " LD, MPP_CORE.MPP_ROLE_LEVEL L1, MPP_CORE.MPP_ROLE_LEVEL L2,MPP_CORE.MPP_USER U1,MPP_CORE.MPP_USER U2 Where LD.APPROVER_LEVEL = L1.ROLE_ID AND LD.USER_LEVEL = L2.ROLE_ID AND U1.USER_ID =LD.USER_ID AND U2.USER_ID=LD.APPROVER_ID AND LD.USER_ID = '" + userName.ToUpper() + "' AND LD.LD_OID = " + ldOID + " AND LD.Approver_Id IS NOT NULL ";
            DataSet dsResult = new DataSet();
            using (GetDataSetValue objGetDataSetValue = new GetDataSetValue())
            {
                dsResult = objGetDataSetValue.GetDataSet(selectQuery, out outMsg);
            }
            return dsResult;
        }
    }
}
