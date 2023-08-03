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
    public class GetSessionValue : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public int GetNextSessionValue(out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            int sessionID = 0;
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    using (var command = mPP_Context.Database.GetDbConnection().CreateCommand())
                    {
                        mPP_Context.Database.GetDbConnection().Open();
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GetNextSequenceValue";

                        var sequenceNameParam = new SqlParameter("@sequenceName", SqlDbType.NVarChar) { Value = "MPP_APP.SEQ_SESSION" };
                        command.Parameters.Add(sequenceNameParam);

                        var nextValueParam = new SqlParameter("@nextValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(nextValueParam);

                        command.ExecuteNonQuery();
                        sessionID = (int)nextValueParam.Value;
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
            return sessionID;
        }
    }
}
