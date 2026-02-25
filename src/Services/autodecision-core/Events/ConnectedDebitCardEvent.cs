namespace AutodecisionCore.Events
{
    public class ConnectedDebitCardEvent
    {
        public int CustomerId { get; set; }
        public string CardNumber { get; set; }
        public string CardBin { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CardBrand { get; set; }
        public string Vendor { get; set; }
        public bool Active { get; set; }
        public string CardBinEmissor { get; set; }
        public bool CardBinStatus { get; set; }
		public DebitCardEventType DebitCardEventType { get; set; }
	}

	public enum DebitCardEventType
	{
		Disconnect = 0,
		Connect = 1
	}
}
