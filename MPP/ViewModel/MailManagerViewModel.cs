using DAL;
using DAL.Common;
using Microsoft.AspNetCore.Identity;
using Model;
using System.Net.Mail;

namespace MPP.ViewModel
{
public class MailManagerViewModel : IDisposable
{
        private readonly string SMTPHost = "zwdmyaa0001.apac.ko.com";
        private readonly string MailFrom = "Do-Not-Reply@coca-cola.com";
    void IDisposable.Dispose()
    {

    }
    public void Mail(string count, string record, int entityID, string EntityName, string[] userName, string url, string action, out string outMsg)
    {
        outMsg = Constant.statusSuccess;        

        Mail_Master _MAIL_MASTER;
        UserInfo _UserInfo = new UserInfo();
        List<UserInfo> _UserInfolist = new List<UserInfo>();
        Mail_Manager _MAIL_MANAGERBL = new Mail_Manager();
        UserManager _UserManagerBL = new UserManager();        
        List<Mail_Master> lstMailMaster = new List<Mail_Master>();       
        string FirstName = string.Empty;     

        try
        {
            _MAIL_MASTER = new Mail_Master();
            _UserInfolist = _UserManagerBL.GetNextLevelApproversList(userName[1].ToUpper(), out outMsg, entityID);
            if (_UserInfolist == null)
            {
                int id = 0;
                if (action == Constant.addNew || action == Constant.import)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.SUBMITWITHOUTWORKFLOW);
                else if (action == Constant.update)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.UPDATEWITHOUTWORKFLOW);
                _MAIL_MASTER = _MAIL_MANAGERBL.GetMailDetails(id, out outMsg);
            }
            else
            {
                int id = 0;
                if (action == Constant.addNew || action == Constant.import)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.SUBMIT);
                else if (action == Constant.update)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.UPDATE);
                _MAIL_MASTER = _MAIL_MANAGERBL.GetMailDetails(id, out outMsg);
            }
            _UserInfo = _UserManagerBL.GetUserEmail(userName[1].ToUpper(), out outMsg);
            string[] Names = _UserInfo.UserName.Split(' ');
            FirstName = Names[0].ToString();
            _MAIL_MASTER.MailTo = _UserInfo.UserEmail;
            string body = _MAIL_MASTER.MailBody;
            body = body.Replace("&username&", FirstName);
            body = body.Replace("&count&", count);
            if (count.ToString() == "1")
                body = body.Replace("records", "record");
            body = body.Replace("&entity&", EntityName);
            body = body.Replace("&url&", url);
            _MAIL_MASTER.MailBody = body;
            lstMailMaster.Add(_MAIL_MASTER);

            if (_UserInfolist != null)
            {
                int id = 0;
                if (action == Constant.addNew || action == Constant.import)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.NOTIFYAPPROVE);
                else if (action == Constant.update)
                    id = Convert.ToInt32(Mail_Master.MailEventDetail.NOTIFYUPDATE);
                _MAIL_MASTER = _MAIL_MANAGERBL.GetMailDetails(id, out outMsg);
                string subject = _MAIL_MASTER.MailSubject;
                body = _MAIL_MASTER.MailBody;
                foreach (UserInfo approver in _UserInfolist)
                {
                    _MAIL_MASTER = new Mail_Master();
                    _MAIL_MASTER.MailTo = approver.UserEmail;
                    _MAIL_MASTER.MailSubject = subject;
                    string MailBody = body;
                    if (count.ToString() == "1")
                        MailBody = body.Replace("records", "record");
                    MailBody = MailBody.Replace("&count&", count);// = body.Replace("&count&", "one");
                    MailBody = MailBody.Replace("&approvername&", approver.UserName);
                    MailBody = MailBody.Replace("&username&", _UserInfo.UserName);
                    MailBody = MailBody.Replace("are", "is");
                    // MailBody = MailBody.Replace("records", "record");
                    MailBody = MailBody.Replace("&entity&", EntityName);
                    MailBody = MailBody.Replace("&url&", url);
                    _MAIL_MASTER.MailBody = MailBody;
                    lstMailMaster.Add(_MAIL_MASTER);
                }
            }
            MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel();
            objMailManagerViewModel.SendMail(lstMailMaster, outMsg);
        }
        catch (Exception ex)
        {
            using (LogError objLogError = new LogError())
            {
                objLogError.LogErrorInTextFile(ex);
            }
            outMsg = ex.Message;
        }
    }

    public List<Mail_Master> CreateMailListForWorkFlow(string InputRowIds, int entityID, string EntityName, string DimensionName, string url,
        string action, out string outMsg)
    {
        string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
        outMsg = Constant.statusSuccess;
        Mail_Master _MAIL_MASTER;
        List<Mail_Master> lstMailMaster = new List<Mail_Master>();
        UserInfo _UserInfo = new UserInfo();
        string[] InputRowId = InputRowIds.Split(',');
        int count = InputRowId.Length;
        int EventId = 0;
        int NotifyEventId = 0;
        string FirstName = string.Empty;
        try
        {
            _MAIL_MASTER = new Mail_Master();
            using (UserManager objUserManager = new UserManager())
            {
                _UserInfo = objUserManager.GetUserEmail(userName[1].ToUpper(), out outMsg);
            }
            if (action == Constant.ABANDON)
            {
                EventId = Convert.ToInt32(Mail_Master.MailEventDetail.ABANDON);
                NotifyEventId = Convert.ToInt32(Mail_Master.MailEventDetail.NOTIFYABANDON);
            }
            using (Mail_Manager objMailManager = new Mail_Manager())
            {
                _MAIL_MASTER = objMailManager.GetMailDetails(EventId, out outMsg);
            }
            string[] Names = _UserInfo.UserName.Split(' ');
            FirstName = Names[0].ToString();
            _MAIL_MASTER.MailTo = _UserInfo.UserEmail;
            string mailBody = _MAIL_MASTER.MailBody;
            mailBody = mailBody.Replace("&username&", FirstName);
            mailBody = mailBody.Replace("&count&", count.ToString());
            if (count.ToString() == "1")
                mailBody = mailBody.Replace("records", "record");
            mailBody = mailBody.Replace("&entity&", EntityName);
            mailBody = mailBody.Replace("&url&", url);
            _MAIL_MASTER.MailBody = mailBody;
            lstMailMaster.Add(_MAIL_MASTER);
            List<UserInfo> lstApproverInfo = new List<UserInfo>();
            using (UserManager objUserManager = new UserManager())
            {
                lstApproverInfo = objUserManager.GetApproversAndCountForAbandoned(InputRowIds, userName[1].ToUpper(), entityID, out outMsg);
            }
            using (Mail_Manager objMailManager = new Mail_Manager())
            {
                _MAIL_MASTER = objMailManager.GetMailDetails(NotifyEventId, out outMsg);
            }
            string body = _MAIL_MASTER.MailBody;
            string subject = _MAIL_MASTER.MailSubject;
            foreach (UserInfo approver in lstApproverInfo)
            {
                if (approver.Total_Records.ToString() != "0")
                {
                    _MAIL_MASTER = new Mail_Master();
                    _MAIL_MASTER.MailTo = approver.UserEmail;
                    string mailbody = body.Replace("&count&", approver.Total_Records.ToString());
                    if (approver.Total_Records.ToString() == "1")
                    {
                        mailbody = mailbody.Replace("records", "record");
                    }
                    mailbody = mailbody.Replace("&approvername&", approver.UserName);
                    mailbody = mailbody.Replace("&entity&", EntityName);
                    mailbody = mailbody.Replace("&username&", _UserInfo.UserName);
                    mailbody = mailbody.Replace("&url&", url);
                    _MAIL_MASTER.MailBody = mailbody;
                    _MAIL_MASTER.MailSubject = subject;
                    lstMailMaster.Add(_MAIL_MASTER);
                }
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
        return lstMailMaster;
    }
    public List<UserInfo> GetUsersListForApproval(string InputRowIds, int entityID, out string outMsg)
    {
        List<UserInfo> userInfolist = new List<UserInfo>();
        try
        {
            outMsg = Constant.statusSuccess;
            string tableName = string.Empty;
            string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
            using (GetViewDetail objGetViewDetail = new GetViewDetail())
            {
                tableName = objGetViewDetail.GetTableName(entityID, out outMsg);
            }
            using (UserManager objUserManager = new UserManager())
            {
                userInfolist = objUserManager.GetUsersListForSelected(InputRowIds.Trim(','), tableName, userName[1], out outMsg);
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
        return userInfolist;
    }
    public void CreateMailListForApproveReject(int eventID, string InputRowIds, int entityID, string EntityName, string DimensionName, string url,
     string action, List<UserInfo> userInfolist, out string outMsg)
    {
        string tableName = string.Empty;
        UserInfo _ApproverInfo = null;
        Mail_Master _MAIL_MASTER;
        string subject = string.Empty;
        string body = string.Empty;
        List<Mail_Master> lstMailMaster = new List<Mail_Master>();
        int ApproveId = Convert.ToInt32(Mail_Master.MailEventDetail.APPROVE);
        int RejectId = Convert.ToInt32(Mail_Master.MailEventDetail.REJECT);
        string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
        try
        {
            using (GetViewDetail objGetViewDetail = new GetViewDetail())
            {
                tableName = objGetViewDetail.GetTableName(entityID, out outMsg);
            }

            if ((eventID == ApproveId) || eventID == RejectId)
            {
                //String ApproverId = HttpContext.User.Identity.Name.ToUpper();
                using (UserManager objUserManager = new UserManager())
                {
                    _ApproverInfo = objUserManager.GetUserEmail(userName[1], out outMsg);
                }
                using (Mail_Manager objmailManager = new Mail_Manager())
                {
                    _MAIL_MASTER = objmailManager.GetMailDetails(eventID, out outMsg);
                }
                subject = _MAIL_MASTER.MailSubject;
                body = _MAIL_MASTER.MailBody;
                foreach (UserInfo user in userInfolist)
                {
                    _MAIL_MASTER = new Mail_Master();
                    _MAIL_MASTER.MailToCC = _ApproverInfo.UserEmail;
                    _MAIL_MASTER.MailTo = user.UserEmail;
                    string MailBody = body.Replace("&count&", user.Total_Records);
                    if (user.Total_Records == "1")
                        MailBody = MailBody.Replace("records", "record");
                    MailBody = MailBody.Replace("&username&", user.UserName);
                    MailBody = MailBody.Replace("&entity&", EntityName);
                    MailBody = MailBody.Replace("&approvername&", _ApproverInfo.UserName);
                    MailBody = MailBody.Replace("&url&", url);
                    _MAIL_MASTER.MailBody = MailBody;
                    _MAIL_MASTER.MailSubject = subject;
                    lstMailMaster.Add(_MAIL_MASTER);
                }
            }
            if (eventID == ApproveId)
            {
                //send to next level of approvers
                foreach (UserInfo user in userInfolist)
                {
                    List<UserInfo> _lstNextLevelApprover = new List<UserInfo>();
                    using (UserManager objUserManager = new UserManager())
                    {
                        _lstNextLevelApprover = objUserManager.GetNextLevelApproversList(InputRowIds, user, tableName, out outMsg);
                    }
                    int NewEventId = Convert.ToInt32(Mail_Master.MailEventDetail.PENDINGAPPROVAL);
                    using (Mail_Manager objmailManager = new Mail_Manager())
                    {
                        _MAIL_MASTER = objmailManager.GetMailDetails(NewEventId, out outMsg);
                    }
                    subject = _MAIL_MASTER.MailSubject;
                    body = _MAIL_MASTER.MailBody;
                    foreach (UserInfo Approver in _lstNextLevelApprover)
                    {
                        _MAIL_MASTER = new Mail_Master();
                        _MAIL_MASTER.MailTo = Approver.UserEmail;
                        string MailBody = body.Replace("&count&", user.Total_Records);
                        if ((user.Total_Records).ToString() == "1")
                        {
                            MailBody = MailBody.Replace("records", "record");
                            MailBody = MailBody.Replace("are", "is");
                        }
                        MailBody = MailBody.Replace("&approvername&", Approver.UserName);
                        MailBody = MailBody.Replace("&username&", user.UserName);
                        MailBody = MailBody.Replace("&entity&", EntityName);
                        MailBody = MailBody.Replace("&url&", url);
                        _MAIL_MASTER.MailBody = MailBody;
                        _MAIL_MASTER.MailSubject = subject;
                        lstMailMaster.Add(_MAIL_MASTER);
                    }
                }
                //check final approver & send mail to user
                int count = 0, FinalApprovalCount = 0;
                //url = Request.Url.GetLeftPart(UriPartial.Authority) + appName + "/CATEGORY.aspx?&Action=" + DimensionName;
                int finalApproval = Convert.ToInt32(Mail_Master.MailEventDetail.FINALAPPROVE);

                using (Mail_Manager objmailManager = new Mail_Manager())
                {
                    _MAIL_MASTER = objmailManager.GetMailDetails(finalApproval, out outMsg);
                }
                subject = _MAIL_MASTER.MailSubject;
                body = _MAIL_MASTER.MailBody;
                foreach (UserInfo user in userInfolist)
                {
                    using (UserManager objUserManager = new UserManager())
                    {
                        count = objUserManager.CheckForFinalApproval(InputRowIds, userName[1], tableName, out outMsg);
                    }
                    FinalApprovalCount = Convert.ToInt32(user.Total_Records) - count;
                    if (FinalApprovalCount > 0)
                    {
                        _MAIL_MASTER = new Mail_Master();
                        _MAIL_MASTER.MailTo = user.UserEmail;
                        string MailBody = body.Replace("&count&", FinalApprovalCount.ToString());
                        if (FinalApprovalCount.ToString() == "1")
                        {
                            MailBody = MailBody.Replace("records", "record");
                            MailBody = MailBody.Replace("are", "is");
                        }
                        MailBody = MailBody.Replace("&username&", user.UserName);
                        MailBody = MailBody.Replace("&entity&", EntityName);
                        MailBody = MailBody.Replace("&url&", url);
                        _MAIL_MASTER.MailBody = MailBody;
                        _MAIL_MASTER.MailSubject = subject;
                        lstMailMaster.Add(_MAIL_MASTER);
                    }
                }
            }

            if (eventID == Convert.ToInt32(Mail_Master.MailEventDetail.REJECT))
            {
                foreach (UserInfo user in userInfolist)
                {
                    List<UserInfo> _lstSameAndLowerLevelApprover = new List<UserInfo>();
                    using (UserManager objUserManager = new UserManager())
                    {
                        _lstSameAndLowerLevelApprover = objUserManager.SameAndLowerLevelApprover(InputRowIds, user.UserID, userName[1],
                            tableName, entityID, out outMsg);
                    }
                    int RejectEventId = Convert.ToInt32(Mail_Master.MailEventDetail.NOTIFYREJECT);
                    using (Mail_Manager objmailManager = new Mail_Manager())
                    {
                        _MAIL_MASTER = objmailManager.GetMailDetails(RejectEventId, out outMsg);
                    }
                    subject = _MAIL_MASTER.MailSubject;
                    body = _MAIL_MASTER.MailBody;
                    foreach (UserInfo Approver in _lstSameAndLowerLevelApprover)
                    {
                        if (Approver.UserName != _ApproverInfo.UserName)
                        {
                            _MAIL_MASTER = new Mail_Master();
                            _MAIL_MASTER.MailTo = Approver.UserEmail;
                            string MailBody = body.Replace("&count&", user.Total_Records);
                            if ((user.Total_Records).ToString() == "1")
                            {
                                MailBody = MailBody.Replace("records", "record");
                            }
                            MailBody = MailBody.Replace("&approvername&", Approver.UserName);
                            MailBody = MailBody.Replace("&username&", user.UserName);
                            MailBody = MailBody.Replace("&rejectername&", _ApproverInfo.UserName);
                            MailBody = MailBody.Replace("&entity&", EntityName);
                            MailBody = MailBody.Replace("&url&", url);
                            _MAIL_MASTER.MailBody = MailBody;
                            _MAIL_MASTER.MailSubject = subject;
                            lstMailMaster.Add(_MAIL_MASTER);
                        }
                    }
                }
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
        MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel();
        objMailManagerViewModel.SendMail(lstMailMaster, outMsg);
    }
    public void SendMailError(int EventId, string InputRowIds, string EntityName, string errorMessage, int entityId, string userName, out string outMsg)
    {
        List<UserInfo> _UserInfolist;
        string tableName = string.Empty;
        outMsg = Constant.statusSuccess;
        try
        {
            using (GetViewDetail objGetViewDetail = new GetViewDetail())
            {
                tableName = objGetViewDetail.GetTableName(entityId, out outMsg);
            }
            using (UserManager objUserManager = new UserManager())
            {
                _UserInfolist = objUserManager.GetUsersListForSelected(InputRowIds.Trim(','), tableName, userName, out outMsg);

            }
            Mail_Master _MAIL_MASTER;
            List<Mail_Master> lstMailMaster = new List<Mail_Master>();

            UserInfo _ApproverInfo = null;
            string subject = string.Empty;
            string body = string.Empty;

            //Get approvers mailid
            String ApproverId = userName;
            using (UserManager objUserManager = new UserManager())
            {
                _ApproverInfo = objUserManager.GetUserEmail(ApproverId, out outMsg);
            }
            using (Mail_Manager objmailManager = new Mail_Manager())
            {
                _MAIL_MASTER = objmailManager.GetMailDetails(EventId, out outMsg);
            }
            subject = _MAIL_MASTER.MailSubject;
            body = _MAIL_MASTER.MailBody;
            foreach (UserInfo user in _UserInfolist)
            {
                _MAIL_MASTER = new Mail_Master();
                _MAIL_MASTER.MailToCC = _ApproverInfo.UserEmail;
                _MAIL_MASTER.MailTo = user.UserEmail;
                string MailBody = body.Replace("&count&", user.Total_Records);
                if (user.Total_Records == "1")
                    MailBody = MailBody.Replace("records", "record");
                MailBody = MailBody.Replace("&username&", user.UserName);
                MailBody = MailBody.Replace("&entity&", EntityName);
                MailBody = MailBody.Replace("&error&", errorMessage);

                _MAIL_MASTER.MailBody = MailBody;
                _MAIL_MASTER.MailSubject = subject;
                lstMailMaster.Add(_MAIL_MASTER);
            }
            MailManagerViewModel objMailManagerViewModel = new MailManagerViewModel();
            objMailManagerViewModel.SendMail(lstMailMaster, outMsg);
        }
        catch (Exception ex)
        {
            using (LogError objLogError = new LogError())
            {
                objLogError.LogErrorInTextFile(ex);
            }
            outMsg = ex.Message;
        }
    }


    #region SendMail
    public void SendMail(List<Mail_Master> lstMailMaster, string outMsg)
    {
        SmtpClient smtpClient = new SmtpClient();
        smtpClient.Host = SMTPHost;
        try
        {
            foreach (Mail_Master mail in lstMailMaster)
            {

                MailMessage mailMessage = new MailMessage();
                mailMessage.Body = mail.MailBody;
                mailMessage.Subject = mail.MailSubject;
                mailMessage.From = new MailAddress(MailFrom);
                mailMessage.To.Add(new MailAddress(mail.MailTo));
                if (mail.MailToCC != null)
                    mailMessage.CC.Add(new MailAddress(mail.MailToCC));
                mailMessage.IsBodyHtml = true;
                smtpClient.Send(mailMessage);
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
    }
    #endregion SendMail
}
}