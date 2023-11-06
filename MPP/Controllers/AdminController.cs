using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Models;
using MPP.ViewModel;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using DAL;
using Microsoft.AspNetCore.Authentication;
using System.DirectoryServices.AccountManagement;

namespace MPP.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AdminController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllUsers()
        {
            string outMsg = Constant.statusSuccess;
            List<UserInfo> allUserInfo = new List<UserInfo>();
            try
            {
                allUserInfo = GetAllUserInfo(out outMsg);
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return PartialView("~/Views/Admin/GetAllUserName.cshtml", allUserInfo);
        }
        public ActionResult GetNewUserIndex(string newUserId)
        {
            int index = 0;
            string outMsg = Constant.statusSuccess;
            try
            {
                List<UserInfo> allUserInfo = new List<UserInfo>();
                allUserInfo = GetAllUserInfo(out outMsg);
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);
                index = allUserInfo.FindIndex(x => x.UserID.ToUpper() == newUserId.ToUpper());
                index = index + 1;
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
            return Content(index.ToString());
        }
        public ActionResult GetUserDetails(string userId)
        {
            UserInfo userInfo = new UserInfo();
            string outMsg = Constant.statusSuccess;
            List<DimensionName> dimensionList = new List<DimensionName>();

            try
            {
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    userInfo = objAdminViewModel.GetUserDetails(userId.ToUpper(), out outMsg);
                    using (MenuViewModel objMenuViewModel = new MenuViewModel())
                    {
                        dimensionList = objMenuViewModel.ShowMenuData(out outMsg);
                    }
                    List<EntityType> objEntityType = new List<EntityType>();
                    foreach (var item in dimensionList)
                    {
                        EntityType entityType = new EntityType();
                        entityType.DimensionDisplayName = item.Dimension;
                        objEntityType.Add(entityType);
                    }
                    userInfo.dimnesionList = objEntityType;
                    TempData["IsSearch"] = "0";
                    string lstUserInfoJson = JsonConvert.SerializeObject(userInfo);

                    _httpContextAccessor.HttpContext.Session.SetString("UserInfo", lstUserInfoJson);

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
            return View(userInfo);
        }
        public ActionResult GetUserDetailsForAddingUser(string userId)
        {
            UserInfo userInfo = new UserInfo();
            UserInfo adUserInfo = new UserInfo();
            string outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();

            try
            {
                if (string.IsNullOrEmpty(userId.Trim(' ')))
                    return Content("error" + Constant.requiredFieldUserID);
                currentUserInfo = GetCurrentUser(out outMsg);
                if (currentUserInfo.IsAdmin != 1)
                    return Content("error" + Constant.accessDenied);

                if (!string.IsNullOrEmpty(userId))
                {
                    using (AdminViewModel objAdminViewModel = new AdminViewModel())
                    {
                        userInfo = objAdminViewModel.GetUserDetails(userId.ToUpper(), out outMsg);
                    }
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                }
                if (userInfo == null)
                {
                    adUserInfo = GetADUserInfo(userId.ToUpper(), out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                    if (adUserInfo != null)
                    {
                        TempData["IsSearch"] = "1";
                        return View("GetUserDetails", adUserInfo);
                    }
                    else
                        return Content(Constant.notValidAdUser);
                }
                else
                    return Content(Constant.userAlreadyExist);
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return View();
        }
        public ActionResult SearchEntity(string dimensionName)
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("UserInfo") == null)
            {
                return Content("error" + Constant.sessionexpire);
            }
            List<UserRowSecurity> lstUserRowSecurity = new List<UserRowSecurity>();
            //to refresh the data when user click cancel button of apply row level security dimension name will be fetch from session
            if (dimensionName == "NA")
            {
                dimensionName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetInt32("DimensionId"));

            }
            List<SearchParameter> fieldCollection = new List<SearchParameter>();
            try
            {
                if (dimensionName != "")
                {
                    SearchParameter objSearchParameter = new SearchParameter();
                    objSearchParameter.DBFieldName = "DIMENSION_NAME";
                    objSearchParameter.SearchValue = dimensionName;
                    objSearchParameter.CompareType = SearchParameter.SearchCompareType.Equal;
                    objSearchParameter.DataType = "VARCHAR";
                    fieldCollection.Add(objSearchParameter);
                }
                else
                {
                    return Content("error" + Constant.commonErrorMsg);
                }
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(_httpContextAccessor.HttpContext.Session.GetString("UserInfo"));
                    lstUserRowSecurity = objAdminViewModel.GetEntities(userInfo, fieldCollection);
                }
                string lstUserRowSecurityJson = JsonConvert.SerializeObject(lstUserRowSecurity);
                _httpContextAccessor.HttpContext.Session.SetString("UserRowSecurity", lstUserRowSecurityJson);
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return View("ShowEntity", lstUserRowSecurity);
        }
        public ActionResult UpdateUserDetails(string[] readFlag, string[] updateFlag, string[] createFlag, string[] importFlag, string[] RoleList,
           string SelectedUserName, string UserId, string UserName, string Email, string DimensionName, string DimensionId, string IsActive, string IsAdmin)
        {
            //if (Session["UserRowSecurity"] == null)
            //{
            //    return Content("error" + Constant.sessionexpire);
            //}
            string outMsg = Constant.statusSuccess;
            try
            {
                UserInfo user = new UserInfo();
                user.UserName = UserName;
                user.UserID = UserId;
                user.UserEmail = Email;
                user.IsAdmin = IsAdmin == "true" ? 1 : 0;
                user.IsActive = IsActive == "true" ? 1 : 0;
                string UserRowSecurityJson = _httpContextAccessor.HttpContext.Session.GetString("UserRowSecurity");
                List<UserRowSecurity> cslst;
                if (string.IsNullOrEmpty(UserRowSecurityJson))
                {
                    cslst = new List<UserRowSecurity>();   // initialize as an empty list if the JSON is null
                }
                else
                {
                    cslst = JsonConvert.DeserializeObject<List<UserRowSecurity>>(UserRowSecurityJson);
                }

                List<EntityPrivileges> entityList = new List<EntityPrivileges>();
                EntityPrivileges entityPrivileges = null;
                Model.Entity entity = null;
                if (!string.IsNullOrEmpty(Convert.ToString(readFlag)))
                {
                    for (int i = 0; i < readFlag.Length; i++)
                    {
                        if (updateFlag[i] == "true" || createFlag[i] == "true" || importFlag[i] == "true")
                        {
                            cslst[i].READ_FLAG = 1;
                        }
                        else
                        {
                            cslst[i].READ_FLAG = readFlag[i] == "true" ? 1 : 0;
                        }
                        cslst[i].UPDATE_FLAG = updateFlag[i] == "true" ? 1 : 0;
                        cslst[i].CREATE_FLAG = createFlag[i] == "true" ? 1 : 0;
                        cslst[i].IMPORT_FLAG = importFlag[i] == "true" ? 1 : 0;
                        cslst[i].ROLE_ID = !string.IsNullOrEmpty(RoleList[i]) ? Convert.ToInt32(RoleList[i]) : 0;
                    }
                }
                if (cslst != null)
                {
                    foreach (var child in cslst)
                    {
                        entityPrivileges = new EntityPrivileges();
                        entity = new Model.Entity();
                        entity.EntityName = child.EntityName;
                        entity.EntityID = Convert.ToInt32(child.ENTITY_TYPE_ID);
                        entityPrivileges.EntityDetails = entity;
                        entityPrivileges.ReadStatus = child.READ_FLAG;
                        entityPrivileges.UpdateStatus = child.UPDATE_FLAG;
                        entityPrivileges.CreateStatus = child.CREATE_FLAG;
                        entityPrivileges.ImportStatus = child.IMPORT_FLAG;
                        entityPrivileges.RoleId = child.ROLE_ID;
                        if (entityPrivileges != null)
                        {
                            if ((entityPrivileges.UpdateStatus.Equals(1) || entityPrivileges.CreateStatus.Equals(1) || entityPrivileges.ImportStatus.Equals(1)))
                                entityPrivileges.ReadStatus = 1;
                            else
                                entityPrivileges.ReadStatus = entityPrivileges.ReadStatus;


                            entityList.Add(entityPrivileges);
                            entityPrivileges = null;
                        }
                    }
                    user.EntityPrivilegesList = entityList;
                }
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    objAdminViewModel.UpdateUserDetails(user, out outMsg);
                }
                if (outMsg != Constant.statusSuccess)
                    return View("error" + outMsg);
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return Content("success");
        }
        public ActionResult SaveUser(string UserId, string UserName, string EmailId, string IsActive, string IsAdmin)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                UserInfo userInfo = new UserInfo();
                userInfo.UserName = UserName;
                userInfo.UserID = UserId;
                userInfo.UserEmail = EmailId;
                userInfo.IsAdmin = IsAdmin == "true" ? 1 : 0;
                userInfo.IsActive = IsActive == "true" ? 1 : 0;
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    objAdminViewModel.CreateUser(userInfo, out outMsg);
                }
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return Content("success");
        }
        public UserInfo GetADUserInfo(string userId, out string outMsg)
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

        public List<UserInfo> GetAllUserInfo(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            List<UserInfo> allUserInfo = new List<UserInfo>();
            try
            {
                currentUserInfo = GetCurrentUser(out outMsg);
                if (currentUserInfo == null || currentUserInfo.IsAdmin != 1)
                {
                    outMsg = Constant.accessDenied;
                    return allUserInfo;

                }
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    allUserInfo = objAdminViewModel.GetAllUsersInfo(out outMsg);
                }
                string allUserInfoJson = JsonConvert.SerializeObject(allUserInfo);
                string currentUserInfoJson = JsonConvert.SerializeObject(currentUserInfo);

                _httpContextAccessor.HttpContext.Session.SetString("AllUserInfo", allUserInfoJson);
                _httpContextAccessor.HttpContext.Session.SetString("CurrentUserInfo", currentUserInfoJson);

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return allUserInfo;
        }
        public ActionResult GetRowLevelSecurityDetails(string EntityName, string DimensionId, string DimensionName, string UserId)
        {
            RowLevelSecurityDetail rowLevelSecurityDetail = new RowLevelSecurityDetail();

            try
            {
                TempData["UserID"] = UserId;
                string outMsg = Constant.statusSuccess;

                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    rowLevelSecurityDetail = objAdminViewModel.GetRowLevelSecurityData(DimensionId, EntityName.Trim(' '), UserId, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                }
                _httpContextAccessor.HttpContext.Session.SetString("EntityName", EntityName);
                _httpContextAccessor.HttpContext.Session.SetString("DimensionId", DimensionId);
                _httpContextAccessor.HttpContext.Session.SetString("DimensionName", DimensionName);

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }

            return View(rowLevelSecurityDetail);
        }
        public ActionResult SaveRowLevelSecurityDetails(string OperatorValue, string UserId,
            string SupplyCode, string[] SelectedData, string[] SelectedValues)
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("EntityName") == null || _httpContextAccessor.HttpContext.Session.GetString("DimensionId") == null || _httpContextAccessor.HttpContext.Session.GetString("DimensionName") == null)
            {
                return Content("error" + Constant.sessionexpire);
            }
            string outMsg = Constant.statusSuccess;
            try
            {
                string EntityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName"));
                string DimensionId = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("DimensionId"));
                string DimensionName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("DimensionName"));
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    outMsg = objAdminViewModel.SaveRowLevelSecurityDetails(DimensionId, EntityName.Trim(' '), OperatorValue, UserId, SupplyCode, SelectedData, SelectedValues);
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            if (outMsg != Constant.statusSuccess)
                return Content("error" + outMsg);
            return Content("success");
        }
        public ActionResult GetDimensionIndex()
        {
            int index = 0;
            string outMsg = Constant.statusSuccess;
            List<DimensionName> dimensionList = new List<DimensionName>();
            string dimensionId = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("DimensionId"));

            try
            {
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    dimensionList = objMenuViewModel.ShowMenuData(out outMsg);
                }
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);
                // index = dimensionList.FindIndex(x => x..ToUpper() == dimensionId.ToUpper());
                index = index + 1;
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return Content(index.ToString());
        }

        public ActionResult DeleteRowLevelSecurity(string userId, string supplyCode)
        {
            
                using (LogError objLogErrorViewModel = new LogError())
                {
                    objLogErrorViewModel.LogErrorInTextFileTest(supplyCode, userId, userId);
                }
            
            if (_httpContextAccessor.HttpContext.Session.GetString("EntityName") == null || _httpContextAccessor.HttpContext.Session.GetString("DimensionId") == null || _httpContextAccessor.HttpContext.Session.GetString("DimensionName") == null)
            {
                return Content("error" + Constant.sessionexpire);
            }
            string outMsg = Constant.statusSuccess;
            try
            {
                string entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName"));
                string dimensionId = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("DimensionId"));
                string dimensionName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("DimensionName"));

                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    outMsg = objAdminViewModel.DeleteRowLevelSecurity(dimensionId, entityName.Trim(' '), userId, supplyCode);
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            if (outMsg != Constant.statusSuccess)
                return Content("error" + outMsg);
            return Content(outMsg);
        }

        public ActionResult GetWorkFlowDataForApplyingSecurity(string EntityName, string DimensionId, string DimensionName, string UserId, string RoleId, string RoleName)
        {
            WorkFlowDetailForRowLevelSecurity objWorkFlow = new WorkFlowDetailForRowLevelSecurity();
            try
            {
                string outMsg = Constant.statusSuccess;
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    objWorkFlow = objAdminViewModel.GetWorkFlowDataForApplyingSecurity(EntityName, UserId, RoleId, RoleName, out outMsg);
                }
                if (outMsg != Constant.statusSuccess)
                    return Content("error" + outMsg);
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);


            }
            return View(objWorkFlow);
        }
        public ActionResult SaveSelectedApprover(string RoleId, string UserId, string EntityName, string SelectedApproverId)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                if (!string.IsNullOrEmpty(SelectedApproverId))
                {
                    SelectedApproverId = SelectedApproverId.Trim(',');
                }                
                using (AdminViewModel objAdminViewModel = new AdminViewModel())
                {
                    objAdminViewModel.SaveSelectedApprover(RoleId, UserId, EntityName, SelectedApproverId);
                }
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            if (outMsg != Constant.statusSuccess)
                return Content("error" + outMsg);
            if (!string.IsNullOrEmpty(SelectedApproverId))
                return Content("Selected approvers saved successfully.");
            else
                return Content("Since no approvers are selected, workflow will be disabled for that user for that entity..");


        }
        public UserInfo GetCurrentUser(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            string sqlQuery;
            try
            {
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

                sqlQuery = "select USER_NAME as UserName, USER_ID as UserID, " +
                 "EMAIL_ID as UserEmail, ADMIN_FLAG as IsAdmin, PASSWORD as Password, " +
                    "ACTIVE as IsActive, ROLE_NAME, Total_Records as TOTAL_RECORDS from MPP_CORE.MPP_USER where UPPER(USER_ID) = '" +
                    userName[1].ToString().ToUpper() + "' ";
                using (Admin objAdmin = new Admin())
                {
                    currentUserInfo = objAdmin.GetCurrentUserInfo(sqlQuery, out outMsg);
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
            return currentUserInfo;
        }
    }
}