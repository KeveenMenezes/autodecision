using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface ICreditRiskRepository
    {
        Task<ApplicationScore> GetApplicationScore(string loanNumber);
    }
}
