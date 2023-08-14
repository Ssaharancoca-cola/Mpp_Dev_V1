using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public static class Constant
    {
        public const string sessionID = "Session_ID";
        public const string ldOID = "LD_OID";
        public const string statusSuccess = "Success";
        public const string notNull = " can not be null.";
        public const string accessDenied = " Access denied";
        public const string sessionExpire = " Your Session has expired.";
        public const string dateDataType = "System.DateTime";
        public const string commonErrorMsg = "Some Problem. Please try again later !";
        public const string search = "search";
        public const string update = "update";
        public const string suppliedCode = "SUPPLIED_CODE";
        public const string cascombo = "CASCOMBO";
        public const string dateFromColumnName = "DATE_FROM";
        public const string notValidAdUser = "The user with the given id is not a valid user in AD";
        public const string formatMisMatch = "Format Mismatch";
        public const string dataSaveSuccessFully = "Data Save SuccessFully";
        public const string noRecordFound = "No Record Found";
        public const string selectAtleastOneSearchCriteria = "Please select At least One Search Criteria";
        public const string toDateGreaterThanFromDate = "Please enter 'To Date' Greater Then 'From Date'";
        public const string datedatatype = "System.DateTime";
        public const string mppAppDataBaseConnName = "MPP_Context";
        public const string mandatoryField = " please enter values in atleast one field apart from effective date";
        public const string rowStatus = "ROW_STATUS";
        public const string ActionTypeWhileUpdateFromWorkFlow = "workFlowUpdate";
        public const string ActionTypeWhileUpdateFromSearch = "searchupdate";
        public const string effectiveDateMandatoryField = "Please enter effective date, its mandatory field";
        public const string importSuccessFullyMessage = "Record has been Imported SuccessFully ";
    }
}
