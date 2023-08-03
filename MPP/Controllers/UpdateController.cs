using DAL;
using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;
using System.Data;
using System.Text;

namespace MPP.Controllers
{
    public class UpdateController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        public UpdateController(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }
        
        public ActionResult Index() { return View(); }
        public ActionResult GetSelectedRecordForUpdate(string OIDList, string ActionType)
        {
            int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
            TempData["ActionType"] = ActionType;
            ViewData["INPUTROWIDS"] = OIDList;
            DataSet ds = new DataSet();
            string outMsg = Constant.statusSuccess;
            bool isInputRowId = false;
            string languageCode;
            string columnData = string.Empty;
            List<string> rowData = new List<string>();
            List<Dictionary<string, string>> ListOfRecordsForUpdate = new List<Dictionary<string, string>>();
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                    if (previlegesData == null || previlegesData.UPDATE_FLAG != 1)
                    {
                        ModelState.AddModelError("error", Constant.accessDenied);
                        return Content("error" + Constant.accessDenied);
                    }
                    else
                    {
                        languageCode = previlegesData.LANGUAGE_CODE.ToString();
                    }
                }
                if (!string.IsNullOrEmpty(OIDList))
                {
                    using (UpdateRecord objUpdateRecord = new UpdateRecord())
                    {
                        if (ActionType == Constant.ActionTypeWhileUpdateFromWorkFlow)
                            isInputRowId = true;
                        ds = objUpdateRecord.GetSelectedRecords(OIDList, isInputRowId, entityTypeId, userName[1], out columnData, out rowData, out outMsg, out ListOfRecordsForUpdate);
                        if (outMsg != Constant.statusSuccess)
                        {
                            ViewBag.ErrorMsg = Constant.commonErrorMsg;
                            ModelState.AddModelError("ErrorMsg", Constant.commonErrorMsg);
                            return Content("error" + Constant.commonErrorMsg);
                        }
                        TempData["columnData"] = columnData;
                        TempData["rowData"] = rowData;
                        TempData["ListOfRecordsForUpdate"] = ListOfRecordsForUpdate;
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
                return Content("error" + Constant.commonErrorMsg);

            }
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateSelectedRecords(FormCollection form, string command)
        {
            string outMsg = Constant.statusSuccess;
            string actionType = Convert.ToString(TempData["ActionType"]);
            #region updateData
            if (command == "update")
            {
                #region fielddeclaration
                bool download;
                int sessionID = 0;
                int CountDiff = 0;
                bool hasLoadErrors;
                string languageCode;
                int ErrorRowCount = 0;
                int TotalRowCount = 0;
                int btnSuppressWarning = 0;
                string strInputRowIds = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("INPUTROWIDS"));
                int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> resultQuery = new List<Dictionary<string, string>>();
                resultQuery = (List<Dictionary<string, string>>)TempData["ListOfRecordsForUpdate"];
                TempData.Keep();
                List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
                List<Dictionary<string, string>> listattrValues = new List<Dictionary<string, string>>();
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                #endregion fielddeclaration
                #region checkUserAccessRights
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    try
                    {
                        Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                        if (previlegesData == null || previlegesData.UPDATE_FLAG != 1)
                        {
                            TempData["message"] = Constant.accessDenied;
                            return RedirectToRoute(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), viewType = "search" });
                        }
                        else
                            languageCode = previlegesData.LANGUAGE_CODE.ToString();
                    }
                    catch (Exception ex)
                    {
                        using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                        {
                            objLogErrorViewModel.LogErrorInTextFile(ex);
                        }
                        outMsg = ex.Message;
                        return Content("error" + outMsg);
                    }
                }
                #endregion checkUserAccessRights
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    (attributeList, outMsg) = await objMenuViewModel.ShowAttributeDataAsync(entityTypeId, "", userName[1].ToUpper());
                }
                outMsg = ValidateData(form, attributeList);
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);
                outMsg = GetLisOfRecordToUpdate(resultQuery, form, attributeList, out listattrValues);
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);
                TotalRowCount = resultQuery.Count;

                using (UpdateRecord objUpdateRecord = new UpdateRecord())
                {
                    outMsg = objUpdateRecord.UpdateSelectedRecords(listattrValues, actionType, entityTypeId, userName[1], btnSuppressWarning,
                         out download, out hasLoadErrors, out sessionID, out dataList, strInputRowIds.Trim(','));
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                }
                try

                //For Back up 
                {

                    string formatedDate = DateTime.Now.ToString("dd-MM-yyyy-hh-mm");
                    string strRejectFileName = "I" + userName[1] + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + formatedDate + ".csv";
                    //string FilePath = Request.MapPath("") + @"\App_Data\";
                    string FilePath = Path.Combine(_environment.ContentRootPath, "App_Data");

                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    FilePath = FilePath + strRejectFileName;
                    using (UpdateViewModel objUpdateViewModel = new UpdateViewModel())
                    {
                        outMsg = objUpdateViewModel.WriteDataSetToFlatFileBackup(listattrValues, sessionID, FilePath);
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
                if (hasLoadErrors == true && download == true)
                {
                    GetLisOfRecordToUpdate(dataList, out listattrValues);
                    string FileName = "E" + userName[1] + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + ".csv";
                    //string FilePath = Request.MapPath("") + @"\App_Data\";
                    string FilePath = Path.Combine(_environment.ContentRootPath, "App_Data");

                    if (!Directory.Exists(FilePath))
                    {
                        Directory.CreateDirectory(FilePath);
                    }
                    FilePath = FilePath + FileName;
                    using (UpdateViewModel objUpdateViewModel = new UpdateViewModel())
                    {
                        outMsg = objUpdateViewModel.WriteDataSetToFlatFile(listattrValues, sessionID, FilePath);
                    }
                    if (outMsg != Constant.statusSuccess)
                        return Content(Constant.commonErrorMsg);
                    else
                    {
                        TempData["filepath"] = FilePath;
                        string path = System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"];
                        if (dataList.Count() > 0)
                            ErrorRowCount = listattrValues.Count;

                        CountDiff = TotalRowCount - ErrorRowCount;
                        //if (CountDiff > 0)
                        //{
                        //    MailManagerViewModel objMAilManagerViewModel = new MailManagerViewModel();
                        //    string url = Request.Url.GetLeftPart(UriPartial.Authority) + ConfigurationManager.AppSettings["Link"].ToString();
                        //    objMAilManagerViewModel.Mail(CountDiff.ToString(), "record", Convert.ToInt32(Session["EntityTypeID"]), Convert.ToString(Session["EntityName"]),
                        //    Convert.ToString(Session["SelectedDimensionData"]), url, Constant.update, out outMsg);
                        //}
                        return Content("export," + _httpContextAccessor.HttpContext.Session.GetString("EntityName") + "," + FilePath);
                    }
                }
                if (dataList.Count() > 0)
                    ErrorRowCount = listattrValues.Count;

                CountDiff = TotalRowCount - ErrorRowCount;
                //if (CountDiff > 0)
                //{
                //    MailManagerViewModel objMAilManagerViewModel = new MailManagerViewModel();

                //    string url = Request.Url.GetLeftPart(UriPartial.Authority) + ConfigurationManager.AppSettings["Link"].ToString();
                //    objMAilManagerViewModel.Mail(CountDiff.ToString(), "record", Convert.ToInt32(Session["EntityTypeID"]), Convert.ToString(Session["EntityName"]),
                //    Convert.ToString(Session["SelectedDimensionData"]), url, Constant.update, out outMsg);
                //}
                if (outMsg == Constant.statusSuccess)
                    return Content("success");
            }

            #endregion updatedata
            #region cancelUpdateCommand
            else if (command == "Cancel")
                return RedirectToRoute(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetString("EntityTypeID")), entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), viewType = "search" });

            #endregion
            return View();
        }
        private string GetLisOfRecordToUpdate(List<Dictionary<string, string>> resultQuery, FormCollection form, List<Entity_Type_Attr_Detail> attributeList, out List<Dictionary<string, string>> listattrValues)
        {
            string outMsg = Constant.statusSuccess;
            listattrValues = new List<Dictionary<string, string>>();
            try
            {
                foreach (var item in resultQuery)
                {
                    Dictionary<string, string> attrValues = new Dictionary<string, string>();
                    foreach (var data in attributeList)
                    {
                        if (data.Isvisible == "N")
                            attrValues.Add(data.AttrName, "UNKNOWN");
                    }
                    foreach (var data in item)
                    {
                        if (data.Key.ToString() == Constant.dateFromColumnName)
                        {
                            string date = string.IsNullOrEmpty(form["txtEffectiveDate"]) ? data.Value.ToString() : form["txtEffectiveDate"];
                            attrValues.Add("DATE_FROM", date);
                        }
                        else if (data.Value.ToString() == "CURRENT_EDIT_LEVEL")
                            attrValues.Add("CURRENT_EDIT_LEVEL", data.Value.ToString());

                        else
                        {
                            string datatype = attributeList.Where(x => x.AttrName == data.Key.ToString()).Select(x => x.AttrDataType).FirstOrDefault();
                            string displayType = attributeList.Where(x => x.AttrName == data.Key.ToString()).Select(x => x.DisplayType).FirstOrDefault();
                            List<DropDownData> dropDownData = new List<DropDownData>();
                            dropDownData = attributeList.Where(x => x.AttrName == data.Key.ToString()).Select(x => x.dropDownDataList).FirstOrDefault();

                            string formvalue = form[data.Key.ToString()];

                            if (displayType != null && displayType.ToUpper() == "CHECKBOXLIST")
                            {
                                string[] splitValue = formvalue.Split(',');
                                StringBuilder finaldata = new StringBuilder();
                                int k = 0;
                                for (int i = 0; i < splitValue.Length; i++)
                                {
                                    if (Convert.ToString(splitValue[i]) == "true")
                                    {
                                        finaldata.Append(dropDownData[k].VALID_VALUES + "#");
                                        i++;
                                    }
                                    k++;

                                }
                                attrValues.Add(data.Key.ToString(), Convert.ToString(finaldata).Trim('#'));

                                // attrValues.Add(data.ATTR_NAME, Convert.ToString(finaldata).Trim('#'));
                            }
                            else
                            {
                                string value = string.Empty;
                                if (datatype == "DT")
                                {
                                    string val = form[data.Key.ToString()];
                                    if (val != String.Empty)
                                    {

                                        value = string.IsNullOrEmpty(form[data.Key.ToString()]) ? "to_date('" + (DateTime.Parse(data.Value)).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')" : "to_date('" + (DateTime.Parse(form[data.Key.ToString()])).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')";
                                    }
                                    else
                                    {
                                        value = "''";
                                    }
                                    // string dateC = val == null || val == "" ? "TRUNC(SYSDATE)" : "to_date('" + (DateTime.Parse(form[data.Key.ToString()])).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')"; ;
                                    //  attrValues.Add(data.Key.ToString(), dateC);
                                }
                                else
                                {
                                    if (datatype == "N" && !string.IsNullOrEmpty(form[data.Key.ToString()]))
                                    {
                                        int res;
                                        bool isNumeric = int.TryParse(form[data.Key.ToString()], out res);
                                        if (!isNumeric)
                                            return Constant.formatMisMatch;
                                        //string value = string.IsNullOrEmpty(form[data.Key.ToString()]) ? Convert.ToString(data.Value.ToString()) : form[data.Key.ToString()];
                                        //attrValues.Add(data.Key.ToString(), value.Replace("'", "''"));
                                    }
                                    value = string.IsNullOrEmpty(form[data.Key.ToString()]) ? Convert.ToString(data.Value.ToString()) : form[data.Key.ToString()];
                                }
                                attrValues.Add(data.Key.ToString(), value);

                            }
                        }
                    }
                    listattrValues.Add(attrValues);
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
        private string GetLisOfRecordToUpdate(List<Dictionary<string, string>> dataList, out List<Dictionary<string, string>> listattrValues)
        {
            string outMsg = Constant.statusSuccess;
            listattrValues = new List<Dictionary<string, string>>();
            try
            {
                foreach (var item in dataList)
                {
                    Dictionary<string, string> attrValues = new Dictionary<string, string>();
                    foreach (var data in item)
                    {
                        attrValues.Add(data.Key, data.Value);
                    }
                    listattrValues.Add(attrValues);
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
            }
            return outMsg;
        }
        private string ValidateData(IFormCollection form, List<Entity_Type_Attr_Detail> attributeList)
        {
            string outMsg = Constant.statusSuccess;
            bool isNotNulll = false;
            try
            {
                if (string.IsNullOrEmpty(form["txtEffectiveDate"]))
                    return Constant.effectiveDateMandatoryField;
                foreach (var data in attributeList)
                {
                    if (data.DisplayType.ToUpper() == "CHECKBOXLIST" && form[data.AttrName].Contains(','))
                    {
                        string[] splitValue = form[data.AttrName].Split(',');
                        for (int i = 0; i < splitValue.Length; i++)
                        {
                            if (splitValue[i] == "true")
                            {
                                isNotNulll = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(form[data.AttrName]))
                            isNotNulll = true;
                    }
                }
                if (!isNotNulll)
                    outMsg = Constant.mandatoryField;

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
        //[HttpGet]
        //public virtual ActionResult Download(string path)
        //{
        //    try
        //    {
        //        string file = "";
        //        string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
        //        string FileName = "E" + userName[1] + path.Split(',')[1] + ".csv";
        //        Response.Clear();
        //        Response.ContentType = "application/vnd.ms-excel";
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
    }
    
}
