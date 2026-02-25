using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface ITransunionRepository
    {
        Task<TransunionResult> GetTransunionResultInfo(int customerId);
        Task<TransunionResult> GetClarityTransunionResultInfo(int customerId);
        Task DeactivateOldRecords(int customerId);
        Task CreateNewTransunionResultFromClarity(Application application);
        Task<bool> IsTransunionProcessedForCustomer(int customerId);
    }
}
