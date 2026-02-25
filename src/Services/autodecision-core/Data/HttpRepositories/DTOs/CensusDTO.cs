namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class CensusDTO
    {
        public int CustomerId { get; set; }
        public int EmployerId { get; set; }
        public decimal? SalaryPerPeriod { get; set; }
        public string TimeType { get; set; }
        public string PayrollGroup { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EmployeeRegistration { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? FlagGrossPayValue { get; set; }
        public decimal? FlagCustomerSimilarityValue { get; set; }
        public string FlagEligibilityRuleValue { get; set; }
        public bool FlagReverseCensusActive { get; set; }
        public string FlagAgenciesEligibilityRuleValue { get; set; }
        public bool CensusValidation { get; set; }
        public bool CensusParamsActive { get; set; }

    }
}
