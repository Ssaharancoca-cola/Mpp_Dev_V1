using DAL.Common;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models;
using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace DAL
{
    public class MasterData : IDisposable
    {
        public void Dispose() { }

        ///<summary>
        /// This method will return list of Dimension 
        /// </summary>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of Dimension </returns>
        
        public List<DimensionName> ShowMenuData (out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            string sqlQuery = "select dimension_display_name from MPP_CORE.entity_type where DIMENSION_DISPLAY_NAME is not null group by dimension_display_name";
            List<DimensionName> menuList = new List<DimensionName>();
            try
            {
                using (MPP_Context objMppComtext = new MPP_Context())
                {
                    menuList = objMppComtext.Set<DimensionName>().FromSqlRaw(sqlQuery).ToList();
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
            return menuList;
        }

        ///<summary>
        /// This method will return list of entity type 
        /// </summary>
        /// <param name="DimensionName"> Dimension Name.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of entity type  </returns>
        
        public List<EntityType> ShowSubMenuData(string dimensionName,out string outMsg)
        {
            outMsg = Constant.statusSuccess;
            List<EntityType> menuList = new List<EntityType>();
            string sqlQuery = "select * from MPP_CORE.entity_type where upper(dimension_display_name) = upper('" + dimensionName + "')";
            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    menuList = mPP_Context.Set<EntityType>().FromSqlRaw(sqlQuery).ToList();
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
            return menuList;
        }

        ///<summary>
        /// This method will return list of entity type attribute
        /// </summary>
        /// <param name="entityTypeId"> Entity Type Id.</param>
        /// <param name="outMsg"> Message.</param>
        /// <returns>List of entity type attribute </returns>

        public async Task<(List<Entity_Type_Attr_Detail>, string)> ShowAttributeData(int entityId, string viewType, string userName)
        {
            string outMsg = Constant.statusSuccess;
            List<EntityTypeAttr> attributeList = new List<EntityTypeAttr>();
            List<Entity_Type_Attr_Detail> attributedetail = new List<Entity_Type_Attr_Detail>();
            string[] listBoxQuery;
            string insertAliasName = string.Empty;

            try
            {
                using (MPP_Context mPP_Context = new MPP_Context())
                {
                    if (viewType == Constant.update)
                        attributeList = mPP_Context.EntityTypeAttr.Where(x => x.EntityTypeId == entityId)
                                                                           .Where(x => x.AttrDataType != Constant.suppliedCode)
                                                                           .Where(x => x.DisplayType != Constant.cascombo)
                                                                           .ToList();
                    else
                        attributeList = mPP_Context.EntityTypeAttr.Where(x => x.EntityTypeId == entityId).ToList();

                }
                foreach (var item in attributeList)
                {
                    if (item.AttrDisplayName != "Sub Channel Code")
                    {
                        Entity_Type_Attr_Detail entity_Type_Attr_Detail = new Entity_Type_Attr_Detail();
                        entity_Type_Attr_Detail.AttrDataType = item.AttrDataType;
                        entity_Type_Attr_Detail.DisplayType = item.DisplayType;
                        entity_Type_Attr_Detail.AttrDisplayName = item.AttrDisplayName;
                        entity_Type_Attr_Detail.AttrDisplayOrder = item.AttrDisplayOrder;
                        entity_Type_Attr_Detail.AttrLength = item.AttrLength;
                        entity_Type_Attr_Detail.EntityTypeId = item.EntityTypeId;
                        entity_Type_Attr_Detail.AttrName = item.AttrName;
                        entity_Type_Attr_Detail.IsMandatoryFlag = item.IsMandatoryFlag;
                        entity_Type_Attr_Detail.IsListable = item.IsListable;
                        entity_Type_Attr_Detail.CasQuery = item.CasQuery;
                        entity_Type_Attr_Detail.CasDrop = item.CasDrop;
                        string selectQuery = "select parent_entity_type_id from MPP_CORE.entity_type_attr where entity_type_id = " + entityId + "and attr_name='" + item.AttrName + "'";

                        if (!string.IsNullOrEmpty(Convert.ToString(item.ListBoxQuery)) && item.ListBoxQuery != "NULL")
                        {
                            dynamic parentId;
                            using (MPP_Context mPP_Context = new MPP_Context())
                            {
                                try
                                {
                                    parentId = mPP_Context.EntityTypeAttr.Where(x => x.EntityTypeId == entityId && x.AttrName == item.AttrName)
                                       .Select(x => x.ParentEntityTypeId)
                                       .FirstOrDefault();


                                }
                                catch
                                {
                                    parentId = mPP_Context.EntityTypeAttr.FromSqlInterpolated($"{selectQuery}")
                                                               .Select(x => x.ParentEntityTypeId)
                                                               .FirstOrDefault();
                                }
                            }
                            if (parentId == null)
                            {
                                List<DropDownData> dropDownDataList = new List<DropDownData>();
                                listBoxQuery = Convert.ToString(item.ListBoxQuery).ToUpper().Split(new string[] { "FROM" }, StringSplitOptions.None);
                                insertAliasName = listBoxQuery[0].Insert(listBoxQuery[0].IndexOf(','), " AS VALID_VALUES ");
                                insertAliasName = insertAliasName.Insert(insertAliasName.Length - 1, " AS VALUE_NAME ");
                                using (var mPP_Context = new MPP_Context())
                                {
                                     //dropDownDataList = mPP_Context.Database.SqlQuery<DropDownData>(insertAliasName + "FROM" + listBoxQuery[1]).ToList();
                                    string sqlQuery = $"{insertAliasName}FROM{listBoxQuery[1]}";
                                    dropDownDataList = mPP_Context
                                                        .Set<DropDownData>()
                                                        .FromSqlInterpolated(FormattableStringFactory.Create(sqlQuery))
                                                        .ToList();
                                }
                                entity_Type_Attr_Detail.dropDownDataList = dropDownDataList;
                            }
                            //else
                            //{
                            //    List<DropDownData> dropDownDataList = new List<DropDownData>();
                            //    listBoxQuery = Convert.ToString(item.ListBoxQuery).ToUpper().Split(new string[] {"FROM"}, StringSplitOptions.None);
                            //    insertAliasName = listBoxQuery[0].Insert(listBoxQuery[0].IndexOf(','), " AS VALID_VALUES ");
                            //    insertAliasName = insertAliasName.Insert(insertAliasName.Length - 1, " AS VALUE_NAME ");
                            //    string ListBoxSelectQuery = Convert.ToString(item.ListBoxQuery.ToUpper());
                            //    int startIndex = ListBoxSelectQuery.IndexOf("FROM");
                            //    int endIndex = ListBoxSelectQuery.IndexOf("WHERE");
                            //    string viewName = string.Empty;
                            //    using (MPP_Context mPP_Context = new MPP_Context())
                            //    {
                            //        string? iResult = null;
                            //        var viewNameParameter = new OutputParameter<string?>(iResult);
                            //        string returnValue = "";
                            //        var returnValueParameter = new OutputParameter<string>(returnValue);
                            //        await mPP_Context.Procedures.MPP_ENTITY_SEC_BASE_VIEWS_FN_PROCAsync(Convert.ToInt32(parentId), userName.ToUpper(), viewNameParameter);
                            //        viewName = viewNameParameter.Value.ToString();
                            //        viewName = " ( " + viewName + " ) ";
                            //    }
                            //    ListBoxSelectQuery = ListBoxSelectQuery.Substring(0, startIndex + 4) + " ( " + viewName + ")" + ListBoxSelectQuery.Substring(endIndex);
                            //    using (MPP_Context mPP_Context = new MPP_Context())
                            //    {
                            //        dropDownDataList = mPP_Context.Set<DropDownData>().FromSqlRaw(insertAliasName + "FROM" + "(" + ListBoxSelectQuery + ")").ToList();
                            //    }
                            //    dropDownDataList = dropDownDataList.OrderBy(x => x.VALUE_NAME).ToList();
                            //    entity_Type_Attr_Detail.dropDownDataList = dropDownDataList;
                            //}

                        }
                        attributedetail.Add(entity_Type_Attr_Detail);
                    }
                    else
                    {
                        //
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
            }
            return (attributedetail, outMsg);

        }
    }
}