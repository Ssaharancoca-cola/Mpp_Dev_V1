using Model;
using DAL;
using Model.Models;
using DAL.Common;

namespace MPP.ViewModel
{
    public class MenuViewModel : IDisposable
    {
        void IDisposable.Dispose() { }
        ///<summary>
        /// This method will return list of Dimension 
        /// </summary>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of Dimension </returns>
        public List<DimensionName> ShowMenuData(out string outMsg)
        {
            List<DimensionName> dimensionList = new List<DimensionName>();
            using (MasterData objMasteData = new MasterData()) 
            {
                dimensionList = objMasteData.ShowMenuData(out outMsg);
            }
            return dimensionList;
        }

        ///<summary>
        /// This method will return list of entity type 
        /// </summary>
        /// /// <param name="DimensionName"> DimensionName.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of Entity Type </returns>

        public List<EntityType> ShowSubMenuData(string DimensionName,out string outMsg)
        {
            List<EntityType> entityTypeList = new List<EntityType>();
            try
            {
                using(MasterData master = new MasterData())
                {
                    entityTypeList = master.ShowSubMenuData(DimensionName, out outMsg);
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
            return entityTypeList;
        }

        ///<summary>
        /// This method will return list of entity type attribute
        /// </summary>
        /// <param name="entityTypeId"> Entity Type Id.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of entity type Attribute</returns>

        public async Task<(List<Entity_Type_Attr_Detail>, string)> ShowAttributeDataAsync(int entityTypeId, string viewType, string userName)
        {
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            string outMsg;

            try
            {
                using (MasterData masterData = new MasterData())
                {
                    (attributeList, outMsg) = await masterData.ShowAttributeData(entityTypeId, viewType, userName);
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

            return (attributeList, outMsg);
        }

    }
}
