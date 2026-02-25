namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class ApplicationDTO
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; }
        public string Type { get; set; }
        public decimal AmountOfPayment { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string BrowserFingerprint { get; set; }
        public decimal LoanAmount { get; set; }
        public int EmployerId { get; set; }
        public string EmployerName { get; set; }
        public string EmployerKey { get; set; }
        public string EmployerPaymentType { get; set; }
        public string FundingMethod { get; set; }
        public int? ProductId { get; set; }
        public string ProductKey { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string Program { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string StateAbbreviation { get; set; }
        public string LoanTermsStateAbbreviation { get; set; }
        public string StateIpUserRequest { get; set; }
        public int CustomerId { get; set; }
        public string PaymentType { get; set; }
        public int? PreviousApplicationId { get; set; }
        public bool IsEmployerCensusEligible { get; set; }
        public int? EmploymentLengthRangeId { get; set; }
        public string UwCluster { get; set; }
        public string ReconciliationSystem { get; set; }
        public bool HighRisk { get; set; }
        public string CreatedBy { get; set; }
        public bool TurndownActive { get; set; }
		public string AllotmentRoutingNumber { get; set; }
		public string AllotmentAccountNumber { get; set; }
        public decimal? VerifiedNetIncome { get; set; }
        public bool IsWebBankRollout { get; set; }
        public DateTime? VerifiedDateOfHire { get; set; }
        public int? PartnerId { get; set; }
        public int? EmployerPartnerId { get; set; }
        public bool EmployerIsAssociation { get; set; }
        public string PartnerName { get; set; }
        public int? ProductIdCreditPolicy { get; set; }
		public bool JpmExistsPending { get; set; }
		public decimal? AdditionalIncome { get; set; }
        public bool IsProbableCashApp { get; set; }
    }
}