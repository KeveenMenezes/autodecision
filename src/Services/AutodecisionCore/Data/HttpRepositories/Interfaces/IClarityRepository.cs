using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IClarityRepository
    {
        Task<Clarity?> GetClarityInfo(string loanNumber);
        bool IsClarityValid(Clarity? clarity);
    }
}
