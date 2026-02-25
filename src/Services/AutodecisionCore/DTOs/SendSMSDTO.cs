namespace AutodecisionCore.DTOs
{
    public class SendSMSDto
    {
        public int CustomerId { get; set; }
        public int ApplicationId { get; set; }
        public string LoanNumber { get; set; }
        public string CommunicationType { get; set; }
        public bool Retry { get; set; }

        public SendSMSDto(int customerId, int applicationId, string loanNumber, string communicationType, bool retry)
        {
            LoanNumber = loanNumber;
            ApplicationId = applicationId;
            CommunicationType = communicationType;
            CustomerId = customerId;
            Retry = retry;
        }
    }
}
