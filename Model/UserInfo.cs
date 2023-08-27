using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Serializable]
    public class UserInfo
    {
        public UserInfo()
        { }
        public UserInfo(string userName, string userID, int isAdmin, List<EntityPrivileges> entityPrivilegesList, String Password, int isActive, String Role, String _TotalRecords)
        {
            this.UserName = userName;
            this.UserID = userID;
            this.IsAdmin = isAdmin;
            this.EntityPrivilegesList = entityPrivilegesList;
            this.Password = Password;
            this.IsActive = isActive;
            this.ROLE_NAME = Role;
            this.Total_Records = _TotalRecords;
        }
        public UserInfo(string userName, string userID, string userEmail, int isAdmin, List<EntityPrivileges> entityPrivilegesList, String Password, int isActive, String Role)
        {
            this.UserName = userName;
            this.UserID = userID;
            this.UserEmail = userEmail;
            this.IsAdmin = isAdmin;
            this.EntityPrivilegesList = entityPrivilegesList;
            this.Password = Password;
            this.IsActive = isActive;
            this.ROLE_NAME = Role;
        }
        public UserInfo(string userName, string userID, int isAdmin, List<EntityPrivileges> entityPrivilegesList, String Password)
        {
            this.UserName = userName;
            this.UserID = userID;
            this.IsAdmin = isAdmin;
            this.EntityPrivilegesList = entityPrivilegesList;
            this.Password = Password;
        }
        public int? IsActive { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string UserEmail { get; set; }
        public int IsAdmin { get; set; }
        //public int IsAllowMapping { get; set; }
        public List<EntityPrivileges> EntityPrivilegesList { get; set; }
        public String Password { get; set; }
        public String ROLE_NAME { get; set; }       
        public String Total_Records { get; set; }
        public List<EntityType> dimnesionList { get; set; }

    }
}
