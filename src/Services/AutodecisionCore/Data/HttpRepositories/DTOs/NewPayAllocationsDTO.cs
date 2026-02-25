namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
	public class NewPayAllocationsDTO
	{
		public NewPayAllocationsDTO(int employerId, List<NewPayAllocation> payAllocations)
		{
			EmployerId = employerId;
			PayAllocations = payAllocations;
		}

		public NewPayAllocationsDTO()
		{

		}

		public int EmployerId { get; set; }
		public List<NewPayAllocation> PayAllocations { get; set; }
	}

	public class NewPayAllocation
	{
		public NewPayAllocation(DateTime createdAt, string bankRoutingNumber, string bankAccountNumber, string bankAccountType, string allocationType, string allocationValue)
		{
			CreatedAt = createdAt;
			RoutingNumber = bankRoutingNumber;
			AccountNumber = bankAccountNumber;
			AccountType = bankAccountType;
			AllocationType = allocationType;
			Amount = allocationValue;
		}

		public DateTime? CreatedAt { get; set; }
		public string? RoutingNumber { get; set; }
		public string? AccountNumber { get; set; }
		public string? AccountType { get; set; }
		public string? AllocationType { get; set; }
		public string? Amount { get; set; }
		public bool Remainder { get; set; }
	}
}
