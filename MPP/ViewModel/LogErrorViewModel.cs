using DAL.Common;

namespace MPP.ViewModel
{
    public class LogErrorViewModel : IDisposable
    {
        void IDisposable.Dispose() { }
        public void LogErrorInTextFile(Exception ex) 
        {
            using (LogError objLogError = new LogError())
            {
                objLogError.LogErrorInTextFile(ex);
            }
        }

    }
}
