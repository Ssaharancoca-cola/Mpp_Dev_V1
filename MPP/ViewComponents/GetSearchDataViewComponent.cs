using DAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;
using Newtonsoft.Json;
using System.Text;

namespace MPP.ViewComponents
{
    public class GetSearchDataViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetSearchDataViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync(IFormCollection form, string Command)
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
            _httpContextAccessor.HttpContext.Session.SetInt32("currentPageNo", currentPageNo);
            List<string> rowData = new List<string>();
            List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();

            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            outMsg = CheckUserAccessRights(Command, userName[1]);
            if (outMsg != Constant.statusSuccess)
                return Content(outMsg);
            List<SearchParameter> fieldCollection = new List<SearchParameter>();
            #region SearchAndViewAll
            if (Command == "Search" || Command == "ViewAll")
            {
                try
                {
                    string serializedAttributeList = TempData["attributeList"] as string;

                    if (serializedAttributeList != null)
                    {
                        attributeList = JsonConvert.DeserializeObject<List<Entity_Type_Attr_Detail>>(serializedAttributeList);
                    }
                    TempData.Keep();
                    if (Command == "Search")
                        fieldCollection = GetSearchCriteria((FormCollection)form, attributeList, out outMsg);

                    outMsg = outMsg == Constant.statusSuccess ? ValidateData(fieldCollection, (FormCollection)form, Command) : outMsg;
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
                            _httpContextAccessor.HttpContext.Session.SetString("currentField", currentSortBy);
                            var fieldCollectionStringg = JsonConvert.SerializeObject(fieldCollection);
                            _httpContextAccessor.HttpContext.Session.SetString("fieldCollection", fieldCollectionStringg);

                            return Content("error" + Constant.noRecordFound);
                        }

                    }
                    ViewData["totalRecord"] = totalRecord;

                    string ListJson = JsonConvert.SerializeObject(dataList);
                    TempData["dataList"] = ListJson;

                    _httpContextAccessor.HttpContext.Session.SetInt32("Current_Page", currentPageNo);
                    _httpContextAccessor.HttpContext.Session.SetString("currentField", currentSortBy);
                    _httpContextAccessor.HttpContext.Session.SetString("SortOrder", currentSortOrder);
                    var fieldCollectionString = JsonConvert.SerializeObject(fieldCollection);
                    _httpContextAccessor.HttpContext.Session.SetString("fieldCollection", fieldCollectionString);

                }
                catch (Exception ex)
                {
                    using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                    {
                        objLogErrorViewModel.LogErrorInTextFile(ex);
                    }
                    return Content("error" + Constant.commonErrorMsg);
                }
                return View("GetSearchData");
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
                     return View("~/Views/AddNewRecord/AddNew.cshtml", entityAttrList);
                }
                else if (Command == "Export")
                {
                    return View("~/Views/Export/Export.cshtml", entityAttrList);
                }
                else if (Command == "Import")
                {
                    return await Task.FromResult<IViewComponentResult>(View("~/Views/Import/Import.cshtml", entityAttrList));
                }

            }
            #endregion AddNewExportImport

            else if (Command == "WorkFlow")
            {
                GetWorkFlowData();
                return await Task.FromResult<IViewComponentResult>(View("~/Views/Workflow/GetWorkFlowData.cshtml"));

            }
            return View("GetSearchData");
        }

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
                        if (data.DisplayType.ToUpper() == ControlType.TEXTBOX.ToString() || data.DisplayType.ToUpper() == ControlType.USERBOX.ToString())
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
                        else if (data.DisplayType.ToUpper() == ControlType.COMBOBOX.ToString() || data.DisplayType.ToUpper() == ControlType.PARCOMBO.ToString() || data.DisplayType.ToUpper() == ControlType.CASCOMBO.ToString())
                        {
                            if (!string.IsNullOrEmpty(form[data.AttrName].ToString()))
                            {
                                searchparameter.SearchValue = form[data.AttrName];
                                searchparameter.CompareType = SearchParameter.SearchCompareType.Equal;
                                searchparameter.DataType = data.AttrDataType;
                                fieldCollection.Add(searchparameter);

                            }
                        }
                        else if (data.DisplayType.ToUpper() == ControlType.DATEPICKER.ToString())
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
        private string GetWorkFlowData()
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
                {
                    int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                    string submittedColumnData;
                    string rejectedColumnData;
                    string approvalPendingColumnData;
                    string existingRecordColumnData;

                    List<string> submittedRowData = new List<string>();
                    List<string> rejectedRowData = new List<string>();
                    List<string> approvalPendingRowData = new List<string>();
                    List<string> existingRecordRowData = new List<string>();

                    List<Dictionary<string, string>> submittedDataList = new List<Dictionary<string, string>>();
                    List<Dictionary<string, string>> rejectedDataList = new List<Dictionary<string, string>>();
                    List<Dictionary<string, string>> approvalPendingDataList = new List<Dictionary<string, string>>();
                    List<Dictionary<string, string>> existingRecordDataList = new List<Dictionary<string, string>>();

                    objWorkFlowViewModel.LoadContentView(entityTypeId, out submittedColumnData, out submittedRowData, out submittedDataList);
                    
                    string ListJson1 = JsonConvert.SerializeObject(submittedColumnData);
                    TempData["submittedColumnData"] = ListJson1;

                    string ListJson2 = JsonConvert.SerializeObject(submittedRowData);
                    TempData["submittedRowData"] = ListJson2;

                    string ListJson3 = JsonConvert.SerializeObject(submittedDataList);
                    TempData["submitteddataList"] = ListJson3;

                    objWorkFlowViewModel.LoadContentReject(entityTypeId, out rejectedColumnData, out rejectedRowData, out rejectedDataList);

                    string ListJson4 = JsonConvert.SerializeObject(rejectedColumnData);
                    TempData["rejectedColumnData"] = ListJson4;

                    string ListJson5 = JsonConvert.SerializeObject(rejectedRowData);
                    TempData["rejectedRowData"] = ListJson5;

                    string ListJson6 = JsonConvert.SerializeObject(rejectedDataList);
                    TempData["rejecteddataList"] = ListJson6;

                    objWorkFlowViewModel.LoadContentMyApproval(entityTypeId, out approvalPendingColumnData, out approvalPendingRowData,
                        out approvalPendingDataList, out existingRecordColumnData, out existingRecordRowData, out existingRecordDataList);

                    string ListJson7 = JsonConvert.SerializeObject(approvalPendingColumnData);
                    TempData["approvalPendingColumnData"] = ListJson7;

                    string ListJson8 = JsonConvert.SerializeObject(approvalPendingRowData);
                    TempData["approvalPendingRowData"] = ListJson8;

                    string ListJson9 = JsonConvert.SerializeObject(approvalPendingDataList);
                    TempData["approvalPendingdataList"] = ListJson9;

                    StringBuilder rowdata = new StringBuilder();
                    foreach (var data in existingRecordRowData)
                    {

                        rowdata.Append(data.ToString());

                        rowdata.Append("^");

                    }
                    _httpContextAccessor.HttpContext.Session.SetString("ExistingList", rowdata.ToString());
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
    }
}
