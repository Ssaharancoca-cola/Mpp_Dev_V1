using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class Mail_Master
    {
        public enum MailEventDetail
        {
            SUBMIT = 1,
            SUBMITWITHOUTWORKFLOW = 10,
            NOTIFYAPPROVE = 3,
            UPDATEWITHOUTWORKFLOW = 11,
            UPDATE = 8,
            NOTIFYUPDATE = 9,
            ABANDON = 6,
            NOTIFYABANDON = 7,
            APPROVE = 2,
            REJECT = 4,
            PENDINGAPPROVAL = 3,
            FINALAPPROVE = 10,
            NOTIFYREJECT =5,
            ERROR =13
                

        }
        //public virtual List<string> Tocollection { get; set; }
        //public virtual string Bodyalternate { get; set; }
        //public virtual List<string> CCcollection { get; set; }
        public virtual string MailToCC { get; set; }
        public virtual string MailBody { get; set; }
        public virtual string MailTo { get; set; }
        //public virtual string MailFrom { get; set; }
        public virtual string MailSubject { get; set; }

    }
}
