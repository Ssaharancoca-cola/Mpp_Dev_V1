using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Common;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using Microsoft.EntityFrameworkCore;
using Model;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using Microsoft.Identity.Client;

namespace DAL
{
    public class Admin : IDisposable
    {
        void IDisposable.Dispose() { }
        public string GetDimensionId(string dimensionName, int entityTypeId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string dimensionId = string.Empty;
            string selectQuery = @"select distinct dimension_name AS Dimension from MPP_CORE.entity_type where upper(dimension_display_name)=
                                      upper('" + dimensionName + "') and id = '" + entityTypeId + "'";
            DimensionName Dm = new DimensionName();

            try
            {
                using (MPP_Context objMppContext = new MPP_Context())
                {
                    Dm = objMppContext.Set<DimensionName>().FromSqlRaw(selectQuery).FirstOrDefault();
                    dimensionId = Dm.Dimension;
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
            return dimensionId;

        }
        public List<UserInfo> GetAllUsers(string selectQuery, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<UserInfo> allUserInfo = new List<UserInfo>();
            try
            {
                using (MPP_Context objMppContext = new MPP_Context())
                {
                    allUserInfo = objMppContext.Set<UserInfo>().FromSqlRaw(selectQuery).ToList();
                }
                UserInfo addnewUser = new UserInfo();
                addnewUser.UserName = "[Add new user]";
                addnewUser.UserID = "0";
                allUserInfo.Add(addnewUser);
                allUserInfo = allUserInfo.OrderBy(x => x.UserName).ToList();

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
        public UserInfo GetCurrentUserInfo(string selectQuery, out string outMsg)
        {
            outMsg= Constant.statusSuccess;
            UserInfo currentUserInfo = new UserInfo();
            try
            {
                using (MPP_Context objMppContext = new MPP_Context())
                {
                    currentUserInfo = objMppContext.Set<UserInfo>().FromSqlRaw(selectQuery).FirstOrDefault();
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
        public List<ROLE> GetRoleDetail(string selectQuery, out string outMsg)
        {
            outMsg= Constant.statusSuccess;
            List<ROLE> roleInfo = new List<ROLE>();

            try
            {
                using(MPP_Context objMppContext = new MPP_Context())
                {
                    roleInfo = objMppContext.Set<ROLE>().FromSqlRaw(selectQuery).ToList();
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
            return roleInfo;
        }
        public List<UserRowSecurity> GetSecurityDetail(string selectQuery, List<ROLE> lstROLE, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<UserRowSecurity> securityInfo = new List<UserRowSecurity>();
            try
            {                
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        var queryResult = objMPP_Context.Set<UserRowSecurity>()
                                   .FromSqlRaw(selectQuery)
                                   .ToList();
                        securityInfo = queryResult;
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
            if (securityInfo.Count > 0)
            {
                securityInfo.ToList().ForEach(c => c.ROLELIST = lstROLE);
            }
            return securityInfo;
        }

        public void UpdateUserDetails(UserInfo userInfo, out string outMsg)
        {
            int count = 0;
            string query = string.Empty;
            outMsg = Constant.statusSuccess;
            StringBuilder queryBuilder = null;
            try
            {
                queryBuilder = new StringBuilder();
                queryBuilder.Append("begin ");
                query = string.Format("update MPP_CORE.MPP_USER SET ADMIN_FLAG = {1}, ACTIVE = {2} where UPPER(USER_ID) = UPPER('{0}');",
                                         userInfo.UserID, Convert.ToInt32(userInfo.IsAdmin), Convert.ToInt32(userInfo.IsActive));
                queryBuilder.Append(query);
                if (userInfo.EntityPrivilegesList != null)
                {
                    foreach (EntityPrivileges entityPrivileges in userInfo.EntityPrivilegesList)
                    {
                        string selectQuery = "select count(*) from mpp_core.mpp_user_privilege  where  UPPER(USER_ID) = UPPER('" + userInfo.UserID + "') AND ENTITY_TYPE_ID = " + entityPrivileges.EntityDetails.EntityID + "";
                        using (MPP_Context objMPP_Context = new MPP_Context())
                        {
                            count = objMPP_Context.Database.ExecuteSqlRaw(selectQuery);
                            //count = objMPP_Context.Set<int>().FromSqlRaw(selectQuery).AsEnumerable().FirstOrDefault();
                        }
                        if (count == 0)
                        {
                            query = string.Format("insert into MPP_USER_PRIVILEGE(ENTITY_TYPE_ID, user_id, update_flag, create_flag, read_flag, import_flag, edit_level, role_id) values({0},UPPER('{1}') ,{2},{3},{4},{5},{6},{7});",

                                entityPrivileges.EntityDetails.EntityID, userInfo.UserID.ToUpper(), Convert.ToInt32(entityPrivileges.UpdateStatus),
                                Convert.ToInt32(entityPrivileges.CreateStatus), Convert.ToInt32(entityPrivileges.ReadStatus),
                                Convert.ToInt32(entityPrivileges.ImportStatus), "MPP_CORE.SEQ_EDIT_LEVEL.NEXTVAL", entityPrivileges.RoleId);
                        }
                        else
                        {
                            query = string.Format("update mpp_core.mpp_user_privilege set READ_FLAG = {0}, CREATE_FLAG = {1}, UPDATE_FLAG = {2}, IMPORT_FLAG = {3}, ROLE_ID = {4}, EDIT_LEVEL = {5} where  UPPER(USER_ID) = UPPER('{6}') AND ENTITY_TYPE_ID = {7};",
                    Convert.ToInt32(entityPrivileges.ReadStatus), Convert.ToInt32(entityPrivileges.CreateStatus), Convert.ToInt32(entityPrivileges.UpdateStatus), Convert.ToInt32(entityPrivileges.ImportStatus), entityPrivileges.RoleId, "mpp_core.seq_edit_level.nextval", userInfo.UserID, entityPrivileges.EntityDetails.EntityID);
                        }
                        if ((queryBuilder.Length + query.Length) < queryBuilder.MaxCapacity)
                        {
                            queryBuilder.Append(query);

                        }
                        else
                        {
                            using (MPP_Context objMPP_Context = new MPP_Context())
                            {
                                using (var dbContextTransaction = objMPP_Context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        queryBuilder.Append(" end;");
                                        int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(queryBuilder.ToString());
                                        dbContextTransaction.Commit();

                                        queryBuilder.Remove(0, queryBuilder.Capacity);

                                        queryBuilder.Append("begin ");
                                        queryBuilder.Append(query);
                                    }
                                    catch (Exception ex)
                                    {
                                        using (LogError objLogError = new LogError())
                                        {
                                            objLogError.LogErrorInTextFile(ex);
                                        }
                                        dbContextTransaction.Rollback();
                                        outMsg = ex.Message;

                                    }

                                }
                            }
                        }
                    }

                }
                queryBuilder.Append(" end;");
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    using (var dbContextTransaction = objMPP_Context.Database.BeginTransaction())
                    {
                        try
                        {
                            int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(queryBuilder.ToString());
                            dbContextTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            using (LogError objLogError = new LogError())
                            {
                                objLogError.LogErrorInTextFile(ex);
                            }
                            dbContextTransaction.Rollback();
                            outMsg = ex.Message;
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
        }
        public void CreateUser(UserInfo userInfo, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string query = string.Empty;
            StringBuilder queryBuilder = null;
            List<int> lstEntityTypeId = new List<int>();
            try
            {
                if (!string.IsNullOrEmpty(userInfo.UserID))
                {
                    string selectCommand = "Select ID From Mpp_Core.Entity_Type";
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        lstEntityTypeId = objMPP_Context.EntityType.FromSqlRaw(selectCommand).Select(o =>o.Id).ToList();
                    }
                }
                queryBuilder = new StringBuilder();
                queryBuilder.Append("begin ");
                query = string.Format("insert into MPP_CORE.MPP_USER (USER_NAME, USER_ID, EMAIL_ID, ROLE_NAME, LANGUAGE_CODE, ADMIN_FLAG, ACTIVE) values ('{0}', UPPER('{1}'), '{2}', '{3}', '{4}', {5},{6});",
                                            userInfo.UserName, userInfo.UserID, userInfo.UserEmail, "MPP", "en", Convert.ToInt32(userInfo.IsAdmin), Convert.ToInt32(userInfo.IsActive));
                queryBuilder.Append(query);

                if (lstEntityTypeId != null && lstEntityTypeId.Count > 0)
                {
                    foreach (var item in lstEntityTypeId)
                    {
                        query = string.Format("Insert into mdm_core.mdm_user_privilege(ENTITY_TYPE_ID, USER_ID, EDIT_LEVEL) values ({0}, UPPER('{1}'), {2});",
                                item, userInfo.UserID, "mdm_core.seq_edit_level.nextval");

                        if ((queryBuilder.Length + query.Length) < queryBuilder.MaxCapacity)
                        {
                            queryBuilder.Append(query);
                        }
                        else
                        {
                            queryBuilder.Append(" end;");
                            using (MPP_Context objMPP_Context = new MPP_Context())
                            {
                                using (var dbContextTransaction = objMPP_Context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(queryBuilder.ToString());
                                        dbContextTransaction.Commit();
                                        queryBuilder.Remove(0, queryBuilder.Capacity);
                                        queryBuilder.Append("begin ");
                                        queryBuilder.Append(query);
                                    }
                                    catch (Exception ex)
                                    {
                                        dbContextTransaction.Rollback();
                                        outMsg = ex.Message;
                                    }
                                }
                            }
                        }
                    }
                    queryBuilder.Append(" end;");

                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        using (var dbContextTransaction = objMPP_Context.Database.BeginTransaction())
                        {
                            try
                            {
                                int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(queryBuilder.ToString());
                                dbContextTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                using (LogError objLogError = new LogError())
                                {
                                    objLogError.LogErrorInTextFile(ex);
                                }
                                outMsg = ex.Message;
                                dbContextTransaction.Rollback();
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

        }

        public int GetEntityTypeId(string entityName, out string outMsg)
        {
            int entityTypeId = 0;
            outMsg = Constant.statusSuccess;
            string selectQuery = "select ID from MPP_CORE.entity_type where name ='" + entityName.Trim(' ') + "'";

            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    entityTypeId = objMPP_Context.EntityType.FromSqlRaw(selectQuery).Select(o => o.Id).FirstOrDefault();
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
            outMsg = Constant.statusSuccess;
            string suppliedCode = string.Empty;
            DimensionName DM =new  DimensionName();
            try
            {
                string selectQuery = @"SELECT 
                                    STUFF((SELECT ',' + attr_name 
                                FROM
                                    MPP_CORE.ENTITY_TYPE_ATTR A, MPP_CORE.ENTITY_TYPE E
                                WHERE
                                    A.ENTITY_TYPE_ID = E.ID
                                AND
                                    ENTITY_TYPE_ID = '" + entityTypeId + @"'
                                   AND DIMENSION_NAME = '" + dimensionName + @"' 
                                   AND IS_PART_OF_CODE = 1
                               FOR XML PATH('')), 1, 1, '') AS Dimension";


                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    DM = objMPP_Context.Set<DimensionName>().FromSqlRaw(selectQuery).FirstOrDefault();
                    suppliedCode = DM.Dimension;
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
                string selectQuery = "SELECT DISTINCT ATTR_VALUES as AttrValue, ATTR_NAME as AttrName FROM MPP_CORE.MPP_LOV WHERE ATTR = 'Operator' ";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    listRowLevelSecurityOperator = objMPP_Context.Set<RowLevelSecurityOperator>().FromSqlRaw(selectQuery).ToList();
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
            string suppliedCode = string.Empty;
            string result = string.Empty;
            String PKeys = String.Empty;
            List<RowLevelSecurityValues> listRowLevelSecurityValues = new List<RowLevelSecurityValues>();
            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    OutputParameter<string> i_result = new OutputParameter<string>();
                    OutputParameter<int> returnValue = new OutputParameter<int>();
                    objMPP_Context.Procedures.MPP_ENTITY_USER_SEC_VIEWS_LOVAsync(entityTypeId, userId.ToUpper(), i_result, returnValue).GetAwaiter().GetResult();
                    result = i_result.Value.ToString();

                }
                string selectQuery = @"SELECT 
                                    STUFF((SELECT ',' + attr_name 
                                FROM
                                    MPP_CORE.ENTITY_TYPE_ATTR A, MPP_CORE.ENTITY_TYPE E
                                WHERE
                                    A.ENTITY_TYPE_ID = E.ID
                                AND
                                    ENTITY_TYPE_ID = '" + entityTypeId + @"'
                                   AND DIMENSION_NAME = '" + dimensionName + @"' 
                                   AND IS_PART_OF_CODE = 1
                               FOR XML PATH('')), 1, 1, '') AS Dimension";

                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    DimensionName DM = new DimensionName();

                    DM = objMPP_Context.Set<DimensionName>().FromSqlRaw(selectQuery).FirstOrDefault();
                    suppliedCode = DM.Dimension;
                }
                string selectCommand = @"SELECT " + suppliedCode + " AS DisplayMember," + entityName + "_OID AS ValueMember FROM " + result + "";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    listRowLevelSecurityValues = objMPP_Context.Set<RowLevelSecurityValues>().FromSqlRaw(selectCommand).ToList();
                }
                if (listRowLevelSecurityValues.Count > 0)
                {
                    RowLevelSecurityValues objRowLevelSecurityDetail = new RowLevelSecurityValues();
                    objRowLevelSecurityDetail.DisplayMember = "--Select--";
                    objRowLevelSecurityDetail.ValueMember = -1;

                    listRowLevelSecurityValues.Insert(0, objRowLevelSecurityDetail);
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
            return listRowLevelSecurityValues;
        }

        public List<UserSecurityValuess> GetRowLevelSecurityData(string dimensionName, string entityName, int entityTyeId, string userID, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string suppliedCode = string.Empty;
            List<UserSecurityValuess> lstUserRowSecurity = new List<UserSecurityValuess>();
            try
            {
                string selectQuery = @"SELECT 
                                    STUFF((SELECT ',' + attr_name 
                                FROM
                                    MPP_CORE.ENTITY_TYPE_ATTR A, MPP_CORE.ENTITY_TYPE E
                                WHERE
                                    A.ENTITY_TYPE_ID = E.ID
                                AND
                                    ENTITY_TYPE_ID = '" + entityTyeId + @"'
                                   AND DIMENSION_NAME = '" + dimensionName + @"' 
                                   AND IS_PART_OF_CODE = 1
                               FOR XML PATH('')), 1, 1, '') AS Dimension";

                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    DimensionName DM = new DimensionName();
                    DM = objMPP_Context.Set<DimensionName>().FromSqlRaw(selectQuery).FirstOrDefault();
                    suppliedCode = DM.Dimension;
                }
                String Query = @"SELECT 
                                    '" + suppliedCode + @"' AS v_SUPPLIED_CODE,
                                    S.OPR AS v_OPERATOR,
                                    F." + suppliedCode + @" AS v_VALUES 
                                FROM 

                                    MPP_CORE.USER_SEC_VAL V
                                    INNER JOIN
                                    MPP_CORE.USER_ROW_SECURITY S
                                    ON V.ROW_SEC_ID = S.ROW_SEC_ID 
                                    INNER JOIN
                                    MPP_APP.FL_" + entityName + @" F 
                                    ON V.EOID = F." + entityName + @"_OID 
                                WHERE 
                                    S.USER_ID = '" + userID + @"' 
                                    AND S.ENTITY_TYPE_ID = '" + entityTyeId + @"' 
                                    AND S.DIMENSION_NAME = '" + dimensionName + @"'";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    lstUserRowSecurity = objMPP_Context.Set<UserSecurityValuess>().FromSqlRaw(Query).ToList();
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
            return lstUserRowSecurity;
        }
        
        public string SaveRowLevelSecurityDetails(List<UserSecurityValues> USV)
        {
            Int64 Row_Sec_ID;
            string Row_Sec_IDs;
            string outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    objMPP_Context.Procedures.MPP_REFRESH_ROW_SECURITYAsync(USV[0]._USER_ID, USV[0]._ENTITY_TYPE_ID, USV[0]._DIMENSION);
                }

                #region Insert New RowLevel Security Values
                string query = "SELECT MPP_CORE.SEQ_ROW_SEC_ID.NEXTVAL FROM DUAL";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    Row_Sec_IDs = objMPP_Context.Set<string>().FromSqlRaw(query).FirstOrDefault();
                    Row_Sec_ID = Int64.Parse(Row_Sec_IDs);
                }

                string InsertQuery = @"INSERT INTO MPP_CORE.USER_ROW_SECURITY(ROW_SEC_ID, DIMENSION_NAME, USER_ID, ENTITY_NAME, ENTITY_TYPE_ID, OPR)
                                  VALUES ('" + (Row_Sec_ID) + "' , " + (USV[0]._DIMENSION == null ? "NULL" : "'" + USV[0]._DIMENSION + "'") + "," +
                                  (USV[0]._USER_ID == null ? "NULL" : "'" + USV[0]._USER_ID + "'") + "," +
                                  (USV[0]._ENTITY_NAME == null ? "NULL" : "'" + USV[0]._ENTITY_NAME + "'") + " , " +
                                  (USV[0]._ENTITY_TYPE_ID == null ? "NULL" : "'" + USV[0]._ENTITY_TYPE_ID + "'") + "," +
                                  (USV[0].v_OPERATOR == null ? "NULL" : "'" + USV[0].v_OPERATOR + "'") + ")";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    int noOfRowInserted = objMPP_Context.Database.ExecuteSqlRaw(InsertQuery.ToString());
                }
                foreach (var item in USV)
                {
                    string insertquery = @"INSERT INTO MPP_CORE.USER_SEC_VAL(ROW_SEC_ID, EOID) VALUES ('" + (Row_Sec_ID) + "','" + (item._EOID) + "')";
                    using (MPP_Context objMPP_Context = new MPP_Context())
                    {
                        int noOfRowInserted = objMPP_Context.Database.ExecuteSqlRaw(insertquery.ToString());
                    }
                }
                #endregion

                #region Update user
                string updateQuery = @"update mdm_core.mdm_user_privilege set CREATE_FLAG = 0 where user_id = '" + USV[0]._USER_ID + "' AND ENTITY_TYPE_ID = " + USV[0]._ENTITY_TYPE_ID + "";
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(updateQuery.ToString());
                }
                #endregion
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

        public string DeleteRowLevelSecurity(UserSecurityValues USV)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    objMPP_Context.Procedures.MPP_REFRESH_ROW_SECURITYAsync(USV._USER_ID, USV._ENTITY_TYPE_ID, USV._DIMENSION);
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
        public List<ApproverDetails> GetSelectedApproverList(string userId, int entityTypeId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<ApproverDetails> selectedApproverList = new List<ApproverDetails>();
            try
            {
                string selecyQuery = @"
                    SELECT 
                    A.value AS ApproverId,
                    U.user_name AS ApproverName
                    FROM 
                    MPP_CORE.MPP_USER_PRIVILAGE AS P 
                    CROSS APPLY STRING_SPLIT(P.APPROVER, ',') AS A
                    INNER JOIN MPP_CORE.MPP_USER AS U ON U.user_id = A.value
                    WHERE 
                    P.USER_ID = '" + userId + @"' 
                    AND ENTITY_TYPE_ID = '" + entityTypeId + @"'";


                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    selectedApproverList = objMPP_Context.Set<ApproverDetails>().FromSqlRaw(selecyQuery).ToList();
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
            return selectedApproverList;
        }
        public List<ApproverDetails> GetApproverList(int roleId, int entityTypeId, out string outMsg)
        {
            // To get next level of approvers for that user
            roleId = roleId + 1;
            outMsg = Constant.statusSuccess;
            List<ApproverDetails> approverList = new List<ApproverDetails>();
            try
            {
                string selectQuery = @"Select U.User_Id As ApproverId, U.User_Name As ApproverName From Mpp_Core.Mpp_User_Privilage P,
                    Mpp_Core.Mpp_User U Where P.User_Id = U.User_Id And P.Entity_Type_Id = '"
                + entityTypeId + "' And P.Role_Id = '" + roleId + "'";

                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    approverList = objMPP_Context.Set<ApproverDetails>().FromSqlRaw(selectQuery).ToList();
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
            return approverList;
        }

        public string SaveSelectedApprover(string RoleId, string UserId, int EntityTypeId, List<ApproverDetail> selectedApproverId)
        {
            string query = string.Empty;
            StringBuilder queryBuilder = null;
            StringBuilder subQueryBuilder = null;
            string outMsg = Constant.statusSuccess;
            try
            {
                queryBuilder = new StringBuilder();
                queryBuilder.Append("begin ");

                if (selectedApproverId != null && selectedApproverId.Count > 0)
                {
                    subQueryBuilder = new StringBuilder();
                    subQueryBuilder.Append("MPP_CORE.APPR_TAB(");

                    foreach (var item in selectedApproverId)
                    {
                        subQueryBuilder.Append("MPP_CORE.APPR_ID('" + item.ApproverId + "'),");
                    }
                    subQueryBuilder.Replace(",", ")", subQueryBuilder.Length - 1, 1);
                    query = string.Format("UPDATE MPP_CORE.MPP_USER_PRIVILEGE SET ROLE_ID = '" + RoleId + "',APPROVER = " + subQueryBuilder + " WHERE USER_ID = '" + UserId + "' AND ENTITY_TYPE_ID = '" + EntityTypeId + "';");

                }
                else
                {
                    query = string.Format("UPDATE MPP_CORE.MPP_USER_PRIVILEGE SET ROLE_ID = '" + RoleId + "',APPROVER = NULL WHERE USER_ID = '" + UserId + "' AND ENTITY_TYPE_ID = '" + EntityTypeId + "';");
                }

                queryBuilder.Append(query);
                queryBuilder.Append(" end;");
                using (MPP_Context objMPP_Context = new MPP_Context())
                {
                    int noOfRowUpdated = objMPP_Context.Database.ExecuteSqlRaw(queryBuilder.ToString());
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
