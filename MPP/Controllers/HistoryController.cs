using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Filter;
using MPP.ViewModel;

namespace MPP.Controllers
{
    [SessionTimeoutEntity]
    [SessionTimeoutDimension]
    public class HistoryController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HistoryController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ShowHistoryDetails(int OID)
        {
            string languageCode;
            string outMsg = Constant.statusSuccess;
            List<EntityTypeData> listEntityTypeData = new List<EntityTypeData>();
            try
            {
                string columnData = string.Empty;
                List<string> rowData = new List<string>();
                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                    if (previlegesData == null || previlegesData.UPDATE_FLAG != 1)
                    {
                        return Content("error" + Constant.accessDenied);
                    }
                    else
                    {
                        languageCode = previlegesData.LANGUAGE_CODE.ToString();
                    }
                }
                using (HistoryViewModel objHistoryViewModel = new HistoryViewModel())
                {
                    objHistoryViewModel.GetHistoryDetails(OID, entityTypeId, userName[1], out columnData, out rowData, out dataList, out outMsg);
                }
                TempData["dataList"] = dataList;
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content("error" + Constant.commonErrorMsg);
            }
            return View(listEntityTypeData);
        }
        public ActionResult ShowHistoryDetailsForWorkFlow(int OID)
        {
            string languageCode;
            string outMsg = Constant.statusSuccess;
            string columnData = string.Empty;
            List<string> rowData = new List<string>();
            List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
            try
            {

                int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                    if (previlegesData == null || previlegesData.READ_FLAG != 1)
                    {
                        return Content("error" + Constant.accessDenied);
                    }
                    else
                    {
                        languageCode = previlegesData.LANGUAGE_CODE.ToString();
                    }
                }
                using (HistoryViewModel objHistoryViewModel = new HistoryViewModel())
                {
                    objHistoryViewModel.GetHistoryDetailsForWorkFLow(OID, entityTypeId, out columnData, out rowData, out dataList, out outMsg);
                }
                TempData["dataList"] = dataList;



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
    }
}
