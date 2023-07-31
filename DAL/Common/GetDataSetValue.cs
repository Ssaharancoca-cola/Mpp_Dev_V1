using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetDataSetValue : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public async Task<DataSet> GetDataSetWithParameterAsync(string selectCommand, int entityTypeId, string sessionId)
        {
            var result = new DataSet();

            using (var context = new MPP_Context())
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = selectCommand;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = new SqlParameter("@i_Entity_type_ID", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = entityTypeId;
                    cmd.Parameters.Add(param);

                    var param3 = new SqlParameter("@i_Session_id", SqlDbType.NVarChar);
                    param3.Direction = ParameterDirection.Input;
                    param3.Value = sessionId;
                    cmd.Parameters.Add(param3);

                    var parameter2 = new SqlParameter("@MAXCUR", SqlDbType.Structured);
                    parameter2.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(parameter2);

                    try
                    {
                        await context.Database.OpenConnectionAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            do
                            {
                                var dtDataTable = new DataTable();
                                dtDataTable.Load(reader);
                                result.Tables.Add(dtDataTable);

                            } while (!reader.IsClosed);
                        }
                    }
                    catch (Exception ex)
                    {
                        using (LogError objLogError = new LogError())
                        {
                            objLogError.LogErrorInTextFile(ex);
                        }
                    }
                    finally
                    {
                        await context.Database.CloseConnectionAsync();
                    }
                }
            }

            return result;
        }
        public DataSet GetDataSet(string selectCommand, out string outMsg)
        {
            var result = new DataSet();
            outMsg = Constant.statusSuccess;

            using (var context = new MPP_Context())
            {
                try
                {
                    var dtDataTable = new DataTable();
                    var command = context.Database.GetDbConnection().CreateCommand();
                    command.CommandText = selectCommand;

                    context.Database.OpenConnection();

                    using (var reader = command.ExecuteReader())
                    {
                        dtDataTable.Load(reader);
                        result.Tables.Add(dtDataTable);
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
                finally
                {
                    context.Database.CloseConnection();
                }
            }

            return result;
        }

    }

}
