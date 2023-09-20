
using DAL;
using DAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Models;
using MPP.Filter;
using MPP.ViewModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    public class MenuController : Controller
    {
       
        [HttpPost]
        public async Task<IActionResult> ShowAttribute(int entityTypeId, string entityName, string viewType)
        {
            
            return await Task.Run(()=> ViewComponent("ShowAttribute", 
                new
                    {
                         entityTypeId,
                         entityName,
                         viewType
                    }));

        }
        public string CheckUserAccessRights()
        {
            string outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new();
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

        public ActionResult ShowSubMenu(string dropdownvalue, string dropdowndata, string selectedIndex)
        {
            TempData["ShowSubMenu"] = "true";

            HttpContext.Session.SetString("selectedIndex", selectedIndex);
            HttpContext.Session.SetString("SelectedDimensionData", dropdowndata.First().ToString().ToUpper() + String.Join("", dropdowndata.Skip(1)).ToLower());
            HttpContext.Session.SetString("SelectedDimensionValue", dropdownvalue);
            ViewBag.SelectedDimensionData = HttpContext.Session.GetString("SelectedDimensionValue");
            List<EntityType> entityTypeList = new List<EntityType>();
            string outMsg = Constant.statusSuccess;
            try
            {
                object Session = null;
                string SelectedDimensionValue = HttpContext.Session.GetString("SelectedDimensionValue");

                using(MenuViewModel menuViewModel = new MenuViewModel())
                {
                    entityTypeList = menuViewModel.ShowSubMenuData(SelectedDimensionValue, out outMsg);
                }
                entityTypeList = entityTypeList.OrderBy(x=> x.DisplayOrder).ToList();
            }
            catch(Exception ex)
            {
                using (LogErrorViewModel logErrorViewModel = new LogErrorViewModel())
                {
                    logErrorViewModel.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
                return Content(outMsg);
            }
            return PartialView("~/Views/Shared/SubMenu.cshtml", entityTypeList);
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
