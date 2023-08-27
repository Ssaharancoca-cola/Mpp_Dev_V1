using DAL.Common;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UserManager : IDisposable
    {
        void IDisposable.Dispose()
        {

        }

        #region GetUser EmailID

        /// <summary>
        /// To get the email id of user 
        /// </summary>
        /// <param name="userID"></param>
        public UserInfo GetUserEmail(string userID, out string outMsg)
        {
            UserInfo userInfo = new UserInfo();
            outMsg = Constant.statusSuccess;
            try
            {
                string query = string.Format("select USER_NAME as UserName, EMAIL_ID as UserEmail from MPP_CORE.MPP_USER where UPPER(USER_ID) = UPPER('{0}')AND ACTIVE=1", userID);
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    userInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(query).FirstOrDefault();
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
        #endregion
        #region Get user info for selected records
        public List<UserInfo> GetUsersListForSelected(string InputRowIds, string tableName, string userName, out string outMsg)
        {
            List<UserInfo> lstUserInfo = new List<UserInfo>();
            UserInfo userInfo = new UserInfo();
            outMsg = Constant.statusSuccess;
            try
            {
                string selectCommand = "SELECT USER_ID as UserID,USER_NAME as UserName, EMAIL_ID as UserEmail FROM MPP_CORE.MPP_USER WHERE USER_ID IN (SELECT DISTINCT USER_ID FROM MPP_APP."+tableName+" WHERE INPUT_ROW_ID IN (" + InputRowIds + "))";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    lstUserInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(selectCommand).ToList();
                }

                foreach (UserInfo user in lstUserInfo)
                {
                    string selectQuery = "SELECT COUNT(DISTINCT LD_OID) AS CNT FROM MPP_APP." + tableName + " WHERE LD_OID IN (SELECT LD_OID FROM MPP_APP." + tableName + " WHERE INPUT_ROW_ID IN (" + InputRowIds + ")) AND UPPER(USER_ID)=UPPER('" + user.UserID + "')";
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        int count = 0;
                        //int count = objMPP_Context.Database.SqlQuery<int>(selectQuery).FirstOrDefault();
                        user.Total_Records = count.ToString();
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
            return lstUserInfo;

        }
        #endregion

        #region Get Next level Approver info for selected users
        public List<UserInfo> GetNextLevelApproversList(string UserId, out string outMsg, int entityId)
        {
            UserInfo Approver = new UserInfo();
            outMsg = Constant.statusSuccess;
            List<UserInfo> lstApproverInfo = new List<UserInfo>();
            string query;
            int count = 0;
            try
            {
                query = "SELECT count(*) as CNT from MPP_CORE.MPP_USER_PRIVILEGE WHERE UPPER(USER_ID)='" + UserId.ToUpper() + "' AND ENTITY_TYPE_ID=" + entityId + " AND APPROVER IS NULL";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    //count = objMPP_Context.Database.SqlQuery<int>(query).FirstOrDefault();
                }
                if (count != 0)
                    lstApproverInfo = null;
                else
                {
                    query = "SELECT U.USER_NAME as UserName ,U.EMAIL_ID as UserEmail FROM MDM_CORE.MDM_USER U,MDM_CORE.MDM_USER_PRIVILEGE t1,TABLE(t1.APPROVER)t2 WHERE U.USER_ID = t2.APPROVER_ID AND UPPER(t1.USER_ID) = '" + UserId.ToUpper() + "' AND t1.ENTITY_TYPE_ID=" + entityId;
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        lstApproverInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(query).ToList();
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
            return lstApproverInfo;
        }
        #endregion
        public List<UserInfo> GetNextLevelApproversList(string InputRowIds, UserInfo _UserInfo,string tableName,out string outMsg)
        {
            List<UserInfo> lstApproverInfo = new List<UserInfo>();
            outMsg = Constant.statusSuccess;
            string ldoid = string.Empty;
            try
            {
                string selectQuery ="SELECT MAX(LD_OID) AS LDOID FROM MPP_APP."+tableName+" WHERE LD_OID IN (SELECT LD_OID FROM MPP_APP."+tableName+" WHERE INPUT_ROW_ID IN (" + InputRowIds + ")) AND USER_ID='" + _UserInfo.UserID + "'";
                string selectCommand = "SELECT U.USER_NAME as USERNAME,U.EMAIL_ID AS EMAIL FROM MPP_APP." + tableName + " LD, MPP_CORE.MDM_USER U WHERE LD.APPROVER_ID = U.USER_ID AND LD.LD_OID = '" + ldoid + "' AND LD.USER_ID = '" + _UserInfo.UserID + "' AND LD.ROW_STATUS = 3 AND LD.APPROVER_STATUS = 'PENDING'";

                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    string ld_oid = objMPP_Context.Set<string>().FromSqlRaw(selectQuery).FirstOrDefault();
                    ldoid = Convert.ToString(ld_oid);
                    lstApproverInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(selectCommand).ToList();
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
            return lstApproverInfo;
        }


        #region Get  approvers  for abandoned record
        public List<UserInfo> GetApproversAndCountForAbandoned(string InputRowIds, string UserId, int entityTypeId, out string outMsg)
        {
            string query = string.Empty;
            outMsg = Constant.statusSuccess;
            string tableName = string.Empty;
            List<UserInfo> lstApproverInfo = new List<UserInfo>();
            List<UserInfo> lstApproverInfoWithCount = new List<UserInfo>();
            try
            {
                //Removed the hard coded value for EntityTypeId

                query = "SELECT USER_ID as UserID,USER_NAME as UserName, EMAIL_ID as UserEmail FROM MDM_CORE.MDM_USER WHERE USER_ID IN (SELECT APPROVER_ID FROM (SELECT t1.USER_ID,P.ROLE_ID AS APPROVER_LEVEL,t2.APPROVER_ID FROM MDM_CORE.MDM_USER_PRIVILEGE P, MDM_CORE.MDM_USER_PRIVILEGE t1,TABLE(t1.APPROVER)t2 WHERE P.USER_ID = t2.APPROVER_ID AND P.ENTITY_TYPE_ID = '" + entityTypeId + "' AND t1.ENTITY_TYPE_ID = P.ENTITY_TYPE_ID )START WITH USER_ID = '" + UserId.ToUpper() + "' CONNECT BY USER_ID = PRIOR APPROVER_ID)";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    lstApproverInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(query).ToList();
                }
                using (GetViewDetail objviewdetail = new GetViewDetail())
                {
                    tableName = objviewdetail.GetTableName(entityTypeId, out outMsg);
                    if (outMsg != Constant.statusSuccess || string.IsNullOrEmpty(tableName))
                        return lstApproverInfo;

                }
                foreach (UserInfo approver in lstApproverInfo)
                {

                    query = "SELECT COUNT(DISTINCT LD_OID) AS LDOIDCOUNT FROM "+tableName+" WHERE ROW_STATUS = 3 AND LD_OID IN (SELECT LD_OID FROM "+tableName+" WHERE INPUT_ROW_ID IN (" + InputRowIds + "))AND APPROVER_ID='" + approver.UserID + "'";
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        int count = 0;
                        //int count = objMPP_Context.Database.SqlQuery<int>(query).FirstOrDefault();
                        approver.Total_Records = count.ToString();
                        lstApproverInfoWithCount.Add(approver);
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
            return lstApproverInfoWithCount;

        }
        #endregion

        #region Check if the record is finally approved by all approvers for each user
        public int CheckForFinalApproval(string InputRowIds, string UserId,string tableName,out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            int count = 0;
            try
            {     
                string selectQuery = "SELECT COUNT(*) AS CNT FROM MPP_APP."+ tableName+" WHERE INPUT_ROW_ID IN (" + InputRowIds + ")AND USER_ID='" + UserId + "'";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                     //count = objMPP_Context.Database.SqlQuery<int>(selectQuery).FirstOrDefault();
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
            return count;
        }
        #endregion Check if the record is finally approved by all approvers for each user
        #region Get same and lower level approvers  for users
        public List<UserInfo> SameAndLowerLevelApprover(string InputRowIds, string UserId, string ApproverId,string inputTable,int entityId,out string outMsg)
        {
            List<UserInfo> lstApproverInfo = new List<UserInfo>();
            outMsg = Constant.statusSuccess;
            string ldoid = string.Empty;
            try
            {
                string selectQuery = "SELECT MAX(LD_OID) AS LDOID FROM MPP_APP."+ inputTable+" WHERE INPUT_ROW_ID IN (" + InputRowIds + ") AND UPPER(USER_ID)=UPPER('" + UserId + "')";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    //int Index = objMPP_Context.Set<int>().FromSqlRaw(selectQuery).AsEnumerable().FirstOrDefault();
                    //ldoid = Convert.ToString(Index);
                }
                string selectCommand = "SELECT U.USER_NAME as USERNAME,U.EMAIL_ID AS EMAIL FROM MPP_APP."+inputTable+" LD, MPP_CORE.MPP_USER U WHERE LD.APPROVER_ID = U.USER_ID AND LD.LD_OID = '" + ldoid + "' AND UPPER(LD.USER_ID) =UPPER('" + UserId + "') AND LD.ROW_STATUS = 4 AND LD.APPROVER_LEVEL <=(SELECT ROLE_ID FROM MPP_CORE.MPP_USER_PRIVILEGE P WHERE P.USER_ID='" + ApproverId + "' AND P.ENTITY_TYPE_ID=" + entityId + ")";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    lstApproverInfo = objMPP_Context.Set<UserInfo>().FromSqlRaw(selectCommand).ToList();
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
            return lstApproverInfo;

        }
        #endregion Get same and lower level approvers  for users


    }
}
