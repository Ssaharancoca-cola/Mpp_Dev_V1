using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Models;
using System.Diagnostics;
using DAL.Common;
using MPP.ViewModel;
using DAL;

namespace MPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            string outMsg = Constant.statusSuccess;
            List<DimensionName> dimensionsList = new List<DimensionName>();
            try
            {
                outMsg = CheckUserAccessRights();
                if (outMsg != Constant.statusSuccess)
                    return View("~/Views/Shared/UnauthorizedAccess.cshtml");
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    dimensionsList = objMenuViewModel.ShowMenuData(out outMsg);
                    dimensionsList = dimensionsList.ToList();
                    return View(dimensionsList);
                }
            }
            catch (Exception ex)
            {
                //  outMsg = ex.Message;
                using (LogErrorViewModel objLogErrorViewModel  = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content(Constant.commonErrorMsg);
            }
            
        }

        public IActionResult Home(string dropDownValue, string dropDownData, string selectedIndex, string actionType)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                outMsg = CheckUserAccessRights();
                if (outMsg != Constant.statusSuccess)
                    return View("~/Views/Shared/AccessDenied.cshtml");
                if (actionType == "Index")
                    HttpContext.Session.SetInt32("selectedIndex", Convert.ToInt32(selectedIndex));
                else
                    HttpContext.Session.SetString("selectedIndex", selectedIndex);
                HttpContext.Session.SetString("SelectedDimensionData", dropDownData.First().ToString().ToUpper() + String.Join("", dropDownData.Skip(1)).ToLower());
                HttpContext.Session.SetString("SelectedDimensionValue", dropDownValue);

                TempData["ShowSubMenu"] = "false";
            }
            catch (Exception ex)
            {                
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;                
            }
            return View("~/Views/Home/Home.cshtml");
        }
        public ActionResult SessionExpire()
        {
            string script = "Session();";
            return Content(script, "text/javascript");
        }
        public string CheckUserAccessRights()
        {
            string outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            try
            {
                currentUserInfo = GetCurrentUser(out outMsg);
                
                if (currentUserInfo == null || currentUserInfo.IsActive != 1)
                    outMsg = Constant.accessDenied;
                TempData["UserName"] = currentUserInfo.UserName;
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