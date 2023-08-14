using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;

namespace MPP.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            string outMsg = Constant.statusSuccess;
            List<DimensionName> dimensionList = new List<DimensionName>();
            try
            {
               // outMsg = CheckUserAccessRights();
                if (outMsg != Constant.statusSuccess)
                    return View("~/Views/Shared/UnauthorizedAccess.cshtml");
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    dimensionList =  objMenuViewModel.ShowMenuData(out outMsg);
                    return View("~/Views/Shared/Menu.cshtml", dimensionList);
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
    }
}
