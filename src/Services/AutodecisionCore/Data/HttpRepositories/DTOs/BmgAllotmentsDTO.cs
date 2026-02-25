namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
	public class BmgAllotmentsDTO
	{
		public string? Amount { get; set; }
		public string? AccountNumber { get; set; }
		public string? RoutingNumber { get; set; }
		public string? BankName { get; set; }
		public string? AccountType { get; set; }
		public string? AccountName { get; set; }
		public DateTime? CreatedAt { get; set; }
		public string? AllocationType { get; set; }
		public string? Status { get; set; }
	}
}
