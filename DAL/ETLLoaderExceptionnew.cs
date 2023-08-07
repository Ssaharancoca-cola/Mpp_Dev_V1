using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    [Serializable]
    public class ETLLoaderExceptionnew : Exception
    {
        private string _moduleName;
        private int _debugCounter;
        private string _progState;
        private string _userID;
        private short _loadID;

        private short _priority;

        public short Priority
        {
            get { return _priority; }
        }

        public short LoadID
        {
            get { return _loadID; }
        }

        public string UserID
        {
            get { return _userID; }
        }

        public string ProgState
        {
            get { return _progState; }
        }

        public new string Message
        {
            get { return base.Message; }
        }

        public new Exception InnerException
        {
            get { return base.InnerException; }
        }

        public int DebugCounter
        {
            get { return _debugCounter; }
        }

        public string ModuleName
        {
            get { return _moduleName; }
        }

        public ETLLoaderExceptionnew() { }
        public ETLLoaderExceptionnew(string message) : base(message) { }
        public ETLLoaderExceptionnew(string message, Exception inner) : base(message, inner) { }
        protected ETLLoaderExceptionnew(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public ETLLoaderExceptionnew(string moduleName, int debugCounter,
           string message, Exception innerException, string progState, string userID, short loadID, short priority)
            : base(message, innerException)
        {
            _moduleName = moduleName;
            _debugCounter = debugCounter;
            _progState = progState;

            _userID = userID;
            _loadID = loadID;
            _priority = priority;
        }

        public override string ToString()
        {
            return ToStringConsole();
        }

        public string ToStringDelimit(string lineDelimiter)
        {
            string strReturn = null;
            strReturn = "Module: " + this.ModuleName + lineDelimiter;
            strReturn = "DebugCounter: " + this.DebugCounter + lineDelimiter;
            strReturn = "Description: " + this.Message + lineDelimiter;
            strReturn = "InnerException: " + this.InnerException.ToString() + lineDelimiter;
            strReturn = "Program State: " + this.ProgState + lineDelimiter;
            strReturn = "Priority: " + this.Priority + lineDelimiter;
            strReturn = "UserID: " + this.UserID + lineDelimiter;
            strReturn = "LoadID: " + this.LoadID + lineDelimiter;
            return strReturn;
        }

        public string ToStringWeb()
        {
            return ToStringDelimit("<BR>");
        }

        public string ToStringConsole()
        {
            return ToStringDelimit("\n\r");
        }
    }
}
