using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Common;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using Microsoft.EntityFrameworkCore;
using Model;

namespace DAL
{
    public class Admin : IDisposable
    {
        void IDisposable.Dispose() { }
        public string GetDimensionId(string dimensionName, int entityTypeId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string dimensionId = string.Empty;
            string selectQuery = @"select distinct dimension_name from entity_type where upper(dimension_display_name)=
                                      upper('" + dimensionName + "') and id = '" + entityTypeId + "'";

            try
            {
                using (MPP_Context objMppContext = new MPP_Context())
                {
                    dimensionId = objMppContext.Set<string>().FromSqlRaw(selectQuery).FirstOrDefault();
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
                addnewUser.UserId = "0";
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
        public List<ROLE> GetRoleDetails(string selectQuery, out string outMsg)
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
    }
}
