using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IOpenBankingRepository
    {
        Task<OpenBanking> GetOpenBankingInfo(int customerId);
    }
}
