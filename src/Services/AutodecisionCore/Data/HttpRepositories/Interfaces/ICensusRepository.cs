using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface ICensusRepository
    {
        Task<Census> GetCensusDataByCustomerId(int customerId, int employerId);
    }
}
