namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class CreditPolicyApiResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CreditPolicyDTO CreditPolicy { get; set; }

        public CreditPolicyApiResultDTO() { }

        public CreditPolicyApiResultDTO(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public CreditPolicyApiResultDTO(CreditPolicyDTO creditPolicy)
        {
            Success = true;
            CreditPolicy = creditPolicy;
        }

        public class CreditPolicyDTO
        {
            public int Id { get; set; }
            public string EmployerKey { get; set; }
            public string ProductKey { get; set; }
            public string StateAbbreviation { get; set; }
            public string PricingKey { get; set; }
            public string PricingKeyName { get; set; }
            public string LoanAmountTableKey { get; set; }
            public string LoanAmountTableKeyName { get; set; }
            public string RuleGroupKey { get; set; }
            public string RuleGroupRefiKey { get; set; }
            public string RuleGroupKeyName { get; set; }
            public string RuleGroupRefiKeyName { get; set; }
            public string DefaultPaymentMethod { get; set; }
            public string FeeType { get; set; }
            public string InterestRateType { get; set; }
            public string PaymentsInARowKey { get; set; }
            public string PaymentsInARowName { get; set; }
            public string DefaultPaymentMethodName { get; set; }
            public string FeeTypeName { get; set; }
            public string InterestRateTypeName { get; set; }
            public string LateFeeKey { get; set; }
            public string EmployerName { get; set; }
        }
    }
}
