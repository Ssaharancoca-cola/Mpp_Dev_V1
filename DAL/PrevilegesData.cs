using DAL.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Model;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PrevilegesData : IDisposable
    {
        
        ///<summary>
        /// This method will check user has access for entity or not
        /// </summary>
        /// <param name="userName"> User Name.</param>
        /// <param name="entityTypeID"> Entity Type Id.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>View</returns>

        public Previleges GetPrevileges(String userName, Int64 entityTypeID, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            Previleges objPrevileges = new Previleges();
            var sqlQuery = "SELECT READ_FLAG, CREATE_FLAG, UPDATE_FLAG, b.LANGUAGE_CODE AS LANGUAGE_CODE, IMPORT_FLAG FROM MPP_CORE.MPP_USER_PRIVILAGE A INNER JOIN MPP_CORE.MPP_USER B ON A.USER_ID = B.USER_ID AND A.ENTITY_TYPE_ID =" + Convert.ToInt64(entityTypeID)+ " AND UPPER(B.USER_ID) = UPPER('" + userName + "')";
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    // objPrevileges = mPP_Context.Set<Previleges>().FromSqlRaw(sqlQuery).FirstOrDefault();
                    long entityTypeIdValue = Convert.ToInt64(entityTypeID);
                    string userNameUpper = userName.ToUpper();

                    var result = (from a in mPP_Context.MppUserPrivilage
                                  join b in mPP_Context.MppUser on a.UserId equals b.UserId
                                  where a.EntityTypeId == entityTypeIdValue && b.UserId.ToUpper() == userNameUpper
                                  select new Previleges
                                  {
                                      READ_FLAG = a.ReadFlag ?? 0,
                                      CREATE_FLAG = a.CreateFlag ?? 0,
                                      UPDATE_FLAG = a.UpdateFlag ?? 0,
                                      LANGUAGE_CODE = b.LanguageCode,
                                      IMPORT_FLAG = a.ImportFlag ?? 0
                                  });

                    objPrevileges = result.FirstOrDefault();
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
            return objPrevileges;
        }

        void IDisposable.Dispose() { }
    }
}
