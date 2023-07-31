using Microsoft.AspNetCore.Mvc;
using DAL;
using Model.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Model;
using Microsoft.EntityFrameworkCore;
using MPP.Filter;
using System.Reflection;
using MPP.ViewModel;
using System.Text;
using DAL.Common;

namespace MPP.Controllers
{
        [SessionTimeoutEntity]
        [SessionTimeoutDimension]
        public class SearchController : Controller
       {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SearchController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        List<SearchParameter> fieldCollection = new List<SearchParameter>();
        public PartialViewResult Index(string sortOrder)
        {
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            List<Dictionary<string, string>> resultQuery = new List<Dictionary<string, string>>();
            string[] splitSortOrder = sortOrder.Split('^');
            Dictionary<string, string> mapClassAndDatabaseProp = new Dictionary<string, string>();
            ViewBag.sortId = String.IsNullOrEmpty(splitSortOrder[1]) ? "sortId" : "";
            resultQuery = (List<Dictionary<string, string>>)TempData["dataList"];
            attributeList = (List<Entity_Type_Attr_Detail>)TempData["attributeList"];
            TempData.Keep();
            PropertyInfo prop = typeof(EntityTypeData).GetProperty(splitSortOrder[0]);
            string dataType = string.Empty;
            string outMsg = Constant.statusSuccess;
            using (SearchDataViewModel objSearchData = new SearchDataViewModel())
            {
                dataType = objSearchData.GetDataType(splitSortOrder[0], Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), out outMsg);
            }
            if (splitSortOrder[1] != "")
            {
                if (dataType == "N")
                    resultQuery = resultQuery.OrderBy(d => Convert.ToInt32(d[splitSortOrder[0]])).ToList();
                else
                    resultQuery = resultQuery.OrderBy(d => d[splitSortOrder[0]]).ToList();
                ViewBag["SortOrder"] = "ASC";
            }
            else
            {
                if (dataType == "N")
                    resultQuery = resultQuery.OrderByDescending(d => Convert.ToInt32(d[splitSortOrder[0]])).ToList();
                else
                    resultQuery = resultQuery.OrderByDescending(d => d[splitSortOrder[0]]).ToList();

                ViewBag["SortOrder"] = "DESC";

            }
            ViewBag["currentField"] = splitSortOrder[0].ToString(); //attributeList.Where(x => x.ATTR_DISPLAY_NAME == fieldName).Select(x => x.ATTR_NAME).FirstOrDefault();
            TempData["dataList"] = resultQuery;
            return PartialView("_GetSearchData", resultQuery);
        }
        public async Task<IActionResult> GetSearchData(FormCollection form, string Command)
        {
            ViewBag.sortId = "";
            int totalRecord = 0;
            int currentPageNo = 1;
            int currentPageSize = 50;
            string currentSortBy = "1";
            string currentSortOrder = "ASC";
            string columnData = string.Empty;
            string OIDColumnName = string.Empty;
            string outMsg = Constant.statusSuccess;
            ViewBag["currentPageNo"] = currentPageNo;
            List<string> rowData = new List<string>();
            List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();

            string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
           // outMsg = CheckUserAccessRights(Command, userName[1]);
            if (outMsg != Constant.statusSuccess)
                return Content(outMsg);
            #region SearchAndViewAll
            if (Command == "Search" || Command == "ViewAll")
            {
                try
                {
                    attributeList = (List<Entity_Type_Attr_Detail>)TempData["attributeList"];
                    TempData.Keep();
                    if (Command == "Search")
                        fieldCollection = GetSearchCriteria(form, attributeList, out outMsg);

                    outMsg = outMsg == Constant.statusSuccess ? ValidateData(fieldCollection, form, Command) : outMsg;
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);

                    using (SearchDataViewModel objSearchData = new SearchDataViewModel())
                    {
                        objSearchData.Search(fieldCollection, currentPageNo, currentPageSize, currentSortBy, currentSortOrder, userName[1],
                            Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), out outMsg, out columnData, out rowData, out dataList, out totalRecord);
                        if (outMsg != Constant.statusSuccess)
                            return Content("error" + outMsg);
                        if (dataList.Count == 0)
                        {
                            ViewBag["currentField"] = currentSortBy;
                            ViewBag["fieldCollection"] = fieldCollection;

                            return Content("error" + Constant.noRecordFound);
                        }

                    }
                    TempData["totalRecord"] = totalRecord;
                    //   TempData["originaldataList"] = dataList;

                    //  dataList = dataList.Take(10).ToList<Dictionary<string, string>>();


                    TempData["dataList"] = dataList;
                    // TempData["mapClassAndDatabaseProp"] = mapClassAndDatabaseProp;
                    // TempData["resultQuery"] = resultQuery;
                    ViewBag["Current_Page"] = currentPageNo;
                    ViewBag["currentField"] = currentSortBy;
                    ViewBag["SortOrder"] = currentSortOrder;
                    ViewBag["fieldCollection"] = fieldCollection;
                }
                catch (Exception ex)
                {
                    using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                    {
                        objLogErrorViewModel.LogErrorInTextFile(ex);
                    }
                    return Content("error" + Constant.commonErrorMsg);
                }
                return PartialView("_GetSearchData");
            }
            #endregion SearchAndViewAll
            #region AddNewExportImport
            else if (Command == "AddNew" || Command == "Export" || Command == "Import")
            {
                List<Entity_Type_Attr_Detail> entityAttrList = new List<Entity_Type_Attr_Detail>();
                using (AddNewRecordViewModel objAddNewRecordViewModel = new AddNewRecordViewModel())
                {
                    entityAttrList = await objAddNewRecordViewModel.GetAddNewField(Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")));
                }
                if (Command == "AddNew")
                {
                    return PartialView("~/Views/AddNewRecord/AddNew.cshtml", entityAttrList);
                }
                else if (Command == "Export")
                {
                    return PartialView("~/Views/ExportData/Export.cshtml", entityAttrList);
                }
                else if (Command == "Import")
                {
                    return PartialView("~/Views/ImportData/Import.cshtml", entityAttrList);

                }


            }
            #endregion AddNewExportImport

            else if (Command == "Workflow")
            {
                //GetWorkFlowData();
                return PartialView("~/Views/WorkFlow/GetWorkFlowData.cshtml");
            }


            return PartialView("_GetSearchData");
        }
        //[SessionTimeoutPaging]
        //public ActionResult Paging(string ActionType)
        //{
        //    int totalRecord = 0;
        //    int currentPageSize = 50;
        //    string currentSortBy = "1";
        //    string currentSortOrder = "ASC";
        //    string columnData = string.Empty;
        //    string OIDColumnName = string.Empty;
        //    string outMsg = Constant.statusSuccess;
        //    List<string> rowData = new List<string>();
        //    int currentPageNo = 0;
        //    try
        //    {
        //        if (ActionType == "next")
        //            currentPageNo = Convert.ToInt32(Session["currentPageNo"]) + 1;
        //        else if (ActionType == "previous")
        //            currentPageNo = Convert.ToInt32(Session["currentPageNo"]) - 1;
        //        Session["currentPageNo"] = currentPageNo;
        //        List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
        //        List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
        //        string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
        //        System.Collections.Generic.List<SearchParameter> fieldCollection = new System.Collections.Generic.List<SearchParameter>();
        //        fieldCollection = (System.Collections.Generic.List<SearchParameter>)Session["fieldCollection"];
        //        using (searchDataViewModel objSearchData = new searchDataViewModel())
        //        {

        //            objSearchData.Search(fieldCollection, currentPageNo, currentPageSize, currentSortBy, currentSortOrder, userName[1],
        //                Convert.ToInt32(Session["EntityTypeID"]), out outMsg, out columnData, out rowData, out dataList, out totalRecord);
        //            if (outMsg != Constant.statusSuccess)
        //                return Content("error" + outMsg);
        //            if (dataList.Count == 0)
        //                return Content("error" + Constant.noRecordFound);

        //        }
        //        TempData["totalRecord"] = totalRecord;

        //        // TempData["originaldataList"] = dataList;

        //        // dataList = dataList.Take(10).ToList<Dictionary<string, string>>();


        //        TempData["dataList"] = dataList;
        //    }
        //    catch (Exception ex)
        //    {
        //        using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
        //        {
        //            objLogErrorViewModel.LogErrorInTextFile(ex);
        //        }
        //        return Content("error" + Constant.commonErrorMsg);

        //    }
        //    return PartialView("_GetSearchData");

        //}
        private List<SearchParameter> GetSearchCriteria(FormCollection form, List<Entity_Type_Attr_Detail> attributeList, out string outMsg)
        {
            string txtFrom = Convert.ToString(form["txtFrom"]);
            string txtTo = Convert.ToString(form["txtTo"]);
            outMsg = Constant.statusSuccess;
            List<SearchParameter> fieldCollection = new List<SearchParameter>();
            try
            {
                foreach (var data in attributeList)
                {
                    if (data.Isvisible != "N")
                    {
                        SearchParameter searchparameter = new SearchParameter();
                        searchparameter.DBFieldName = data.AttrName;
                        if (data.DisplayType == ControlType.TEXTBOX.ToString() || data.DisplayType == ControlType.USERBOX.ToString())
                        {
                            if (!string.IsNullOrEmpty(form[data.AttrName].ToString()))
                            {
                                searchparameter.SearchValue = form[data.AttrName];
                                searchparameter.CompareType = SearchParameter.SearchCompareType.Like;
                                searchparameter.DataType = data.AttrDataType;
                                if (data.AttrDataType.ToUpper() == "N")
                                {
                                    int res;
                                    bool isNumeric = int.TryParse(form[data.AttrName], out res);
                                    if (!isNumeric)
                                    {
                                        outMsg = Constant.formatMisMatch;
                                        return fieldCollection;
                                    }
                                }
                                fieldCollection.Add(searchparameter);
                            }
                        }
                        else if (data.DisplayType == ControlType.COMBOBOX.ToString() || data.DisplayType == ControlType.PARCOMBO.ToString() || data.DisplayType == ControlType.CASCOMBO.ToString())
                        {
                            if (!string.IsNullOrEmpty(form[data.AttrName].ToString()))
                            {
                                searchparameter.SearchValue = form[data.AttrName];
                                searchparameter.CompareType = SearchParameter.SearchCompareType.Equal;
                                searchparameter.DataType = data.AttrDataType;
                                fieldCollection.Add(searchparameter);

                            }
                        }
                        else if (data.DisplayType == ControlType.DATEPICKER.ToString())
                        {
                            if (!string.IsNullOrEmpty(form[data.AttrName].ToString()))
                            {
                                searchparameter.SearchValue = form[data.AttrName];
                                searchparameter.CompareType = SearchParameter.SearchCompareType.Equal;
                                searchparameter.DataType = "DATE";
                                fieldCollection.Add(searchparameter);
                            }

                        }
                    }
                    if ((txtFrom != "" && txtFrom != null) && (txtTo != "" && txtTo != null))
                    {
                        SearchParameter DATE_FROMfield = new SearchParameter();
                        DATE_FROMfield.DBFieldName = "DATE_FROM";
                        DATE_FROMfield.SearchValue = txtFrom + "," + txtTo;
                        DATE_FROMfield.CompareType = SearchParameter.SearchCompareType.Between;
                        DATE_FROMfield.DataType = "DATE";
                        fieldCollection.Add(DATE_FROMfield);
                    }
                    else if (txtFrom != "" && txtFrom != null)
                    {
                        SearchParameter DATE_FROMfield = new SearchParameter();
                        DATE_FROMfield.DBFieldName = "DATE_FROM";
                        DATE_FROMfield.SearchValue = txtFrom;
                        DATE_FROMfield.CompareType = SearchParameter.SearchCompareType.GreaterthanOrEqual;
                        DATE_FROMfield.DataType = "DATE";
                        fieldCollection.Add(DATE_FROMfield);
                    }
                    else if (txtTo != "" && txtTo != null)
                    {
                        SearchParameter DATE_FROMfield = new SearchParameter();
                        DATE_FROMfield.DBFieldName = "DATE_FROM";
                        DATE_FROMfield.SearchValue = txtTo;
                        DATE_FROMfield.CompareType = SearchParameter.SearchCompareType.LessthanOrEqual;
                        DATE_FROMfield.DataType = "DATE";
                        fieldCollection.Add(DATE_FROMfield);
                    }
                }

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = Constant.commonErrorMsg;
            }
            return fieldCollection;
        }
        private string ValidateData(List<SearchParameter> fieldCollection, FormCollection form, string Command)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                if (fieldCollection.Count == 0 && Command == "Search")
                {
                    outMsg = Constant.selectAtleastOneSearchCriteria;
                }
                string txtFrom = Convert.ToString(form["txtFrom"]);
                string txtTo = Convert.ToString(form["txtTo"]);
                if ((txtFrom != "" && txtFrom != null && Command == "Search") && (txtTo != "" && txtTo != null && Command == "Search"))
                {
                    DateTime fromDate = Convert.ToDateTime(txtFrom);
                    DateTime toDate = Convert.ToDateTime(txtTo);
                    if (!((DateTime.Compare(fromDate, toDate) == 0) || DateTime.Compare(toDate, fromDate) > 0))
                    {
                        outMsg = Constant.toDateGreaterThanFromDate;
                    }
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return outMsg;
        }
        //        private string GetWorkFlowData()
        //        {
        //            string outMsg = Constant.statusSuccess;
        //            try
        //            {
        //                using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
        //                {
        //                    int entityTypeId = Convert.ToInt32(Session["EntityTypeID"]);
        //                    string submittedColumnData;
        //                    string rejectedColumnData;
        //                    string approvalPendingColumnData;
        //                    string existingRecordColumnData;

        //                    List<string> submittedRowData = new List<string>();
        //                    List<string> rejectedRowData = new List<string>();
        //                    List<string> approvalPendingRowData = new List<string>();
        //                    List<string> existingRecordRowData = new List<string>();

        //                    List<Dictionary<string, string>> submittedDataList = new List<Dictionary<string, string>>();
        //                    List<Dictionary<string, string>> rejectedDataList = new List<Dictionary<string, string>>();
        //                    List<Dictionary<string, string>> approvalPendingDataList = new List<Dictionary<string, string>>();
        //                    List<Dictionary<string, string>> existingRecordDataList = new List<Dictionary<string, string>>();

        //                    objWorkFlowViewModel.LoadContentView(entityTypeId, out submittedColumnData, out submittedRowData, out submittedDataList);
        //                    TempData["submittedColumnData"] = submittedColumnData;
        //                    TempData["submittedRowData"] = submittedRowData;
        //                    TempData["submitteddataList"] = submittedDataList;

        //                    objWorkFlowViewModel.LoadContentReject(entityTypeId, out rejectedColumnData, out rejectedRowData, out rejectedDataList);
        //                    TempData["rejectedColumnData"] = rejectedColumnData;
        //                    TempData["rejectedRowData"] = rejectedRowData;
        //                    TempData["rejecteddataList"] = rejectedDataList;

        //                    objWorkFlowViewModel.LoadContentMyApproval(entityTypeId, out approvalPendingColumnData, out approvalPendingRowData,
        //                        out approvalPendingDataList, out existingRecordColumnData, out existingRecordRowData, out existingRecordDataList);
        //                    TempData["approvalPendingColumnData"] = approvalPendingColumnData;
        //                    TempData["approvalPendingRowData"] = approvalPendingRowData;
        //                    TempData["approvalPendingdataList"] = approvalPendingDataList;

        //                    StringBuilder rowdata = new StringBuilder();
        //                    foreach (var data in existingRecordRowData)
        //                    {

        //                        rowdata.Append(data.ToString());

        //                        rowdata.Append("^");

        //                    }
        //                    Session["ExistingList"] = rowdata.ToString();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
        //                {
        //                    objLogErrorViewModel.LogErrorInTextFile(ex);
        //                }
        //                outMsg = ex.Message;
        //            }
        //            return outMsg;


        //        }
        private string CheckUserAccessRights(string Command, string userName)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName, Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), out outMsg);
                    if (Command == "Search" || Command == "ViewAll" || Command == "Export")
                    {
                        if ((outMsg != Constant.statusSuccess) || (previlegesData == null || previlegesData.READ_FLAG != 1))
                            outMsg = "error" + Constant.accessDenied;
                    }
                    else if (Command == "AddNew")
                    {
                        if ((outMsg != Constant.statusSuccess) || previlegesData == null || previlegesData.CREATE_FLAG != 1)
                            outMsg = "error" + Constant.accessDenied;
                    }
                    else if (Command == "Import")
                    {
                        if ((outMsg != Constant.statusSuccess) || previlegesData == null || previlegesData.IMPORT_FLAG != 1 || Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")) == 110)
                            outMsg = "error" + Constant.accessDenied;
                    }
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = Constant.commonErrorMsg;
            }
            return outMsg;

        }
        public JsonResult GetCasCombo(string Id, string cId)
        {
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            attributeList = (List<Entity_Type_Attr_Detail>)TempData["attributeList"];
            string[] listBoxQuery;
            string inserAliasName = string.Empty;

            List<DropDownData> dropdownDataList = new List<DropDownData>();
            List<SelectListItem> casCombo = new List<SelectListItem>();

            if (attributeList != null)
            {
                TempData["attributeList"] = attributeList;
                foreach (var item in attributeList)
                {
                    if (item.DisplayType.ToUpper().Equals("PARCOMBO") && item.CasDrop.Equals(cId))
                    {
                        listBoxQuery = Convert.ToString(item.CasQuery).ToUpper().Split(new string[] { "FROM" }, StringSplitOptions.None);
                        inserAliasName = listBoxQuery[0].Insert(listBoxQuery[0].IndexOf(','), " AS VALiD_VALUES ");
                        inserAliasName = inserAliasName.Insert(inserAliasName.Length - 1, " AS VALUE_NAME ");

                        string value = "'" + Id.ToString() + "'";
                        string listquery = listBoxQuery[1].Replace("#REPLACECOND", value);

                        using (var mPP_Context = new MPP_Context())
                        {
                            dropdownDataList = mPP_Context.Set<DropDownData>().FromSqlRaw(inserAliasName + "FROM" + listquery).ToList();
                        }

                        dropdownDataList.ForEach(x =>
                        {
                            casCombo.Add(new SelectListItem { Text = x.VALUE_NAME, Value = x.VALID_VALUES.ToString() });
                        });
                    }
                }
            }
            return Json(new SelectList(casCombo, "Value", "Text"));
        }
    }
    }
