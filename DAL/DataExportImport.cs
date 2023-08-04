using DAL.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DataExportImport : IDisposable
    {
        void IDisposable.Dispose()
        {

        }       
        public async Task<DataSet> GetData(string query)
        {
            var result = new DataSet();
            using var context = new MPP_Context();
            await context.Database.OpenConnectionAsync();

            using var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = query;
            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                do
                {
                    var dtDataTable = new DataTable();
                    dtDataTable.Load(reader);
                    result.Tables.Add(dtDataTable);

                } while (!reader.IsClosed);
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
            return result;
        }
        public string InsertInLandingTable(string insertQuery)
        {
            int noOfRowInserted = 0;
            string outMsg = Constant.statusSuccess;
            try
            {
                using (MPP_Context mppContext = new MPP_Context())
                {
                    noOfRowInserted = mppContext.Database.ExecuteSqlRaw(insertQuery);
                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                if (ex.Message.ToLower().Contains("column not allowed here"))
                {
                    outMsg = "Data Type mismatch";
                }
                else if (ex.Message.ToLower().Contains("cannot insert null into"))
                {
                    outMsg = "Mandatory field is missing";
                }
                else if (ex.Message.ToLower().Contains("value too large for column"))
                {
                    outMsg = "Length too large for column" + ex.Message.Substring(ex.Message.LastIndexOf("."));
                }
                else
                {
                    //dr[colErrorMessage] = ex.Message;
                    string message = ex.Message;
                    message = message.Substring(message.IndexOf(":", 0) + 2, message.IndexOf("\n") - 11);
                    outMsg = message;
                }
            }
            return outMsg;
        }
    }

}

