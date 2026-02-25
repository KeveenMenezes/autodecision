
namespace AutodecisionMultipleFlagsProcessor.Extensions
{
    public class Constants
    {
        public const string ApiKey = "";
        public const string Claims = "claims";
        public const string EmployerPromissoryNote = "Employer Promissory Note";
    }

    public class CacheKeys
    {
        public const string Cache = "cache:";
        public const string Tags = "_doc_template_tags_redis";
    }

    public static class SystemUser
    {
        public const int Id = 2;
    }

    public static class ExceptionMessages
    {
        public const string NotFound = "Not found";
    }

    public static class CensusTransactionStatus
    {
        public const string Found = "Found";
        public const string NotFound = "NotFound";
        public const string Error = "Error";
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
        public const string ExcessiveAddressChanges = "214";
        public const string NoTUResponse = "213";
        public const string HighestCreditLimit = "215";
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
        public const string AllotmentValidationOptionTwo = "219";
        public const string AllotmentValidationOptionOne = "234";
        public const string FirstnetCreditHistory = "235";
        public const string OpenBankingOrPayrollNotConnected = "236";
        public const string DebitCardBankAccountAnalysis = "241";
        public const string OpenPayrollNotConnected = "243";
        public const string OpenBankingNotConnected = "244";
        public const string OpenPayrollSSNDoesNotMatch = "245";
        public const string DailyReceivings = "246";
        public const string CreditPolicyIsMissing = "248";
        public const string FraudAlert = "249";
        public const string MandatoryHRVerification = "250";
        public const string PartnerAssociation = "251";
        public const string FactorTrustInconsistency = "252";
        public const string ProbableCashApp = "254";
        public const string OpenPayrollInconsistency = "253";
        public const string OpenPayrollOcrolusAccuracy = "256";
        public const string OpenPayrollOcrolusSignalFraud = "257";
        public const string OpenPayrollOcrolusDocumentStatus = "258";
        public const string OpenPayrollMismatchInformation = "259";
        public const string OpenPayrollOcrolusHireDate = "260";
        public const string LevelOfCommitment = "255";
        public const string IncomeValidation = "262";
        public const string ClarityOFACHIT = "263";
        public const string PhoneValidated = "264";
    }

    public static class BMGMoneyProgram
    {
        public const string NotSet = "0";
        public const string LoansAtWork = "1";
        public const string LoansForFeds = "2";
        public const string LoansForAll = "4";

        public static string GetProgramName(string program) => program switch
        {
            LoansAtWork => "LoansAtWork",
            LoansForFeds => "LoansForFeds",
            LoansForAll => "LoansForAll",
            _ => NotSet,
        };
    }

    public static class NewCreditPolicyRule
    {
        public const string NetIncome = "1";
        public const string FaceIdMandatory = "2";
        public const string GrossIncome = "3";
        public const string EndOfDayBalance = "5";
        public const string EndBalanceCommitmentLevel = "7";
        public const string OpenBankingMandatory = "8";
        public const string OpenPayrollMandatory = "9";
        public const string OpenPayrollOrBankingMandatory = "10";
        public const string DebitCard = "11";
        public const string DebitCardRefi = "12";
        public const string ForceAutomaticAllotmentOrSDD = "13";
    }

    public static class ProductId
    {
        public const int Standard = 1;
        public const int Cashback = 2;
        public const int Cashless = 3;
        public const int CashlessAndDeferredPayment = 4;
        public const int LowIncome = 5;
        public const int LowIncomeCashback = 6;
        public const int CashlessChangeEmployer = 7;
        public const int DebtSettlement = 8;
        public const int RetentionProduct = 9;
        public const int ProductAtWork = 10;

        public static string GetProductName(this int id) =>
           id switch
           {
               Standard => "Standard",
               Cashback => "Cashback",
               Cashless => "Refi Cashless",
               CashlessAndDeferredPayment => "Refi Cashless + Deferred Payment",
               LowIncome => "Low Income",
               LowIncomeCashback => "Low Income Cashback",
               DebtSettlement => "Debt settlement",
               RetentionProduct => "Retention Product",
               ProductAtWork => "Product At Work",
               _ => "NotSet"
           };
    }

    public class ApplicationTypeLocal
    {
        public const string NewLoan = "1";
        public const string Refi = "2";

        public static string GetName(string type) => type switch
        {
            Refi => "Refi",
            _ => "New Loan"
        };
    }
    public class CommunicationTypeNewCommunication
    {
        public const string OldestPaystub = "oldest_paystub";
    }

    public class DocumentType
    {
        public const string Unknow = "0";
        public const string ID = "1";
        public const string ProofofAddress = "2";
        public const string AllotmentProof = "3";
        public const string SplitDirectDepositProof = "4";
        public const string SocialSecurityCard = "5";
        public const string BankStatement = "6";
        public const string Paystub = "7";
        public const string W2 = "8";
        public const string PromissoryNote = "9";
        public const string PaymentTransferForm = "10";
        public const string PayrollDeductionAuthorizationForm = "11";
        public const string eConsent = "12";
        public const string ProofOfNameChange = "13";
        public const string KansasAllotmentAuthorizationForm = "14";
        public const string Selfie = "15";
        public const string Internal = "16";
        public const string VerificationOfEmpoyment = "17";
        public const string CashbackChurn = "18";
        public const string DriverLicenseFront = "19";
        public const string DriverLicenseBack = "20";
        public const string BenefitsStatementLetter = "21";
        public const string VeteranDisabilityLetter = "22";
        public const string PaymentTransferFormDisabilityRetireesAch = "23";
        public const string MilitaryID = "24";
        public const string SignatureDocuments = "25";
        public const string AnnuityStatement = "26";
        public const string BenefitVerificationLetter = "27";
        public const string RetireeAccountStatement = "28";
        public const string USCGAnnuitantStatementOfMonthlyIncome = "29";
        public const string AditionalSourceIncomeDocument = "30";
        public const string NoticeOfAction = "C16";
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

    public static class DocumentReviewStatus
    {
        public const string Declined = "0";
        public const string Approved = "1";
        public const string Requested = "2";
    }
}