using AutodecisionCore.Contracts.ViewModels.Helpers;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IFlagHelperRepository
    {
        Task<FlagValidatorHelper> GetFlagHelperInformationAsync(int customerId, int employerId, int applicationId);
    }
}