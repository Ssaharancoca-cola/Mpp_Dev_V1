using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetSequenceValue : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        //To get sequence next value 

        public int GetNextSequanceValue(string sequencename, out string outMsg)
        {
            outMsg = Constant.statusSuccess; 
            int nextVal = 0;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    using (var command = mPP_Context.Database.GetDbConnection().CreateCommand())
                    {
                        mPP_Context.Database.GetDbConnection().Open();
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GetNextSequenceValue";

                        var sequenceNameParam = new SqlParameter("@sequenceName", SqlDbType.NVarChar) { Value = "MPP_CORE.SEQ_LD_OID" };
                        command.Parameters.Add(sequenceNameParam);

                        var nextValueParam = new SqlParameter("@nextValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(nextValueParam);

                        command.ExecuteNonQuery();

                        nextVal = (int)nextValueParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return nextVal;
        }
        //Get Max Id for Entity Type
        public int GetMaxIdForEntityType()
        {
            using (MPP_Context mPP_Context = new MPP_Context())
            {
                var nextVal = mPP_Context.Database.SqlQueryRaw<int>("select max(ID) from Mpp_Core.entity_type").FirstOrDefault();
                return nextVal;
            }
        }
    }
}
