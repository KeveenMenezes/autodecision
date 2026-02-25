using AutodecisionCore.Data.HttpRepositories.DTOs.Ocrolus;

namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
	public class NewOpenPayrollApiResultDTO
	{
		public bool Success { get; set; }
		public NewOpenPayrollDTO Data { get; set; }
	}

	public class NewOpenPayrollDTO
	{
		public string? Name { get; set; }
		public string? Ssn { get; set; }
		public string? Employer { get; set; }
		public int VendorType { get; set; }

        public bool IsConnected { get; set; }
		public List<NewPayAllocation> PayAllocations { get; set; }
		public List<BmgAllotmentsDTO> BmgAllotments { get; set; }
		public List<PayoutsDTO> Payouts { get; set; }


		public DateTime? HireDate { get; set; }
		//Ocrolus
		public int? OcrolusDocumentScore { get; set; }
        public int? OcrolusDocumentStatusId { get; set; }
        public List<OcrolusDocumentSignalsDTO>? OcrolusDocumentSignals { get; set; } 


    }
}
