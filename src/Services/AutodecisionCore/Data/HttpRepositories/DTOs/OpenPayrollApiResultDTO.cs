
namespace AutodecisionCore.Data.HttpRepositories.DTOs
{

    public class OpenPayrollApiResultDTO
    {
        public bool Success { get; set; }
        public List<OpenPayrollDTO> Data { get; set; }
    }

    public class OpenPayrollDTO
    {
        public string Name { get; set; }
        public string Ssn { get; set; }
        public bool IsActive { get; set; }
        public DateTime ConnectionUpdatedAt { get; set; }
        public List<PayoutsDTO> Payouts { get; set; }
        public PayAllocationsDTO PayAllocation { get; set; }
        public List<EmploymentsDTO> Employments { get; set; }
    }

}
