namespace AutodecisionCore.Data.HttpRepositories.DTOs
{

    public class AccountsCustomerDTO
    {
        public SummaryInfo Summary { get; set; } = new SummaryInfo();

        public List<AccountsDTO> List { get; set; } = new List<AccountsDTO>();

    }
    public class AccountsCustomerReturn
    {
        public bool Success { get; set; }
        public AccountsCustomerDTO Data { get; set; }
    }
    public class SummaryInfo
    {
        public bool HasActiveConnections { get; set; }
    }
    public class AccountsDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CardNumber { get; set; }
        public string CardBin { get; set; }
        public string Expiration { get; set; }
        public string CardBrand { get; set; }
        public string Vendor { get; set; }
        public bool Active { get; set; }
        public string CardBinEmissor { get; set; }
        public bool CardBinStatus { get; set; }
    }




}
