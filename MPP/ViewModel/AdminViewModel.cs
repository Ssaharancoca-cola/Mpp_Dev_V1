using Model;
using DAL;
using DAL.Common;

namespace MPP.ViewModel
{
    public class AdminViewModel : IDisposable
    {
        void IDisposable.Dispose() { }
        public List<UserInfo> GetALlUsersInfo(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<UserInfo> allUserInfo = new List<UserInfo>();
            string sqlQuery;
            try
            {
                sqlQuery = "SELECT USER_NAME as UserName, User_ID as UserID, " +
                    "EMAIL_ID as UserEmail, ACTIVE as Active, ROLE_NAME," +
                    "ADMIN_FLAG as IsAdmin, PASSWORD as Password " +
                    "FROM MPP_CORE.MPP_USER ORDER BY USER_NAME";
                    using (Admin objAdmin = new Admin())
                    {
                        allUserInfo = objAdmin.GetAllUsers(sqlQuery, out outMsg);
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
            return allUserInfo;
        }
        public UserInfo GetCurrentUser(out string outMsg) 
        {
            outMsg= Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            string sqlQuery;
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);

                sqlQuery = "select USER_NAME as UserName, USER_ID as UserID, " +
                 "EMAIL_ID as UserEmail, ADMIN_FLAG as IsAdmin, PASSWORD as Password, " +
                    "ACTIVE as IsActive, ROLE_NAME from MPP_CORE.MPP_USER where UPPER(USER_ID) = '" +
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
        public UserInfo GetUserDetails(string userId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            UserInfo userInfo = new UserInfo();
            string sqlQuery;
            try
            {
                sqlQuery = "select USER_NAME as UserName, USER_ID as UserID, EMAIL_ID as UserEmail, ADMIN_FLAG as IsAdmin, PASSWORD as Password, ACTIVE as IsActive, ROLE_NAME from MPP_CORE where UPPER(USER_ID) = '" + userId + "' ";
                using (Admin objAdmin = new Admin())
                {
                    userInfo = objAdmin.GetCurrentUserInfo(sqlQuery, out outMsg);
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
            return userInfo;
        }
        public bool CheckUserAdminRights()
        {
            string outMsg = Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            currentUserInfo = GetCurrentUser(out outMsg);
            if (currentUserInfo.IsAdmin == 1)
            {
                return true;
            }
            else
                return false;
        }

    }
}
