using DAL.Common;
using Model.Models;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;

namespace DAL
{
    public class SearchData : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public string GetDataType(string FieldName, int entityTypeId, out string outMsg)
        {
            string sqlQuery = "select  attr_data_type from entity_type_attr where attr_display_name= '" + FieldName + "'  and entity_type_id= " + entityTypeId + "";
            string dataType = string.Empty;
            outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context objmdmContext = new MPP_Context())
                {
                     dataType = objmdmContext.Database.SqlQueryRaw<string>(sqlQuery).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
            }
            return dataType;

        }
        public DataSet search(List<SearchParameter> searchParameter, int PageNo, int PageSize, string SortBy, string SortOrder, string UserName, int entityTypeId, out string OIDColumnName, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string fieldList = string.Empty;
            string searchQuery = string.Empty;
            string viewName = string.Empty;
            OIDColumnName = string.Empty;
            string[] tableName = null;
            OIDColumnName = string.Empty;
            DataSet dsResult = new DataSet();
            List<EntityTypeAttr> attrNameList = new List<EntityTypeAttr>();
            try
            {
                using (GetViewDetail objviewdetail = new GetViewDetail())
                {
                    outMsg = objviewdetail.GetViewName(entityTypeId, UserName.ToUpper(),out viewName);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(viewName))
                        return dsResult;
                    if (viewName.Length > 100)
                    {
                        string[] splitViewName = viewName.Split(' ');
                        int count = 0;
                        for (int i = 0; i < splitViewName.Length; i++)
                        {
                            if (splitViewName[i].Contains("MPP_APP"))
                                count++;
                            if (count == 2)
                            {
                                tableName = splitViewName[i].Split('.');
                                break;
                            }
                        }
                    }
                    else
                    {
                        tableName = viewName.Split('.');
                    }
                   outMsg = objviewdetail.GetOIDColumnNameByEntityId(entityTypeId, out OIDColumnName);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(OIDColumnName))
                        return dsResult;
                    outMsg = objviewdetail.GetFieldList(entityTypeId, out fieldList, out attrNameList);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(fieldList) || attrNameList.Count() == 0)
                        return dsResult;

                }
                if (viewName.ToUpper().Contains("FL_DSP"))
                    outMsg = GetSearchQuery1(fieldList, PageNo, PageSize, SortBy, SortOrder, viewName, searchParameter, attrNameList, OIDColumnName, out searchQuery);
                else
                    outMsg = GetSearchQuery(fieldList, PageNo, PageSize, SortBy, SortOrder, viewName, searchParameter, attrNameList, OIDColumnName, out searchQuery);

                if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(searchQuery))
                    return dsResult;

                using (GetDataSetValue objGetDataSetValue = new GetDataSetValue())
                {
                    dsResult = objGetDataSetValue.GetDataSet(searchQuery, out outMsg);
                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                viewName = "";
                outMsg = ex.Message;
            }
            return dsResult;

        }
        public string GetSearchQuery1(string fieldList, int PageNo, int PageSize, string SortBy, string SortOrder, string tableName, List<SearchParameter> searchParameter, List<EntityTypeAttr> attrNameList, string OIDColumnName, out string searchQuery)
        {
            string outMsg = Constant.statusSuccess;
            string strAsSelectClause = string.Empty;
            String strSubQuery1 = "";
            string strSubQuery2 = "";
            try
            {
                fieldList = fieldList + "," + "DATE_FROM" + "," + OIDColumnName;
                StringBuilder strQuery = new StringBuilder();
                string strWhereClause = GetWhereClause(searchParameter, tableName);
                string strSelectClause = (fieldList == "*" ? "t.*" : fieldList);
                string strSortClause = GetSortClause(SortBy, SortOrder, tableName);
                int RowStart = ((PageNo - 1) * PageSize) + 1;
                int RowEnd = PageNo * PageSize;

                strSubQuery2 = " ( select count(*) total_records from " + " ( " + tableName + " ) " + strWhereClause + " ) ";
                strSubQuery1 = " select rownum recno, t.* from " + " ( " + tableName + " ) " + " t " + strWhereClause;
                strSubQuery1 = " ( select t.* from (" + strSubQuery1 + ") t Where recno >= " + RowStart + " and recno <=  " + RowEnd + " ) ";

                strQuery.Append("Select *  from ( Select " + strSelectClause + ", recno, total_records, ceil(total_records / " + PageSize + ") total_pages from ");
                strQuery.Append(strSubQuery1);
                strQuery.Append(" t1, ");
                strQuery.Append(strSubQuery2);
                strQuery.Append(" t2 ");
                searchQuery = strQuery.ToString();



            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                searchQuery = string.Empty;
                outMsg = ex.Message;

            }
            return outMsg;
        }

        public string GetSearchQuery(string fieldList, int PageNo, int PageSize, string SortBy, string SortOrder, string tableName, List<SearchParameter> searchParameter, List<EntityTypeAttr> attrNameList, string OIDColumnName, out string searchQuery)
        {
            string outMsg = Constant.statusSuccess;
            string strAsSelectClause = string.Empty;
            try
            {
                //fieldList = fieldList + "," + "DATE_FROM" + "," + OIDColumnName;
                StringBuilder strQuery = new StringBuilder();
                string strWhereClause = GetWhereClause(searchParameter, tableName);
                string strSelectClause = (fieldList == "*" ? "t.*" : fieldList);
                string strSortClause = GetSortClause(SortBy, SortOrder, tableName);
                int RowStart = ((PageNo - 1) * PageSize) + 1;
                int RowEnd = PageNo * PageSize;
                Dictionary<string, string> PropNameType = new Dictionary<string, string>();

                //strQuery.Append("Select * from ( Select " + strSelectClause + ", row_number() over(order by " + strSortClause + " ) recno, count(*) over() Total_Records, ceil((count(*) over()) / " + PageSize.ToString() + ") Total_Pages from ");
                //strQuery.Append(" ( " + tableName + " ) ");
                //strQuery.Append(" t ");
                //strQuery.Append(strWhereClause);
                //strQuery.Append(") Where recno between ");
                //strQuery.Append(RowStart.ToString());
                //strQuery.Append(" and ");
                //strQuery.Append(RowEnd.ToString());
                //searchQuery = strQuery.ToString();
                strQuery.Append("Select * from ( Select " + strSelectClause + ", row_number() over(order by " + strSortClause + " ) recno, count(*) over() Total_Records, CEILING((count(*) over()) / " + PageSize.ToString() + ") Total_Pages from ");
                strQuery.Append("  " + tableName + "  ");
                strQuery.Append(" t ");
                strQuery.Append(strWhereClause);
                strQuery.Append(") AS SUBQUERY Where recno between ");
                strQuery.Append(RowStart.ToString());
                strQuery.Append(" and ");
                strQuery.Append(RowEnd.ToString());
                searchQuery = strQuery.ToString();


            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                searchQuery = string.Empty;
                outMsg = ex.Message;

            }
            return outMsg;
        }
        private string GetWhereClause(List<SearchParameter> searchParameter, string tableName)
        {
            string strStarPlaceholder = "|^=^|";
            string strQuestionPlaceholder = "|^#^|";
            StringBuilder strWhereClause = new StringBuilder();
            string value = "-1";
            strWhereClause.Append(" ");
            if (searchParameter != null && searchParameter.Count > 0)
            {
                strWhereClause.Append(" Where ");
                foreach (SearchParameter var in searchParameter)
                {
                    string lValue = "";
                    string rValue = "";
                    string op = " = ";
                    if (var.SearchValue != value)
                    {

                        if (strWhereClause.ToString().EndsWith("Where ") == false)
                        {
                            strWhereClause.Append(" and ");
                        }
                        switch (var.CompareType)
                        {
                            case SearchParameter.SearchCompareType.Equal:
                                switch (var.DataType)
                                {
                                    case "VARCHAR":
                                    case "VC":
                                    case "PARENT_CODE":
                                    case "SUPPLIED_CODE":
                                        lValue = "UPPER(" + var.DBFieldName + ")";
                                        rValue = "'" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "'";
                                        break;
                                    case "NUMERIC":
                                    case "INTEGER":
                                    case "DECIMAL":
                                    case "N":
                                        lValue = var.DBFieldName;
                                        rValue = var.SearchValue;
                                        break;
                                    case "DATE":
                                    case "DATETIME":
                                    case "DT":
                                        lValue = var.DBFieldName;
                                        rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                        break;
                                }
                                strWhereClause.Append(lValue);
                                strWhereClause.Append(op);
                                strWhereClause.Append(rValue);
                                break;
                            case SearchParameter.SearchCompareType.Like:

                                switch (var.DataType)
                                {
                                    case "VARCHAR":
                                    case "VC":
                                    case "PARENT_CODE":
                                    case "SUPPLIED_CODE":

                                        lValue = "UPPER(" + var.DBFieldName + ")";


                                        rValue = var.SearchValue;
                                        if (rValue.Contains(@"\*"))
                                        {
                                            rValue = rValue.Replace(@"\*", strStarPlaceholder);
                                        }
                                        if (rValue.Contains(@"\?"))
                                        {
                                            rValue = rValue.Replace(@"\?", strQuestionPlaceholder);
                                        }
                                        //if (rValue.Contains("*") || rValue.Contains("?"))
                                        //    op = " Like ";
                                        //else
                                        //    op = " = ";
                                        //rValue = "'" + rValue.ToUpper().Replace("\'", "\'\'").Replace('*', '%').Replace('?', '_') + "'";

                                        //rValue = rValue.Replace(strStarPlaceholder, @"*");
                                        //rValue = rValue.Replace(strQuestionPlaceholder, @"?");
                                        //if (rValue.Contains("*") || rValue.Contains("?"))
                                        //    op = " Like ";
                                        else
                                            op = " LIKE ";
                                        if (op == " LIKE ")
                                        {
                                            rValue = "  '%" + rValue + "%'";
                                        }
                                        else
                                        {
                                            rValue = "'" + rValue.ToUpper().Replace("\'", "\'\'").Replace('*', '%').Replace('?', '_') + "'";

                                            rValue = rValue.Replace(strStarPlaceholder, @"*");
                                            rValue = rValue.Replace(strQuestionPlaceholder, @"?");
                                        }
                                        break;
                                    case "NUMERIC":
                                    case "INTEGER":
                                    case "DECIMAL":
                                    case "N":
                                        lValue = var.DBFieldName;
                                        rValue = var.SearchValue;
                                        break;
                                    case "DATE":
                                    case "DATETIME":
                                    case "DT":
                                        lValue = var.DBFieldName;
                                        rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                        break;
                                }
                                strWhereClause.Append(lValue);
                                strWhereClause.Append(op);
                                strWhereClause.Append(rValue);
                                break;
                            case SearchParameter.SearchCompareType.IN:

                                op = " IN ";
                                switch (var.DataType)
                                {
                                    case "VARCHAR":
                                    case "VC":
                                    case "PARENT_CODE":
                                    case "SUPPLIED_CODE":
                                        lValue = var.DBFieldName;
                                        rValue = var.SearchValue;
                                        break;
                                    case "NUMERIC":
                                    case "INTEGER":
                                    case "DECIMAL":
                                    case "N":
                                        lValue = var.DBFieldName;
                                        rValue = var.SearchValue;
                                        break;
                                }
                                strWhereClause.Append(lValue);
                                strWhereClause.Append(op);
                                strWhereClause.Append("(");
                                strWhereClause.Append(rValue);
                                strWhereClause.Append(")");
                                break;

                            case SearchParameter.SearchCompareType.Custom:
                                rValue = var.SearchValue;
                                strWhereClause.Append(rValue);
                                break;

                            case SearchParameter.SearchCompareType.GreaterthanOrEqual:
                                switch (var.DataType)
                                {
                                    case "DATE":
                                    case "DATETIME":
                                    case "DT":
                                        lValue = var.DBFieldName;
                                        rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                        break;
                                }
                                strWhereClause.Append(lValue);
                                strWhereClause.Append(" >= ");
                                strWhereClause.Append(rValue);
                                break;

                            case SearchParameter.SearchCompareType.LessthanOrEqual:
                                switch (var.DataType)
                                {
                                    case "DATE":
                                    case "DATETIME":
                                    case "DT":
                                        lValue = var.DBFieldName;
                                        rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                        break;
                                }
                                strWhereClause.Append(lValue);
                                strWhereClause.Append(" <= ");
                                strWhereClause.Append(rValue);
                                break;

                            case SearchParameter.SearchCompareType.Between:
                                string[] dates = var.SearchValue.Split(',');
                                string fromDate = dates[0].ToString();
                                string toDate = dates[1].ToString();
                                DateTime dtFrom = Convert.ToDateTime(dates[0].ToString());
                                DateTime dtTo = Convert.ToDateTime(dates[1].ToString());
                                if (DateTime.Compare(dtFrom, dtTo) == 0)
                                {
                                    switch (var.DataType)
                                    {
                                        case "DATE":
                                        case "DATETIME":
                                        case "DT":
                                            lValue = var.DBFieldName;
                                            rValue = "TO_DATE('" + fromDate.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                            break;
                                    }
                                    strWhereClause.Append(lValue);
                                    strWhereClause.Append(op);
                                    strWhereClause.Append(rValue);
                                    break;
                                }
                                else
                                {
                                    switch (var.DataType)
                                    {
                                        case "DATE":
                                        case "DATETIME":
                                        case "DT":

                                            lValue = var.DBFieldName;
                                            rValue = "TO_DATE('" + fromDate.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY') AND TO_DATE('" + toDate.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY')";
                                            break;
                                    }
                                    strWhereClause.Append(lValue);
                                    strWhereClause.Append(" BETWEEN ");
                                    strWhereClause.Append(rValue);
                                    break;
                                }

                            default:
                                break;
                        }
                    }

                }
            }
            if (tableName.Contains("FL_STATE_PROVINCE"))
            {
                if (strWhereClause.ToString().Contains("Where"))
                {
                    strWhereClause.Append(" AND STATE_CODE <> '-' ");
                }
                else
                {
                    strWhereClause.Append(" Where STATE_CODE <> '-' ");
                }
            }
            return strWhereClause.ToString();
        }
        private string GetSortClause(string SortBy, string SortOrder, string tableName)
        {
            string strSortClause = "";

            //if (SortBy.ToUpper() == "PROPERTY_ID" || SortBy.ToUpper() == "SUB_CHAIN_CODE" || SortBy.ToUpper() == "CHAIN_CODE" || SortBy.ToUpper() == "CORPORATE_CHAIN_CODE")
            //{
            //    strSortClause = "TO_NUMBER(" + SortBy + ")";
            //}
            //else
            //{
            //    strSortClause = SortBy;
            //}

            //strSortClause += " " + SortOrder;

            // Formulate additional sort by to make ensure unique sorting 
            if (tableName.ToUpper().StartsWith("MPP_APP.FL_"))
            {
                if (tableName.ToUpper() == "MPP_APP.FL_DSP")
                {
                    strSortClause += ", PROPERTY_SEQ ASC ";
                }
                else
                {
                    strSortClause +=  tableName.ToUpper().Replace("MPP_APP.FL_", "") + "_OID ASC ";
                }
            }

            return strSortClause;
        }
    }
}
