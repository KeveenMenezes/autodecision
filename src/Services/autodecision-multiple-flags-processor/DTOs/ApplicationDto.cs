namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class ApplicationDto
    {
        public int? Id { get; set; }

        public string LoanNumber { get; set; }

        public string Type { get; set; }

        public decimal? AmountOfPayment { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string BrowserFingerprint { get; set; }

        public decimal? LoanAmount { get; set; }

        public int? EmployerId { get; set; }

        public string EmployerKey { get; set; }

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

        public int? CustomerId { get; set; }

        public string PaymentType { get; set; }
    }
}
