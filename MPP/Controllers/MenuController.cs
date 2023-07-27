
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
        ///<summary>
        /// This method will return partial view which will display menu bar on top 
        ///</summary>
        ///<returns>Partial View</returns>
        public IActionResult Menu()
        {
            string outMsg = Constant.statusSuccess;
            List<DimensionName> dimensionList = new List<DimensionName>();
            try
            {
                outMsg = CheckUserAccessRights();
                if (outMsg != Constant.statusSuccess)
                    return View("~/Views/Shared/UnauthorizedAccess.cshtml");
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    dimensionList = objMenuViewModel.ShowMenuData(out outMsg);
                    return PartialView("~/Views/Shared/Menu.cshtml", dimensionList);
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogErrorViewModel logErrorViewModel = new LogErrorViewModel())
                {
                    logErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content(Constant.commonErrorMsg);
            }
        }

        // [ChildActionOnly]
        ///<summary>
        /// This method will return partial view which will display sub menu bar on left hand side
        ///</summary>
        ///<returns>Partial View</returns>
        public IActionResult SubMenu()
        {
            List<EntityType> entityTypeList = new List<EntityType>();
            string outMsg = Constant.statusSuccess;
            try
            {
                string SelectedDimensionValue = HttpContext.Session.GetString("SelectedDimensionValue");
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    entityTypeList = objMenuViewModel.ShowSubMenuData(SelectedDimensionValue, out outMsg);
                }
                entityTypeList = entityTypeList.OrderBy(x => x.DisplayOrder).ToList();
            }
            catch (Exception ex)
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

        /// <summary>
        /// This method will return partial view which will display Attributes
        /// </summary>
        /// <returns>Partial View</returns>
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
                using (AdminViewModel adminViewModel = new AdminViewModel())
                {
                    currentUserInfo = adminViewModel.GetCurrentUser(out outMsg);
                }
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
                entityTypeList = entityTypeList.OrderBy(x => int.TryParse(x.DisplayOrder, out int num) ? num : int.MaxValue).ToList();
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
    }
}
