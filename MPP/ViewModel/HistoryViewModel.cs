using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Model;
using MPP.Filter;
using MPP.ViewModel;
using System.Data;
using System.Text;
using DAL;

namespace MPP.ViewModel
{
    public class HistoryViewModel : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public void GetHistoryDetails(int OIDCode, int entityTypeId, string userName, out string columnData, out List<string> rowData,
            out List<Dictionary<string, string>> dataList, out string outMsg)
        {
            columnData = string.Empty;
            rowData = new List<string>();
            DataSet dsResult = new DataSet();
            dataList = new List<Dictionary<string, string>>();
            using (HistoryData objHistoryData = new HistoryData())
            {
                dsResult = objHistoryData.GetHistoryDetails(OIDCode, entityTypeId, userName, out outMsg);
            }
            GetDataFromDataSet(dsResult, entityTypeId, out columnData, out rowData, out dataList);

        }
        public void GetHistoryDetailsForWorkFLow(string[] userName, int OIDCode, int entityTypeId, out string columnData, out List<string> rowData,
            out List<Dictionary<string, string>> dataList, out string outMsg)
        {
            string tableName = string.Empty;
            DataSet dsResult = new DataSet();
            columnData = string.Empty;
            rowData = new List<string>();
            dataList = new List<Dictionary<string, string>>();
            //string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);

            using (MPP_Context objMPP_Context = new MPP_Context())
            {
                tableName = objMPP_Context.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.InputTableName).FirstOrDefault();
            }
            using (HistoryData objHistoryData = new HistoryData())
            {
                dsResult = objHistoryData.GetHistoryDataForWorkFLow(tableName, userName[1], OIDCode, out outMsg);
            }
            GetDataFromDataSet(dsResult, out columnData, out rowData, out dataList);
        }
        public string GetDataFromDataSet(DataSet dsResult, int entityTypeId, out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList)
        {
            rowData = new List<string>();
            string outMsg = Constant.statusSuccess;
            dataList = new List<Dictionary<string, string>>();
            StringBuilder strCoulmnDetail = new StringBuilder();
            DataColumnCollection dataCols = dsResult.Tables[0].Columns;
            List<EntityTypeAttr> attributeList = new List<EntityTypeAttr>();

            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    attributeList = objMPP_Context.EntityTypeAttr.Where(x => x.EntityTypeId == entityTypeId).ToList();
                }
                if (dsResult.Tables.Count > 0)
                {
                    DataRowCollection dataRows = dsResult.Tables[0].Rows;
                    foreach (DataRow dataRow in dataRows)
                    {
                        StringBuilder strRowDetail = new StringBuilder();
                        Dictionary<string, string> listRowData = new Dictionary<string, string>();
                        for (int i = 0; i < dsResult.Tables[0].Columns.Count; i++)
                        {
                            string strcolName = string.Empty;
                            string colName = string.Empty;
                            string coldatatype = string.Empty;

                            strcolName = dataCols[i].ColumnName.ToString();
                            coldatatype = dataCols[i].DataType.FullName.ToString();
                            if (strcolName == Constant.dateFromColumnName)
                                colName = "Effective Date";
                            else
                                colName = attributeList.Where(x => x.AttrName == strcolName).Select(x => x.AttrDisplayName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(colName))
                            {
                                strCoulmnDetail.Append(colName + ",");
                                if (coldatatype == Constant.datedatatype)
                                {
                                    string dtcol = string.Empty;
                                    if (dataRow[i].ToString() != "")
                                    {
                                        dtcol = Convert.ToDateTime(dataRow[i].ToString()).ToShortDateString();
                                    }

                                    listRowData.Add(colName, dtcol);
                                }
                                else
                                    listRowData.Add(colName, dataRow[i].ToString());
                                strRowDetail.Append(dataRow[i].ToString() + ",");
                            }
                        }
                        dataList.Add(listRowData);
                        rowData.Add(strRowDetail.ToString().Trim(','));
                        strRowDetail.AppendLine();
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
            columnData = Convert.ToString(strCoulmnDetail);
            return outMsg;
        }

        public string GetDataFromDataSet(DataSet dsResult, out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList)
        {
            rowData = new List<string>();
            string outMsg = Constant.statusSuccess;
            dataList = new List<Dictionary<string, string>>();
            StringBuilder strCoulmnDetail = new StringBuilder();
            DataColumnCollection dataCols = dsResult.Tables[0].Columns;
            try
            {
                DataRowCollection dataRows = dsResult.Tables[0].Rows;
                foreach (DataRow dataRow in dataRows)
                {
                    StringBuilder strRowDetail = new StringBuilder();
                    Dictionary<string, string> listRowData = new Dictionary<string, string>();
                    for (int i = 0; i < dsResult.Tables[0].Columns.Count; i++)
                    {
                        string strcolName = string.Empty;
                        strcolName = dataCols[i].ColumnName.ToString();
                        string colName = string.Empty;
                        switch (strcolName)
                        {
                            case "USER_NAME":
                                colName = "User Name";
                                break;
                            case "USER_LEVEL":
                                colName = "User Level";
                                break;
                            case "APPROVER_NAME":
                                colName = "Approver Name";
                                break;
                            case "APPROVER_LEVEL":
                                colName = "Approver Level";
                                break;
                            case "APPROVER_STATUS":
                                colName = "Status";
                                break;
                            case "COMMENTS":
                                colName = "Comments";
                                break;

                        }
                        if (!string.IsNullOrEmpty(colName))
                        {
                            strCoulmnDetail.Append(colName + ",");
                            listRowData.Add(colName, dataRow[i].ToString());
                            strRowDetail.Append(dataRow[i].ToString() + ",");
                        }
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

    }
}