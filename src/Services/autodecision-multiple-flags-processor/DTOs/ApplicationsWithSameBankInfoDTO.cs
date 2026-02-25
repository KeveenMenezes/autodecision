namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class ApplicationsWithSameBankInfoDTO
    {

        public List<ApplicationsWithSameBankInfo> Data { get; set; }

    }
    public class ApplicationsWithSameBankInfo
    {
        public string LoanNumber { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
    }
}
