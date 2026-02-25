using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;

namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class PayAllocationsDTO
    {
        public PayAllocationsDTO(int employerId, List<PayAllocation> payAllocations)
        {
            EmployerId = employerId;
            PayAllocations = payAllocations;
        }

        public PayAllocationsDTO()
        {

        }

        public int EmployerId { get; set; }
        public List<PayAllocation> PayAllocations { get; set; }
    }

    public class PayAllocation
    {
        public PayAllocation(DateTime createdAt, string bankRoutingNumber, string bankAccountNumber, string bankAccountType, string allocationType, string allocationValue)
        {
            CreatedAt = createdAt;
            BankRoutingNumber = bankRoutingNumber;
            BankAccountNumber = bankAccountNumber;
            BankAccountType = bankAccountType;
            AllocationType = allocationType;
            AllocationValue = allocationValue;
        }

        public DateTime CreatedAt { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountType { get; set; }
        public string AllocationType { get; set; }
        public string AllocationValue { get; set; }
    }

}
