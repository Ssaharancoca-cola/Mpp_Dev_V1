using DAL.Common;
using Model.Models;
using Model;
using System.Data;
using System.Text;

namespace MPP.ViewModel
{
    //public class SearchDataViewModel : IDisposable
    //{
    //    void IDisposable.Dispose()
    //    {

    //    }
    //    public void Search(List<SearchParameter> searchParameter, int pageNo, int pageSize, string sortBy, string sortOrder, string userName, int entityTypeId, out string outMsg, out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList, out int totalRecord)
    //    {
    //        columnData = string.Empty;
    //        rowData = new List<string>();
    //        DataSet dsResult = new DataSet();
    //        string OIDColumnName = string.Empty;
    //        dataList = new List<Dictionary<string, string>>();
    //        using (SearchData objMasterData = new SearchData())
    //        {
    //            dsResult = objMasterData.search(searchParameter, pageNo, pageSize, sortBy, sortOrder, userName, entityTypeId, out OIDColumnName, out outMsg);
    //        }
    //        GetDataFromDataSet(dsResult, entityTypeId, OIDColumnName, out columnData, out rowData, out dataList, out totalRecord);

    //    }
    //    public string GetDataFromDataSet(DataSet dsResult, int entityTypeId, string OIDColumnName, out string columnData, out List<string> rowData, out List<Dictionary<string, string>> dataList, out int totalRecord)
    //    {
    //        totalRecord = 0;
    //        rowData = new List<string>();
    //        string outMsg = Constant.statusSuccess;
    //        dataList = new List<Dictionary<string, string>>();
    //        StringBuilder strCoulmnDetail = new StringBuilder();
    //        DataColumnCollection dataCols = dsResult.Tables[0].Columns;
    //        List<EntityTypeAttr> attributeList = new List<EntityTypeAttr>();

    //        try
    //        {
    //            using (MPP_Context objMdmContext = new MPP_Context())
    //            {
    //                attributeList = objMdmContext.EntityTypeAttr.Where(x => x.EntityTypeId == entityTypeId).ToList();
    //            }
    //            if (dsResult.Tables.Count > 0)
    //            {
    //                DataRowCollection dataRows = dsResult.Tables[0].Rows;
    //                foreach (DataRow dataRow in dataRows)
    //                {
    //                    StringBuilder strRowDetail = new StringBuilder();
    //                    Dictionary<string, string> listRowData = new Dictionary<string, string>();
    //                    for (int i = 0; i < dsResult.Tables[0].Columns.Count; i++)
    //                    {
    //                        string strcolName = string.Empty;
    //                        string colName = string.Empty;
    //                        string coldatatype = string.Empty;

    //                        strcolName = dataCols[i].ColumnName.ToString();
    //                        coldatatype = dataCols[i].DataType.FullName.ToString();
    //                        if (strcolName == Constant.dateFromColumnName)
    //                            colName = "Effective Date";
    //                        else if (strcolName == OIDColumnName)
    //                            colName = "OID";
    //                        //else if(strcolName == "TOTAL_RECORDS")
    //                        //    colName = "Total Record";

    //                        else
    //                            colName = attributeList.Where(x => x.AttrName == strcolName).Select(x => x.AttrDisplayName).FirstOrDefault();
    //                        if (!string.IsNullOrEmpty(colName))
    //                        {
    //                            strCoulmnDetail.Append(colName + ",");
    //                            if (coldatatype == Constant.datedatatype)
    //                            {
    //                                string dtcol = string.Empty;
    //                                if (dataRow[i].ToString() != "")
    //                                {
    //                                    dtcol = Convert.ToDateTime(dataRow[i].ToString()).ToShortDateString();
    //                                }

    //                                listRowData.Add(colName, dtcol);
    //                            }
    //                            else
    //                                listRowData.Add(colName, dataRow[i].ToString());
    //                            strRowDetail.Append(dataRow[i].ToString() + ",");
    //                        }
    //                        if (strcolName == "TOTAL_RECORDS")
    //                        {
    //                            totalRecord = Convert.ToInt32(dataRow[i]);
    //                        }
    //                    }
    //                    dataList.Add(listRowData);
    //                    rowData.Add(strRowDetail.ToString().Trim(','));
    //                    strRowDetail.AppendLine();
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            using (LogError objLogError = new LogError())
    //            {
    //                objLogError.LogErrorInTextFile(ex);
    //            }
    //            outMsg = ex.Message;
    //        }
    //        columnData = Convert.ToString(strCoulmnDetail);
    //        return outMsg;
    //    }
    //    public string GetDataType(string FieldName, int entityTypeId, out string outMsg)
    //    {
    //        outMsg = Constant.statusSuccess;
    //        string dataType = string.Empty;
    //        try
    //        {
    //            using (SearchData objMasterData = new SearchData())
    //            {
    //                dataType = objMasterData.GetDataType(FieldName, entityTypeId, out outMsg);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            outMsg = ex.Message;
    //        }
    //        return dataType;
    //    }
        
    //}
}
