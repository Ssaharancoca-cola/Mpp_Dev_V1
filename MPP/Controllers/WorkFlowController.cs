using DAL.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Filter;
using MPP.ViewModel;
using System.Security.Policy;
using System.Text;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    [SessionTimeoutEntity]
    public class WorkFlowController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string Link = "/MPP/";
        public WorkFlowController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        // GET: WorkFlow
        public ActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> WorkFlowAction(string Command, FormCollection form)
        {
            if (Command == "Abandon")
            {

            }
            else if (Command == "Cancel")
            {
                return await Task.Run(() => ViewComponent("ShowAttribute",
                                                  new
                                                  {
                                                      entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")),
                                                      entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")),
                                                      viewType = "search"
                                                  }));
            }
            return View();
        }
        public async Task<IActionResult> UpdateRecords(string OIDList, string Comment, string ActionType)
        {
            string returnMsg = string.Empty;
            List<UserInfo> userInfolist = new List<UserInfo>();
            if (!string.IsNullOrEmpty(OIDList))
            {
                // string[] InputRowIdList = OIDList.Split(',') ;
                List<Mail_Master> lstMailMaster = new List<Mail_Master>();
                string outMsg = Constant.statusSuccess;
                string Status = string.Empty;
                int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

                if (ActionType == "Abandon")
                {
                    Status = Constant.ABANDON;
                    returnMsg = Constant.abandonSuccessFullyMessage;
                    lstMailMaster = CreateMailListForAbandoned(OIDList.Trim(','), out outMsg);
                }
                else if (ActionType == "Approve")
                {
                    Status = Constant.APPROVE;
                    returnMsg = Constant.approvedSuccessFullyMessage;
                    using (MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel())
                    {
                        userInfolist = objMailManagerViewModel.GetUsersListForApproval(OIDList.Trim(','), entityTypeId, out outMsg);
                    }

                }
                else if (ActionType == "Delete")
                {
                    Status = Constant.DELETE;
                    returnMsg = Constant.deletedSuccessFullyMessage;
                }
                else if (ActionType == "Reject")
                {
                    Status = Constant.REJECT;
                    returnMsg = Constant.rejectedSuccessFullyMessage;
                }
                using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
                {
                    outMsg = objWorkFlowViewModel.UpdateStatus(OIDList.Trim(','), userName[1].ToUpper(), Status, Comment, entityTypeId);
                }
                if (outMsg == Constant.statusSuccess)
                {
                    if (ActionType == "Abandon")
                    {
                        MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel();
                        objMailManagerViewModel.SendMail(lstMailMaster, outMsg);
                    }
                }
                if (ActionType == "Approve" || ActionType == "Reject")
                {
                    if (ActionType == "Reject")
                    {
                        using (MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel())
                        {
                            userInfolist = objMailManagerViewModel.GetUsersListForApproval(OIDList.Trim(','), entityTypeId, out outMsg);
                        }
                    }
                    //if (outMsg == Constant.statusSuccess)
                    //    SendMailApproveReject(ActionType, OIDList.Trim(','), userInfolist, out outMsg);
                    //else
                    //{
                    //    //send error mail only in case of error in approving record
                    //    if (ActionType == "Approve")
                    //    {
                    //        SendMailError(OIDList, outMsg, out outMsg);
                    //        return Content("error" + "Please contact MPP team for support by mailing them the snapshot of the error which is given below" + outMsg);
                    //    }
                    //}


                }

                if (outMsg != Constant.statusSuccess)
                    return Content("error" + Constant.commonErrorMsg);                

            }
            return Content("success" + returnMsg);

        }
        public async Task<IActionResult> CancelAbandonRecord()
        {
            return await Task.Run(() => ViewComponent("ShowAttribute",
                                                             new
                                                             {
                                                                 entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")),
                                                                 entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")),
                                                                 viewType = "search"
                                                             }));
        }
    
        //public ActionResult GetWorkFlowData()
        //{
        //    string outMsg = Constant.statusSuccess;
        //    try
        //    {
        //        using (WorkFlowViewModel objWorkFlowViewModel = new WorkFlowViewModel())
        //        {
        //            int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
        //            string submittedColumnData;
        //            string rejectedColumnData;
        //            string approvalPendingColumnData;
        //            string existingRecordColumnData;

        //            List<string> submittedRowData = new List<string>();
        //            List<string> rejectedRowData = new List<string>();
        //            List<string> approvalPendingRowData = new List<string>();
        //            List<string> existingRecordRowData = new List<string>();

        //            List<Dictionary<string, string>> submittedDataList = new List<Dictionary<string, string>>();
        //            List<Dictionary<string, string>> rejectedDataList = new List<Dictionary<string, string>>();
        //            List<Dictionary<string, string>> approvalPendingDataList = new List<Dictionary<string, string>>();
        //            List<Dictionary<string, string>> existingRecordDataList = new List<Dictionary<string, string>>();


        //            objWorkFlowViewModel.LoadContentView(entityTypeId, out submittedColumnData, out submittedRowData, out submittedDataList);
        //            TempData["submittedColumnData"] = submittedColumnData;
        //            TempData["submittedRowData"] = submittedRowData;
        //            TempData["submitteddataList"] = submittedDataList;

        //            objWorkFlowViewModel.LoadContentReject(entityTypeId, out rejectedColumnData, out rejectedRowData, out rejectedDataList);
        //            TempData["rejectedColumnData"] = rejectedColumnData;
        //            TempData["rejectedRowData"] = rejectedRowData;
        //            TempData["rejecteddataList"] = rejectedDataList;

        //            objWorkFlowViewModel.LoadContentMyApproval(entityTypeId, out approvalPendingColumnData, out approvalPendingRowData,
        //                out approvalPendingDataList, out existingRecordColumnData, out existingRecordRowData, out existingRecordDataList);
        //            TempData["approvalPendingColumnData"] = approvalPendingColumnData;
        //            TempData["approvalPendingRowData"] = approvalPendingRowData;
        //            TempData["approvalPendingdataList"] = approvalPendingDataList;

        //            StringBuilder rowdata = new StringBuilder();
        //            foreach (var data in existingRecordRowData)
        //            {

        //                rowdata.Append(data.ToString());

        //                rowdata.Append(";");

        //            }
        //            _httpContextAccessor.HttpContext.Session.SetString("ExistingList", rowdata.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
        //        {
        //            objLogErrorViewModel.LogErrorInTextFile(ex);
        //        }
        //        outMsg = ex.Message;
        //    }
        //    return PartialView("~/Views/WorkFlow/GetWorkFlowData.cshtml");

        //}
        private List<Mail_Master> CreateMailListForAbandoned(string OIDList, out string outMsg)
        {
            List<Mail_Master> lstMailMaster = new List<Mail_Master>();
            try
            {
                outMsg = Constant.statusSuccess;
                using (MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel())
                {
                    string url = Request.GetDisplayUrl() + Link;
                    lstMailMaster = objMailManagerViewModel.CreateMailListForWorkFlow(OIDList.Trim(','), Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")),
                     Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("SelectedDimensionData")), url, Constant.ABANDON, out outMsg);
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
            return lstMailMaster;
        }
        private void SendMailApproveReject(string actionType, string OIDList, List<UserInfo> userInfoList, out string outMsg)
        {
            try
            {
                outMsg = Constant.statusSuccess;
                int eventId = 0;
                if (actionType == "Approve")
                    eventId = Convert.ToInt32(Mail_Master.MailEventDetail.APPROVE);
                else if (actionType == "Reject")
                    eventId = Convert.ToInt32(Mail_Master.MailEventDetail.REJECT);
                using (MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel())
                {
                    string url = Request.GetDisplayUrl() + Link;
                    objMailManagerViewModel.CreateMailListForApproveReject(eventId, OIDList.Trim(','), Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetString("EntityName")),
                    Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("SelectedDimensionData")), url, Constant.ABANDON, userInfoList,
                    out outMsg);
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
        }
        private void SendMailError(string OIDList, string errorMessage, out string outMsg)
        {
            try
            {
                outMsg = Constant.statusSuccess;
                int eventId = Convert.ToInt32(Mail_Master.MailEventDetail.ERROR);
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

                using (MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel())
                {
                    string url = Request.GetDisplayUrl() + Link;
                    objMailManagerViewModel.SendMailError(eventId, OIDList.Trim(','), Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), errorMessage,
                        Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), userName[1], out outMsg);
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
        }

    }
}
