using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
   public class RowLevelSecurityOperator
    {
        public string AttrValue { get; set; }
        public string AttrName { get; set; }
    }
    public class RowLevelSecurityValues
    {
        public string DisplayMember { get; set; }
        public int ValueMember { get; set; }

    }


}
