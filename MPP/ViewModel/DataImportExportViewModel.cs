using DAL.Common;

namespace MPP.ViewModel
{
    public class DataImportExportViewModel : IDisposable
    {
        void IDisposable.Dispose()
        {
           
        }
        string g_UserID = string.Empty;
        short g_LoadID;
        public string GetViewName(int entityTypeId, string userName, out string viewName)
        {
            string outMsg = Constant.statusSuccess;
            viewName = string.Empty;
            try
            {
                using (GetViewDetail getViewDetail = new GetViewDetail())
                {
                    viewName = string.Empty;
                    outMsg = getViewDetail.GetViewName(entityTypeId, userName.ToUpper(), out viewName);
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
            }
            return outMsg;
        }

    }
}
