using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using static AutodecisionCore.Data.HttpRepositories.DTOs.AccountsCustomerDTO;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IDebitCardRepository
    {
        Task<DebitCard> GetAccountsByCustomerId(int customerId);
        Task<DebitCard> BinNameLink(DebitCard account, string bankName);
    }
}
