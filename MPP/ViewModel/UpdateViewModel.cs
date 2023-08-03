using DAL.Common;
using System.Data;
using System.Text;

namespace MPP.ViewModel
{
    public class UpdateViewModel : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        public string WriteDataSetToFlatFileBackup(List<Dictionary<string, string>> listattrValues, int sessionID, string FileName)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                DataSet dsResult = CreateDataSet(listattrValues, sessionID);
                StreamWriter swCSVWriter = null;
                string TextToBeWritten = "";
                StringBuilder TextData = new StringBuilder();
                TextToBeWritten = "";
                DataColumnCollection dataCols = dsResult.Tables[0].Columns;
                foreach (var list in listattrValues)
                {
                    foreach (var data in list)
                    {

                        string test = data.Key;
                        if ((test.IndexOf("OID") == -1) && (data.Key != "CURRENT_EDIT_LEVEL"))
                        {
                            TextToBeWritten += data.Key + ",";
                        }
                    }
                    break;
                }
                TextData.AppendLine(TextToBeWritten.Trim(','));
                TextToBeWritten = "";
                // To write data into the stream
                foreach (var list in listattrValues)
                {
                    TextToBeWritten = "";

                    foreach (var data in list)
                    {
                        string test = data.Key;
                        if ((test.IndexOf("OID") == -1) && (data.Key != "CURRENT_EDIT_LEVEL"))
                        {
                            TextToBeWritten += "\"" + data.Value.ToString().Replace("\"", "\"\"") + "\"" + ",";
                        }
                    }
                    TextData.AppendLine(TextToBeWritten.Trim(','));

                }

                if (FileName.Length > 0)
                {
                    swCSVWriter = File.CreateText(FileName);
                    swCSVWriter.Write(TextData);
                    swCSVWriter.Close();
                    swCSVWriter = null;
                    // File.AppendAllText(FileName, TextData.ToString());

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
        public string WriteDataSetToFlatFile(List<Dictionary<string, string>> listattrValues, int sessionID, string FileName)
        {
            string outMsg = Constant.statusSuccess;
            try
            {
                DataSet dsResult = CreateDataSet(listattrValues, sessionID);
                StreamWriter swCSVWriter = null;
                string TextToBeWritten = "";
                StringBuilder TextData = new StringBuilder();
                TextToBeWritten = "";
                DataColumnCollection dataCols = dsResult.Tables[0].Columns;
                foreach (var list in listattrValues)
                {
                    foreach (var data in list)
                    {
                        TextToBeWritten += data.Key + ",";
                    }
                    break;
                }
                TextData.AppendLine(TextToBeWritten.Trim(','));
                TextToBeWritten = "";
                // To write data into the stream
                foreach (var list in listattrValues)
                {
                    TextToBeWritten = "";

                    foreach (var data in list)
                    {
                        if (data.Key == "Effective Date")
                            TextToBeWritten += "\"" + Convert.ToString(string.Format("{0:MM/dd/yyyy}", data.Value)).Replace("\"", "\"\"") + "\"" + ",";


                        else
                            TextToBeWritten += "\"" + data.Value.ToString().Replace("\"", "\"\"") + "\"" + ",";

                    }
                    TextData.AppendLine(TextToBeWritten.Trim(','));

                }

                if (FileName.Length > 0)
                {
                    swCSVWriter = File.CreateText(FileName);
                    swCSVWriter.Write(TextData);
                    swCSVWriter.Close();
                    swCSVWriter = null;
                    // File.AppendAllText(FileName, TextData.ToString());

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
        private DataSet CreateDataSet(List<Dictionary<string, string>> listattrValues, int sessionID)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            foreach (var list in listattrValues)
            {
                foreach (var data in list)
                {
                    dt.Columns.Add(data.Key);
                }
                break;
            }
            foreach (var list in listattrValues)
            {
                DataRow row = dt.NewRow();

                foreach (var data in list)
                {
                    row[data.Key] = data.Value;
                }
                dt.Rows.Add(row);
            }

            ds.Tables.Add(dt);
            return ds;
        }

    }
}
