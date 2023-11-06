using Model;
using DAL;
using DAL.Common;
using System.Text;
using Model.Models;

namespace MPP.ViewModel
{
    public class AdminViewModel : IDisposable
    {
        void IDisposable.Dispose() { }
        public List<UserInfo> GetAllUsersInfo(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<UserInfo> allUserInfo = new List<UserInfo>();
            string sqlQuery;
            try
            {
                sqlQuery = "SELECT USER_NAME as UserName, User_ID as UserID, " +
                    "EMAIL_ID as UserEmail, ACTIVE as Active, ROLE_NAME," +
                    "ADMIN_FLAG as IsAdmin,ISACTIVE as IsActive, Total_Records as TOTAL_RECORDS, PASSWORD as Password " +
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
        public UserInfo GetUserDetails(string userId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            UserInfo userInfo = new UserInfo();
            string sqlQuery;
            try
            {
                sqlQuery = "select USER_NAME as UserName, USER_ID as UserID, EMAIL_ID as UserEmail, ADMIN_FLAG as IsAdmin, PASSWORD as Password, TOTAL_RECORDS as Total_Records, ACTIVE as IsActive, ROLE_NAME from MPP_CORE.MPP_USER where UPPER(USER_ID) = '" + userId + "' ";
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
        public List<UserRowSecurity> GetEntities(UserInfo User, List<SearchParameter> fields)
        {
            List<UserRowSecurity> lstUserRowSecurity = new List<UserRowSecurity>();
            try
            {
                string outMsg = Constant.statusSuccess;
                var searchValue = fields[0].SearchValue;
                UserRowSecurity objUserRowSecurity = new UserRowSecurity();
                string selectQuery = "Select ROLE_ID, ROLE_NAME from MPP_CORE.MPP_ROLE_LEVEL";
                List<ROLE> lstROLE = new List<ROLE>();
                using (Admin objAdmin = new Admin())
                {
                    lstROLE = objAdmin.GetRoleDetail(selectQuery, out outMsg);
                }
                
                String Query = @"SELECT DISTINCT
                                    P.USER_ID AS UserID,
                                    ISNULL(P.ROLE_ID,0)  as ROLE_ID,
                                    E.NAME AS EntityName,
                                    E.ID AS ENTITY_TYPE_ID,
                                    E.DIMENSION_NAME AS DIMENSION,                                    
                                    ISNULL(P.READ_FLAG,0)   AS READ_FLAG,
                                    ISNULL(P.UPDATE_FLAG,0) AS UPDATE_FLAG,
                                    ISNULL(P.CREATE_FLAG,0) AS CREATE_FLAG,
                                    ISNULL(P.IMPORT_FLAG,0) AS IMPORT_FLAG,
                                    CASE WHEN Q.ENTITY_TYPE_ID IS NULL THEN 'Apply' ELSE 'Edit' END AS FLG,
                                    CASE WHEN P.APPROVER IS NULL THEN 'Apply' ELSE 'Edit' END As WFLG,
                                    (SELECT COUNT(*) 
                                    FROM MPP_CORE.MPP_USER_PRIVILAGE t1 
                                    CROSS APPLY (SELECT * FROM MPP_CORE.MPP_USER_PRIVILAGE t WHERE t.approver = '" + User.UserID + @"') t2
                                    WHERE t2.approver = '" + User.UserID + @"' AND t1.entity_type_id = P.ENTITY_TYPE_ID ) as APR_FLG  
                        FROM (SELECT * FROM MPP_CORE.Entity_Type where dimension_display_name = '" + fields[0].SearchValue + @"' ) E
                        LEFT JOIN
                        MPP_CORE.MPP_USER_PRIVILAGE P
                        ON P.ENTITY_TYPE_ID = E.ID AND P.USER_ID = '" + User.UserID + @"'
                        LEFT OUTER JOIN MPP_CORE.USER_ROW_SECURITY Q
                        ON 
                        Q.ENTITY_TYPE_ID = P.ENTITY_TYPE_ID    
                        AND 
                        Q.USER_ID = P.USER_ID
                        AND 
                        Q.USER_ID = '" + User.UserID + @"'";
                using (Admin objAdmin = new Admin())
                {
                    lstUserRowSecurity = objAdmin.GetSecurityDetail(Query, lstROLE, out outMsg);
                }

            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
            }
            return lstUserRowSecurity;
        }

        public void UpdateUserDetails(UserInfo userInfo, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            try
            {
                using (Admin objAdmin = new Admin())
                {
                    objAdmin.UpdateUserDetails(userInfo, out outMsg);
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

        public void CreateUser(UserInfo userInfo, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            try
            {
                using (Admin objAdmin = new Admin())
                {
                    objAdmin.CreateUser(userInfo, out outMsg);
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
        public int GetEntityTypeId(string entityName, out string outMsg)
        {
            int entityTypeId = 0;
            outMsg = Constant.statusSuccess;
            try
            {
                using (Admin objadmin = new Admin())
                {
                    entityTypeId = objadmin.GetEntityTypeId(entityName, out outMsg);
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
            return entityTypeId;
        }
        public string GetSuppliedCode(string dimensionName, int entityTypeId, out string outMsg)
        {
            string suppliedCode = string.Empty;
            outMsg = Constant.statusSuccess;
            string dimensionId = string.Empty;
            try
            {
                using (Admin objadmin = new Admin())
                {
                    dimensionId = objadmin.GetDimensionId(dimensionName, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(dimensionId) || outMsg != Constant.statusSuccess)
                        return suppliedCode;
                    suppliedCode = objadmin.GetSuppliedCode(dimensionId, entityTypeId, out outMsg);
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
            return suppliedCode;
        }

        public List<RowLevelSecurityOperator> GetOperatorList(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<RowLevelSecurityOperator> listRowLevelSecurityOperator = new List<RowLevelSecurityOperator>();
            try
            {
                using (Admin objadmin = new Admin())
                {
                    listRowLevelSecurityOperator = objadmin.GetOperatorList(out outMsg);
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
            return listRowLevelSecurityOperator;
        }

        public List<RowLevelSecurityValues> PopulateValues(string dimensionName, int entityTypeId, string userId, string entityName, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string dimensionId = string.Empty;
            List<RowLevelSecurityValues> listCommonDataEntity = new List<RowLevelSecurityValues>();
            try
            {
                using (Admin objadmin = new Admin())
                {
                    dimensionId = objadmin.GetDimensionId(dimensionName, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(dimensionId) || outMsg != Constant.statusSuccess)
                        return listCommonDataEntity;
                    listCommonDataEntity = objadmin.PopulateValues(dimensionId, entityTypeId, userId, entityName.Trim(' '), out outMsg);
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
            return listCommonDataEntity;
        }
        public RowLevelSecurityDetail GetRowLevelSecurityData(string dimensionId, string entityName, string userID, out string outMsg)
        {
            int entityTypeId = 0;
            outMsg = Constant.statusSuccess;
            string suppliedCode = string.Empty;
            List<UserSecurityValuess> listUserSecurityValues = new List<UserSecurityValuess>();
            RowLevelSecurityDetail listRowLevelSecurityDetail = new RowLevelSecurityDetail();
            List<RowLevelSecurityValues> listRowLevelSecurityValues = new List<RowLevelSecurityValues>();
            List<RowLevelSecurityOperator> listRowLevelSecurityOperator = new List<RowLevelSecurityOperator>();
            try
            {
                using (Admin objadmin = new Admin())
                {

                    entityTypeId = objadmin.GetEntityTypeId(entityName.Trim(' '), out outMsg);
                    if (entityTypeId == 0 || outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    dimensionId = objadmin.GetDimensionId(dimensionId, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(dimensionId) || outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    suppliedCode = objadmin.GetSuppliedCode(dimensionId, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(suppliedCode) || outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    listRowLevelSecurityOperator = objadmin.GetOperatorList(out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    listRowLevelSecurityValues = objadmin.PopulateValues(dimensionId, entityTypeId, userID, entityName, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    listUserSecurityValues = objadmin.GetRowLevelSecurityData(dimensionId, entityName.Trim(' '), entityTypeId, userID, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return listRowLevelSecurityDetail;

                    listRowLevelSecurityDetail.SuppliedCode = suppliedCode;
                    listRowLevelSecurityDetail.UserID = userID;
                    listRowLevelSecurityDetail.RowLevelSecurityOperator = listRowLevelSecurityOperator;
                    listRowLevelSecurityDetail.RowLevelSecurityValues = listRowLevelSecurityValues;
                    listRowLevelSecurityDetail.UserSecurityValues = listUserSecurityValues;
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
            return listRowLevelSecurityDetail;

        }
        public string SaveRowLevelSecurityDetails(string DimensionId, string EntityName, string OperatorValue, string UserId, string SupplyCode, string[] SelectedData, string[] SelectedValues)
        {
            int entityTypeId = 0;
            string outMsg = Constant.statusSuccess;
            try
            {
                using (Admin objadmin = new Admin())
                {
                    entityTypeId = objadmin.GetEntityTypeId(EntityName.Trim(' '), out outMsg);
                    if (entityTypeId == 0 || outMsg != Constant.statusSuccess)
                        return outMsg;

                    DimensionId = objadmin.GetDimensionId(DimensionId, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(DimensionId) || outMsg != Constant.statusSuccess)
                        return outMsg;
                }


                List<UserSecurityValues> cslst = new List<UserSecurityValues>();
                for (int i = 0; i < SelectedData.Length; i++)
                {
                    UserSecurityValues USV = new UserSecurityValues();
                    USV._DIMENSION = DimensionId;
                    USV._ENTITY_NAME = EntityName;
                    USV.v_OPERATOR = OperatorValue;
                    USV._USER_ID = UserId;
                    USV._ENTITY_TYPE_ID = Convert.ToString(entityTypeId);
                    USV.v_SUPPLIED_CODE = SupplyCode;
                    USV.v_VALUES = SelectedData[i];
                    USV._EOID = SelectedValues[i];
                    cslst.Add(USV);
                }
                using (Admin objadmin = new Admin())
                {
                    outMsg = objadmin.SaveRowLevelSecurityDetails(cslst);
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
            return outMsg;

        }
        public string DeleteRowLevelSecurity(string dimensionId, string entityName, string userId, string supplyCode)
        {
            int entityTypeId = 0;
            string outMsg = Constant.statusSuccess;
            try
            {
                using (Admin objadmin = new Admin())
                {
                    entityTypeId = objadmin.GetEntityTypeId(entityName.Trim(' '), out outMsg);
                    if (entityTypeId == 0 || outMsg != Constant.statusSuccess)
                        return outMsg;
                    dimensionId = objadmin.GetDimensionId(dimensionId, entityTypeId, out outMsg);
                    if (string.IsNullOrEmpty(dimensionId) || outMsg != Constant.statusSuccess)
                        return outMsg;
                }

                UserSecurityValues USV = new UserSecurityValues();
                USV._DIMENSION = dimensionId;
                USV._ENTITY_NAME = entityName;
                USV._USER_ID = userId;
                USV._ENTITY_TYPE_ID = Convert.ToString(entityTypeId);
                USV.v_SUPPLIED_CODE = supplyCode;
                using (Admin objadmin = new Admin())
                {
                    outMsg = objadmin.DeleteRowLevelSecurity(USV);
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
            return outMsg;
        }

        public WorkFlowDetailForRowLevelSecurity GetWorkFlowDataForApplyingSecurity(string entityName, string UserId, string roleID, string roleName, out string outMsg)
        {
            int entityTypeId = 0;
            outMsg = Constant.statusSuccess;
            WorkFlowDetailForRowLevelSecurity objworkFlow = new WorkFlowDetailForRowLevelSecurity();
            List<ApproverDetails> selectedApproverList = new List<ApproverDetails>();
            List<ApproverDetails> approverList = new List<ApproverDetails>();

            try
            {
                using (Admin objadmin = new Admin())
                {
                    entityTypeId = objadmin.GetEntityTypeId(entityName.Trim(' '), out outMsg);
                    if (entityTypeId == 0 || outMsg != Constant.statusSuccess)
                        return objworkFlow;
                    selectedApproverList = objadmin.GetSelectedApproverList(UserId, entityTypeId, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return objworkFlow;
                    approverList = objadmin.GetApproverList(Convert.ToInt32(roleID), entityTypeId, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return objworkFlow;

                }
                StringBuilder selectedApproverId = new StringBuilder();
                foreach (var item in selectedApproverList)
                {
                    if (approverList.Exists(x => x.ApproverId == item.ApproverId))
                    {
                        approverList.RemoveAll(x => x.ApproverId == item.ApproverId);
                    }
                    selectedApproverId.Append(item.ApproverId + ",");
                }
                objworkFlow.EntityId = entityName;
                objworkFlow.UserID = UserId;
                objworkFlow.UserRole = roleName;
                objworkFlow.RoleID = roleID;
                objworkFlow.SelectedApproverId = selectedApproverId.ToString().Trim(',');
                objworkFlow.SelectedApproverDetail = selectedApproverList;
                objworkFlow.ApproverDetail = approverList;


            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return objworkFlow;

        }
        public string SaveSelectedApprover(string RoleId, string UserId, string EntityName, string selectedApproverId)
        {
            int entityTypeId = 0;
            string outMsg = Constant.statusSuccess;
            string[] approverId = null;
            List<ApproverDetail> listApproverDetail = new List<ApproverDetail>();
            try
            {
                if (!string.IsNullOrEmpty(selectedApproverId))
                {
                    approverId = selectedApproverId.Split(',');
                    foreach (var item in approverId)
                    {
                        ApproverDetail approverDetail = new ApproverDetail();
                        approverDetail.ApproverId = item.ToString();
                        listApproverDetail.Add(approverDetail);
                    }
                }
                using (Admin objAdmin = new Admin())
                {
                    entityTypeId = objAdmin.GetEntityTypeId(EntityName.Trim(' '), out outMsg);
                    if (entityTypeId == 0 || outMsg != Constant.statusSuccess)
                        return outMsg;
                    objAdmin.SaveSelectedApprover(RoleId, UserId, entityTypeId, listApproverDetail);
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
            return outMsg;

        }
    }
}