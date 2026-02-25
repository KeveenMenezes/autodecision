using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Services.Interfaces
{
    public interface IDebitCard
    {
        Task<AccountsCustomerDTO> GetAccountsCustomer(int customerId);
    }
}
