using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace MPP.Controllers
{
    public class ExportController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        public ExportController(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _configuration = configuration;
        }
        public ActionResult Index() { return View(); }

        [HttpPost]
        public IActionResult ExportData(IFormCollection form, string Command)
        {
            #region ExportCommand
            if (Command == "Export")
            {
                try
                {
                    string datatype = string.Empty;
                    string viewName = string.Empty;
                    string outMsg = Constant.statusSuccess;
                    int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                    string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                    string FileName = "E" + userName[1] + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + ".csv";
                    string FilePath = Path.Combine(_environment.ContentRootPath, "App_Data\\");
                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    FilePath = FilePath + FileName;
                    Dictionary<string, string> attrValues = new Dictionary<string, string>();
                    List<Entity_Type_Attr_Detail> attrbuteList = new List<Entity_Type_Attr_Detail>();
                   // attrbuteList = (List<Entity_Type_Attr_Detail>)TempData["attrbuteList"];
                    string serializedAttributeList = TempData["attributeList"] as string;

                    if (serializedAttributeList != null)
                    {
                        attrbuteList = JsonConvert.DeserializeObject<List<Entity_Type_Attr_Detail>>(serializedAttributeList);
                    }
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
                    string whereClause = GetWhereClause((List<SearchParameter>)ViewData["fieldCollection"]);
                    string SortBy = string.IsNullOrEmpty(Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString(""))) ? " 1 " : Convert.ToString(_httpContextAccessor.HttpContext.Session.GetInt32("currentField"));
                    if (SortBy.Trim() != "1")
                    {
                        datatype = attrbuteList.Find(x => x.AttrDisplayName == SortBy).AttrDataType;
                        SortBy = attrbuteList.Find(x => x.AttrDisplayName == SortBy).AttrName;
                    }
                    if (datatype == "N")
                    {
                        SortBy = "TO_NUMBER(" + SortBy + ")";
                    }
                    string sortOrder = string.IsNullOrEmpty(Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("SortOrder"))) ? "ASC" : Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("SortOrder"));
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
                            ViewData["filepath"] = FilePath;
                            string path = _configuration["FILE:FileDownLoadPath"];
                            //System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"];

                            return Content("export," + _httpContextAccessor.HttpContext.Session.GetString("EntityName") + "," + FilePath);
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
                return ViewComponent("ShowAttribute", new { entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeId")), entityName = _httpContextAccessor.HttpContext.Session.GetString("EntityName"), viewType = "search" });
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
        //public virtual ActionResult Download(string path)
        //{
        //    try
        //    {
        //        string file = "";
        //        string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
        //        string FileName = "E" + userName[1] + path.Split(',')[1] + ".csv";
        //        Response.Clear();
        //        Response.ContentType = "text/plain";
        //        Response.AddHeader("Content-Disposition",
        //        "attachment; filename=\"" + FileName + "\"");
        //        Response.Flush();
        //        Response.WriteFile(path.Split(',')[2]);
        //        Response.End();
        //        return Content("");
        //    }
        //    catch (Exception ex)
        //    {
        //        using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
        //        {
        //            objLogErrorViewModel.LogErrorInTextFile(ex);
        //        }
        //        return Content(ex.Message + ex.StackTrace);
        //    }
        //}
       
        public virtual IActionResult Download(string path)
        {
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                string fileName = "E" + userName[1] + path.Split(',')[1] + ".csv";
                string newPath = path.Split(",")[2];
                var fileStream = System.IO.File.OpenRead(newPath);

                return File(fileStream,
                            "application/octet-stream",
                            fileName,
                            enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                // Log the error.
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content(ex.Message + ex.StackTrace);
            }
        }
    
    public ActionResult CancelExport()
        {
            return RedirectToRoute(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeId")), entityName = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), viewType = "search" });
        }
    }
}
