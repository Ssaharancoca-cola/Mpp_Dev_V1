using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WorkFlowDetailForRowLevelSecurity
    {
        public string UserID { get; set; }
        public string UserRole { get; set; }
        public string RoleID { get; set; }

        public string EntityId { get; set; }
        public string SelectedApproverId { get; set; }
        public List<ApproverDetail> SelectedApproverDetail { get; set; }
        public List<ApproverDetail> ApproverDetail { get; set; }


    }
    public class ApproverDetail
    {

        public string UserName { get; set; }
        public string UserLevel { get; set; }
        public string ApproverName { get; set; }
        public string ApproverLevel { get; set; }
        public string ApproverStatus { get; set; }
        public string ApproverComments { get; set; }
        public string ApproverId { get; set; }

    }

}
