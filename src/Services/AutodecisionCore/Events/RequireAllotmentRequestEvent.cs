namespace AutodecisionCore.Events
{
    public class RequireAllotmentRequestEvent
    {
        public string LoanNumber { get; set; }
        public string PaymentType { get; set; }
    }
}
