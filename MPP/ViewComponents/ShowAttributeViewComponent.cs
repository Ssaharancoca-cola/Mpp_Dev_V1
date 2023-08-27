using DAL;
using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.ViewModel;
using Newtonsoft.Json;

namespace MPP.ViewComponents
{

    public class ShowAttributeViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int entityTypeId, string entityName, string viewType)
        {

            HttpContext.Session.Remove("fieldCollection");
            string outMsg = Constant.statusSuccess;
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            try
            {
                TempData["entityTypeId"] = entityTypeId;
                TempData.Peek("entityTypeId");
                HttpContext.Session.SetInt32("EntityTypeID", entityTypeId);
                HttpContext.Session.SetString("EntityName", entityName);
                ViewData["EntityTypeID"] = HttpContext.Session.GetString("EntityTypeID");
                ViewData["fieldCollection"] = null;
                ViewData["SelectedDimensionData"] = HttpContext.Session.GetString("SelectedDimensionData");
                ViewData["SelectedDimensionValue"] = HttpContext.Session.GetString("SelectedDimensionValue");
                ViewData["EntityTypeID"] = entityTypeId;
                ViewData["EntityName"] = HttpContext.Session.GetString("EntityName");
                 ViewData["selectedIndex"] = HttpContext.Session.GetString("selectedIndex");
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (PrevilegesDataViewModel previlegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = previlegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                    if ((outMsg != Constant.statusSuccess) || (previlegesData == null || previlegesData.READ_FLAG != 1))
                    {
                        ModelState.AddModelError("error", "error" + Constant.statusSuccess);
                        return Content(Constant.statusSuccess);
                    }
                }
                using (MenuViewModel menuViewModel = new MenuViewModel())
                {
                    (attributeList, outMsg) = await menuViewModel.ShowAttributeDataAsync(entityTypeId, viewType, userName[1].ToString());
                }
                attributeList = attributeList.OrderBy(x => x.AttrDisplayOrder).ToList();               

                // Serialize to JSON
                string attributeListJson = JsonConvert.SerializeObject(attributeList);
                TempData["attributeList"] = attributeListJson;
                TempData.Keep();

            }
            catch (Exception ex)
            {
                using (LogErrorViewModel logErrorViewModel = new LogErrorViewModel())
                {
                    logErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + ex.Message);
            }

            if (viewType == Constant.search)
            {
                return await Task.FromResult<IViewComponentResult>(View("~/Views/Shared/Attribute.cshtml", attributeList));
                //return View("~/Views/Shared/Attribute.cshtml", attributeList);
            }
            else if (viewType == Constant.update)
                return await Task.FromResult<IViewComponentResult>(View("~/Views/Shared/AttributeForUpdate.cshtml", attributeList));

            //return View("AttributeForUpdate", attributeList);
            return View("Attribute", attributeList);
        }
    }
}
