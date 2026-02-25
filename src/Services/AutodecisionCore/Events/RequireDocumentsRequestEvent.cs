namespace AutodecisionCore.Events
{
    public class RequireDocumentsRequestEvent
    {
        public string LoanNumber { get; set; }
        public List<string> Flags { get; set; }
    }
}
