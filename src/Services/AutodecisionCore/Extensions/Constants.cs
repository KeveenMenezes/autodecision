using AutodecisionCore.Events;

namespace AutodecisionCore.Extensions
{
    public class Constants
    {
        public const string ApiKey = "";
        public const string Claims = "claims";
        public const string EmployerPromissoryNote = "Employer Promissory Note";
        public const string DefaultTrigger = "Undefined";
        public const int Zero = 0;
    }

    public class CacheKeys
    {
        public const string Cache = "cache:";
        public const string Tags = "_doc_template_tags_redis";
    }

    public class RedLockKeys
    {
        public const string RequestProcess = "redlock:request_process_";
    }

    public static class AutoDecisionUser
    {
        public const int Id = 74;
        public const string Name = "Autodecision";
    }

    public static class ExceptionMessages
    {
        public const string NotFound = "Not found";
    }

    public static class OpenPayrollVendor
    {
        public const string Argyle = "Argyle";
        public const string Atomic = "Atomic";
        public const string Pinwheel = "Pinwheel";
    }

    public class ApplicationStatus
    {
        public const string IncompleteApplication = "1";
        public const string Processing = "2";
        public const string PendingApproval = "3";
        public const string PendingSignature = "4";
        public const string Signed = "5";
        public const string Booked = "6";
        public const string Liquidated = "7";
        public const string Deleted = "8";
        public const string Canceled = "9";
        public const string OpenedForChanges = "10";
        public const string Denied = "11";
        public const string Expired = "12";
    }

    public enum InternalStatusEnum
    {
        Pending = 0,
        AutoDeny = 1,
        AutoApproval = 2,
        PendingApproval = 3,
        PendingDocuments = 4
    }

    public enum AutoApprovalResultEnum
    {
        PendingAllotment = 0,
        Approved = 1,
        Pending = 3,
        Denied = 2,
        Ignored = 4,
        Error = 5
    }

    public static class BMGMoneyProgram
    {
        public const string NotSet = "0";
        public const string LoansAtWork = "1";
        public const string LoansForFeds = "2";
        public const string LoansForAll = "4";

        public static List<string> GetList() => new List<string> { NotSet, LoansAtWork, LoansForFeds, LoansForAll };

        public static bool IsValidProgram(string value = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return GetList().Any(a => a.Equals(value));
        }

        public static bool IsInvalidProgram(string value = null) => !IsValidProgram(value);
    }

    public static class BMGMoneySubProgram
    {
        public const int NotSet = 0;
        public const int LoansAtWork = 1;
        public const int LoansForFeds = 2;
        public const int LoansForAll = 4;
        public const int Retired = 5;
        public const int AlternativeLoans = 6;
        public const int VeteransDisability = 7;
        public const int LoansForRetireesACH = 8;
        public const int LoansForAllHighTurnOver = 9;
        public const int LoansForAllMediumTurnOver = 10;
    }

    public static class Topics
    {
        public const string DebitCardConnected = "debit-card.connected";
        public const string FaceIdCreated = "face-id.created";
        public const string OpenConnections = "open_connections_proccess";
    }

    public static class EmployersIds
    {
        public const int CityOfSweetwater = 2;
        public const int Odebrecht = 5;
        public const int BrowardCountyFL = 76;
        public const int UnitedStatesPostalService = 698;
    }

    public static class PayAllocationType
    {
        public const string Amount = "amount";
        public const string Percentage = "percentage";
        public const string Remainder = "remainder";

        public static List<string> GetTypesWithoutAllocationValue() => new() { Percentage, Remainder };

        public static bool HasNoAllocationValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return GetTypesWithoutAllocationValue().Any(a => a.Equals(value));
        }
    }

    public static class Reason
    {
        public const string DebitCard = "Debit Card";
        public const string CreatedFaceId = "Created Face Id";
        public const string AllotmentSDDReceived = "Allotment SDD Received";
        public const string AllFlagsApproved = "All flags approved";
        public const string DeclineInsuficientResponse = "Decline Insuficient Response";

        public static string GetDebitCardReason(DebitCardEventType DebitCardEventType) => $"{DebitCardEventType} {DebitCard}";
    }

    public static class FlagCode
    {
        public const string OneLoanPerSSN = "21";
        public const string FoundOnBlockList = "22";
        public const string HouseHoldHit = "182";
        public const string BankInfoFound = "187";
        public const string FoundOnWhiteList = "189";
        public const string GrossPay = "190";
        public const string CustomerAuthentication = "192";
        public const string TUBankruptcy = "193";
        public const string EligibilityRule = "194";
        public const string ReverseCensus = "195";
        public const string InternetBank = "197";
        public const string RoutingNumberVerification = "200";
        public const string TransunionScores = "206";
        public const string TUCriticalAndInternetBank = "207";
        public const string SimilarCustomer = "211";
        public const string NoTUResponse = "213";
        public const string PhoneValidation = "217";
        public const string BrowserFingerprint = "218";
        public const string EmploymentLength = "220";
        public const string PhoneVoip = "221";
        public const string AgenciesEligibilityRule = "225";
        public const string Giact = "226";
        public const string ActiveMilitaryDuty = "227";
        public const string DifferentStates = "228";
        public const string CustomerIdentityFlag = "230";
        public const string DifferentStatesGeolocation = "232";
        public const string AllotmentValidation = "219";
        public const string FirstnetCreditHistory = "235";
        public const string OpenBankingOrPayrollNotConnected = "236";
        public const string DebitCardxBankAccountAnalysis = "241";
        public const string OpenPayrollNotConnected = "243";
        public const string OpenBankingNotConnected = "244";
        public const string OpenPayrollSSNDoesNotMatch = "245";
        public const string DailyReceivings = "246";
        public const string Customer1xN = "247";
        public const string LoanVerification = "180";
        public const string CreditPolicyIsMissing = "248";
        public const string Flag209 = "209";
        public const string OpenForChanges = "183";
        public const string FraudAlert = "249";
        public const string OpenPayrollInconsistency = "253";
    }

    public static class DebitCardVendor
    {
        public const string PayNearMe = "paynearme";
        public const string Repay = "repay";
        public const string Tabapay = "tabapay";
    }

    public static class JWTIssuer
    {
        public const string Webportal = "webportal";
    }

    public static class JWTUserId
    {
        public const int System = 2;
    }

    public static class JWTUserName
    {
        public const string System = "System";
    }

    public static class NewCreditPolicyRule
    {
        public const string NetIncome = "1";
        public const string FaceIdMandatory = "2";
        public const string GrossIncome = "3";
        public const string CommitmentLevel = "4";
        public const string EndOfDayBalance = "5";
        public const string LengthOfEmployment = "6";
        public const string EndBalanceCommitmentLevel = "7";
        public const string OpenBankingMandatory = "8";
        public const string OpenPayrollMandatory = "9";
        public const string OpenPayrollOrBankingMandatory = "10";
        public const string DebitCard = "11";
        public const string DebitCardRefi = "12";
        public const string ForceAutomaticAllotmentOrSDD = "13";
    }

    public static class CreditPolicyParameterKey
    {
        public const string CashlessCommitmentLevel = "cashless_commitment_level";
    }

    public enum VendorIds
    {
        Plaid = 1,
        MoneyKit = 2,
    }

    public static class DocumentType
    {
        public const string AllotmentProof = "3";
        public const string SplitDirectDepositProof = "4";
        public const string OldestPaystub = "33";
    }
}