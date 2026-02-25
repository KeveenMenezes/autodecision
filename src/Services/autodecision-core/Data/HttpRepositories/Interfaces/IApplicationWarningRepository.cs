using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IApplicationWarningRepository
    {
        Task<List<ApplicationWarning>> GetApplicationWarningsInfo(int applicationId);
    }
}
