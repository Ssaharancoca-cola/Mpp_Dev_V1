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
using Newtonsoft.Json;
using System.Collections.Generic;

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
             resultQuery = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>((string)TempData["dataList"]);

            string serializedList1 = TempData["attributeList"] as string;
             attributeList = JsonConvert.DeserializeObject<List<Entity_Type_Attr_Detail>>(serializedList1);
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
                ViewData["SortOrder"] = "ASC";
            }
            else
            {
                if (dataType == "N")
                    resultQuery = resultQuery.OrderByDescending(d => Convert.ToInt32(d[splitSortOrder[0]])).ToList();
                else
                    resultQuery = resultQuery.OrderByDescending(d => d[splitSortOrder[0]]).ToList();

                ViewData["SortOrder"] = "DESC";

            }
            ViewData["currentField"] = splitSortOrder[0].ToString(); 
            string ListJson = JsonConvert.SerializeObject(resultQuery);
            TempData["dataList"] = ListJson;
            return PartialView("GetSearchData", resultQuery);
        }       

        [HttpPost]
        public async Task<IActionResult> GetSearchData(IFormCollection form, string Command)
        {

            return await Task.Run(() => ViewComponent("GetSearchData",
                new
                {
                    form,
                    Command
                }));

        }

        [SessionTimeoutPaging]
        public IActionResult Paging(string ActionType)
        {
            int totalRecord = 0;
            int currentPageSize = 50;
            string currentSortBy = "1";
            string currentSortOrder = "ASC";
            string columnData = string.Empty;
            string OIDColumnName = string.Empty;
            string outMsg = Constant.statusSuccess;
            List<string> rowData = new List<string>();
            int currentPageNo = 0;
            try
            {
                if (ActionType == "next")
                    currentPageNo = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("currentPageNo")) + 1;
                else if (ActionType == "previous")
                    currentPageNo = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("currentPageNo")) - 1;
                _httpContextAccessor.HttpContext.Session.SetInt32("currentPageNo", currentPageNo);
                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                List<SearchParameter> fieldCollection = new List<SearchParameter>();
                var fieldCollectionString = _httpContextAccessor.HttpContext.Session.GetString("fieldCollection");
                fieldCollection = JsonConvert.DeserializeObject<List<SearchParameter>>(fieldCollectionString);
                using (SearchDataViewModel objSearchData = new SearchDataViewModel())
                {

                    objSearchData.Search(fieldCollection, currentPageNo, currentPageSize, currentSortBy, currentSortOrder, userName[1],
                        Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), out outMsg, out columnData, out rowData, out dataList, out totalRecord);
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                    if (dataList.Count == 0)
                        return Content("error" + Constant.noRecordFound);

                }
                ViewData["totalRecord"] = totalRecord;


                string ListJson = JsonConvert.SerializeObject(dataList);
                TempData["dataList"] = ListJson;
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
