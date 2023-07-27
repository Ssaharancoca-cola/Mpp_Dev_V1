using Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Entity_Type_Attr_Detail : EntityTypeAttr 
    {
        public List<DropDownData>? dropDownDataList {get; set;}
    }
    public enum ControlType
    {
        TEXTBOX = 0,
        COMBOBOX = 1,
        DATEPICKER = 2,
        PARCOMBO = 3,
        CASCOMBO = 4,
        USERBOX = 5
    };
}
