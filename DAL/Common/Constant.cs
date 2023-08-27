using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Common
{
    public static class Constant
    {
        public const string ldOID = "LD_OID";
        public const string ABANDON = "ABANDON";
        public const string DELETE = "DELETE";
        public const string APPROVE = "APPROVE";
        public const string REJECT = "REJECT";
        public const string addNew = "AddNew";
        public const string import = "Import";
        public const string update = "update";
        public const string search = "search";
        public const string rowStatus = "ROW_STATUS";
        public const string sessionID = "SESSION_ID";
        public const string statusSuccess = "Success";
        public const string notNull = " can not be null.";
        public const string accessDenied = "Access denied";
        public const string formatMisMatch = "Format Mismatch";
        public const string suppliedCode = "SUPPLIED_CODE";
        public const string noRecordFound = "No record Found";
        public const string dateFromColumnName = "DATE_FROM";
        public const string inputRowIdColumnName = "INPUT_ROW_ID";
        public const string sessionexpire = "Your session has expired.";
        public const string requiredFieldUserID = "Please enter User Id";
        public const string userAlreadyExist = "The user already exist!";
        public const string historyViewEntityOIDColumnName = "ENTITY_OID";
        public const string dataSaveSuccessFully = "Data Save successfully";
        public const string ActionTypeWhileUpdateFromSearch = "searchupdate";
        public const string ActionTypeWhileUpdateFromWorkFlow = "workFlowUpdate";
        public const string commonErrorMsg = " Some Problem. Please try again later !";
        public const string selectAtleastOneSearchCriteria = "Please select atleast one search criteria";
        public const string toDateGreaterThanFromDate = "Please enter 'To Date' greater than 'From Date'";
        public const string mandatoryField = "Please enter values in atleast one field apart from effective date";
        public const string effectiveDateMandatoryField = "Please enter effective date, its mandatory field";
        public const string importSuccessFullyMessage = "Record has been imported successfully";
        public const string abandonSuccessFullyMessage = "The selected records have been abandoned successfully";
        public const string deletedSuccessFullyMessage = "The selected records have been deleted successfully";
        public const string approvedSuccessFullyMessage = "The selected records have been approved successfully";
        public const string rejectedSuccessFullyMessage = "The selected records have been Rejected successfully";
        public const string notValidAdUser = "The user with the given user id is not a valid user in AD";
        public const string datedatatype = "System.DateTime";
        public const string parcombo = "PARCOMBO";
        public const string cascombo = "CASCOMBO";
    }
}
