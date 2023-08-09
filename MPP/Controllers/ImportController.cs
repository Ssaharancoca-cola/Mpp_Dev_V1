using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Filter;
using MPP.ViewModel;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    [SessionTimeoutEntity]
    public class ImportController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        public ImportController(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }
        public ActionResult Index() { return View(); }
        [HttpPost]
        /// <summary>
        /// This method is to import data
        /// </summary>    
        /// <param name="form">form values .</param>
        /// <param name="Command">command name for checking button type .</param>
        /// <returns> View</returns>
        public IActionResult ImportData(IFormCollection form, string Command, IFormFile file)
        {
            if (Command == "Import")
            {
                #region varaiable declaration
                bool download = true;
                int totalRowcount = 0;
                int errorRowcount = 0;
                int loadErrorCount = 0;
                bool hasLoadErrors = false;
                string outMsg = Constant.statusSuccess;
                StringBuilder strExport = new StringBuilder();
                StringBuilder strDataType = new StringBuilder();
                #endregion varaiable declaration
                try
                {
                    int entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID"));
                    List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
                    string serializedAttributeList = TempData["attributeList"] as string;

                    if (serializedAttributeList != null)
                    {
                        attributeList = JsonConvert.DeserializeObject<List<Entity_Type_Attr_Detail>>(serializedAttributeList);
                    }
                    TempData.Keep();
                    string formatedDate = DateTime.Now.ToString("dd-MM-yyyy-hh-mm");

                    string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                    string FileName = "I" + userName[1] + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + formatedDate + ".csv";
                    string FilePath = Path.Combine(_environment.ContentRootPath, "App_Data\\");

                    string strRejectFileName = "RejI" + userName[1] + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + ".csv";
                    string strRejectFilePath = Path.Combine(_environment.ContentRootPath, "App_Data\\");

                    if (!Directory.Exists(FilePath))
                        Directory.CreateDirectory(FilePath);
                    FilePath = FilePath + FileName;
                    if (!Directory.Exists(strRejectFilePath))
                        Directory.CreateDirectory(strRejectFilePath);
                    strRejectFilePath = strRejectFilePath + strRejectFileName;                    
                    ValidateUploadedFile(file, form, out outMsg);
                    if (outMsg != Constant.statusSuccess)
                        return Content("error" + outMsg);
                    // Create a FileStream and save the file
                    using (var fileStream = new FileStream(FilePath, FileMode.Create))
                    {
                         file.CopyToAsync(fileStream);
                    }
                    //file.s(FilePath);

                    foreach (var data in attributeList)
                    {

                        if (data.Isvisible != "N")
                        {
                            string attrName = attributeList.Where(x => x.AttrDataType == "SUPPLIED_CODE").Where(y => y.AttrName == data.AttrName).Select(x => x.AttrName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(attrName))
                            {
                                strExport.Append(data.AttrName + ",");
                                strDataType.Append("VARCHAR" + ",");
                            }
                            if (!(form[data.AttrName] == "false"))
                            {
                                strExport.Append(data.AttrName + ",");
                                strDataType.Append(FindDataType(attributeList, data.AttrName) + ",");
                            }
                        }
                    }
                    if (!(form[Constant.dateFromColumnName] == "false"))
                    {
                        strExport.Append(Constant.dateFromColumnName + ",");
                        strDataType.Append("DATE" + ",");
                    }

                    int[] ArrayRowsCount = new int[] { 0, 0 };
                    using (DataImportExportViewModel objDataExportViewModel = new DataImportExportViewModel())
                    {
                        if (form["ddlFileFormat"] == "Excel")
                        {
                            outMsg = objDataExportViewModel.LoadExcelToTable2(attributeList, entityTypeId, FilePath, "", strExport.ToString().Trim(','), true, "", "", true,
                        userName[1].ToString(), 1, strRejectFilePath, strDataType.ToString().Trim(','), out ArrayRowsCount, out loadErrorCount,
                        out hasLoadErrors, out download);
                        }
                        else if (form["ddlFileFormat"] == "Csv")
                        {
                            outMsg = objDataExportViewModel.LoadFlatFileToTable(attributeList, entityTypeId, FilePath, strExport.ToString().Trim(','), true, ", ",
                        userName[1].ToString(), 1, strRejectFilePath, strDataType.ToString().Trim(','), out ArrayRowsCount, out loadErrorCount,
                        out hasLoadErrors, out download);
                        }
                        if (outMsg != Constant.statusSuccess && hasLoadErrors == false)
                            return Content("error" + outMsg);
                        totalRowcount = ArrayRowsCount[0];
                        errorRowcount = ArrayRowsCount[1];

                    }

                    errorRowcount = errorRowcount + loadErrorCount;
                    int successRowCount = totalRowcount - errorRowcount;
                    //if (successRowCount > 0)
                    //{
                    //    SendMail(successRowCount.ToString(), out outMsg);
                    //}
                    if ((errorRowcount != 0 || hasLoadErrors == true) && download == true)
                    {
                        TempData["filepath"] = strRejectFilePath;
                        string path = System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"];

                        return Content("import," + Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")) + "," + strRejectFilePath);
                    }

                    else if (download == true)
                    {
                        return Content("success" + Constant.importSuccessFullyMessage);
                    }

                }
                catch (Exception ex)
                {
                    using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                    {
                        objLogErrorViewModel.LogErrorInTextFile(ex);
                    }
                    return Content("error" + Constant.commonErrorMsg);

                }

            }
            #region cancelUpdateCommand
            else if (Command == "Cancel")
            {
                return View(new { controller = "Menu", action = "ShowAttribute", entityTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("EntityTypeID")), entityName = Convert.ToString(_httpContextAccessor.HttpContext.Session.GetString("EntityName")), viewType = "search" });
            }
            #endregion
            return Content("");
        }
        private string FindDataType(List<Entity_Type_Attr_Detail> attributeList, string attrName)
        {
            string dataType = attributeList.Where(y => y.AttrName == attrName).Select(x => x.AttrDataType).FirstOrDefault();
            string rDataType = string.Empty; ;
            switch (dataType)
            {
                case "VARCHAR":
                case "VC":
                case "PARENT_CODE":
                case "SUPPLIED_CODE":
                    rDataType = "VARCHAR";
                    break;
                case "NUMERIC":
                case "INTEGER":
                case "DECIMAL":
                case "N":
                    rDataType = "NUMERIC";
                    break;
                case "DATE":
                case "DATETIME":
                case "DT":
                    rDataType = "DATE";
                    break;
            }
            return rDataType;
        }

        [HttpGet]
        public virtual IActionResult Download(string path)
        {
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                string fileName = "RejI" + userName[1] + path.Split(',')[1] + ".csv";
                string newPath = path.Split(",")[2];
                var fileStream = System.IO.File.OpenRead(newPath);

                return File(fileStream,
                            "application/octet-stream",
                            fileName,
                            enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
                {
                    objLogErrorViewModel.LogErrorInTextFile(ex);
                }
                return Content(ex.Message + ex.StackTrace);
            }

        }

        //private void SendMail(string successRowCount, out string outMsg)
        //{
        //    try
        //    {
        //        outMsg = Constant.statusSuccess;
        //        MailManagerViewModel objMAilManagerViewModel = new MailManagerViewModel();
        //        string url = Request.Url.GetLeftPart(UriPartial.Authority) + ConfigurationManager.AppSettings["Link"].ToString();
        //        objMAilManagerViewModel.Mail(successRowCount, "record", Convert.ToInt32(Session["EntityTypeID"]), Convert.ToString(Session["EntityName"]),
        //        Convert.ToString(Session["SelectedDimensionData"]), url, Constant.import, out outMsg);
        //    }

        //    catch (Exception ex)
        //    {
        //        using (LogErrorViewModel objLogErrorViewModel = new LogErrorViewModel())
        //        {
        //            objLogErrorViewModel.LogErrorInTextFile(ex);
        //        }
        //        outMsg = ex.Message;
        //    }
        //}

        private void ValidateUploadedFile(IFormFile file, IFormCollection form, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            if (file == null)
                outMsg = "Please Select the Source File.";
            else
            {
                switch (Path.GetExtension(Path.GetFileName(file.FileName)).ToUpper())
                {
                    case ".CSV":
                        if (form["ddlFileFormat"] != "Csv")
                            outMsg = "Selected File Type and file are mismatched.";
                        break;
                    case ".XLSX":
                        if (form["ddlFileFormat"] != "Excel")
                            outMsg = "Selected File Type and file are mismatched.";
                        break;
                    case ".XLS":
                        if (form["ddlFileFormat"] != "Excel")
                            outMsg = "Selected File Type and file are mismatched.";
                        break;
                    default:
                        outMsg = "Please choose appropriate file either excel or Csv.";
                        break;
                }

            }
        }

    }
}
