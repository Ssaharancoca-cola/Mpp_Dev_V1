using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public class GetViewDetail : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public string GetViewName(int entityTypeId, string userName, out string viewName)
        {
            string outMsg = Constant.statusSuccess;            
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {                    
                    var viewNameOutput = new OutputParameter<string>();
                    mPP_Context.Procedures.GET_VIEW_NAMEAsync(entityTypeId, userName.ToUpper(), viewNameOutput).GetAwaiter().GetResult();
                    viewName = viewNameOutput.Value;
                }
            }
            catch (Exception ex)
            {
                outMsg = ex.Message;
                using (LogError logError = new LogError())
                {
                    logError.LogErrorInTextFile(ex);
                }
                viewName = "";
                outMsg = ex.Message;
            }
            return outMsg;
        }
        public string GetOIDColumnName(string tablename, out string OIDColumnName)
        {
            List<string> columnname = new List<string>();
            string outMsg = Constant.statusSuccess;
            string[] splittableName = tablename.Split('_');
            try
            {
                using (MPP_Context objmdmContext = new MPP_Context())
                {
                    var query = "select column_name as columnname from MPP_APP.ALL_TAB_COLUMNS where TABLE_NAME   ='" + tablename + "'  order by column_name";
                    columnname = objmdmContext.Database.SqlQueryRaw<string>(query).ToList();
                    OIDColumnName = columnname.Find((c => (c.Contains(splittableName[1])) && (c.Contains("OID"))));

                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                OIDColumnName = "";
            }
            return outMsg;
        }

        public string GetOIDColumnNameByEntityId(int entityTypeId, out string OIDColumnName)
        {
            string outMsg = Constant.statusSuccess;
            var TableName = "";
            try
            {
                using (MPP_Context objContext = new MPP_Context())
                {
                    TableName = objContext.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.Name).FirstOrDefault();
                }
                OIDColumnName = TableName + "_OID";
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                OIDColumnName = "";
            }
            return outMsg;
        }
        public string GetFieldListVisible(int entittypeId, out string fieldList, out List<EntityTypeAttr> entityTypeAttrList)
        {
            string outMsg = Constant.statusSuccess;
            StringBuilder attrName = new StringBuilder();
            try
            {
                using (MPP_Context objContext = new MPP_Context())
                {
                    entityTypeAttrList = objContext.EntityTypeAttr.Where(x => x.EntityTypeId == entittypeId).OrderBy(x => x.AttrDisplayOrder).ToList();
                }
                foreach (var data in entityTypeAttrList)
                {
                    attrName.Append(data.AttrName + ",");
                }
                attrName = attrName.Length > 0 ? attrName.Remove(attrName.Length - 1, 1) : attrName;
                fieldList = attrName.ToString();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                fieldList = string.Empty;
                entityTypeAttrList = null;
                outMsg = ex.Message;
            }
            return outMsg;
        }

        public string GetFieldList(int entittypeId, out string fieldList, out List<EntityTypeAttr> entityTypeAttrList)
        {
            string outMsg = Constant.statusSuccess;
            StringBuilder attrName = new StringBuilder();
            try
            {
                using (MPP_Context objContext = new MPP_Context())
                {
                    entityTypeAttrList = objContext.EntityTypeAttr.Where(x => x.EntityTypeId == entittypeId).Where(x => x.Isvisible != "N").OrderBy(x => x.AttrDisplayOrder).ToList();
                }
                foreach (var data in entityTypeAttrList)
                {
                    attrName.Append(data.AttrName + ",");
                }
                attrName = attrName.Length > 0 ? attrName.Remove(attrName.Length - 1, 1) : attrName;
                fieldList = attrName.ToString();
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                fieldList = string.Empty;
                entityTypeAttrList = null;
                outMsg = ex.Message;
            }
            return outMsg;
        }
        public string GetStrAsSelectClause(string strSelectClause, List<EntityTypeAttr> attrNameList, Dictionary<string, string> PropNameType, string OIDColumnName, out Dictionary<string, string> mapClassAndDatabaseProp, out string outMsg)
        {
            StringBuilder strAsSelectClause = new StringBuilder();
            mapClassAndDatabaseProp = new Dictionary<string, string>();
            outMsg = Constant.statusSuccess;
            try
            {
                string[] splitFieldList = strSelectClause.Split(',');
                int i = 1;
                foreach (var field in splitFieldList)
                {
                    string attrDataType = attrNameList.Where(x => x.AttrName == field).Select(x => x.AttrDataType).FirstOrDefault();
                    string attrName = attrNameList.Where(x => x.AttrName == field).Select(x => x.AttrDisplayName).FirstOrDefault();

                    string datatype = string.Empty;
                    switch (attrDataType)
                    {
                        case "VC":
                        case "PARENT_CODE":
                        case "SUPPLIED_CODE":
                            datatype = "string";
                            break;
                        case "N":
                            datatype = "int";
                            break;
                        case "DT":
                            datatype = "datetime";
                            break;
                    }
                    if (field == OIDColumnName)
                    {
                        datatype = "int";
                        attrName = "OID";

                    }
                    if (field == "DATE_FROM")
                    {
                        datatype = "datetime";
                        attrName = "Effective Date";

                    }
                    if (field == "CURRENT_EDIT_LEVEL")
                    {
                        datatype = "int";
                        attrName = "Current Edit Level";

                    }
                    if (field == "ERROR_MESSAGE")
                    {
                        datatype = "string";
                        attrName = "Error Message";
                    }
                    for (int index = i; index < PropNameType.Count; index++)
                    {
                        if (datatype == PropNameType.Values.ElementAt(index))
                        {
                            if (!strAsSelectClause.ToString().Contains(PropNameType.Keys.ElementAt(index)))
                            {
                                strAsSelectClause.Append(field + " as " + PropNameType.Keys.ElementAt(index) + ",");
                                mapClassAndDatabaseProp.Add(PropNameType.Keys.ElementAt(index), attrName);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                outMsg = ex.Message;
                strAsSelectClause = null;

            }
            return strAsSelectClause.ToString();

        }
        public Dictionary<string, string> GetPropertyInfo()
        {
            Dictionary<string, string> PropNameType = new Dictionary<string, string>();
            EntityTypeData obj = new EntityTypeData();
            try
            {
                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        PropNameType.Add(propertyInfo.Name, "string");
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        PropNameType.Add(propertyInfo.Name, "int");
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        PropNameType.Add(propertyInfo.Name, "datetime");
                    }
                }

            }
            catch (Exception ex)
            {
                using (LogError objLogError = new LogError())
                {
                    objLogError.LogErrorInTextFile(ex);
                }
                PropNameType = null;
            }
            return PropNameType;
        }

        public string GetTableName(int entityTypeId, out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string tableName = string.Empty;
            try
            {
                using (MPP_Context objMdmContext = new MPP_Context())
                {
                    tableName = objMdmContext.EntityType.Where(x => x.Id == entityTypeId).Select(x => x.InputTableName).FirstOrDefault();
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
            return tableName;
        }

    }


    public class EntityTypeData
    {
        public string type1 { get ; set;}
        public string type2 { get; set;}    
        public string type3 { get; set;}            
        public string type4 { get; set;}
        public string type5 { get; set;}
        public string type6 { get; set;}
        public string type7 { get; set;}
        public string type8 { get; set;}
        public string type9 { get; set;}
        public string type10 { get; set;}
        public string type11 { get; set;}
        public string type12 { get; set;}
        public string type13 { get; set;}
        public string type14 { get; set;}
        public string type15 { get; set;}
        public string type16 { get; set;}
        public string type17 { get; set;}
        public string type18 { get; set;}
        public string type19 { get; set;}
        public string type20 { get; set;}
        public int type21 { get; set;}
        public int type22 { get; set;}
        public int type23 { get; set;}
        public int type24 { get; set;}
        public int type25 { get; set;}
        public int type26 { get; set;}
        public int type27 { get; set;}
        public int type28 { get; set;}
        public int type29 { get; set;}
        public int type30 { get; set;}
        public int type31 { get; set;}
        public int type32 { get; set;}
        public int type33 { get; set;}
        public int type34 { get; set;}
        public int type35 { get; set;}
        public int type36 { get; set;}
        public int type37 { get; set;}
        public int type38 { get; set;}
        public int type39 { get; set;}
        public int type40 { get; set;}
        public DateTime type41 { get; set;}
        public DateTime type42 { get; set;}
        public DateTime type43 { get; set;}
        public DateTime type44 { get; set;}
        public DateTime type45 { get; set;}
        public DateTime type46 { get; set;}
        public DateTime type47 { get; set;}
        public DateTime type48 { get; set;}
        public DateTime type49 { get; set;}
        public DateTime type50 { get; set;}

    }
}
