using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces;

public interface INewOpenBankingRepository
{
    Task<OpenBanking> GetNewOpenBanking(int customerId);
}