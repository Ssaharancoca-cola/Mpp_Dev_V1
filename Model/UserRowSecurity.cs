using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
   
        public class UserRowSecurity
        {
            public UserRowSecurity()
            {
                this.Dimension = string.Empty;
                this.EntityName = string.Empty;
                //this.Query = string.Empty;
                this.UserID = string.Empty;
                this.ENTITY_TYPE_ID = 0;
                this.READ_FLAG = 0;
                this.UPDATE_FLAG = 0;
                this.CREATE_FLAG = 0;
                this.IMPORT_FLAG = 0;
                //this.Total_Records = string.Empty;
                //this.Total_Pages = string.Empty;
                //this.ROLE_NAME = string.Empty;
            }

            public UserRowSecurity(String _dimension, string _user_ID, String _entity_name, String _query, int _entity_type_id,
                int _read_flag, int _update_flag, int _create_flag, int _import_flag, String _total_records, String _total_pages,
                String _ROLE_NAME, String _Operator, int _ROLE_ID)
            {
                this.Dimension = _dimension;
                this.EntityName = _entity_name;
                //this.Query = _query;
                this.UserID = _user_ID;
                this.ENTITY_TYPE_ID = _entity_type_id;
                this.READ_FLAG = _read_flag;
                this.UPDATE_FLAG = _update_flag;
                this.CREATE_FLAG = _create_flag;
                this.IMPORT_FLAG = _import_flag;
                //this.Total_Records = _total_records;
                //this.Total_Pages = _total_pages;
                //this.ROLE_NAME = _ROLE_NAME;
                //this.OPERATOR = _Operator;
                this.ROLE_ID = _ROLE_ID;
            }
            public String Dimension { get; set; }
            public String UserID { get; set; }
            public String EntityName { get; set; }
            //public String Query { get; set; }
            public int ENTITY_TYPE_ID { get; set; }
            public int READ_FLAG { get; set; }
            public int UPDATE_FLAG { get; set; }
            public int CREATE_FLAG { get; set; }
            public int IMPORT_FLAG { get; set; }
            //public String Total_Pages { get; set; }
            //public String Total_Records { get; set; }
            //public String ROLE_NAME { get; set; }
            public String FLG { get; set; }
            //public string OPERATOR { get; set; }
            public int ROLE_ID { get; set; }
            public List<ROLE> ROLELIST { get; set; }
            public String WFLG { get; set; }
            public int APR_FLG { get; set; }
        }
            
        

    
}
