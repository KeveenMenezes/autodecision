namespace AutodecisionCore.Events
{
    public class NotifyAllotmentSddReceivedEvent
    {
        public string LoanNumber { get; set; }
        public string ReconciliationSystem { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
    }
}
