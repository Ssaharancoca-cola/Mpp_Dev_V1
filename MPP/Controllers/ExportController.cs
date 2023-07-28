using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace MPP.Controllers
{
    public class ExportController : Controller
    {
        public ActionResult Index() { return View(); }

        [AcceptVerbs(HttpVerbs.Post)]

        public ActionResult ExportData(FormCollection form, string Command)
        {
            #region ExportCommand
            if (Command == "Export")
            {
                try
                {
                    string datatype = string.Empty;
                    string viewName = string.Empty;
                    string outMsg = Constant.statusSuccess;
                    int entityTypeId = Convert.ToInt32(Session["EntityTypeId"]);
                    string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                    string FileName = "E" + userName[1] + Convert.ToString(Session["EntityName"]) + ".csv";
                    string FilePath = Request.MapPath("") + @"\App_Data";
                    //string FileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // FilePath = FilePath + @"\Download\";
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    FilePath = FilePath + FileName;
                    Dictionary<string, string> attrValues = new Dictionary<string, string>();
                    List<Entity_Type_Attr_Detail> attrbuteList = new List<Entity_Type_Attr_Detail>();
                    attrbuteList = (List<Entity_Type_Attr_Detail>)TempData["attrbuteList"];
                    TempData.Keep();
                    StringBuilder strExport = new StringBuilder();
                    foreach (var data in attrbuteList)
                    {
                        if (data.Isvisible != "N")
                        {
                            string attrName = attrbuteList.Where(x => x.AttrDataType == "SUPPLIED_CODE").Where(y => y.AttrName == data.AttrName).Select(x => x.AttrName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(attrName))
                                strExport.Append(data.AttrName + ",");
                            strExport.Append(form[data.AttrName] == "false" ? "" : data.AttrName + ",");
                        }
                    }
                    strExport.Append(form[Constant.dateFromColumnName] == "false" ? "" : Constant.dateFromColumnName);
                    string whereClause = GetWhereClause((List<SearchParameter>)Session["fieldCollection"]);
                    string SortBy = string.IsNullOrEmpty(Convert.ToString(Session[""])) ? " 1 " : Convert.ToString(Session["currentField"]);
                    if (SortBy.Trim() != "")
                    {
                        datatype = attrbuteList.Find(x => x.AttrDisplayName == SortBy).AttrDataType;
                        SortBy = attrbuteList.Find(x => x.AttrDisplayName == SortBy).AttrName;
                    }
                    if (datatype == "N")
                    {
                        SortBy = "TO_NUMBER(" + SortBy + ")";
                    }
                    string sortOrder = string.IsNullOrEmpty(Convert.ToString(Session["SortOrder"])) ? "ASC" : Convert.ToString(Session["SortOrder"]);
                    SortBy += " " + sortOrder;
                    using (DataImportExportViewModel dataImportExportViewModel = new DataImportExportViewModel())
                    {
                        DataSet ds = new DataSet();
                        outMsg = dataImportExportViewModel.GetViewName(entityTypeId, userName[1].ToUpper(), out viewName);
                        if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(viewName))
                        {
                            return Content("error" + Constant.commonErrorMsg);
                        }
                        if (outMsg == Constant.statusSuccess)
                        {
                            TempData["filepath"] = FilePath;
                            string path = System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"];

                            return Content("export," + Session["EntityName"] + "," + FilePath);
                        }
                        else
                            return Content("error" + Constant.commonErrorMsg);
                    }
                }
                catch (Exception ex)
                {
                    using (LogErrorViewModel logErrorViewModel = new LogErrorViewModel())
                    {
                        logErrorViewModel.LogErrorInTextFile(ex);
                    }
                    return Content("error" + Constant.commonErrorMsg);
                }
            }
            #endregion

            #region cancelUpdateCommand
            else if (Command == "Cancel")
            {
                return RedirectToRoute(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(Session["EntityTypeId"]), entityName = Convert.ToInt32(Session["EntityName"]), viewType = "search" });
            }
            return View();
            #endregion
        }
        private string GetWhereClause(List<SearchParameter> _fields)
        {
            StringBuilder strQuery = new StringBuilder();
            string value = "-1";
            string strStarPlaceholder = "|^=^|";
            string strQuestionPlaceholder = "|^#^|";
            try
            {
                if (_fields != null && _fields.Count > 0)
                {
                    foreach (SearchParameter var in _fields)
                    {
                        string lValue = "";
                        string rValue = "";
                        string op = " = ";
                        if (var.SearchValue != value)
                        {
                            if (strQuery.ToString() != "")
                            {
                                strQuery.Append(" and ");
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
                                            rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "','MM/DD/YYYY";
                                            break;

                                    }
                                    strQuery.Append(lValue);
                                    strQuery.Append(op);
                                    strQuery.Append(rValue);
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
                                            if (rValue.Contains("*") || rValue.Contains("?"))
                                                op = " Like ";
                                            else
                                                op = " = ";
                                            rValue = "'" + rValue.ToUpper().Replace("\'", "\'\'").Replace('*', '%').Replace('?', '_') + "'";

                                            rValue = rValue.Replace(strStarPlaceholder, @"*");
                                            rValue = rValue.Replace(strQuestionPlaceholder, @"?");
                                            break;

                                        case "NUMERIC":
                                        case "INTEGER":
                                        case "DECIMAL":
                                        case "N":
                                            lValue = var.DBFieldName;
                                            rValue = "TO_DATE('" + var.SearchValue.ToUpper().Replace("\'", "\'\'") + "', 'MM/DD/YYYY')";
                                            break;
                                    }
                                    strQuery.Append(lValue);
                                    strQuery.Append(op);
                                    strQuery.Append(rValue);
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
                if ("MPP_APP.FNL_CATEGORY".Contains("FL_STATE_PROVINCE"))
                {
                    if (strQuery.ToString() != "")
                    {
                        strQuery.Append(" AND ");
                    }
                    strQuery.Append("STATE_CODE <> '-'");
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel logErrorViewModel = new LogErrorViewModel())
                {
                    logErrorViewModel.LogErrorInTextFile(ex);
                }
            }
            return strQuery.ToString();
        }

        public ActionResult CancelExport()
        {
            return RedirectToRoute(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(Session["EntityTypeId"]), entityName = Convert.ToInt32(Session["EntityName"]), viewType = "search" });
        }
    }
}
