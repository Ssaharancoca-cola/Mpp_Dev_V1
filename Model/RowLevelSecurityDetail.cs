using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
   public class RowLevelSecurityDetail
    {
        public string UserID { get; set; }
        public string SuppliedCode { get; set; }
        public List<RowLevelSecurityValues> RowLevelSecurityValues { get; set; }
        public List<UserSecurityValuess> UserSecurityValues { get; set; }
        public List<RowLevelSecurityOperator> RowLevelSecurityOperator { get; set; }
    }
}
