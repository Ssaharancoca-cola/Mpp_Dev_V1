using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Serializable]
    public class Entity
    {
        public Entity()
        { }

        public Entity(string entityName, int entityID, string displayName)
        {
            this.EntityName = entityName;
            this.EntityID = entityID;
            this.DisplayName = displayName;
        }

        public string EntityName { get; set; }
        public int EntityID { get; set; }
        public string DisplayName { get; set; }
    }

    public class EntityPrivileges
    {
        public EntityPrivileges()
        { }

        public EntityPrivileges(Entity entityDetails, int readStatus, int updateStatus, int createStatus, int importStatus, int roleId)
        {
            this.EntityDetails = entityDetails;
            this.ReadStatus = readStatus;
            this.UpdateStatus = updateStatus;
            this.CreateStatus = createStatus;
            this.ImportStatus = importStatus;
            this.RoleId = roleId;
        }

        public Entity EntityDetails { get; set; }
        public int ReadStatus { get; set; }
        public int UpdateStatus { get; set; }
        public int CreateStatus { get; set; }
        public int ImportStatus { get; set; }
        public int RoleId { get; set; }

    }
}
