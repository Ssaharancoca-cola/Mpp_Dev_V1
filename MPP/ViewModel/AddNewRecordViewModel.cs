using DAL.Common;
using DAL;
using Model;

namespace MPP.ViewModel
{
    public class AddNewRecordViewModel : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        
        public string SaveRecord(List<Entity_Type_Attr_Detail> attrList, Dictionary<string, string> attrValues, int entityTypeid, string userName, int bSupressWarning, string sourceSystemName, string languageCode)
        {
            string outMsg = Constant.statusSuccess;
            using (AddNewRecord objnewrecord = new AddNewRecord())
            {
                try
                {
                    outMsg = objnewrecord.SaveRecord(attrList, attrValues, entityTypeid, userName, bSupressWarning, sourceSystemName, languageCode);
                }
                catch (Exception ex)
                {
                    using (LogError objLogError = new LogError())
                    {
                        objLogError.LogErrorInTextFile(ex);
                    }
                    outMsg = ex.Message;
                }
            }
            return outMsg;

        }
        public async Task<List<Entity_Type_Attr_Detail>> GetAddNewField(int entityTypeId)
        {
            List<Entity_Type_Attr_Detail> entityAttrList = new List<Entity_Type_Attr_Detail>();
            string outMsg = Constant.statusSuccess;
            try
            {
                string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
                using (PrevilegesDataViewModel objPrevilegesDataViewModel = new PrevilegesDataViewModel())
                {
                    Previleges previlegesData = objPrevilegesDataViewModel.GetPrevileges(userName[1], entityTypeId, out outMsg);
                    if (previlegesData == null || previlegesData.READ_FLAG != 1)
                    {
                        outMsg = Constant.accessDenied;
                    }
                }
                using (MenuViewModel objMenuViewModel = new MenuViewModel())
                {
                    (entityAttrList, outMsg) = await objMenuViewModel.ShowAttributeDataAsync(entityTypeId, "", userName[1].ToUpper());
                }
                entityAttrList = entityAttrList.OrderBy(x => x.AttrDisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
            }
            return entityAttrList;
        }

    }
}
