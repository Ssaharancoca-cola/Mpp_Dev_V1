using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Previleges
    {
        public Previleges()
        {
            READ_FLAG = 0;
            CREATE_FLAG = 0;
            UPDATE_FLAG = 0;
            LANGUAGE_CODE = "";
            IMPORT_FLAG = 0;
        }
        public String LANGUAGE_CODE {get; set;}
        public Int32 IMPORT_FLAG { get; set;}
        public Int32 CREATE_FLAG { get; set;} 
        public Int32 UPDATE_FLAG { get; set;}
        public Int32 READ_FLAG { get; set;} 
            
    }
}
