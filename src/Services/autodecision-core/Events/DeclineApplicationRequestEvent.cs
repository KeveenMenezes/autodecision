namespace AutodecisionCore.Events
{
    public class DeclineApplicationRequestEvent
    {
        public string LoanNumber { get; set; }
		public string Reason { get; set; }
	}
}
