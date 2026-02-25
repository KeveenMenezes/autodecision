namespace AutodecisionCore.Events
{
    public class OpenConnectionRequestEvent
    {
        public int ApplicationId { get; set; }
        public string LoanNumber { get; set; }
        public string Reason { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
