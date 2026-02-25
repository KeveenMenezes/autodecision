namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class LastApplicationDTO
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; }
        public decimal AmountOfPayment { get; set; }
        public string BrowserFingerprint { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReconciliationSystem { get; set; }
        public ApplicationConnectionsDTO? ApplicationConnections { get; set; }
    }
}
