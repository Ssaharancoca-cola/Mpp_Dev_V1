using DAL;
using DAL.Common;
using Model;

namespace MPP.ViewModel
{
    public class PrevilegesDataViewModel : IDisposable
    {
        ///<summary>
        /// This method will check user has access for entity or not
        /// </summary>
        /// <param name="userName"> User Name.</param>
        /// <param name="entityTypeID"> Entity Type Id.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of entity type Attribute</returns>

        public Previleges GetPrevileges(System.String userName, System.Int64 entityTypeID, out string outMsg)
        {
            Previleges previlegesData = new Previleges();
            try
            {
                using(PrevilegesData objPrevilegesData = new PrevilegesData())
                {
                    previlegesData = objPrevilegesData.GetPrevileges(userName, entityTypeID, out outMsg);
                }
            }
            catch(Exception ex) 
            {
                using(LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return previlegesData;
        }
        void IDisposable.Dispose() { }
    }
}
