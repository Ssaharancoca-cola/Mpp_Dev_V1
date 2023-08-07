using DAL.Common;
using ExcelDataReader;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace DAL
{
    public class ETLExcelReader : IDisposable
    {
        private string _strFilename;
        private bool _hasHeader = false;
        private string _strSheetName;
        private string _strSheetRange = "";

        private OleDbConnection _connExcel;
        private OleDbCommand _cmdSelectExcel;
        private OleDbDataAdapter _daExcel;

        #region Properties

        public string SheetRange
        {
            get { return _strSheetRange; }
            set { _strSheetRange = value; }
        }

        public string SheetName
        {
            get { return _strSheetName; }
            set { _strSheetName = value; }
        }

        public bool HasHeader
        {
            get { return _hasHeader; }
            set { _hasHeader = value; }
        }

        public string FileName
        {
            get { return _strFilename; }
            set { _strFilename = value; }
        }
        #endregion

        #region PropertyMethods
        public string ColName(int columnNumber)
        {
            string columnName = "";
            if (columnNumber < 26)
                columnName = Convert.ToString(Convert.ToChar((Convert.ToByte((char)'A') + columnNumber)));
            else
            {
                int intFirst = ((int)columnNumber / 26);
                int intSecond = ((int)columnNumber % 26);
                columnName = Convert.ToString(Convert.ToByte((char)'A') + intFirst);
                columnName += Convert.ToString(Convert.ToByte((char)'A') + intSecond);
            }
            return columnName;
        }

        public int ColNumber(string columnName)
        {
            columnName = columnName.ToUpper();
            int columnNumber = 0;
            if (columnName.Length > 1)
            {
                columnNumber = Convert.ToInt16(Convert.ToByte(columnName[1]) - 65);
                columnNumber += Convert.ToInt16(Convert.ToByte(columnName[1]) - 64) * 26;
            }
            else
                columnNumber = Convert.ToInt16(Convert.ToByte(columnName[0]) - 65);
            return columnNumber;
        }

        public String[] GetExcelSheetNames()
        {

            DataTable dt = null;
            String[] excelSheets = null;
            try
            {
                if (_connExcel == null) InitAndOpenFileConnection("");

                // Get the data table containing the schema
                dt = _connExcel.GetSchema("TABLES");

                if (dt == null)
                {
                    return null;
                }

                excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    string strSheetTableName = row["TABLE_NAME"].ToString();
                    excelSheets[i] = strSheetTableName.Substring(0, strSheetTableName.Length - 1);
                    i++;
                }

                return excelSheets;
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
                // Clean up.
                if (dt != null)
                {
                    dt.Dispose();
                    dt = null;
                }
            }
            return excelSheets;

        }
        #endregion

        private string ExcelConnection()
        {
            string _strConnOptions = "";
            if (this.HasHeader)
                _strConnOptions += "HDR=Yes;IMEX=1";
            else
                _strConnOptions += "HDR=No;IMEX=1";

            //Start - To read from .xlsx format.

            //return
            //    @"Provider=Microsoft.Jet.OLEDB.4.0;" +
            //    @"Data Source=" + _strFilename + ";" +
            //    @"Extended Properties=" + Convert.ToChar(34).ToString() +
            //    @"Excel 8.0;" + _strConnOptions + Convert.ToChar(34).ToString();

            return
               @"Provider=Microsoft.ACE.OLEDB.12.0;" +
               @"Data Source=" + _strFilename + ";" +
               @"Extended Properties=" + Convert.ToChar(34).ToString() +
               @"Excel 12.0;" + _strConnOptions + Convert.ToChar(34).ToString();

            //End - To read from .xlsx format.
        }

        public bool InitAndOpenFileConnection(string tableColumnNames)
        {
            try
            {
                if (_connExcel != null) //checking connection
                {
                    if (_connExcel.State == ConnectionState.Open)
                    {
                        _connExcel.Close();
                    }
                    _connExcel = null;
                }

                if (File.Exists(_strFilename) == false)    //check if the file exists
                {
                    throw new Exception("Excel file " + _strFilename + "could not be found.");
                }
                _connExcel = new OleDbConnection(ExcelConnection());
                _connExcel.Open();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                throw ex;
            }

            try
            {
                if (_strSheetName.Length == 0)
                {
                    if (this.GetExcelSheetNames()[0].Contains("_xlnm#_FilterDatabas") && this.GetExcelSheetNames()[1] != null)
                        _strSheetName = this.GetExcelSheetNames()[1];
                    else
                        _strSheetName = this.GetExcelSheetNames()[0];
                }
                //remove extra characters
                if (_strSheetName.Contains("'"))
                    _strSheetName = _strSheetName.Replace("'", "");
                if (_strSheetName.Contains("$"))
                    _strSheetName = _strSheetName.Replace("$", "");

                string strColString = "*";
                if (tableColumnNames.Length != 0)
                {
                    strColString = tableColumnNames;
                }
                _cmdSelectExcel = new OleDbCommand(
                    @"SELECT " + strColString + " FROM ["
                    + _strSheetName
                    + "$" + _strSheetRange
                    + "]", _connExcel);

                return true;
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                throw ex;
            }
        }

        public void CloseFileConnection()
        {
            if (_connExcel != null)
            {
                if (_connExcel.State != ConnectionState.Closed)
                    _connExcel.Close();
                _connExcel.Dispose();
                _connExcel = null;
            }
        }

        public ETLExcelReader()
        {

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connExcel != null)
            {
                _connExcel.Dispose();
                _connExcel = null;
            }
            if (_cmdSelectExcel != null)
            {
                _cmdSelectExcel.Dispose();
                _cmdSelectExcel = null;
            }
            // Dispose of remaining objects.
        }

        #endregion

        protected static void FillError(Object sender, FillErrorEventArgs args)
        {

            string str = args.Values.ToString();
            args.Continue = false;
        }

        public DataSet ReadDataSet(string tableName, string tableColumnNames)
        {
            try
            {
                DataSet ds = null;
                this.InitAndOpenFileConnection(tableColumnNames);
                _daExcel = new OleDbDataAdapter(_cmdSelectExcel);
                ds = new DataSet();
                _daExcel.Fill(ds, tableName);

                return ds;
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                Dispose();
                if (ex.Message == "No value given for one or more required parameters.")
                    throw new Exception("Column Names Mismatch.");
                else
                    throw ex;
            }
        }

        //public DataSet ReadDataSet(string filePath, string tableColumnNames)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();

        //        // Read the contents of the Excel file
        //        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        //        {
        //            using (var reader = ExcelReaderFactory.CreateReader(stream))
        //            {
        //                var conf = new ExcelDataSetConfiguration
        //                {
        //                    UseColumnDataType = true,
        //                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
        //                    {
        //                        UseHeaderRow = true
        //                    }
        //                };

        //                // Populate the DataSet with the contents of the Excel file
        //                ds = reader.AsDataSet(conf);
        //            }
        //        }

        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //        using (LogError objLogError = new LogError())
        //        {
        //            objLogError.LogErrorInTextFile(ex);
        //        }
        //        // Dispose(); // Remove this line if not required

        //        if (ex.Message == "No value given for one or more required parameters.")
        //            throw new Exception("Column Names Mismatch.");
        //        else
        //            throw ex;
        //    }
        //}

    }
}


