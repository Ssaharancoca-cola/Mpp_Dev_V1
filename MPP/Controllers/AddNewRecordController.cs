using Azure.Core;
using DAL.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Filter;
using MPP.ViewModel;
using Newtonsoft.Json;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    [SessionTimeoutEntity]
    public class AddNewRecordController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AddNewRecordController> _logger;
        private readonly IConfiguration _configuration;
        private string Link = "//MPP//";

        public AddNewRecordController(IHttpContextAccessor httpContextAccessor, ILogger<AddNewRecordController> logger, IConfiguration configuration)  
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveRecord(IFormCollection form, string Command)
        {
            try
            {
                if (Command == "Save")
                {
                    int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                    string outMsg;
                    string languageCode;
                    int btnSuppressWarning = 0;
                    StringBuilder errMsg = new StringBuilder();
                    Dictionary<string, string> attrValues = new Dictionary<string, string>();
                    List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();

                    string serializedAttributeList = TempData["attributeList"] as string;

                    if (serializedAttributeList != null)
                    {
                        attributeList = JsonConvert.DeserializeObject<List<Entity_Type_Attr_Detail>>(serializedAttributeList);
                    }
                    TempData.Keep();
                    string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                    using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                    {
                        Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                        if (previlegesData == null || previlegesData.CREATE_FLAG != 1)
                        {
                            TempData["message"] = Constant.accessDenied;
                            return await Task.Run(() => ViewComponent("ShowAttribute",
                                                                          new
                                                                          {
                                                                              entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")),
                                                                              entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")),
                                                                              viewType = "search"
                                                                          }));
                        }
                        else
                            languageCode = previlegesData.LANGUAGE_CODE.ToString();
                    }
                    foreach (var data in attributeList)
                    {
                        string value = string.Empty;
                        string isMandatory = attributeList.Where(x => x.AttrName == data.AttrName).Select(y => y.IsMandatoryFlag).FirstOrDefault().ToString();
                        if (string.IsNullOrEmpty(form[data.AttrName]) && isMandatory == "1" && data.Isvisible != "N")
                        {
                            ModelState.AddModelError("error", data.AttrDisplayName + Constant.notNull);
                            errMsg.Append(data.AttrDisplayName + Constant.notNull);
                            errMsg.Append(Environment.NewLine);
                        }
                        else if (data.DisplayType.ToUpper() == "USERBOX" && !string.IsNullOrEmpty(form[data.AttrName]))
                        {
                            UserInfo adUserInfo = GetADUserInfo(form[data.AttrName], out outMsg);

                            if (adUserInfo != null)
                            {
                                value = form[data.AttrName];
                            }
                            else
                            {
                                ModelState.AddModelError("error", Constant.notValidAdUser);
                                errMsg.Append(Constant.notValidAdUser);
                                errMsg.Append(Environment.NewLine);
                            }
                        }
                        else if (data.DisplayType.ToUpper() == "CHECKBOXLIST" && isMandatory == "true")
                        {
                            form.TryGetValue(data.AttrName, out var inputValue);
                            if (inputValue.Count > 0 && inputValue.ToString().Contains(','))
                            {
                                string[] splitValue = inputValue.ToString().Split(',');
                                bool isNull = true;
                                for (int i = 0; i < splitValue.Length; i++)
                                {
                                    if (splitValue[i] == "true")
                                    {
                                        isNull = false;
                                        break;
                                    }
                                }
                                if (isNull)
                                {
                                    ModelState.AddModelError("error", data.AttrDisplayName + Constant.notNull);
                                    errMsg.Append(data.AttrDisplayName + Constant.notNull);
                                    errMsg.Append(Environment.NewLine);
                                }
                            }
                        }

                        else
                            value = form[data.AttrName];
                        if (!string.IsNullOrEmpty(form[data.AttrName]) && (data.AttrDataType.ToUpper() == "N" || (data.AttrDataType.ToUpper() != "DT" && (!string.IsNullOrEmpty(form[data.AttrName]) && (data.AttrName).StartsWith("ADDRESS")))))
                        {
                            int res;
                            bool isNumeric = int.TryParse(form[data.AttrName], out res);
                            if (!isNumeric)
                            {
                                errMsg.Append(Constant.formatMisMatch + " in " + data.AttrDisplayName);
                                errMsg.Append(Environment.NewLine);
                            }

                            // return Content(Constant.formatMisMatch);
                        }
                        if (data.DisplayType.ToUpper() == "CHECKBOXLIST")
                        {
                            form.TryGetValue(data.AttrName, out var inputValue);
                            if (inputValue.Count > 0 && inputValue.ToString().Contains(','))
                            {
                                string[] splitValue = inputValue.ToString().Split(',');
                                StringBuilder finaldata = new StringBuilder();
                                int k = 0;
                                for (int i = 0; i < splitValue.Length; i++)
                                {
                                    if (Convert.ToString(splitValue[i]) == "true")
                                    {
                                        finaldata.Append(data.dropDownDataList[k].VALID_VALUES + "#");
                                        i++;
                                    }
                                    k++;
                                }
                                attrValues.Add(data.AttrName, Convert.ToString(finaldata).Trim('#'));
                            }
                        }

                        else if (data.Isvisible == "N")
                        {
                            attrValues.Add(data.AttrName, null);
                        }
                        else if (data.AttrDataType.ToUpper() == "DT")
                        {
                            string dateC = value == null || value == "" ? "convert(date,getdate())" : "to_date('" + (DateTime.Parse(value)).ToString("MM/dd/yyyy") + "','MM/DD/YYYY')"; ;
                            attrValues.Add(data.AttrName, dateC);
                        }
                        else
                            attrValues.Add(data.AttrName, value);

                    }
                    if (errMsg.Length > 0)
                    {
                        return Content(errMsg.Append("error").ToString());
                    }
                    form.TryGetValue("txtEditLevel", out var editLevelInput);
                    string editLevel = string.IsNullOrEmpty(editLevelInput) ? "NULL" : editLevelInput.ToString();

                    form.TryGetValue("txtEffectiveDate", out var effectiveDateInput);
                    string effectivedate = string.IsNullOrEmpty(effectiveDateInput) ? "convert(date,getdate())" : "CONVERT(DATE, '" + (DateTime.Parse(effectiveDateInput.ToString())).ToString("MM/dd/yyyy") + "', 101)";

                    Dictionary<string, string> attrValuesbp = new Dictionary<string, string>(attrValues);

                    attrValues.Add("CURRENT_EDIT_LEVEL", editLevel);
                    attrValues.Add("DATE_FROM", effectivedate);
                    attrValues.Add("USER_NAME", userName[1]);


                    if (form["txtEffectiveDate"].ToString() != "")
                    {
                        attrValuesbp.Add("DATE_FROM", (DateTime.Parse(form["txtEffectiveDate"])).ToString("MM/dd/yyyy"));

                    }

                    using (AddNewRecordViewModel objAddNewRecordViewModel = new AddNewRecordViewModel())
                    {
                        outMsg = objAddNewRecordViewModel.SaveRecord(attributeList, attrValues, entityTypeId, userName[1], btnSuppressWarning, "MPP_UI_NEW", languageCode);
                    }
                    if (outMsg == Constant.statusSuccess)
                    {
                        MailManagerViewModel objMAilManagerViewModel = new MailManagerViewModel();
                        string url = Request.GetDisplayUrl() + Link;
                        string[] user = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                        objMAilManagerViewModel.Mail("1", "record", Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")),
                        user, url, Constant.addNew, out outMsg);
                    }
                    else
                        return Content(outMsg + "error");
                    return Content(Constant.dataSaveSuccessFully);
                }
                   
                else if (Command == "Cancel")

                    return await Task.Run(() => ViewComponent("ShowAttribute", new { entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), viewType = "search" }));


            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content(Constant.commonErrorMsg);
            }
            return Content(Constant.commonErrorMsg);

        }
        private UserInfo GetADUserInfo(string userId, out string outMsg)
        {
            UserInfo userInfo = null;
            outMsg = Constant.statusSuccess;
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
                {
                    var user = UserPrincipal.FindByIdentity(context, userId);

                    if (user != null)
                    {
                        userInfo = new UserInfo();
                        userInfo.UserID = user.Name.ToString();
                        userInfo.UserName = user.DisplayName.ToString();
                        userInfo.UserEmail = user.EmailAddress.ToString();
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
            return userInfo;
        }
    }
}
