using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using MPP.ViewModel;

namespace MPP.ViewComponents
{
    public class SubMenuViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<EntityType> entityTypeList = new List<EntityType>();
            string outMsg = Constant.statusSuccess;
            try
            {
                ViewBag.SelectedDimensionData = HttpContext?.Session.GetString("SelectedDimensionValue");
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    entityTypeList = await Task.Run(()=>  objMenuViewModel.ShowSubMenuData(ViewBag.SelectedDimensionData, out outMsg));
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
            return View("~/Views/Shared/SubMenu.cshtml", entityTypeList);
            
        }
    }
}
