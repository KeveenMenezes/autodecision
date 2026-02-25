using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IFactorTrustRepository
    {
        Task<FactorTrust?> GetFactorTrustInfo(int customerId, string loanNumber);
    }
}