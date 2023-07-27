using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Serializable]
    public class UserInfo
    {
        public UserInfo() { }

        public int IsActive { get; set; }
        public string UserName { get; set; }        
        public string UserId { get; set; }
        public string? UserEmail { get; set; }
        public int IsAdmin { get; set; }
        public string? Password { get; set; }
        public string ROLE_NAME { get; set;}
    }
}
