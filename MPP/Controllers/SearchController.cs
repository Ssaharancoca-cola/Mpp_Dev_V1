using Microsoft.AspNetCore.Mvc;
using DAL;
using Model.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Model;
using Microsoft.EntityFrameworkCore;

namespace MPP.Controllers
{
    public class SearchController : Controller
    {
        List<SearchParameter> fieldCollection = new List<SearchParameter>();

        public JsonResult GetCasCombo(string Id, string cId)
        {
            List<Entity_Type_Attr_Detail> attributeList = new List<Entity_Type_Attr_Detail>();
            attributeList = (List<Entity_Type_Attr_Detail>)TempData["attributeList"];
            string[] listBoxQuery;
            string inserAliasName = string.Empty;

            List<DropDownData> dropdownDataList = new List<DropDownData>();
            List<SelectListItem> casCombo = new List<SelectListItem>();

            if(attributeList != null)
            {
                TempData["attributeList"] = attributeList;
                foreach(var item in attributeList)
                {
                    if(item.DisplayType.ToUpper().Equals("PARCOMBO") && item.CasDrop.Equals(cId))
                    {
                        listBoxQuery = Convert.ToString(item.CasQuery).ToUpper().Split(new string[] {"FROM"}, StringSplitOptions.None);
                        inserAliasName = listBoxQuery[0].Insert(listBoxQuery[0].IndexOf(','), " AS VALiD_VALUES ");
                        inserAliasName = inserAliasName.Insert(inserAliasName.Length - 1, " AS VALUE_NAME ");

                        string value = "'" + Id.ToString() + "'";
                        string listquery = listBoxQuery[1].Replace("#REPLACECOND", value);

                        using(var mPP_Context = new MPP_Context())
                        {
                            dropdownDataList = mPP_Context.Set<DropDownData>().FromSqlRaw(inserAliasName + "FROM" + listquery).ToList();
                        }

                        dropdownDataList.ForEach(x =>
                        {
                            casCombo.Add(new SelectListItem { Text = x.VALUE_NAME, Value = x.VALID_VALUES.ToString() });
                        });
                    }
                }
            }
            return Json(new SelectList(casCombo, "Value", "Text"));
        }
    }
}
