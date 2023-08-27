using DAL;
using DAL.Common;
using Model;
using Model.Models;
using System.Data;
using System.Text;

namespace MPP.ViewModel
{
    public class WorkFlowViewModel : IDisposable
    {
        private readonly string SubmittedStatus = "2";
        private readonly string RejectedStatus = "4";
        private readonly string ApproverStatusPending = "PENDING";
        void IDisposable.Dispose()
        {

        }
        public DataSet GetSubmittedRecords(string userName, string ApproverId, string status, int entityTypeid, out string outMsg,
            out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList)
        {
            columnData = string.Empty;
            string colName = string.Empty;
            outMsg = Constant.statusSuccess;
            string tableName = string.Empty;
            DataSet dsresult = new DataSet();
            string selectCommand = string.Empty;
            rowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    tableName = objMPP_Context.EntityType.Where(x => x.Id == entityTypeid).Select(x => x.InputTableName).FirstOrDefault();
                }
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    (attributeList, outMsg) = Task.Run(() => objMenuViewModel.ShowAttributeDataAsync(entityTypeid, "", userName.ToUpper())).Result;
                    attributeList = attributeList.Where(x => x.Isvisible != "N").OrderBy(x => x.AttrDisplayOrder).ToList();

                }
                colName = GetColumnName(attributeList, out outMsg);
                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(colName))
                    return dsresult;

                if (string.IsNullOrEmpty(ApproverId))
                {
                    if (status == "4")
                    {
                        colName += ",ROW_STATUS,LD_OID";
                        selectCommand = "Select " + colName + " from MPP_APP." + tableName + " Where User_Id = '" + userName.ToUpper() + "' AND ROW_STATUS IN (4,5) AND Approver_Id IS NULL";
                    }
                    else
                    {
                        int StatusValue = Convert.ToInt32(status);
                        colName = colName + ",STATUS,LD_OID";
                                        selectCommand = @"
                    SELECT " + colName + @" FROM (
                        SELECT 
                            CONCAT(
                            SUM(CASE WHEN APPROVER_ID IS NOT NULL AND APPROVER_STATUS = 'APPROVED' THEN 1 ELSE 0 END) OVER (PARTITION BY LD_OID), '/',
                            SUM(CASE WHEN APPROVER_ID IS NOT NULL THEN 1 ELSE 0 END) OVER (PARTITION BY LD_OID)
                            ) AS STATUS, LD.* 
                        FROM MPP_APP." + tableName + @" LD 
                        WHERE User_Id = '" + userName.ToUpper() + @"'
                    ) T
                    WHERE APPROVER_ID IS NULL AND ROW_STATUS = " + StatusValue;

                    }
                }
                else
                {
                    selectCommand = "Select " + colName + " from MPP_APP." + tableName + " Where Approver_Id = '" + ApproverId + "' AND Approver_Status = '" + status + "' AND ROW_STATUS = 3 ";
                }
                using (WorkFlow objWorkFlow = new WorkFlow())
                {
                    dsresult = objWorkFlow.GetSubmittedRecords(selectCommand, out outMsg);
                }
                GetDataFromDataSet(dsresult, entityTypeid, out columnData, out rowData, out dataList);
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return dsresult;

        }
        public string GetDataFromDataSet(DataSet dsResult, int entityTypeId, out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList)
        {
            string outMsg;
            rowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            StringBuilder strCoulmnDetail = new StringBuilder();
            DataColumnCollection dataCols = dsResult.Tables[0].Columns;
            try
            {
                outMsg = Constant.statusSuccess;
                for (int i = 0; i < dataCols.Count; i++)
                {

                    string colName = string.Empty;
                    string dscolName = dataCols[i].ColumnName.ToString();
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        colName = objMPP_Context.EntityTypeAttr.Where(x => x.AttrName == dscolName)
                            .Select(x => x.AttrDisplayName).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(colName))
                        strCoulmnDetail.Append(colName + ",");
                    if (dscolName == Constant.dateFromColumnName)
                        strCoulmnDetail.Append("Effective Date");
                    if (dscolName == "STATUS")
                        strCoulmnDetail.Append(",Status");
                    if (dscolName == Constant.inputRowIdColumnName)
                        strCoulmnDetail.Append(",inputRowId");
                    if (dscolName == "ROW_STATUS")
                        strCoulmnDetail.Append(",rowstatus");
                    if (dscolName == "LD_OID")
                        strCoulmnDetail.Append(",ldOID");

                }
                DataRowCollection dataRows = dsResult.Tables[0].Rows;
                foreach (DataRow dataRow in dataRows)
                {
                    StringBuilder strRowDetail = new StringBuilder();
                    Dictionary<string, string> listRowData = new Dictionary<string, string>();
                    for (int i = 0; i < dsResult.Tables[0].Columns.Count; i++)
                    {
                        listRowData.Add(dataCols[i].ColumnName.ToString(), dataRow[i].ToString());
                        strRowDetail.Append(dataRow[i].ToString() + ",");
                    }
                    dataList.Add(listRowData);
                    rowData.Add(strRowDetail.ToString().Trim(','));
                    strRowDetail.AppendLine();
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
            columnData = Convert.ToString(strCoulmnDetail);
            return outMsg;
        }
        public string GetColumnName(List<Entity_Type_Attr_Detail> attributeList, out string outMsg)
        {
            StringBuilder colName = new StringBuilder();
            try
            {
                outMsg = Constant.statusSuccess;
                foreach (var data in attributeList)
                {
                    colName.Append(data.AttrName + ",");
                }
                colName.Append(Constant.dateFromColumnName + ",");
                colName.Append(Constant.inputRowIdColumnName);
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return colName.ToString();
        }
        public string LoadContentView(int entityTypeId, out string submittedColumnData, out List<string> submittedRowData, out List<Dictionary<string, string>> dataList)
        {
            DataSet dsResult = new DataSet();
            string ApproverId = String.Empty;
            submittedColumnData = string.Empty;
            submittedRowData = new List<string>();
            string outMsg = Constant.statusSuccess;
            dataList = new List<Dictionary<string, string>>();

            try
            {

                string status = SubmittedStatus;
                
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                if (!String.IsNullOrEmpty(userName[1].ToString()))
                {
                    using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
                    {
                        dsResult = objWorkFlowViewModel.GetSubmittedRecords(userName[1].ToString().ToUpper(), ApproverId, status, entityTypeId,
                            out outMsg, out submittedColumnData, out submittedRowData, out dataList);
                    }

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
        public string LoadContentReject(int entityTypeId, out string rejectedColumnData, out List<string> rejectedRowData, out List<Dictionary<string, string>> dataList)
        {
            DataSet dsResult = new DataSet();
            string ApproverId = String.Empty;
            rejectedRowData = new List<string>();
            string outMsg = Constant.statusSuccess;
            rejectedColumnData = Constant.statusSuccess;
            dataList = new List<Dictionary<string, string>>();
            try
            {
                string status = RejectedStatus;
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                if (!String.IsNullOrEmpty(userName[1].ToString()))
                {
                    using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
                    {
                        dsResult = objWorkFlowViewModel.GetSubmittedRecords(userName[1].ToString().ToUpper(), ApproverId, status, entityTypeId,
                            out outMsg, out rejectedColumnData, out rejectedRowData, out dataList);
                    }

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
        public string LoadContentMyApproval(int entityTypeId, out string approvalPendingColumnData, out List<string> approvalPendingRowData,
            out List<Dictionary<string, string>> dataList, out string existingRecordColumnData, out List<string> existingRecordRowData,
            out List<Dictionary<string, string>> existingRecordDataList)
        {
            DataSet dsResult = new DataSet();
            string ApproverId = String.Empty;
            string outMsg = Constant.statusSuccess;
            approvalPendingColumnData = string.Empty;
            existingRecordColumnData = string.Empty;
            existingRecordRowData = new List<string>();
            approvalPendingRowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            existingRecordDataList = new List<Dictionary<string, string>>();
            try
            {
                string status = ApproverStatusPending;
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                ApproverId = userName[1].ToString().ToUpper();
                if (!String.IsNullOrEmpty(userName[1].ToString()))
                {
                    using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
                    {
                        dsResult = objWorkFlowViewModel.GetSubmittedRecords(userName[1].ToString(), ApproverId, status, entityTypeId,
                            out outMsg, out approvalPendingColumnData, out approvalPendingRowData, out dataList);
                        if (outMsg != Constant.statusSuccess)
                            return outMsg;
                        objWorkFlowViewModel.GetExistingRecordList(entityTypeId, userName[1].ToString(), status, out existingRecordColumnData, out existingRecordRowData,
                          out existingRecordDataList, out outMsg);
                        if (outMsg != Constant.statusSuccess)
                            return outMsg;


                    }

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
        public string UpdateStatus(string InputRowIds, string UserId, string Status, string Comments, int entityTypeid)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (WorkFlow objworkFlow = new WorkFlow())
                {
                    outMsg = objworkFlow.UpdateStatus(InputRowIds, UserId, Status, Comments, entityTypeid);
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

        public string GetExistingRecordList(int entityTypeId, string approverId, string status, out string columnData, out List<string> rowData,
         out List<Dictionary<string, string>> dataList, out string outMsg)
        {
            DataSet dsresult = new DataSet();
            outMsg = Constant.statusSuccess;
            columnData = string.Empty;
            rowData = new List<string>();
            string colName = string.Empty;
            dataList = new List<Dictionary<string, string>>();
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
              
                //string str = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    (attributeList, outMsg) = Task.Run(() => objMenuViewModel.ShowAttributeDataAsync(entityTypeId, "", userName[1].ToUpper())).Result;
                    attributeList = attributeList.OrderBy(x => x.AttrDisplayOrder).ToList();
                }
                colName = GetColumnName(attributeList, out outMsg);
                colName = colName.Replace(Constant.inputRowIdColumnName, "");
                // colName += "," + Constant.dateFromColumnName;
                using (WorkFlow objWorkFlow = new WorkFlow())
                {
                    dsresult = objWorkFlow.GetExistingRecordList(entityTypeId, colName.Trim(','), approverId.ToUpper(), status, out outMsg);
                }
                if (outMsg != Constant.statusSuccess)
                    return outMsg;
                GetDataFromDataSet(dsresult, entityTypeId, out columnData, out rowData, out dataList);

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