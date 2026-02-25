namespace AutodecisionCore.DTOs
{
    public class AccountsResponse
    {
        public int Id { get; set; }
        public int ConnectionId { get; set; }
        public string AccountVendorIdentifier { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool? IsDefault { get; set; }
        public string AccountType { get; set; }
        public string BankName { get; set; }
        public decimal? BalanceAvailable { get; set; }
        public decimal? BalanceCurrent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
